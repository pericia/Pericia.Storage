using Microsoft.Extensions.Configuration;
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
            return builder.AddService<FileSystemStorage, FileSystemStorageOptions>(storageOptionsConfig);
        }

        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder, IConfigurationSection configuration)
        {
            return builder.AddService<FileSystemStorage, FileSystemStorageOptions>(configuration, "FileSystem");
        }
    }
}
