using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pericia.Storage.AspNetCore.DataProtection
{
    internal class PericiaStorageXmlRepository : IXmlRepository
    {
        private readonly DataProtectionStorageOptions storageOptions;
        private readonly ILogger logger;

        private readonly IFileStorageContainer container;

        public PericiaStorageXmlRepository(IFileStorage fileStorage, DataProtectionStorageOptions storageOptions, ILogger<PericiaStorageXmlRepository> logger)
        {
            this.storageOptions = storageOptions;
            this.logger = logger;

            this.container = fileStorage.GetContainer(storageOptions.DataProtectionContainer);
        }

        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return GetAllElementsCore().Result.AsReadOnly();
        }

        private async Task<List<XElement>> GetAllElementsCore()
        {
            var resultList = new List<XElement>();
            var files = await container.ListFiles(storageOptions.SubDirectory ?? "");
            foreach (var file in files)
            {
                resultList.Add(await ReadElementFromFile(file));
            }

            return resultList;
        }

        private async Task<XElement> ReadElementFromFile(string file)
        {
            using (var fileStream = await container.GetFile(file))
            {
                return XElement.Load(fileStream!);
            }
        }

        public void StoreElement(XElement element, string friendlyName)
        {
            Task.Run(() => StoreElementCore(element, friendlyName));
        }

        public Task StoreElementCore(XElement element, string friendlyName)
        {
            var fileName = string.IsNullOrEmpty(storageOptions.SubDirectory) ?
                friendlyName :
                storageOptions.SubDirectory + "/" + friendlyName;

            using var memStream = new MemoryStream();
            element.Save(memStream);
            memStream.Position = 0;
            return container.SaveFile(memStream, fileName);
        }
    }
}
