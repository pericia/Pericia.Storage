using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pericia.Storage;
using Pericia.Storage.Minio;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddMinIO(this FileStorageServiceBuilder builder, Action<MinioStorageOptions> storageOptionsConfig)
        {
            return builder.AddService<MinioStorage, MinioStorageOptions>(storageOptionsConfig);
        }

        public static FileStorageServiceBuilder AddMinIO(this FileStorageServiceBuilder builder, IConfigurationSection configuration)
        {
            return builder.AddService<MinioStorage, MinioStorageOptions>(configuration, "MinIO");
        }
    }
}
