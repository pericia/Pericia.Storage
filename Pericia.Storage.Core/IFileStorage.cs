using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage
{
    public interface IFileStorage
    {
        FileStorageOptions Options { get; set; }

        IFileStorageContainer GetContainer(string container);
    }
}
