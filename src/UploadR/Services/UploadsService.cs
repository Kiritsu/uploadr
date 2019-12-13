using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Models;
using UploadR.Enum;
using UploadR.Interfaces;

namespace UploadR.Services
{
    public sealed class UploadsService
    {
        private readonly IServiceProvider _sp;
        private readonly FilesConfiguration _fc;
        private readonly ILogger<UploadsService> _logger;

        public UploadsService(IServiceProvider sp, IFilesConfigurationProvider fc, ILogger<UploadsService> logger)
        {
            _sp = sp;
            _fc = fc.GetConfiguration();
            _logger = logger;
        }

        public bool IsCorrectPassword(Upload upload, string inputPassword)
        {
            return upload.Password == null || upload.Password == inputPassword;
        }

        public async Task<ServiceResult<Upload>> UploadFileAsync(Guid authorGuid, IFormFile file, string password)
        {
            await using var _db = _sp.GetRequiredService<UploadRContext>();

            var extension = Path.GetExtension(file.FileName);
            var filename = $"{Guid.NewGuid().ToString().Replace("-", "")}{extension}";

            var upload = new Upload
            {
                AuthorGuid = authorGuid,
                CreatedAt = DateTime.Now,
                ViewCount = 0,
                Removed = false,
                Guid = Guid.NewGuid(),
                FileName = filename,
                ContentType = file.ContentType,
                Password = password,
                LastSeen = DateTime.Now
            };

            await _db.Uploads.AddAsync(upload);

            await _db.SaveChangesAsync();

            if (!Directory.Exists("./uploads"))
            {
                Directory.CreateDirectory("./uploads");
            }

            using var fs = File.Create($"./uploads/{filename}");
            await file.CopyToAsync(fs);

            return ServiceResult<Upload>.Success(upload);
        }

        public ServiceResult<bool> IsValidFile(IFormFile file)
        {
            if (file is null)
            {
                return ServiceResult<bool>.Fail(ResultErrorType.Null);
            }

            if (file.Length > _fc.SizeMax)
            {
                return ServiceResult<bool>.Fail(ResultErrorType.TooBig);
            }

            if (file.Length < _fc.SizeMin)
            {
                return ServiceResult<bool>.Fail(ResultErrorType.TooSmall);
            }

            var extension = Path.GetExtension(file.FileName);
            if (!_fc.FileExtensions.Any(x => x == extension.Replace(".", "")))
            {
                return ServiceResult<bool>.Fail(ResultErrorType.UnsupportedFileExtension);
            }

            return ServiceResult<bool>.Success(true);
        }

        public IReadOnlyList<IFormFile> FilterBadFiles(IFormFileCollection files)
        {
            var goodFiles = new List<IFormFile>();

            foreach (var file in files)
            {
                var result = IsValidFile(file);
                if (!result.IsSuccess)
                {
                    continue;
                }

                goodFiles.Add(file);
            }

            return goodFiles.AsReadOnly();
        }

        public async Task<IReadOnlyList<string>> CleanupAsync(TimeSpan timeSpan)
        {
            await using var db = _sp.GetRequiredService<UploadRContext>();

            var dateTime = DateTime.Now - timeSpan;

            var files = db.Uploads;
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                if (file.LastSeen > dateTime)
                {
                    continue;
                }

                fileNames.Add(file.FileName);
                file.Removed = true;
                db.Uploads.Update(file);

                try
                {
                    File.Delete($"./uploads/{file.FileName}");
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Couldn't remove file {file.FileName}: {e.Message}");
                }
            }

            await db.SaveChangesAsync();
            return fileNames.AsReadOnly();
        }

        public async Task<ServiceResult<bool>> RemoveAsync(string name, Guid authorGuid)
        {
            await using var db = _sp.GetRequiredService<UploadRContext>();

            var upload = await IsValidUploadByNameAsync(name);
            if (!upload.IsSuccess)
            {
                return ServiceResult<bool>.Fail(upload.Code);
            }

            if (upload.Value.AuthorGuid != authorGuid)
            {
                return ServiceResult<bool>.Fail(ResultErrorType.Unauthorized);
            }

            var path = $"./uploads/{upload.Value.FileName}";

            File.Delete(path);
            upload.Value.Removed = true;
            db.Uploads.Update(upload.Value);

            await db.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<Upload>> TryGetUploadByNameAsync(string name)
        {
            await using var db = _sp.GetRequiredService<UploadRContext>();

            var upload = await IsValidUploadByNameAsync(name);
            if (!upload.IsSuccess)
            {
                return upload;
            }

            upload.Value.LastSeen = DateTime.Now;
            upload.Value.ViewCount++;

            db.Uploads.Update(upload.Value);
            await db.SaveChangesAsync();

            return upload;
        }

        public async Task<ServiceResult<Upload>> IsValidUploadByNameAsync(string name)
        {
            await using var db = _sp.GetRequiredService<UploadRContext>();

            var file = await db.Uploads.FirstOrDefaultAsync(x => x.FileName == name);
            if (file is null)
            {
                return ServiceResult<Upload>.Fail(ResultErrorType.NotFound);
            }

            if (file.Removed)
            {
                return ServiceResult<Upload>.Fail(ResultErrorType.Removed);
            }

            var path = $"./uploads/{file.FileName}";
            if (!File.Exists(path))
            {
                file.Removed = true;
                db.Uploads.Update(file);
                await db.SaveChangesAsync();

                return ServiceResult<Upload>.Fail(ResultErrorType.NotFoundRemoved);
            }

            return ServiceResult<Upload>.Success(file);
        }
    }
}
