using Pericia.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileStorageServiceCollectionExtensions
    {
        public static FileStorageServiceBuilder AddStorage(this IServiceCollection services)
        {
            return new FileStorageServiceBuilder()
            {
                Services = services,
                Options = new FileStorageOptions(),
            };
        }

        public static FileStorageServiceBuilder AddStorage(this IServiceCollection services, FileStorageOptions storageOptions)
        {
            return new FileStorageServiceBuilder()
            {
                Services = services,
                Options = storageOptions,
            };
        }

        public static FileStorageServiceBuilder AddStorage(this IServiceCollection services, Action<FileStorageOptions> storageOptionsConfig)
        {
            var options = new FileStorageOptions();
            storageOptionsConfig?.Invoke(options);
            return AddStorage(services, options);
        }
    }


}
