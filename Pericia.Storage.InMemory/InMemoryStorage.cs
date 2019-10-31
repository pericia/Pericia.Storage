using System.Collections.Generic;

namespace Pericia.Storage.InMemory
{
    public class InMemoryStorage : BaseFileStorage<InMemoryStorageContainer, FileStorageOptions>
    {
        private Dictionary<string, IFileStorageContainer> containers = new Dictionary<string, IFileStorageContainer>();

        public InMemoryStorage() : base(default!)
        {
        }

        public InMemoryStorage(FileStorageOptions options) : base(options)
        {
        }

        public override IFileStorageContainer GetContainer(string container)
        {
            if (containers.ContainsKey(container))
            {
                return containers[container];
            }

            var newContainer = base.GetContainer(container);
            containers.Add(container, newContainer);
            return newContainer;
        }
    }
}
