using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage
{
    public interface IFileStorageContainer
    {
        FileStorageOptions Options { get; set; }
        string Container { get; set; }

        Task CreateContainer();
        Task CreateContainer(CancellationToken cancellationToken);

        Task<string> SaveFile(Stream fileData);

        Task<string> SaveFile(Stream fileData, string fileId);

        Task<string> SaveFile(Stream fileData, CancellationToken cancellationToken);

        Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken);

        Task<Stream?> GetFile(string fileId);

        Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken);

        Task DeleteFile(string fileId);

        Task DeleteFile(string fileId, CancellationToken cancellationToken);

        Task<bool> FileExists(string fileId);
        Task<bool> FileExists(string fileId, CancellationToken cancellationToken);
    }
}
