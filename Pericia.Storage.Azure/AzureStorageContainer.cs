using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.Azure
{
    public class AzureStorageContainer : BaseFileStorageContainer<AzureStorageOptions>
    {
        private Lazy<BlobContainerClient> _cloudBlobContainer;

        public AzureStorageContainer()
            : this(default!, default!)
        {
        }

        public AzureStorageContainer(AzureStorageOptions options, string container)
        {
            this.Options = options;
            this.Container = container;
            _cloudBlobContainer = new Lazy<BlobContainerClient>(() =>
            {
                return new BlobContainerClient(Options.ConnectionString, this.Container);
            });
        }


        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var blob = _cloudBlobContainer.Value.GetBlobClient(fileId);
            await blob.UploadAsync(fileData, cancellationToken);
            return fileId;
        }

        public override async Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var blob = _cloudBlobContainer.Value.GetBlobClient(fileId);
            try
            {
                return await blob.OpenReadAsync(cancellationToken: cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == 404)
                {
                    return null;
                }

                throw;
            }

        }

        public override Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var blob = _cloudBlobContainer.Value.GetBlobClient(fileId);
            return blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }

        public override Task CreateContainer(CancellationToken cancellationToken)
        {
            return _cloudBlobContainer.Value.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        }
    }
}
