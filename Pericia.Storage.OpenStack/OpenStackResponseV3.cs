using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pericia.Storage.OpenStack
{
#pragma warning disable CA1812 // Classes are not directly instantiated, but they are deserialized from json
    internal class OpenStackResponseV3
    {
        [JsonPropertyName("token")]
        public TokenV3 Token { get; set; } = default!;
    }

    internal class TokenV3
    {

        [JsonPropertyName("expires_at")]
        public DateTime Expires { get; set; }
    }
#pragma warning restore CA1812
}
