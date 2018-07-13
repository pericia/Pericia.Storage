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
        public static FileStorageServiceBuilder AddOpenStack(this FileStorageServiceBuilder builder, Action<OpenStackStorageOptions> storageOptionsConfig)
        {
            return builder.AddService<OpenStackStorage, OpenStackStorageOptions>(storageOptionsConfig);
        }
    }
}

