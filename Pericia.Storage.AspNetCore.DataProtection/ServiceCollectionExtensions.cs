using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pericia.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pericia.Storage.AspNetCore.DataProtection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IDataProtectionBuilder PersistKeysToPericiaStorage(this IDataProtectionBuilder builder)
        {
            return PersistKeysToPericiaStorageCore(builder, new DataProtectionStorageOptions());
        }


        public static IDataProtectionBuilder PersistKeysToPericiaStorage(this IDataProtectionBuilder builder, string? container = null, string? subDirectory = null)
        {
            return PersistKeysToPericiaStorageCore(builder, new DataProtectionStorageOptions
            {
                DataProtectionContainer = container,
                SubDirectory = subDirectory,
            });
        }

        // Configure ASP.NET DataProtection to use our IXmlRepository
        private static IDataProtectionBuilder PersistKeysToPericiaStorageCore(IDataProtectionBuilder builder, DataProtectionStorageOptions storageOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(services =>
            {
                var loggerFactory = services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
                var storage = services.GetService<IFileStorage>() ?? throw new Exception("Pericia.Storage not configured");

                return new ConfigureOptions<KeyManagementOptions>(options =>
                {
                    options.XmlRepository = new PericiaStorageXmlRepository(storage, storageOptions, loggerFactory.CreateLogger<PericiaStorageXmlRepository>());
                });
            });

            return builder;
        }

    }
}
