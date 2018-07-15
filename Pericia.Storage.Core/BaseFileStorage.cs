using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage
{
    public abstract class BaseFileStorage<TContainer, TOptions> : IFileStorage
        where TContainer : IFileStorageContainer, new()
        where TOptions : FileStorageOptions
    {
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


        public IFileStorageContainer GetContainer(string container)
        {
            var containerService = new TContainer();
            containerService.Options = this.Options;
            containerService.Container = container;
            return containerService;
        }
    }
}
