using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Pericia.Storage.Aws
{
    public class AwsStorage : IFileStorageContainer
    {
        private IAmazonS3 _s3Client;
        private string _bucketName;

        public AwsStorage()
        {
        }

        public AwsStorage(AwsStorageOptions options, string container)
        {
            Init(options, container);
        }

        public void Init(FileStorageOptions options, string container)
        {
            var o = (AwsStorageOptions)options;
            var credentials = new BasicAWSCredentials(o.AccessKey, o.SecretKey);
            _s3Client = new AmazonS3Client(credentials);

            _bucketName = container;
        }

        public Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString());
        }

        public async Task<string> SaveFile(Stream fileData, string fileId)
        {
            using (var memStream = new MemoryStream())
            {
                fileData.CopyTo(memStream);
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(memStream, _bucketName, fileId);
            }

            return fileId;
        }

        public async Task<Stream> GetFile(string fileId)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileId
                };

                var response = await _s3Client.GetObjectAsync(request);
                var memStream = new MemoryStream();
                response.ResponseStream.CopyTo(memStream);
                return memStream;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == "NoSuchKey")
                {
                    // Le fichier n'existe pas
                    return null;
                }

                throw;
            }
        }

        public Task DeleteFile(string fileId)
        {
            return _s3Client.DeleteObjectAsync(_bucketName, fileId);
        }

    }
}
