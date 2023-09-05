using System.Text;
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

        public static Uri BuildUploadUri(this AuthResult auth, string bucketId)
        {
            UriBuilder builder = new UriBuilder($"{auth.ApiInfo.StorageApi.ApiUrl}/b2api/v3/b2_get_upload_url");
            builder.Query = HttpUtility.ParseQueryString($"bucketId={bucketId}").ToString(); ;
            return builder.Uri;

        }
    }
}
