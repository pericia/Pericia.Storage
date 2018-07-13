using Microsoft.Extensions.DependencyInjection.Extensions;
using Pericia.Storage;
using Pericia.Storage.FileSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder, Action<FileSystemStorageOptions> storageOptionsConfig)
        {
            var options = new FileSystemStorageOptions();
            builder.ApplyOptions(options);
            storageOptionsConfig?.Invoke(options);

            var storage = new FileSystemStorage(options);
            builder.Services.TryAddSingleton<IFileStorage>(storage);
            builder.Services.AddSingleton(storage);

            return builder;
        }
    }
}
