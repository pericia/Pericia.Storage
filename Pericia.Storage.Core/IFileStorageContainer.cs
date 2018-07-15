using System;
using System.IO;
using System.Threading.Tasks;

namespace Pericia.Storage
{
    public interface IFileStorageContainer
    {
        void Init(FileStorageOptions options, string container);

        Task<string> SaveFile(Stream fileData);

        Task<string> SaveFile(Stream fileData, string fileId);

        Task<Stream> GetFile(string fileId);

        Task DeleteFile(string fileId);
    }
}
