using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.AzureBlobs
{
    public class AzureBlobsStorageOptions : FileStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
