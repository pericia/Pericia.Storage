using Microsoft.Extensions.Configuration;
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

            RegisterFileStorageProvider<TFileStorage, TFileStorageOptions>(builder, options);
            return builder;
        }

        public static FileStorageServiceBuilder AddService<TFileStorage, TFileStorageOptions>(this FileStorageServiceBuilder builder, IConfigurationSection configuration)
            where TFileStorage : class, IFileStorage, new()
            where TFileStorageOptions : FileStorageOptions, new()
        {
            var options = configuration.Get<TFileStorageOptions>();

            RegisterFileStorageProvider<TFileStorage, TFileStorageOptions>(builder, options);
            return builder;
        }

        public static FileStorageServiceBuilder AddService<TFileStorage, TFileStorageOptions>(this FileStorageServiceBuilder builder, IConfiguration configuration)
            where TFileStorage : class, IFileStorage, new()
            where TFileStorageOptions : FileStorageOptions, new()
        {
            var section = configuration.GetSection("Pericia.Storage");
            var options = section.Get<TFileStorageOptions>();

            RegisterFileStorageProvider<TFileStorage, TFileStorageOptions>(builder, options);
            return builder;
        }

        private static void RegisterFileStorageProvider<TFileStorage, TFileStorageOptions>(FileStorageServiceBuilder builder, TFileStorageOptions options)
            where TFileStorage : class, IFileStorage, new()
            where TFileStorageOptions : FileStorageOptions
        {
            var storage = new TFileStorage();
            builder.Services.TryAddSingleton<IFileStorage>(storage);
            builder.Services.AddSingleton<TFileStorage>(storage);
        }
    }


}
