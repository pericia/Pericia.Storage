using Microsoft.Extensions.Configuration;
using Pericia.Storage;
using Pericia.Storage.S3;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddS3(this FileStorageServiceBuilder builder, Action<S3StorageOptions> storageOptionsConfig)
        {
            return builder.AddService<S3Storage, S3StorageOptions>(storageOptionsConfig);
        }

        public static FileStorageServiceBuilder AddS3(this FileStorageServiceBuilder builder, IConfigurationSection configuration)
        {
            return builder.AddService<S3Storage, S3StorageOptions>(configuration, "S3");
        }
    }
}
