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
        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder)
        {
            return AddFileSystem(builder, builder.Options);
        }

        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder, FileStorageOptions options)
        {
            var storage = new FileSystemStorage(options.Path, options.Container);

            builder.Services.TryAddSingleton<IFileStorage>(storage);
            builder.Services.AddSingleton(storage);

            return builder;
        }

        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder, Action<FileStorageOptions> storageOptionsConfig)
        {
            storageOptionsConfig?.Invoke(builder.Options);
            return AddFileSystem(builder, builder.Options);
        }

    }
}
