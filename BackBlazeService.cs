using backblaze_directory_monitor.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Web;

namespace backblaze_directory_monitor
{
    internal class BackBlazeService
    {
        private const string authUrl = "https://api.backblazeb2.com/b2api/v3/b2_authorize_account";
        private const string AUTHORIZATION = "Authorization";

        private readonly BackblazeUser AdminUser; 
        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
        };

        public BackBlazeService(IConfiguration config)
        {
            // user-secrets
            string appId = config["BACKBLAZE:APPID"];
            string appKey = config["BACKBLAZE:APPKEY"];
            AdminUser = new BackblazeUser(appId, appKey);
        }

        public async Task<AuthResult> Authorize()
        {
            
            using (HttpClient client = new HttpClient())
            {
                // todo: is this ApiKey part correct?
                client.DefaultRequestHeaders.TryAddWithoutValidation(AUTHORIZATION, $"Basic{AdminUser.GetApiKey()}");
                var response = await client.GetAsync(authUrl);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {    
                    return JsonSerializer.Deserialize<AuthResult>(result, serializerOptions);
                }

                throw new Exception("Error authorizing:" + result);
            }
        }

        public async Task UploadFile(GetUploadUrlResponse upload, string filePath)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(AUTHORIZATION, upload.AuthorizationToken);

                byte[] fileBytes = File.ReadAllBytes(filePath);
                var info = new FileInfo(filePath);

                string sha1 = Convert.ToHexString(SHA1.HashData(fileBytes));

                ByteArrayContent byteArrayContent = new ByteArrayContent(fileBytes);
                byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
                client.DefaultRequestHeaders.Add("X-Bz-File-Name", HttpUtility.UrlEncode($"{info.Directory.Name}/{info.Name}"));
                byteArrayContent.Headers.Add("X-Bz-Content-Sha1", sha1);
                byteArrayContent.Headers.Add("X-Bz-Info-Author", "temporal");

                var response = await client.PostAsync(upload.UploadUrl, (HttpContent)byteArrayContent);
                var result = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error uploading: [{response.StatusCode}] - {result}");
                }
            }
        }

        public async Task<GetUploadUrlResponse> GetUploadUrl(AuthResult auth, string bucketId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(AUTHORIZATION, $"{auth.AuthorizationToken}");
                var response =await client.GetAsync(auth.BuildUploadUri(bucketId));
                var result = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GetUploadUrlResponse>(result, serializerOptions);
            }
        }



    }
}
