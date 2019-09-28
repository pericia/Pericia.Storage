using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.OpenStack
{
    public class OpenStackStorage : BaseFileStorage<OpenStackStorageContainer, OpenStackStorageOptions>
    {
        public OpenStackStorage() : base(default!)
        {
        }

        public OpenStackStorage(OpenStackStorageOptions options) : base(options)
        {
        }
    }
}
