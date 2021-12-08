using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pericia.Storage.OpenStack
{
#pragma warning disable CA1812 // Classes are not directly instantiated, but they are deserialized from json
    internal class OpenStackResponseV2
    {
        [JsonPropertyName("access")]
        public AccessV2 Access { get; set; } = default!;
    }

    internal class AccessV2
    {
        [JsonPropertyName("token")]
        public TokenV2 Token { get; set; } = default!;
    }

    internal class TokenV2
    {
        [JsonPropertyName("issued_at")]
        public DateTime IssuedAt { get; set; }

        [JsonPropertyName("expires")]
        public DateTime Expires { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
    }
#pragma warning restore CA1812
}
