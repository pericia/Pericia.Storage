using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage
{
    public class DataProtectionOptions: FileStorageOptions
    {
        public string? DataProtectionContainer { get; set; }
        public string? SubDirectory { get; set; }
    }
}
