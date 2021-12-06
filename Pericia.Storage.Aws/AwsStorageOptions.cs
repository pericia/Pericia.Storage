using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.Aws
{
    public class AwsStorageOptions : FileStorageOptions
    {
        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string? RegionEndpoint { get; set; } 

        public string? ServiceUrl { get; set; }

    }
}
