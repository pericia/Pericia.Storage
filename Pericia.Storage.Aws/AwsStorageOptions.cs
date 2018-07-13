using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.Aws
{
    public class AwsStorageOptions : FileStorageOptions
    {
        public string AccessKey { get; set; }

        public string SecretKey { get; set; }
    }
}
