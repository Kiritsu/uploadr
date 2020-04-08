using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Enums;
using UploadR.Database.Models;
using UploadR.Enums;
using UploadR.Models;

namespace UploadR.Services
{
    public class UploadService
    {
        private readonly IServiceProvider _services;
        private readonly SHA512Managed _sha512Managed;
        private readonly ILogger<AccountService> _logger;
        private readonly UploadConfiguration _uploadConfiguration;
        
        public UploadService(
            IServiceProvider services, 
            SHA512Managed sha512Managed,
            ILogger<AccountService> logger, 
            UploadConfigurationProvider uploadConfigurationProvider)
        {
            _services = services;
            _sha512Managed = sha512Managed;
            _logger = logger;
            _uploadConfiguration = uploadConfigurationProvider.GetConfiguration();
        }

        /// <summary>
        ///     Checks whether an upload is considered as valid or not.
        /// </summary>
        /// <param name="file">File to check.</param>
        /// <param name="password">Password to apply to that file.</param>
        /// <param name="upload">Out parameter representing a valid or not upload.</param>
        public bool TryCreateUpload(
            IFormFile file, 
            string password,
            out UploadOutModel upload)
        {
            upload = new UploadOutModel
            {
                Filename = file.FileName ?? "unknown",
                Size = file.Length,
                HasPassword = !string.IsNullOrWhiteSpace(password),
                ContentType = file.ContentType,
                StatusCode = UploadStatusCode.Ok
            };

            if (upload.Size > _uploadConfiguration.SizeMax)
            {
                upload.StatusCode = UploadStatusCode.TooBig;
                return false;
            }
            
            if (upload.Size < _uploadConfiguration.SizeMin)
            {
                upload.StatusCode = UploadStatusCode.TooSmall;
                return false;
            }

            if (_uploadConfiguration.EnabledTypes is null)
            {
                return true;
            }
            
            if (!_uploadConfiguration.EnabledTypes.Contains(upload.ContentType))
            {
                upload.StatusCode = UploadStatusCode.UnsupportedType;
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        ///     Upload the list of uploads to the server. It will check whether the uploads are valid.
        ///     It returns a model which indicates the succeeded uploads and failed ones.
        /// </summary>
        /// <param name="userGuid">Id of the user uploading the files.</param>
        /// <param name="files">Files to upload.</param>
        /// <param name="password">Password of the files. Can be null.</param>
        public async Task<UploadListOutModel> UploadAsync(
            Guid userGuid, 
            IEnumerable<IFormFile> files, 
            string password)
        {
            var model = new UploadListOutModel();
            var failed = new List<UploadOutModel>();
            var succeeded = new List<UploadOutModel>();
            
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            foreach (var file in files)
            {
                if (TryCreateUpload(file, password, out var upload))
                {
                    var name = Guid.NewGuid();
                    upload.Filename = name.ToString().Replace("-", "") 
                                      + Path.GetExtension(file.FileName);

                    var passwordHash = "";
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        var byteHash = _sha512Managed.ComputeHash(
                            Encoding.UTF8.GetBytes(password));
                        passwordHash = string.Join("", 
                            byteHash.Select(x => x.ToString("X2")));
                    }

                    await db.Uploads.AddAsync(new Upload
                    {
                        Guid = name,
                        Password = passwordHash,
                        Removed = false,
                        AuthorGuid = userGuid,
                        ContentType = upload.ContentType,
                        FileName = upload.Filename,
                        CreatedAt = DateTime.Now,
                        LastSeen = DateTime.Now,
                        DownloadCount = 0
                    });

                    await using var fs = File.Create(
                        Path.Combine(_uploadConfiguration.UploadsPath, upload.Filename));
                    await file.CopyToAsync(fs);

                    succeeded.Add(upload);
                }
                else
                {
                    failed.Add(upload);
                }
                
                _logger.LogInformation(
                    $"Upload by {userGuid}: [name:{upload.Filename};size:{upload.Size}b;" +
                    $"haspassword:{upload.HasPassword};success_code:{upload.StatusCode}]");
            }

            await db.SaveChangesAsync();
            
            model.SucceededUploads = succeeded.ToArray();
            model.FailedUploads = failed.ToArray();

            return model;
        }

        /// <summary>
        ///     Deletes an upload.
        /// </summary>
        /// <param name="userGuid">Id of the user requesting deletion.</param>
        /// <param name="filename">Name of the file to delete.</param>
        public async Task<ResultCode> DeleteUploadAsync(
            Guid userGuid, 
            string filename)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var upload = await GetUploadByNameAsync(filename);
            if (upload is null)
            {
                return ResultCode.NotFound;
            }

            var userDb = await db.Users.FindAsync(userGuid);
            if (userGuid != upload.AuthorGuid && userDb.Type != AccountType.Admin)
            {
                return ResultCode.Unauthorized;
            }

            upload.Removed = true;
            var path = Path.Combine(_uploadConfiguration.UploadsPath, upload.FileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            db.Uploads.Update(upload);
            await db.SaveChangesAsync();

            _logger.LogInformation(
                $"Upload deleted by {userGuid}: " +
                $"[authorguid:{upload.AuthorGuid};guid:{upload.Guid}]");

            return ResultCode.Ok;
        }

        /// <summary>
        ///     Gets the details of an upload by its name or guid.
        /// </summary>
        /// <param name="filename">Name of the file to see the details.</param>
        public async Task<UploadDetailsModel> GetUploadDetailsAsync(
            string filename)
        {
            var upload = await GetUploadByNameAsync(filename);
            if (upload is null)
            {
                return null;
            }
            
            return new UploadDetailsModel
            {
                AuthorGuid = upload.AuthorGuid,
                UploadGuid = upload.Guid,
                ContentType = upload.ContentType,
                CreatedAt = upload.CreatedAt,
                LastSeen = upload.LastSeen,
                DownloadCount = upload.DownloadCount,
                FileName = upload.FileName,
                HasPassword = !string.IsNullOrWhiteSpace(upload.Password)
            };
        }

        /// <summary>
        ///     Gets the details of every upload created by a userId.
        /// </summary>
        /// <param name="userGuid">Name of the file to see the details.</param>
        /// <param name="limit">Amount of uploads to lookup.</param>
        /// <param name="afterGuid">Guid that defines the start of the query.</param>
        public async Task<IReadOnlyList<UploadDetailsModel>> GetUploadsDetailsAsync(
            Guid userGuid, 
            int limit, 
            Guid afterGuid)
        {
            if (limit > 100)
            {
                return null;
            }
            
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            if (await db.Users.FindAsync(userGuid) is null)
            {
                return null;
            }

            var uploads = db.Uploads.Where(x => x.AuthorGuid == userGuid);
            uploads = uploads.OrderBy(x => x.CreatedAt);
            
            var firstUpload = await uploads.FirstOrDefaultAsync(x => x.Guid == afterGuid);
            if (firstUpload is null)
            {
                return null;
            }
            
            var createdAt = firstUpload.CreatedAt;
            uploads = uploads.Where(x => x.CreatedAt > createdAt);
            uploads = uploads.Take(limit);

            foreach (var upload in uploads)
            {
                upload.DownloadCount++;
                upload.LastSeen = DateTime.Now;
            }
            
            db.Uploads.UpdateRange(uploads);
            await db.SaveChangesAsync();

            return await uploads.Select(upload => new UploadDetailsModel
            {
                AuthorGuid = upload.AuthorGuid,
                UploadGuid = upload.Guid,
                ContentType = upload.ContentType,
                CreatedAt = upload.CreatedAt,
                LastSeen = upload.LastSeen,
                DownloadCount = upload.DownloadCount,
                FileName = upload.FileName,
                HasPassword = !string.IsNullOrWhiteSpace(upload.Password)
            }).ToListAsync();
        }

        /// <summary>
        ///     Gets the content of a specific upload by its name or guid.
        /// </summary>
        /// <param name="filename">Name of the file to get the content.</param>
        /// <param name="password">Password of the file, if any is set.</param>
        public async Task<(byte[] Content, string Type)> GetUploadAsync(
            string filename, 
            string password)
        {
            var upload = await GetUploadByNameAsync(filename);
            if (upload is null)
            {
                return (Array.Empty<byte>(), string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(upload.Password))
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return (Array.Empty<byte>(), upload.ContentType);
                }
                
                var byteHash = _sha512Managed.ComputeHash(Encoding.UTF8.GetBytes(password));
                var passwordHash = string.Join("", byteHash.Select(x => x.ToString("X2")));

                if (upload.Password != passwordHash)
                {
                    return (Array.Empty<byte>(), upload.ContentType);
                }
            }

            var path = Path.Combine(_uploadConfiguration.UploadsPath, upload.FileName);
            return (File.ReadAllBytes(path), upload.ContentType);
        }

