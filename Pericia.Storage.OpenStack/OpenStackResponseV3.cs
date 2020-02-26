using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pericia.Storage.OpenStack
{
#pragma warning disable CA1812 // Classes are not directly instantiated, but they are deserialized from json
    internal class OpenStackResponseV3
    {
        [JsonProperty]
        public TokenV3 Token { get; set; } = default!;
    }

    internal class TokenV3
    {

        [JsonProperty("expires_at")]
        public DateTime Expires { get; set; }
    }
#pragma warning restore CA1812
}
