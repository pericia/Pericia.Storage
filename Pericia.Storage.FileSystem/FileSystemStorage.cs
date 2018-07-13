using System;
using System.IO;
using System.Threading.Tasks;

namespace Pericia.Storage.FileSystem
{
    public class FileSystemStorage : IFileStorage
    {
        private string _folder;

        public FileSystemStorage()
        {
        }

        public FileSystemStorage(FileSystemStorageOptions options)
        {
            _folder = Path.Combine(options.Path, options.Container);
        }

        public void Init(FileStorageOptions options)
        {
            var o = (FileSystemStorageOptions)options;
            _folder = Path.Combine(o.Path, o.Container);
        }


        public Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString());
        }

        public async Task<string> SaveFile(Stream fileData, string fileId)
        {
            if (fileData == null)
            {
                throw new ArgumentException("File data is mandatory", nameof(fileData));
            }
            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new ArgumentException("File id is mandatory", nameof(fileId));
            }

            var filePath = Path.Combine(_folder, fileId);
            var folder = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var stream = File.Create(filePath))
            {
                await fileData.CopyToAsync(stream);
            }

            return fileId;
        }

        public Task<Stream> GetFile(string fileId)
        {
            var filePath = Path.Combine(_folder, fileId);

            if (File.Exists(filePath))
            {
                return Task.FromResult((Stream)File.OpenRead(filePath));
            }

            return Task.FromResult<Stream>(null);
        }

        public Task DeleteFile(string fileId)
        {
            var filePath = Path.Combine(_folder, fileId);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
