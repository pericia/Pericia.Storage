using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pericia.Storage.OpenStack
{
    internal class OpenStackResponse
    {
        [JsonProperty("access")]
        public Access Access { get; set; }
    }

    internal class Access
    {
        [JsonProperty("token")]
        public Token Token { get; set; }
    }

    internal class Token
    {
        [JsonProperty("issued_at")]
        public DateTime IssuedAt { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
