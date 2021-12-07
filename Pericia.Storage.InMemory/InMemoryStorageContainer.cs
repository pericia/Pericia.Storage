using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.InMemory
{

    public class InMemoryStorageContainer : BaseFileStorageContainer<FileStorageOptions>
    {
        private Dictionary<string, MemoryStream> files = new Dictionary<string, MemoryStream>();

        public override Task CreateContainer(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            if (files.ContainsKey(fileId))
            {
                files[fileId].Dispose();
                files.Remove(fileId);
            }

            return Task.CompletedTask;
        }

        public override Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            if (files.ContainsKey(fileId))
            {
                return Task.FromResult<Stream?>(files[fileId]);
            }

            return Task.FromResult<Stream?>(null);
        }

        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));
         
            var stream = new MemoryStream();
            await fileData.CopyToAsync(stream, 81920, cancellationToken).ConfigureAwait(false);
            stream.Position = 0;

            files[fileId] = stream;

            return fileId;
        }

        public override Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            return Task.FromResult(files.ContainsKey(fileId));
        }

        public override Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
        {
            var keys = files.Select(kvp => kvp.Key);
            return Task.FromResult(keys);
        }

        public override Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            var keys = files.Select(kvp => kvp.Key).Where(k => k.StartsWith(subfolder));
            return Task.FromResult(keys);
        }
    }
}
