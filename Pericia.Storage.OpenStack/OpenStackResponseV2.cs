using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pericia.Storage.OpenStack
{
#pragma warning disable CA1812 // Classes are not directly instantiated, but they are deserialized from json
    internal class OpenStackResponseV2
    {
        [JsonProperty("access")]
        public AccessV2 Access { get; set; } = default!;
    }

    internal class AccessV2
    {
        [JsonProperty("token")]
        public TokenV2 Token { get; set; } = default!;
    }

    internal class TokenV2
    {
        [JsonProperty("issued_at")]
        public DateTime IssuedAt { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = default!;
    }
#pragma warning restore CA1812
}
