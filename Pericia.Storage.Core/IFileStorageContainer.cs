using System;
using System.IO;
using System.Threading.Tasks;

namespace Pericia.Storage
{
    public interface IFileStorageContainer
    {
        FileStorageOptions Options { get; set; }
        string Container { get; set; }

        Task CreateContainer();

        Task<string> SaveFile(Stream fileData);

        Task<string> SaveFile(Stream fileData, string fileId);

        Task<Stream> GetFile(string fileId);

        Task DeleteFile(string fileId);
    }
}
