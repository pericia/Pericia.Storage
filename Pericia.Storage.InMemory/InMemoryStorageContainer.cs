using System;
using System.Collections.Generic;
using System.IO;
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
            if (files.ContainsKey(fileId))
            {
                files[fileId].Dispose();
                files.Remove(fileId);
            }

            return Task.CompletedTask;
        }

        public override Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            if (files.ContainsKey(fileId))
            {
                return Task.FromResult<Stream?>(files[fileId]);
            }

            return Task.FromResult<Stream?>(null);
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

            var stream = new MemoryStream();
            await fileData.CopyToAsync(stream, 81920, cancellationToken).ConfigureAwait(false);

            files[fileId] = stream;

            return fileId;
        }
    }
}
