using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.FileSystem
{
    public class FileSystemStorageContainer : BaseFileStorageContainer<FileSystemStorageOptions>
    {
        private string _folder
        {
            get
            {
                return Path.Combine(Options.Path, Container);
            }
        }


        public FileSystemStorageContainer()
        {
        }

        public FileSystemStorageContainer(FileSystemStorageOptions options, string container)
        {
            Options = options;
            Container = container;
        }



        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
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
                await fileData.CopyToAsync(stream, 81920, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }

            return fileId;
        }

        public override Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            var filePath = Path.Combine(_folder, fileId);

            if (File.Exists(filePath))
            {
                return Task.FromResult((Stream?)File.OpenRead(filePath));
            }

            return Task.FromResult<Stream?>(null);
        }

        public override Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            var filePath = Path.Combine(_folder, fileId);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        public override Task CreateContainer(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            return Task.CompletedTask;
        }
    }
}
