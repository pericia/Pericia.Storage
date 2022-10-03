using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage
{
    public abstract class BaseFileStorage<TContainer, TOptions> : IFileStorage
        where TContainer : IFileStorageContainer, new()
        where TOptions : FileStorageOptions
    {
        protected BaseFileStorage(TOptions options)
        {
            Options = options;
        }

        public TOptions Options { get; set; }
        FileStorageOptions IFileStorage.Options
        {
            get
            {
                return Options;
            }
            set
            {
                Options = (TOptions)value;
            }
        }

        public virtual IFileStorageContainer GetContainer()
        {
            return GetContainer(null);
        }

        public virtual IFileStorageContainer GetContainer(string? container)
        {
            if (string.IsNullOrWhiteSpace(container))
            {
                container = Options.Container;
            }

            if (string.IsNullOrWhiteSpace(container))
            {
                throw new ArgumentException("Container name not provided", nameof(container));
            }

            var containerService = new TContainer();
            containerService.Options = this.Options;
            containerService.Container = container!;
            return containerService;
        }
    }
}
