using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.Azure
{
    public class AzureStorageContainer : BaseFileStorageContainer<AzureStorageOptions>
    {
        private Lazy<CloudBlobContainer> _cloudBlobContainer;

        public AzureStorageContainer()
            : this(default!, default!)
        {
        }

        public AzureStorageContainer(AzureStorageOptions options, string container)
        {
            this.Options = options;
            this.Container = container;
            _cloudBlobContainer = new Lazy<CloudBlobContainer>(() =>
            {
                var storageAccount = CloudStorageAccount.Parse(Options.ConnectionString);
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                return cloudBlobClient.GetContainerReference(this.Container);
            });
        }


        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.Value.GetBlockBlobReference(fileId);
            await cloudBlockBlob.UploadFromStreamAsync(fileData, default(AccessCondition), default(BlobRequestOptions), default(OperationContext), cancellationToken).ConfigureAwait(false);
            return fileId;
        }

        public override Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.Value.GetBlockBlobReference(fileId);
            return cloudBlockBlob.OpenReadAsync(default(AccessCondition), default(BlobRequestOptions), default(OperationContext), cancellationToken);
        }

        public override Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.Value.GetBlockBlobReference(fileId);
            return cloudBlockBlob.DeleteIfExistsAsync(default(DeleteSnapshotsOption), default(AccessCondition), default(BlobRequestOptions), default(OperationContext), cancellationToken);
        }

        public override Task CreateContainer(CancellationToken cancellationToken)
        {
            return _cloudBlobContainer.Value.CreateIfNotExistsAsync(default(BlobContainerPublicAccessType), default(BlobRequestOptions), default(OperationContext), cancellationToken);
        }

        public override Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.Value.GetBlockBlobReference(fileId);
            return cloudBlockBlob.ExistsAsync(default(BlobRequestOptions), default(OperationContext), cancellationToken);
        }
    }
}
