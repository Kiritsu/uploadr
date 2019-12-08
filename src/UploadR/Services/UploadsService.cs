using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UploadR.Configurations;
using UploadR.Database;
using UploadR.Database.Models;
using UploadR.Interfaces;

namespace UploadR.Services
{
    public sealed class UploadsService
    {
        private readonly UploadRContext _db;
        private readonly FilesConfiguration _fc;

        public UploadsService(UploadRContext db, IFilesConfigurationProvider fc)
        {
            _db = db;
            _fc = fc.GetConfiguration();
        }

        public async Task<ServiceResult<Upload>> UploadFileAsync(Guid authorGuid, IFormFile file)
        {
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
                ContentType = file.ContentType
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
                return ServiceResult<bool>.Fail(11);
            }

            if (file.Length > _fc.SizeMax)
            {
                return ServiceResult<bool>.Fail(12);
            }

            if (file.Length < _fc.SizeMin)
            {
                return ServiceResult<bool>.Fail(13);
            }

            var extension = Path.GetExtension(file.FileName);
            if (!_fc.FileExtensions.Any(x => x == extension.Replace(".", "")))
            {
                return ServiceResult<bool>.Fail(14);
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
            var dateTime = DateTime.Now - timeSpan;

            var files = _db.Uploads;
            var fileNames = new List<string>();

            foreach (var file in files)
            {
                if (file.LastSeen > dateTime)
                {
                    continue;
                }

                fileNames.Add(file.FileName);
                file.Removed = true;
                _db.Uploads.Update(file);

                File.Delete($"./uploads/{file.FileName}");
            }

            await _db.SaveChangesAsync();
            return fileNames.AsReadOnly();
        }

        public async Task<ServiceResult<bool>> RemoveAsync(string name, Guid authorGuid)
        {
            var upload = await IsValidUploadByNameAsync(name);
            if (!upload.IsSuccess)
            {
                return ServiceResult<bool>.Fail(upload.Code);
            }

            if (upload.Value.AuthorGuid != authorGuid)
            {
                return ServiceResult<bool>.Fail(4);
            }

            var path = $"./uploads/{upload.Value.FileName}";

            File.Delete(path);
            upload.Value.Removed = true;
            _db.Uploads.Update(upload.Value);

            await _db.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<Upload>> TryGetUploadByNameAsync(string name)
        {
            var upload = await IsValidUploadByNameAsync(name);
            if (!upload.IsSuccess)
            {
                return upload;
            }

            upload.Value.LastSeen = DateTime.Now;
            upload.Value.ViewCount++;

            _db.Uploads.Update(upload.Value);
            await _db.SaveChangesAsync();

            return upload;
        }

        public async Task<ServiceResult<Upload>> IsValidUploadByNameAsync(string name)
        {
            var file = await _db.Uploads.FirstOrDefaultAsync(x => x.FileName == name);
            if (file is null)
            {
                return ServiceResult<Upload>.Fail(1);
            }

            if (file.Removed)
            {
                return ServiceResult<Upload>.Fail(2);
            }

            var path = $"./uploads/{file.FileName}";
            if (!File.Exists(path))
            {
                file.Removed = true;
                _db.Uploads.Update(file);
                await _db.SaveChangesAsync();

                return ServiceResult<Upload>.Fail(3);
            }

            return ServiceResult<Upload>.Success(file);
        }
    }
}
