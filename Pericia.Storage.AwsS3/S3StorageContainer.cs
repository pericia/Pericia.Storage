using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.AwsS3
{
    public class S3StorageContainer : BaseFileStorageContainer<S3StorageOptions>
    {
        private Lazy<IAmazonS3> _s3Client;

        public S3StorageContainer()
            : this(default!, default!)
        {
        }

        public S3StorageContainer(S3StorageOptions options, string container)
        {
            this.Options = options;
            this.Container = container;
            _s3Client = new Lazy<IAmazonS3>(() =>
            {
                var credentials = new BasicAWSCredentials(Options.AccessKey, Options.SecretKey);

                var config = new AmazonS3Config();

                if (!string.IsNullOrEmpty(Options.RegionEndpoint))
                {
                    config.RegionEndpoint = RegionEndpoint.GetBySystemName(Options.RegionEndpoint);
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

        public override async Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
        {
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = Container
            };

            var response = await _s3Client.Value.ListObjectsV2Async(request, cancellationToken);
            return response.S3Objects.Select(o => o.Key);
        }

        public override async Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(subfolder))
            {
                return await ListFiles(cancellationToken);
            }

            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = Container,
                Prefix = subfolder + "/"
            };

            var response = await _s3Client.Value.ListObjectsV2Async(request, cancellationToken);
            return response.S3Objects.Select(o => o.Key);
        }
    }
}
