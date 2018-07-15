using Pericia.Storage.FileSystem;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Pericia.Storage.Tests
{
    public class FileSystemTest
    {
        private string TestLine = Guid.NewGuid().ToString();

        [Fact]
        public async Task SaveFile()
        {
            var options = new FileSystemStorageOptions
            {
                Path = @"C:\Temp\"
            };
            var fileStorage = new FileSystemStorage(options, "TestContainer");

            string fileId;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    writer.WriteLine(TestLine);
                    writer.Flush();

                    memoryStream.Position = 0;
                    fileId = await fileStorage.SaveFile(memoryStream);
                    Assert.NotNull(fileId);
                }
            }

            using (var fileStream = await fileStorage.GetFile(fileId))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    string line = reader.ReadLine();
                    Assert.Equal(TestLine, line);
                }
            }

            await fileStorage.DeleteFile(fileId);
            var nullFileStream = await fileStorage.GetFile(fileId);
            Assert.Null(nullFileStream);
        }
    }
}
