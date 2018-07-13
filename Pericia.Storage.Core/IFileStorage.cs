using System;
using System.IO;
using System.Threading.Tasks;

namespace Pericia.Storage
{
    public interface IFileStorage
    {
        void Init(FileStorageOptions options);

        Task<string> SaveFile(Stream fileData);

        Task<string> SaveFile(Stream fileData, string fileId);

        Task<Stream> GetFile(string fileId);

        Task DeleteFile(string fileId);
    }
}
