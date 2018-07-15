using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.FileSystem
{
    public class FileSystemStorage : IFileStorage
    {
        public FileStorageOptions Options { get; set; }

        public IFileStorageContainer GetContainer(string container)
        {
            throw new NotImplementedException();
        }
    }
}
