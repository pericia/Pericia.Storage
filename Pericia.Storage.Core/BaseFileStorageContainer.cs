using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage
{
    public abstract class BaseFileStorageContainer<TOptions> : IFileStorageContainer
        where TOptions : FileStorageOptions
    {
        public string Container { get; set; } = default!;

        public TOptions Options { get; set; } = default!;
        FileStorageOptions IFileStorageContainer.Options
        {
            get => Options;
            set => Options = (TOptions)value;
        }

        public virtual Task CreateContainer()
        {
            return CreateContainer(CancellationToken.None);
        }
        public abstract Task CreateContainer(CancellationToken cancellationToken);


        public virtual Task DeleteFile(string fileId)
        {
            return DeleteFile(fileId, CancellationToken.None);
        }
        public abstract Task DeleteFile(string fileId, CancellationToken cancellationToken);

        public virtual Task<Stream?> GetFile(string fileId)
        {
            return GetFile(fileId, CancellationToken.None);
        }
        public abstract Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken);


        public virtual Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString(), CancellationToken.None);
        }
        public virtual Task<string> SaveFile(Stream fileData, string fileId)
        {
            return SaveFile(fileData, fileId, CancellationToken.None);
        }
        public virtual Task<string> SaveFile(Stream fileData, CancellationToken cancellationToken)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString(), cancellationToken);
        }
        public abstract Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken);

        public  Task<bool> FileExists(string fileId)
        {
            return FileExists(fileId, CancellationToken.None);
        }
        public abstract Task<bool> FileExists(string fileId, CancellationToken cancellationToken);
    }
}
