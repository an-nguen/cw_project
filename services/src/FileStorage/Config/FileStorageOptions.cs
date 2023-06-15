namespace FileStorage.Config;

public class FileStorageOptions
{
    public const string FileStorage = "FileStorage";
    public string StoragePath { get; set; } = string.Empty;
    // Max uploadable file size in bytes
    public long MaxFileSize { get; set; } = 104_857_600;
}