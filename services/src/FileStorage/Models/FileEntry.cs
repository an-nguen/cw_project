namespace FileStorage.Models;

public sealed class FileEntry
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
}