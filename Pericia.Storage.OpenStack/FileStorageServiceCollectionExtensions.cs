using Microsoft.Extensions.DependencyInjection.Extensions;
using Pericia.Storage;
using Pericia.Storage.OpenStack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder, Action<OpenStackStorageOptions> storageOptionsConfig)
        {
            var options = new OpenStackStorageOptions();
            builder.ApplyOptions(options);
            storageOptionsConfig?.Invoke(options);

            var storage = new OpenStackStorage(options);
            builder.Services.TryAddSingleton<IFileStorage>(storage);
            builder.Services.AddSingleton(storage);

            return builder;
        }
    }
}

