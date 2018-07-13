using Microsoft.Extensions.DependencyInjection.Extensions;
using Pericia.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddStorage(this IServiceCollection services, Action<FileStorageOptions> storageOptionsConfig = null)
        {
            return new FileStorageServiceBuilder()
            {
                Services = services,
                OptionsConfig = storageOptionsConfig,
            };
        }

        public static FileStorageServiceBuilder AddService<TFileStorage, TFileStorageOptions>(this FileStorageServiceBuilder builder, Action<TFileStorageOptions> storageOptionsConfig)
            where TFileStorage : class, IFileStorage, new()
            where TFileStorageOptions : FileStorageOptions, new()
        {
            var options = new TFileStorageOptions();
            builder.ApplyOptions(options);
            storageOptionsConfig?.Invoke(options);

            var storage = new TFileStorage();
            storage.Init(options);
            builder.Services.TryAddSingleton<IFileStorage>(storage);
            builder.Services.AddSingleton<TFileStorage>(storage);

            return builder;
        }
    }


}
