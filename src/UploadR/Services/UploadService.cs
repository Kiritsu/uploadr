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
                ContentType = file.ContentType
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
            IReadOnlyList<IFormFile> files, string password)
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
                    upload.Filename = name.ToString().Replace("-", "") + Path.GetExtension(file.FileName);
                    
                    var byteHash = _sha512Managed.ComputeHash(Encoding.UTF8.GetBytes(password));
                    var passwordHash = string.Join("", byteHash.Select(x => x.ToString("X2")));
                    
                    await db.Uploads.AddAsync(new Upload
                    {
                        Guid = name,
                        Password = passwordHash,
                        Removed = false,
                        AuthorGuid = Guid.Parse(userId),
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
    }
}