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
        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder, OpenStackStorageSettings settings)
        {
            var storage = new OpenStackStorage(settings);

            builder.Services.TryAddSingleton<IFileStorage>(storage);
            builder.Services.AddSingleton(storage);

            return builder;
        }

        public static FileStorageServiceBuilder AddFileSystem(this FileStorageServiceBuilder builder, Action<OpenStackStorageSettings> settingsConfig)
        {
            var settings = new OpenStackStorageSettings
            {
                Container = builder.Options.Container
            };
            settingsConfig?.Invoke(settings);
            return AddFileSystem(builder, settings);
        }

    }
}

