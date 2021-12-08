
using Pericia.Storage.FileSystem;
using Pericia.Storage.InMemory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pericia.Storage.Tests
{
    public class StorageTests
    {
        [Fact]
        public Task TestInMemory()
        {
            var fileStorage = new InMemoryStorageContainer();
            return TestStorage(fileStorage);
        }

        [Fact]
        public Task TestFileSystem()
        {
            var options = new FileSystemStorageOptions
            {
                Path = Path.GetTempPath()
            };
            var fileStorage = new FileSystemStorageContainer(options, "TestContainer");

            return TestStorage(fileStorage);
        }

        protected async Task TestStorage(IFileStorageContainer fileStorage)
        {
            var testFilename = Guid.NewGuid().ToString();
            var testLine = Guid.NewGuid().ToString();
            string fileId;

            var currentFileCount = (await fileStorage.ListFiles()).Count();
            Assert.False(await fileStorage.FileExists(testFilename));

            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);

                writer.WriteLine(testLine);
                writer.Flush();

                memoryStream.Position = 0;
                fileId = await fileStorage.SaveFile(memoryStream, testFilename);
                Assert.NotNull(fileId);
                Assert.Equal(testFilename, fileId);
            }

            var newFileCount = (await fileStorage.ListFiles()).Count();
            Assert.Equal(currentFileCount + 1, newFileCount);
            Assert.True(await fileStorage.FileExists(testFilename));

            {
                using var fileStream = await fileStorage.GetFile(fileId);
                Assert.NotNull(fileStream);

                using var reader = new StreamReader(fileStream ?? default!);
                var line = reader.ReadLine();
                Assert.Equal(testLine, line);
            }

            await fileStorage.DeleteFile(fileId);
            var nullFileStream = await fileStorage.GetFile(fileId);
            Assert.Null(nullFileStream);
            Assert.False(await fileStorage.FileExists(fileId));
        }


    }
}
