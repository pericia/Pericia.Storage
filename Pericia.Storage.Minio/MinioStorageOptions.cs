using System;

namespace Pericia.Storage.Minio
{
    public class MinioStorageOptions : FileStorageOptions
    {
        public string Endpoint { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string? Region { get; set; }

        public bool Insecure { get; set; }
    }
}
