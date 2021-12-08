using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pericia.Storage;
using Pericia.Storage.AzureBlobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddAzureBlobs(this FileStorageServiceBuilder builder, Action<AzureBlobsStorageOptions> storageOptionsConfig)
        {
            return builder.AddService<AzureBlobsStorage, AzureBlobsStorageOptions>(storageOptionsConfig);
        }

        public static FileStorageServiceBuilder AddAzureBlobs(this FileStorageServiceBuilder builder, IConfigurationSection configuration)
        {
            return builder.AddService<AzureBlobsStorage, AzureBlobsStorageOptions>(configuration, "Azure");
        }
    }
}

