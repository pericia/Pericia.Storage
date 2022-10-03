namespace Pericia.Storage.Minio
{
    public class MinioStorage : BaseFileStorage<MinioStorageContainer, MinioStorageOptions>
    {
        public MinioStorage() : base(default!)
        {
        }

        public MinioStorage(MinioStorageOptions options) : base(options)
        {
        }
    }
}
