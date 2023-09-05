using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace backblaze_directory_monitor.Models
{
    internal static class ModelsExtensions
    {
        public static string GetApiKey(this BackblazeUser user)
        {
            return 
                Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $"{user.AppId}:{user.AppKey}"
            ));
        }

        public static string FileName_Safe(this AudioFileUpload data) => HttpUtility.UrlEncode(data.FileName);

        public static ByteArrayContent BuildContent(this AudioFileUpload data)
        {
            string sha1 = Convert.ToHexString(SHA1.HashData(data.FileData));

            var content = new ByteArrayContent(data.FileData);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/mpeg");
            content.Headers.Add("X-Bz-Content-Sha1", sha1);
            content.Headers.Add("X-Bz-Info-Author", data.Author);

            return content;
        }

        public static Uri BuildUploadUri(this AuthResult auth, string bucketId)
        {
            UriBuilder builder = new UriBuilder($"{auth.ApiInfo.StorageApi.ApiUrl}/b2api/v3/b2_get_upload_url");
            builder.Query = HttpUtility.ParseQueryString($"bucketId={bucketId}").ToString(); ;
            return builder.Uri;

        }
    }
}
