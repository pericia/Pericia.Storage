using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.Aws
{
    public class AwsStorage : BaseFileStorage<AwsStorageContainer, AwsStorageOptions>
    {
        public AwsStorage() : base(default!)
        {
        }

        public AwsStorage(AwsStorageOptions options) : base(options)
        {
        }
    }
}
