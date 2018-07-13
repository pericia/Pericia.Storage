using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage
{
    public class FileStorageServiceBuilder
    {
        public IServiceCollection Services { get; set; }
        
        public Action<FileStorageOptions> OptionsConfig { get; set; }

        public void ApplyOptions(FileStorageOptions options)
        {
            OptionsConfig?.Invoke(options);
        }
    }
}
