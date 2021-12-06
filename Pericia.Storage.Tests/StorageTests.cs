using Pericia.Storage.Aws;
using Pericia.Storage.Azure;
using Pericia.Storage.FileSystem;
using Pericia.Storage.InMemory;
using Pericia.Storage.OpenStack;
using System;
using System.Collections.Generic;
using System.IO;
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

        [Fact]
        public Task TestAzureBlobs()
        {
            var options = new AzureStorageOptions
            {
                ConnectionString = ""
            };
            var fileStorage = new AzureStorageContainer(options, "TestContainer");

            return TestStorage(fileStorage);
        }

        [Fact]
        public Task TestAwsS3()
        {
            var options = new AwsStorageOptions
            {
                AccessKey = "",
                SecretKey = "",
                ServiceUrl = ""
            };
            var fileStorage = new AwsStorageContainer(options, "TestContainer");

            return TestStorage(fileStorage);
        }

        [Fact]
        public Task TestOpenStack()
        {
            var options = new OpenStackStorageOptions
            {
                ApiEndpoint = "",
                AuthEndpoint = "",
                TenantName = "",
                UserId = "",
                Password = "",
            };
            var fileStorage = new OpenStackStorageContainer(options, "TestContainer");

            return TestStorage(fileStorage);
        }

        private async Task TestStorage(IFileStorageContainer fileStorage)
        {
            var testLine = Guid.NewGuid().ToString();
            string fileId;

            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream);

                writer.WriteLine(testLine);
                writer.Flush();

                memoryStream.Position = 0;
                fileId = await fileStorage.SaveFile(memoryStream);
                Assert.NotNull(fileId);
            }

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
        }


    }
}
