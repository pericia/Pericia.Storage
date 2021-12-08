
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
            var testLine = Guid.NewGuid().ToString();
            string fileId;

            var currentFileCount = (await fileStorage.ListFiles()).Count();

            // Save new file
            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);

                writer.WriteLine(testLine);
                writer.Flush();

                memoryStream.Position = 0;
                fileId = await fileStorage.SaveFile(memoryStream);
                Assert.NotNull(fileId);
            }

            var newFileCount = (await fileStorage.ListFiles()).Count();
            Assert.Equal(currentFileCount + 1, newFileCount);
            Assert.True(await fileStorage.FileExists(fileId));

            // Get the file
            {
                using var fileStream = await fileStorage.GetFile(fileId);
                Assert.NotNull(fileStream);

                using var reader = new StreamReader(fileStream ?? default!);
                var line = reader.ReadLine();
                Assert.Equal(testLine, line);
            }

            // Overwrite the file
            testLine = Guid.NewGuid().ToString();
            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);

                writer.WriteLine(testLine);
                writer.Flush();

                memoryStream.Position = 0;
                _ = await fileStorage.SaveFile(memoryStream, fileId);
            }

            // Delete the file
            {
                await fileStorage.DeleteFile(fileId);
                var nullFileStream = await fileStorage.GetFile(fileId);
                Assert.Null(nullFileStream);
                Assert.False(await fileStorage.FileExists(fileId));
            }
        }


    }
}
