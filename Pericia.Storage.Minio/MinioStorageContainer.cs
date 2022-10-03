using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Minio;
using Minio.Exceptions;

namespace Pericia.Storage.Minio
{
    public class MinioStorageContainer : BaseFileStorageContainer<MinioStorageOptions>
    {
        private Lazy<MinioClient> _minioClient;

        public MinioStorageContainer()
            : this(default!, default!)
        {
        }

        public MinioStorageContainer(MinioStorageOptions options, string container)
        {
            this.Options = options;
            this.Container = container;
            _minioClient = new Lazy<MinioClient>(() =>
            {
                var clientBuilder = new MinioClient()
                            .WithEndpoint(Options.Endpoint)
                            .WithCredentials(Options.AccessKey, Options.SecretKey);

                if (!string.IsNullOrEmpty(Options.Region))
                {
                    clientBuilder.WithRegion(Options.Region);
                }

                if (!Options.Insecure)
                {
                    clientBuilder.WithSSL();
                }

                return clientBuilder.Build();
            });
        }

        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var args = new PutObjectArgs()
                                .WithBucket(Container)
                                .WithObject(fileId)
                                .WithObjectSize(fileData.Length)
                                .WithStreamData(fileData);

            await _minioClient.Value.PutObjectAsync(args, cancellationToken);

            return fileId;
        }

        public override async Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var args = new GetObjectArgs()
                                .WithBucket(Container)
                                .WithObject(fileId)
                                .WithCallbackStream(cb => { });

            try
            {
                await _minioClient.Value.GetObjectAsync(args, cancellationToken);
                return true;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
        }

        public override async Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            Stream? resultStream = new MemoryStream();
            var args = new GetObjectArgs()
                                .WithBucket(Container)
                                .WithObject(fileId)
                                .WithCallbackStream(cb =>
                                {
                                    cb.CopyTo(resultStream);
                                    resultStream.Position = 0;
                                });

            try
            {
                var objectResult = await _minioClient.Value.GetObjectAsync(args, cancellationToken);
                return resultStream;
            }
            catch (ObjectNotFoundException)
            {
                return null;
            }
        }

        public override async Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            var args = new RemoveObjectArgs()
                            .WithBucket(Container)
                            .WithObject(fileId);

            await _minioClient.Value.RemoveObjectAsync(args, cancellationToken);
        }

        public override async Task CreateContainer(CancellationToken cancellationToken)
        {
            var args = new MakeBucketArgs().WithBucket(Container);

            await _minioClient.Value.MakeBucketAsync(args, cancellationToken);
        }

        public override Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
        {
            return ListFiles(subfolder: string.Empty, cancellationToken);
        }

        public override async Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            var args = new ListObjectsArgs().WithBucket(Container);

            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                args.WithPrefix(subfolder + "/");
            }

            var result = new List<string>();

            var response = _minioClient.Value.ListObjectsAsync(args, cancellationToken: cancellationToken);

            await response.ToAsyncEnumerable().ForEachAsync(item =>
            {
                result.Add(item.Key);
            });

            return result;

        }
    }
}
