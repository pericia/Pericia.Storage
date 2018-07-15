using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Pericia.Storage.Azure
{
    public class AzureStorageContainer : BaseFileStorageContainer<AzureStorageOptions>
    {
        private Lazy<CloudBlobContainer> _cloudBlobContainer;

        public AzureStorageContainer()
            : this(null, null)
        {
        }

        public AzureStorageContainer(AzureStorageOptions options, string container)
        {
            this.Options = Options;
            this.Container = container;
            _cloudBlobContainer = new Lazy<CloudBlobContainer>(() =>
            {
                if (CloudStorageAccount.TryParse(this.Options.ConnectionString, out var storageAccount))
                {
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                    return cloudBlobClient.GetContainerReference(this.Container);
                }
                return null;
            });
        }

        public override Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString());
        }

        public override async Task<string> SaveFile(Stream fileData, string fileId)
        {
            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.Value.GetBlockBlobReference(fileId);
            await cloudBlockBlob.UploadFromStreamAsync(fileData);
            return fileId;
        }

        public override Task<Stream> GetFile(string fileId)
        {
            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.Value.GetBlockBlobReference(fileId);
            return cloudBlockBlob.OpenReadAsync();
        }

        public override Task DeleteFile(string fileId)
        {
            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.Value.GetBlockBlobReference(fileId);
            return cloudBlockBlob.DeleteIfExistsAsync();
        }

    }
}
