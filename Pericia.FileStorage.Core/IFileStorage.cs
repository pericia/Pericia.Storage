using System;
using System.IO;
using System.Threading.Tasks;

namespace Pericia.FileStorage
{
    public interface IFileStorage
    {
        Task<string> SaveFile(Stream fileData);

        Task<string> UpdateFile(Stream fileData, string fileId);

        Task<Stream> GetFile(string fileId);

        Task DeleteFile(string fileId);
    }
}
