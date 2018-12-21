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

        public static FileStorageServiceBuilder AddService<TFileStorage, TFileStorageOptions>(this FileStorageServiceBuilder builder, IConfigurationSection configuration, string providerName = null)
            where TFileStorage : class, IFileStorage, new()
            where TFileStorageOptions : FileStorageOptions, new()
        {
            var options = configuration.Get<TFileStorageOptions>();

            if (!string.IsNullOrEmpty(providerName) && !string.IsNullOrEmpty(options.Provider) &&
                !string.Equals(providerName, options.Provider, StringComparison.OrdinalIgnoreCase))
            {
                // Configuration asked for another specific provider, so we skip this one
                return builder;
            }

            RegisterFileStorageProvider<TFileStorage, TFileStorageOptions>(builder, options);
            return builder;
        }

        public static FileStorageServiceBuilder AddService<TFileStorage, TFileStorageOptions>(this FileStorageServiceBuilder builder, IConfiguration configuration, string providerName = null)
            where TFileStorage : class, IFileStorage, new()
            where TFileStorageOptions : FileStorageOptions, new()
        {
            var section = configuration.GetSection("Pericia.Storage");
            return builder.AddService<TFileStorage, TFileStorageOptions>(section, providerName);
        }

        private static void RegisterFileStorageProvider<TFileStorage, TFileStorageOptions>(FileStorageServiceBuilder builder, TFileStorageOptions options)
            where TFileStorage : class, IFileStorage, new()
            where TFileStorageOptions : FileStorageOptions
        {
            var storage = new TFileStorage();
            storage.Options = options;            
            builder.Services.TryAddSingleton<IFileStorage>(storage);
            builder.Services.AddSingleton<TFileStorage>(storage);
            builder.LastStorageAdded = storage;
        }

        public static FileStorageServiceBuilder AddContainer(this FileStorageServiceBuilder builder, string container)
        {
            var storage = builder.LastStorageAdded ?? throw new Exception("You must add a Storage service before adding a container");

            var containerService = storage.GetContainer(container);
            builder.Services.TryAddSingleton<IFileStorageContainer>(containerService);
            builder.Services.AddSingleton(containerService.GetType(), containerService);

            return builder;
        }

        public static FileStorageServiceBuilder AddContainer(this FileStorageServiceBuilder builder)
        {
            var storage = builder.LastStorageAdded ?? throw new Exception("You must add a Storage service before adding a container");
            var container = storage.Options?.Container ?? throw new Exception("You must define the container name");

            var containerService = storage.GetContainer(container);
            builder.Services.TryAddSingleton<IFileStorageContainer>(containerService);
            builder.Services.AddSingleton(containerService.GetType(), containerService);

            return builder;
        }
    }


}
