namespace BookApi.Config;
public sealed class BookApiOptions
{
  public const string BookApi = "BookApi";
  public string FileStorageUrl { get; set; } = string.Empty;
}