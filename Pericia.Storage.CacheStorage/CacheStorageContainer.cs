using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.CacheStorage
{
    public sealed class CacheStorageContainer : IFileStorageContainer
    {
        private readonly IFileStorageContainer _referenceStorage;
        private readonly IFileStorageContainer _cacheStorage;

        FileStorageOptions IFileStorageContainer.Options { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        string IFileStorageContainer.Container { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }


        public CacheStorageContainer(IFileStorageContainer referenceStorage, IFileStorageContainer cacheStorage)
        {
            _referenceStorage = referenceStorage;
            _cacheStorage = cacheStorage;
        }


        public async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            await _referenceStorage.SaveFile(fileData, fileId, cancellationToken).ConfigureAwait(false);

            try
            {
                await _cacheStorage.SaveFile(fileData, fileId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Cache problem ? let's delete the (maybe) outdated file
                try
                {
                    await _cacheStorage.DeleteFile(fileId, CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // ok, cache doesn't work, let's forget it 
                }
            }

            return fileId;
        }

        public async Task<Stream> GetFile(string fileId, CancellationToken cancellationToken)
        {
            try
            {
                var cachedFile = await _cacheStorage.GetFile(fileId, cancellationToken).ConfigureAwait(false);
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
            var file = await _referenceStorage.GetFile(fileId, cancellationToken).ConfigureAwait(false);
            try
            {
                await _cacheStorage.SaveFile(file, fileId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // cache error, I don't mind
            }

            return file;
        }

        public async Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            try
            {
                await _cacheStorage.DeleteFile(fileId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // cache error, I don't mind
            }

            await _referenceStorage.DeleteFile(fileId, cancellationToken).ConfigureAwait(false);
        }

        public Task CreateContainer(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_referenceStorage.CreateContainer(), _cacheStorage.CreateContainer());
        }

        public Task CreateContainer()
        {
            return CreateContainer(CancellationToken.None);
        }

        public Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString(), CancellationToken.None);
        }

        public Task<string> SaveFile(Stream fileData, string fileId)
        {
            return SaveFile(fileData, fileId, CancellationToken.None);
        }

        public Task<string> SaveFile(Stream fileData, CancellationToken cancellationToken)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString(), cancellationToken);
        }

        public Task<Stream> GetFile(string fileId)
        {
            return GetFile(fileId, CancellationToken.None);
        }

        public Task DeleteFile(string fileId)
        {
            return DeleteFile(fileId, CancellationToken.None);
        }
    }
}
