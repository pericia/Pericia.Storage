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
    public class PericiaStorageXmlRepository : IXmlRepository
    {
        private readonly IFileStorage fileStorage;
        private readonly DataProtectionOptions storageOptions;
        private readonly ILogger logger;

        private readonly IFileStorageContainer container;

        public PericiaStorageXmlRepository(IFileStorage fileStorage, DataProtectionOptions storageOptions, ILogger<PericiaStorageXmlRepository> logger)
        {
            this.fileStorage = fileStorage;
            this.storageOptions = storageOptions;
            this.logger = logger;

            this.container = GetContainer();
        }

        private IFileStorageContainer GetContainer()
        {
            var containerName = storageOptions.DataProtectionContainer ?? storageOptions.Container;

            if (string.IsNullOrEmpty(containerName))
            {
                logger.LogError("No container has been registered for Data Protection storage");
                throw new Exception("No container has been registered for Data Protection storage");
            }

            return fileStorage.GetContainer(containerName);
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
            using var memStream = new MemoryStream();
            element.Save(memStream);
            memStream.Position = 0;
            return container.SaveFile(memStream, friendlyName);
        }
    }
}
