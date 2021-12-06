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
        private Lazy<IAmazonS3> _s3Client;

        public AwsStorageContainer()
            : this(default!, default!)
        {
        }

        public AwsStorageContainer(AwsStorageOptions options, string container)
        {
            this.Options = options;
            this.Container = container;
            _s3Client = new Lazy<IAmazonS3>(() =>
            {
                var credentials = new BasicAWSCredentials(Options.AccessKey, Options.SecretKey);

                var config = new AmazonS3Config();

                if (!string.IsNullOrEmpty(Options.RegionEndpoint))
                {
                    var regionField = typeof(RegionEndpoint).GetField(Options.RegionEndpoint);
                    var region = regionField.GetValue(null) as RegionEndpoint;
                    config.RegionEndpoint = region;
                }

                if (!string.IsNullOrEmpty(Options.ServiceUrl))
                {
                    config.ServiceURL = Options.ServiceUrl;
                }

                return new AmazonS3Client(credentials, config);
            });
        }

        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            using var fileTransferUtility = new TransferUtility(_s3Client.Value);
            await fileTransferUtility.UploadAsync(fileData, Container, fileId, cancellationToken).ConfigureAwait(false);

            return fileId;
        }

        public override async Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = Container,
                    Key = fileId
                };

                var response = await _s3Client.Value.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
                var memStream = new MemoryStream();
                return response.ResponseStream;
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
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            return _s3Client.Value.DeleteObjectAsync(Container, fileId, cancellationToken);
        }

        public override async Task CreateContainer(CancellationToken cancellationToken)
        {
            if (!await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client.Value, Container).ConfigureAwait(false))
            {
                var putBucketRequest = new PutBucketRequest
                {
                    BucketName = Container,
                    UseClientRegion = true
                };

                PutBucketResponse putBucketResponse = await _s3Client.Value.PutBucketAsync(putBucketRequest, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
