using System;
using System.IO;
using System.Threading.Tasks;

namespace Pericia.Storage.CacheStorage
{
    public class CacheStorage : IFileStorage
    {
        private readonly IFileStorage _referenceStorage;
        private readonly IFileStorage _cacheStorage;

        public CacheStorage()
        {

        }

        public CacheStorage(IFileStorage referenceStorage, IFileStorage cacheStorage)
        {
            _referenceStorage = referenceStorage;
            _cacheStorage = cacheStorage;
        }

        public void Init(FileStorageOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString());
        }

        public async Task<string> SaveFile(Stream fileData, string fileId)
        {
            await _referenceStorage.SaveFile(fileData, fileId);

            try
            {
                await _cacheStorage.SaveFile(fileData, fileId);
            }
            catch (Exception)
            {
                // Cache problem ? let's delete the (maybe) outdated file
                try
                {
                    await _cacheStorage.DeleteFile(fileId);
                }
                catch (Exception)
                {
                    // ok, cache doesn't work, let's forget it 
                }
            }

            return fileId;
        }

        public async Task<Stream> GetFile(string fileId)
        {
            try
            {
                var cachedFile = await _cacheStorage.GetFile(fileId);
                if (cachedFile != null)
                {
                    return cachedFile;
                }
            }
            catch (Exception)
            {
                // Error getting file from cache
            }

            // No cached file, let's get it from reference and cache it
            var file = await _referenceStorage.GetFile(fileId);
            try
            {
                await _cacheStorage.SaveFile(file, fileId);
            }
            catch (Exception)
            {
                // cache error, I don't mind
            }

            return file;
        }

        public async Task DeleteFile(string fileId)
        {
            try
            {
                await _cacheStorage.DeleteFile(fileId);
            }
            catch (Exception)
            {
                // cache error, I don't mind
            }

            await _referenceStorage.DeleteFile(fileId);
        }

    }
}
