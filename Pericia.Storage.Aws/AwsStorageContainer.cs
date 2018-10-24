using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            using (var memStream = new MemoryStream())
            {
                fileData.CopyTo(memStream);
                var fileTransferUtility = new TransferUtility(_s3Client.Value);
                await fileTransferUtility.UploadAsync(memStream, Container, fileId, cancellationToken);
            }

            return fileId;
        }

        public override async Task<Stream> GetFile(string fileId, CancellationToken cancellationToken)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = Container,
                    Key = fileId
                };

                var response = await _s3Client.Value.GetObjectAsync(request, cancellationToken);
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

        public override Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            return _s3Client.Value.DeleteObjectAsync(Container, fileId, cancellationToken);
        }

        public override async Task CreateContainer(CancellationToken cancellationToken)
        {
            if (!(await AmazonS3Util.DoesS3BucketExistAsync(_s3Client.Value, Container)))
            {
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = Container,
                    UseClientRegion = true
                };

                PutBucketResponse putBucketResponse = await _s3Client.Value.PutBucketAsync(putBucketRequest, cancellationToken);
            }
        }
    }
}
