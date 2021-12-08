using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.S3
{
    public class S3Storage : BaseFileStorage<S3StorageContainer, S3StorageOptions>
    {
        public S3Storage() : base(default!)
        {
        }

        public S3Storage(S3StorageOptions options) : base(options)
        {
        }
    }
}
