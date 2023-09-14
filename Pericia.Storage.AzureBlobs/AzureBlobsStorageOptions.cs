using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.AzureBlobs
{
    public class AzureBlobsStorageOptions : FileStorageOptions
    {
        public string? ConnectionString { get; set; }

        public string? AccountName { get; set; }

        public string? ManagedIdentityClientId { get; set; }

    }
}
