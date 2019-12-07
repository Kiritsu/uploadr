using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UploadR.Database;
using UploadR.Database.Models;

namespace UploadR.Services
{
    public sealed class FileService
    {
        private readonly UploadRContext _db;

        public FileService(UploadRContext db)
        {
            _db = db;
        }

        public async Task<FileServiceResult<Upload>> TryGetUploadByNameAsync(string name)
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

        public async Task<FileServiceResult<Upload>> IsValidUploadByNameAsync(string name)
        {
            var file = await _db.Uploads.FirstOrDefaultAsync(x => x.FileName == name);
            if (file is null)
            {
                return FileServiceResult<Upload>.Fail(1);
            }

            if (file.Removed)
            {
                return FileServiceResult<Upload>.Fail(2);
            }

            var path = $"./uploads/{file.FileName}";
            if (!File.Exists(path))
            {
                file.Removed = true;
                _db.Uploads.Update(file);
                await _db.SaveChangesAsync();

                return FileServiceResult<Upload>.Fail(3);
            }

            return FileServiceResult<Upload>.Success(file);
        }
    }
}
