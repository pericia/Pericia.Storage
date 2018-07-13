using Microsoft.Extensions.DependencyInjection.Extensions;
using Pericia.Storage;
using Pericia.Storage.Azure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddAzure(this FileStorageServiceBuilder builder, Action<AzureStorageOptions> storageOptionsConfig)
        {
            return builder.AddService<AzureStorage, AzureStorageOptions>(storageOptionsConfig);           
        }
    }
}