        /// <summary>
        ///     Check if the given upload exists and is not removed.
        /// </summary>
        /// <param name="filename">Name or guid of the upload.</param>
        private async Task<Upload> GetUploadByNameAsync(
            string filename)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var upload = Guid.TryParse(filename, out var fileGuid) 
                ? await db.Uploads.FindAsync(fileGuid) 
                : db.Uploads.FirstOrDefault(x => x.FileName == filename);

            if (upload is null)
            {
                return null;
            }
            
            var path = Path.Combine(_uploadConfiguration.UploadsPath, upload.FileName);
            if (File.Exists(path))
            {
                if (!upload.Removed)
                {
                    upload.DownloadCount++;
                    upload.LastSeen = DateTime.Now;
                    db.Uploads.Update(upload);
                    await db.SaveChangesAsync();
                    
                    return upload;
                }
                
                File.Delete(path);
                
                return null;
            }
            
            upload.Removed = true;
            db.Uploads.Update(upload);
            await db.SaveChangesAsync();
            
            return null;
        }

        /// <summary>
        ///     Deletes all the given uploads by their GUIDs.
        /// </summary>
        /// <param name="userGuid">Id of the user.</param>
        /// <param name="uploadIds">Ids of the uploads to delete. Limited to 100.</param>
        public async Task<UploadBulkDeleteModel> DeleteUploadsAsync(
            Guid userGuid, 
            IEnumerable<string> uploadIds)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();
            
            var model = new UploadBulkDeleteModel();
            var succeeded = new List<string>();
            var failed = new List<string>();
            
            var uploads = await db.Uploads.Where(x => x.AuthorGuid == userGuid).ToListAsync();
            foreach (var uploadId in uploadIds)
            {
                var upload = uploads.FirstOrDefault(x => x.Guid.ToString() == uploadId);
                if (upload is null || upload.Removed)
                {
                    failed.Add(uploadId);
                }
                else
                {
                    var path = Path.Combine(_uploadConfiguration.UploadsPath, upload.FileName);
                    
                    succeeded.Add(uploadId);
                    
                    upload.Removed = true;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    db.Uploads.Update(upload);
                }
            }
            
            await db.SaveChangesAsync();

            model.FailedDeletes = failed;
            model.SucceededDeletes = succeeded;
            return model;
        }
    }
}