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
                var regionField = typeof(RegionEndpoint).GetField(Options.RegionEndpoint);
                if (regionField == null)
                {
                    throw new Exception("Incorrect AWS region");
                }
                var region = regionField.GetValue(null) as RegionEndpoint;

                return new AmazonS3Client(credentials, region);
            });
        }

        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            using var memStream = new MemoryStream();
            fileData.CopyTo(memStream);
            using var fileTransferUtility = new TransferUtility(_s3Client.Value);
            await fileTransferUtility.UploadAsync(memStream, Container, fileId, cancellationToken).ConfigureAwait(false);

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

        public override async Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = Container,
                    Key = fileId
                };

                _ = await _s3Client.Value.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.ErrorCode == "NoSuchKey")
                {
                    // Le fichier n'existe pas
                    return false;
                }

                throw;
            }
        }
    }
}
