namespace backblaze_directory_monitor.Models
{
    public record BackblazeUser (string AppId, string AppKey);
    public record AuthResult(string AccountId, string AuthorizationToken, ApiInfo ApiInfo);
    public record ApiInfo(StorageApi StorageApi);
    public record StorageApi(string ApiUrl, string BucketName, string DownloadUrl);
    public record UploadParams(string BucketId, string FileName, string ContentType);
    public record GetUploadUrlResponse(string AuthorizationToken, string BucketId, string UploadUrl);
    public record AudioFileUpload(byte[] FileData, string Author, string FileName);
}
