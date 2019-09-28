using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage
{
    public class FileStorageServiceBuilder
    {
        public FileStorageServiceBuilder(IServiceCollection services, Action<FileStorageOptions>? optionsConfig)
        {
            Services = services;
            OptionsConfig = optionsConfig;
        }

        internal IServiceCollection Services { get; set; }

        internal Action<FileStorageOptions>? OptionsConfig { get; set; }

        internal void ApplyOptions(FileStorageOptions options)
        {
            OptionsConfig?.Invoke(options);
        }

        internal IFileStorage? LastStorageAdded { get; set; }
    }
}
