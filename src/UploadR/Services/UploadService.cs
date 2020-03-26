using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        
        public UploadService(IServiceProvider services, SHA512Managed sha512Managed,
            ILogger<AccountService> logger, UploadConfigurationProvider uploadConfigurationProvider)
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
        public bool TryCreateUpload(IFormFile file, string password, out UploadOutModel upload)
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
        /// <param name="userId">Id of the user uploading the files.</param>
        /// <param name="files">Files to upload.</param>
        /// <param name="password">Password of the files. Can be null.</param>
        public async Task<UploadListOutModel> UploadAsync(string userId, 
            IEnumerable<IFormFile> files, string password)
        {
            var model = new UploadListOutModel();
            var failed = new List<UploadOutModel>();
            var succeeded = new List<UploadOutModel>();
            
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var userGuid = Guid.Parse(userId);
            
            foreach (var file in files)
            {
                if (TryCreateUpload(file, password, out var upload))
                {
                    var name = Guid.NewGuid();
                    upload.Filename = name.ToString().Replace("-", "") + Path.GetExtension(file.FileName);
                    
                    var byteHash = _sha512Managed.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var passwordHash = string.Join("", byteHash.Select(x => x.ToString("X2")));
                    
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

                    succeeded.Add(upload);
                }
                else
                {
                    failed.Add(upload);
                }
                
                _logger.LogInformation($"Upload by {userId}: [name:{upload.Filename};size:{upload.Size}b;" +
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
        /// <param name="userId">Id of the user requesting deletion.</param>
        /// <param name="filename">Name of the file to delete.</param>
        public async Task<ResultCode> DeleteUploadAsync(string userId, string filename)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var upload = await GetUploadByNameAsync(filename);
            if (upload is null)
            {
                return ResultCode.NotFound;
            }

            var userGuid = Guid.Parse(userId);
            var userDb = await db.Users.FindAsync(userGuid);
            if (userGuid != upload.AuthorGuid && userDb.Type != AccountType.Admin)
            {
                return ResultCode.Unauthorized;
            }

            upload.Removed = true;
            var path = $"./uploads/{upload.FileName}";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            db.Uploads.Update(upload);
            await db.SaveChangesAsync();

            _logger.LogInformation($"Upload deleted by {userId}: " +
                                   $"[authorguid:{upload.AuthorGuid};guid:{upload.Guid}]");

            return ResultCode.Ok;
        }

        /// <summary>
        ///     Gets the details of an upload by its name or guid.
        /// </summary>
        /// <param name="filename">Name of the file to see the details.</param>
        public async Task<UploadDetailsModel> GetUploadDetailsAsync(string filename)
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
                FileName = upload.FileName
            };
        }

        /// <summary>
        ///     Gets the content of a specific upload by its name or guid.
        /// </summary>
        /// <param name="filename">Name of the file to get the content.</param>
        public async Task<(byte[] Content, string Type)> GetUploadAsync(string filename)
        {
            var upload = await GetUploadByNameAsync(filename);
            if (upload is null)
            {
                return (Array.Empty<byte>(), string.Empty);
            }
            
            var path = $"./uploads/{upload.FileName}";
            return (File.ReadAllBytes(path), upload.ContentType);
        }

        /// <summary>
        ///     Check if the given upload exists and is not removed.
        /// </summary>
        /// <param name="filename">Name or guid of the upload.</param>
        private async Task<Upload> GetUploadByNameAsync(string filename)
        {
            using var scope = _services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<UploadRContext>();

            var upload = Guid.TryParse(filename, out _) 
                ? await db.Uploads.FindAsync(filename) 
                : db.Uploads.FirstOrDefault(x => x.FileName == filename);

            if (upload is null)
            {
                return null;
            }
            
            var path = $"./uploads/{upload.FileName}";
            if (File.Exists(path))
            {
                if (!upload.Removed)
                {
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
    }
}