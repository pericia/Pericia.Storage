using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

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
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var filePath = Path.Combine(_folder, fileId);

            if (File.Exists(filePath))
            {
                return Task.FromResult((Stream?)File.OpenRead(filePath));
            }

            return Task.FromResult<Stream?>(null);
        }

        public override Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

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

        public override Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_folder))
            {
                return Task.FromResult(false);
            }

            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var filePath = Path.Combine(_folder, fileId);

            return Task.FromResult(File.Exists(filePath));
        }

        public override Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_folder))
            {
                return Task.FromResult(Enumerable.Empty<string>());
            }

            var files = Directory.GetFiles(_folder).Select(Path.GetFileName);
            return Task.FromResult<IEnumerable<string>>(files);
        }

        public override Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(subfolder))
            {
                return ListFiles(cancellationToken);
            }

            var files = Directory.GetFiles(Path.Combine(_folder, subfolder)).Select(f => subfolder + "/" + Path.GetFileName(f));
            return Task.FromResult<IEnumerable<string>>(files);
        }
    }
}
