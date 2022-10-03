using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.OpenStack
{
    public class OpenStackStorageContainer : BaseFileStorageContainer<OpenStackStorageOptions>
    {
        private static readonly HttpClient client = new HttpClient();

        public OpenStackStorageContainer()
        {
        }

        public OpenStackStorageContainer(OpenStackStorageOptions options, string container)
        {
            Options = options;
            Container = container;
        }


        public override async Task<string> SaveFile(Stream fileData, string fileId, CancellationToken cancellationToken)
        {
            _ = fileData ?? throw new ArgumentNullException(nameof(fileData));
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var url = Options.ApiEndpoint + Container + "/" + fileId;
            var request = await CreateRequest(HttpMethod.Put, url, cancellationToken);
            request.Content = new StreamContent(fileData);
            var response = await client.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            return fileId;

        }

        public override async Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var request = await CreateRequest(HttpMethod.Get, Options.ApiEndpoint + Container + "/" + fileId, cancellationToken);
            var response = await client.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public override async Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var request = await CreateRequest(HttpMethod.Delete, Options.ApiEndpoint + Container + "/" + fileId, cancellationToken);
            var response = await client.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        public override async Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            var request = await CreateRequest(HttpMethod.Head, Options.ApiEndpoint + Container + "/" + fileId, cancellationToken);
            var response = await client.SendAsync(request, cancellationToken);

            return response.StatusCode == HttpStatusCode.OK;
        }

        public override Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
            => ListFiles(null!, cancellationToken);

        public override async Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            var url = Options.ApiEndpoint + Container;

            if (!string.IsNullOrEmpty(subfolder))
            {
                url += "?prefix=" + subfolder + "/";
            }

            var request = await CreateRequest(HttpMethod.Get, url, cancellationToken);
            var response = await client.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
            {
                return Enumerable.Empty<string>();
            }

            var result = new List<string>();
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    result.Add(line);
                }
            }

            return result;
        }


        public override async Task CreateContainer(CancellationToken cancellationToken)
        {
            var request = await CreateRequest(HttpMethod.Put, Options.ApiEndpoint + Container, cancellationToken);
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }


        #region Request helpers

        private async Task<HttpRequestMessage> CreateRequest(HttpMethod method, string url, CancellationToken cancellationToken)
        {
            var token = await GetToken(cancellationToken).ConfigureAwait(false);

            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-Auth-Token", token);

            return request;
        }

        #endregion

        #region Token
        private string? TokenId;
        private DateTime TokenExpires = DateTime.MinValue;

        private Task<string> GetToken(CancellationToken cancellationToken)
        {
            if (TokenExpires < DateTime.Now.AddMinutes(1))
            {
                TokenId = null;
            }

            if (TokenId != null)
            {
                return Task.FromResult(TokenId);
            }

            switch (Options.AuthApiVersion)
            {
                case 2:
                    return GetTokenV2(cancellationToken);

                case 3:
                default:
                    return GetTokenV3(cancellationToken);
            }
        }

        private async Task<string> GetTokenV3(CancellationToken cancellationToken)
        {
            var auth = "{ \"auth\": { \"identity\": { \"methods\": [\"password\"], \"password\": { \"user\": { \"name\": \"" + Options.UserId + "\", \"domain\": { \"id\": \"default\" }, \"password\": \"" + Options.Password + "\" } } }, \"scope\": { \"project\": { \"name\": \"" + Options.TenantName + "\", \"domain\": { \"id\": \"default\" } } } } }";
            var content = new StringContent(auth, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Options.AuthEndpoint + "/auth/tokens", content, cancellationToken);

            var jsonResponse = await response.Content.ReadAsStreamAsync();
            var objectResponse = JsonSerializer.Deserialize<OpenStackResponseV3>(jsonResponse);

            var tokenId = response.Headers.GetValues("X-Subject-Token").FirstOrDefault();
            var expires = objectResponse?.Token?.Expires;

            if (tokenId != null && expires != null)
            {
                TokenId = tokenId;
                TokenExpires = expires.Value;

                return TokenId;
            }

            return string.Empty;
        }

        private async Task<string> GetTokenV2(CancellationToken cancellationToken)
        {
            var auth = "{\"auth\": {\"tenantName\": \"" + Options.TenantName + "\", \"passwordCredentials\": {\"username\": \"" + Options.UserId + "\", \"password\": \"" + Options.Password + "\"}}}";
            var content = new StringContent(auth, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Options.AuthEndpoint + "tokens", content, cancellationToken);
            var jsonResponse = await response.Content.ReadAsStreamAsync();

            var objectResponse = JsonSerializer.Deserialize<OpenStackResponseV2>(jsonResponse);
            var token = objectResponse?.Access?.Token;

            if (token != null)
            {
                TokenId = token.Id;
                TokenExpires = token.Expires;

                return TokenId;
            }

            return string.Empty;
        }
        #endregion
    }
}
