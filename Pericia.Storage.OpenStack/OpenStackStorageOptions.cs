using System;
using System.Collections.Generic;
using System.Text;

namespace Pericia.Storage.OpenStack
{
    public class OpenStackStorageOptions : FileStorageOptions
    {
        public OpenStackIdentityApiVersion ApiVersion { get; set; } = OpenStackIdentityApiVersion.V3;
        // Authentication
        public string AuthEndpoint { get; set; } = string.Empty;

        public string TenantName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Openstack access
        public string ApiEndpoint { get; set; } = string.Empty;

    }

    public enum OpenStackIdentityApiVersion
    {
        V2 = 2,
        V3 = 3,
    }
}
