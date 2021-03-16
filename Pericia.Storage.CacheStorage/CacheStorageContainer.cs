using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.CacheStorage
{
#pragma warning disable CA1031 // Do not catch general exception types
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
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

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

        public async Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

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
            if (file != null)
            {
                try
                {
                    await _cacheStorage.SaveFile(file, fileId, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // cache error, I don't mind
                }
            }

            return file;
        }

        public async Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

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
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));

            return SaveFile(fileData, Guid.NewGuid().ToString(), CancellationToken.None);
        }

        public Task<string> SaveFile(Stream fileData, string fileId)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            return SaveFile(fileData, fileId, CancellationToken.None);
        }

        public Task<string> SaveFile(Stream fileData, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));

            return SaveFile(fileData, Guid.NewGuid().ToString(), cancellationToken);
        }

        public async Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            return await _cacheStorage.FileExists(fileId, cancellationToken)
                   || await _referenceStorage.FileExists(fileId, cancellationToken);
        }

        public Task<Stream?> GetFile(string fileId)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            return GetFile(fileId, CancellationToken.None);
        }

        public Task DeleteFile(string fileId)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            return DeleteFile(fileId, CancellationToken.None);
        }

        public Task<bool> FileExists(string fileId)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            return FileExists(fileId, CancellationToken.None);
        }

        public Task<IEnumerable<string>> ListFiles()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ListFiles(string subfolder)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
#pragma warning restore CA1031 // Do not catch general exception types
