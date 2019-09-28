using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.Azure
{
    public class AzureStorageOptions : FileStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
