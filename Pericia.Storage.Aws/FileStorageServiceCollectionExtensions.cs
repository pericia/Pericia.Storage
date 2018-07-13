using Pericia.Storage;
using Pericia.Storage.Aws;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddOpenStack(this FileStorageServiceBuilder builder, Action<AwsStorageOptions> storageOptionsConfig)
        {
            return builder.AddService<AwsStorage, AwsStorageOptions>(storageOptionsConfig);
        }
    }
}
