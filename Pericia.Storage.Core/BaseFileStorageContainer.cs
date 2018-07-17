using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pericia.Storage
{
    public abstract class BaseFileStorageContainer<TOptions> : IFileStorageContainer
        where TOptions : FileStorageOptions
    {
        public string Container { get; set; }

        public TOptions Options { get; set; }
        FileStorageOptions IFileStorageContainer.Options
        {
            get
            {
                return Options;
            }
            set
            {
                Options = (TOptions)value;
            }
        }

        public abstract Task CreateContainer();
        public abstract Task DeleteFile(string fileId);
        public abstract Task<Stream> GetFile(string fileId);
        public abstract Task<string> SaveFile(Stream fileData);
        public abstract Task<string> SaveFile(Stream fileData, string fileId);


    }
}
