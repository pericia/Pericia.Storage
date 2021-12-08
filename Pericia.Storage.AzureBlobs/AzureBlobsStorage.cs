using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.AzureBlobs
{
    public class AzureBlobsStorage : BaseFileStorage<AzureBlobsStorageContainer, AzureBlobsStorageOptions>
    {
        public AzureBlobsStorage() : base(default!)
        {
        }

        public AzureBlobsStorage(AzureBlobsStorageOptions options) : base(options)
        {
        }
    }
}
