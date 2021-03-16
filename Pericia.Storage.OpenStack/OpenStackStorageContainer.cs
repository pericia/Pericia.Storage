using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pericia.Storage.OpenStack
{
    public class OpenStackStorageContainer : BaseFileStorageContainer<OpenStackStorageOptions>
    {

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
            var request = await CreateRequest("PUT", url, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            if (request == null)
            {
                throw new Exception("OpenStackStorage.SaveFile : Error on request - url : " + url + " - request is null");
            }

            using Stream dataStream = await request.GetRequestStreamAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            using var fileStream = new MemoryStream();

            fileData.CopyTo(fileStream);
            await dataStream.WriteAsync(fileStream.ToArray(), 0, (int)fileStream.Length, cancellationToken).ConfigureAwait(false);
            dataStream.Close();

            await request.GetResponseAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            return fileId;

        }

        public override async Task<Stream?> GetFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var request = await CreateRequest("GET", Options.ApiEndpoint + Container + "/" + fileId, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var response = await request.GetResponseAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                return response.GetResponseStream();
            }
            catch (WebException ex)
            {
                var httpResponse = (HttpWebResponse)ex.Response;
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    // 404 error means the file doesn't exist, we just return null
                    return null;
                }

                throw;
            }
        }

        public override async Task DeleteFile(string fileId, CancellationToken cancellationToken)
        {
            _ = fileId ?? throw new ArgumentNullException(nameof(fileId));

            var request = await CreateRequest("DELETE", Options.ApiEndpoint + Container + "/" + fileId, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await request.GetResponseAsync().ConfigureAwait(false);
        }


        public override async Task CreateContainer(CancellationToken cancellationToken)
        {
            var request = await CreateRequest("PUT", Options.ApiEndpoint + Container, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await request.GetResponseAsync().ConfigureAwait(false);
        }


        #region Request helpers
        private async Task<WebRequest> CreateRequest(string method, string url, CancellationToken cancellationToken, bool useToken = true)
        {
            var request = WebRequest.Create(new Uri(url));
            request.Method = method;

            if (useToken)
            {
                var token = await GetToken(cancellationToken).ConfigureAwait(false);
                request.Headers.Add("X-Auth-Token", token);
            }

            return request;
        }

        private async Task<WebRequest> CreatePostJsonRequest(string url, string data, CancellationToken cancellationToken, bool useToken = true)
        {
            var request = await CreateRequest("POST", url, cancellationToken, useToken).ConfigureAwait(false);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = await request.GetRequestStreamAsync().ConfigureAwait(false);
            await dataStream.WriteAsync(byteArray, 0, byteArray.Length).ConfigureAwait(false);
            dataStream.Close();

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
            var request = await CreatePostJsonRequest(Options.AuthEndpoint + "/auth/tokens", auth, cancellationToken, false).ConfigureAwait(false);

            var response = await request.GetResponseAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                string jsonResponse = await reader.ReadToEndAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                var objectResponse = JsonConvert.DeserializeObject<OpenStackResponseV3>(jsonResponse);

                var tokenId = response.Headers["X-Subject-Token"];
                var expires = objectResponse?.Token?.Expires;

                if (tokenId != null && expires != null)
                {
                    TokenId = tokenId;
                    TokenExpires = expires.Value;

                    return TokenId;
                }
            }

            throw new NotImplementedException();
        }

        private async Task<string> GetTokenV2(CancellationToken cancellationToken)
        {
            var auth = "{\"auth\": {\"tenantName\": \"" + Options.TenantName + "\", \"passwordCredentials\": {\"username\": \"" + Options.UserId + "\", \"password\": \"" + Options.Password + "\"}}}";
            var request = await CreatePostJsonRequest(Options.AuthEndpoint + "tokens", auth, cancellationToken, false).ConfigureAwait(false);

            var response = await request.GetResponseAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                string jsonResponse = await reader.ReadToEndAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                var objectResponse = JsonConvert.DeserializeObject<OpenStackResponseV2>(jsonResponse);
                var token = objectResponse?.Access?.Token;

                if (token != null)
                {
                    TokenId = token.Id;
                    TokenExpires = token.Expires;

                    return TokenId;
                }
            }


            return string.Empty;
        }

        public override Task<bool> FileExists(string fileId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<string>> ListFiles(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<string>> ListFiles(string subfolder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
