namespace Pericia.Storage.InMemory
{
    public class InMemoryStorage : BaseFileStorage<InMemoryStorageContainer, FileStorageOptions>
    {
        public InMemoryStorage() : base(default!)
        {
        }

        public InMemoryStorage(FileStorageOptions options) : base(options)
        {
        }
    }
}
