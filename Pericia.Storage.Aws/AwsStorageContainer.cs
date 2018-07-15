using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Pericia.Storage.Aws
{
    public class AwsStorageContainer : BaseFileStorageContainer<AwsStorageOptions>
    {
        //private IAmazonS3 _s3Client;
        private Lazy<IAmazonS3> _s3Client;

        public AwsStorageContainer()
            : this(null, null)
        {
        }

        public AwsStorageContainer(AwsStorageOptions options, string container)
        {
            this.Options = Options;
            this.Container = container;
            _s3Client = new Lazy<IAmazonS3>(() =>
            {
                var credentials = new BasicAWSCredentials(Options.AccessKey, Options.SecretKey);
                RegionEndpoint region;
                try
                {
                    region = (RegionEndpoint)typeof(RegionEndpoint).GetField("EUWest3").GetValue(null);
                }
                catch (Exception)
                {
                    throw new Exception("Incorrect AWS region");
                }

                return new AmazonS3Client(credentials, region);
            });
        }

        public override Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString());
        }

        public override async Task<string> SaveFile(Stream fileData, string fileId)
        {
            using (var memStream = new MemoryStream())
            {
                fileData.CopyTo(memStream);
                var fileTransferUtility = new TransferUtility(_s3Client.Value);
                await fileTransferUtility.UploadAsync(memStream, Container, fileId);
            }

            return fileId;
        }

        public override async Task<Stream> GetFile(string fileId)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = Container,
                    Key = fileId
                };

                var response = await _s3Client.Value.GetObjectAsync(request);
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

        public override Task DeleteFile(string fileId)
        {
            return _s3Client.Value.DeleteObjectAsync(Container, fileId);
        }

    }
}
