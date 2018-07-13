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
    }


}
