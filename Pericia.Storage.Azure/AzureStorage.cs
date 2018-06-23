using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Pericia.FileStorage.Azure
{
    public class AzureStorage : IFileStorage
    {
        private readonly CloudBlobContainer _cloudBlobContainer;

        public AzureStorage(string connectionString, string container)
        {
            var storageConnectionString = connectionString;
            var containerName = container;

            if (CloudStorageAccount.TryParse(storageConnectionString, out var storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();
                _cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            }
        }

        public Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString());
        }

        public async Task<string> SaveFile(Stream fileData, string fileId)
        {
            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.GetBlockBlobReference(fileId);
            await cloudBlockBlob.UploadFromStreamAsync(fileData);
            return fileId;
        }

        public Task<Stream> GetFile(string fileId)
        {
            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.GetBlockBlobReference(fileId);
            return cloudBlockBlob.OpenReadAsync();
        }

        public Task DeleteFile(string fileId)
        {
            CloudBlockBlob cloudBlockBlob = _cloudBlobContainer.GetBlockBlobReference(fileId);
            return cloudBlockBlob.DeleteIfExistsAsync();
        }

    }
}
