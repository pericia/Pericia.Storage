using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.AzureBlobs
{
    public class AzureBlobsStorageContainer : BaseFileStorageContainer<AzureBlobsStorageOptions>
    {
        private Lazy<BlobContainerClient> _cloudBlobContainer;

        public AzureBlobsStorageContainer()
            : this(default!, default!)
        {
        }

        private BlobServiceClient GetBlobServiceClient()
        {
            if (!String.IsNullOrEmpty(Options.ConnectionString))
            {
                return new BlobServiceClient(Options.ConnectionString);
            }
            else
            {
                if (string.IsNullOrEmpty(Options.AccountName))
                {
                    throw new InvalidOperationException("Either ConnectionString or AccountName is required");
                }

                if (!String.IsNullOrEmpty(Options.ManagedIdentityClientId))
                {
                    var options = new DefaultAzureCredentialOptions();
                    options.ManagedIdentityClientId = Options.ManagedIdentityClientId;
                    return new BlobServiceClient(
                        new Uri($"https://{Options.AccountName}.blob.core.windows.net"),
                        new DefaultAzureCredential(options));
                }
                else
                {
                    return new BlobServiceClient(
                        new Uri($"https://{Options.AccountName}.blob.core.windows.net"),
                        new DefaultAzureCredential());
                }
            }
        }

        public AzureBlobsStorageContainer(AzureBlobsStorageOptions options, string container)
        {
            this.Options = options;
            this.Container = container;
            _cloudBlobContainer = new Lazy<BlobContainerClient>(() =>
            {
                var client = GetBlobServiceClient();
                return client.GetBlobContainerClient(this.Container);
            });
        }


        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var blob = _cloudBlobContainer.Value.GetBlobClient(fileId);
            await blob.UploadAsync(fileData, overwrite: true, cancellationToken);
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

        public override async Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            var blob = _cloudBlobContainer.Value.GetBlobClient(fileId);
            var exists = await blob.ExistsAsync(cancellationToken);
            return exists.Value;
        }

        public override async Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
        {
            var result = new List<string>();

            var asyncBlobs = _cloudBlobContainer.Value.GetBlobsAsync(cancellationToken: cancellationToken);
            await foreach (var item in asyncBlobs)
            {
                result.Add(item.Name);
            }

            return result;
        }

        public override async Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            var result = new List<string>();

            var asyncBlobs = _cloudBlobContainer.Value.GetBlobsAsync(prefix: subfolder + "/", cancellationToken: cancellationToken);
            await foreach (var item in asyncBlobs)
            {
                result.Add(item.Name);
            }

            return result;
        }
    }
}
