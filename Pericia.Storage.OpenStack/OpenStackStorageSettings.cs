using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.OpenStack
{
    public class OpenStackStorageSettings
    {
        // Authentication
        public string AuthEndpoint { get; set; }

        public string TenantName { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        // Openstack access
        public string ApiEndpoint { get; set; }

        public string Container { get; set; }
    }
}
