using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        

        public override Task<string> SaveFile(Stream fileData)
        {
            return SaveFile(fileData, Guid.NewGuid().ToString());
        }

        public override async Task<string> SaveFile(Stream fileData, string fileId)
        {
            if (fileData == null)
            {
                throw new ArgumentException("OpenStackStorage.SaveFile : fileData is null");
            }

            var url = Options.ApiEndpoint + Container + "/" + fileId;
            var request = await CreateRequest("PUT", url);

            if (request == null)
            {
                throw new Exception("OpenStackStorage.SaveFile : Error on request - url : " + url + " - request is null");
            }

            Stream dataStream = await request.GetRequestStreamAsync();
            var fileStream = new MemoryStream();
            fileData.CopyTo(fileStream);
            await dataStream.WriteAsync(fileStream.ToArray(), 0, (int)fileStream.Length);
            dataStream.Close();

            await request.GetResponseAsync();

            return fileId;
        }

        public override async Task<Stream> GetFile(string fileId)
        {
            var request = await CreateRequest("GET", Options.ApiEndpoint + Container + "/" + fileId);

            try
            {
                var response = await request.GetResponseAsync();

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

        public override async Task DeleteFile(string fileId)
        {
            var request = await CreateRequest("DELETE", Options.ApiEndpoint + Container + "/" + fileId);
            await request.GetResponseAsync();
        }


        public override async Task CreateContainer()
        {
            var request = await CreateRequest("PUT", Options.ApiEndpoint + Container);
            await request.GetResponseAsync();
        }


        #region Request helpers
        private async Task<WebRequest> CreateRequest(string method, string url, bool useToken = true)
        {
            var request = WebRequest.Create(url);
            request.Method = method;

            if (useToken)
            {
                var token = await GetToken();
                request.Headers.Add("X-Auth-Token", token);
            }

            return request;
        }

        private async Task<WebRequest> CreatePostJsonRequest(string url, string data, bool useToken = true)
        {
            var request = await CreateRequest("POST", url, useToken);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json";

            Stream dataStream = await request.GetRequestStreamAsync();
            await dataStream.WriteAsync(byteArray, 0, byteArray.Length);
            dataStream.Close();

            return request;
        }
        #endregion

        #region Token
        private static string TokenId;
        private static DateTime TokenExpires = DateTime.MinValue;

        private async Task<string> GetToken()
        {
            if (TokenExpires < DateTime.Now.AddMinutes(1))
            {
                TokenId = null;
            }

            if (TokenId != null)
            {
                return TokenId;
            }

            if (TokenId != null)
            {
                return TokenId;
            }

            var auth = "{\"auth\": {\"tenantName\": \"" + Options.TenantName + "\", \"passwordCredentials\": {\"username\": \"" + Options.UserId + "\", \"password\": \"" + Options.Password + "\"}}}";
            var request = await CreatePostJsonRequest(Options.AuthEndpoint + "tokens", auth, false);

            var response = await request.GetResponseAsync();

            using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                string jsonResponse = await reader.ReadToEndAsync();
                var objectResponse = JsonConvert.DeserializeObject<OpenStackResponse>(jsonResponse);
                var token = objectResponse?.Access?.Token;

                if (token != null)
                {
                    TokenId = token.Id;
                    TokenExpires = token.Expires;

                    return TokenId;
                }
            }


            return "";
        }
        #endregion
    }
}
