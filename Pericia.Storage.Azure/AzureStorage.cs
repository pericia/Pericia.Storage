using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.Azure
{
    public class AzureStorage : BaseFileStorage<AzureStorageContainer, AzureStorageOptions>
    {
        public AzureStorage() : base(default!)
        {
        }

        public AzureStorage(AzureStorageOptions options) : base(options)
        {
        }
    }
}
