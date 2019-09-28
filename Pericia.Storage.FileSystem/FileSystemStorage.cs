using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.FileSystem
{
    public class FileSystemStorage : BaseFileStorage<FileSystemStorageContainer, FileSystemStorageOptions>
    {
        public FileSystemStorage() : base(default!)
        {
        }

        public FileSystemStorage(FileSystemStorageOptions options) : base(options)
        {
        }
    }
}
