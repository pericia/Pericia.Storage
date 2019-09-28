using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pericia.Storage.OpenStack
{
#pragma warning disable CA1812 // Classes are not directly instantiated, but they are deserialized from json
    internal class OpenStackResponse
    {
        [JsonProperty("access")]
        public Access Access { get; set; } = default!;
    }

    internal class Access
    {
        [JsonProperty("token")]
        public Token Token { get; set; } = default!;
    }

    internal class Token
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
