using Books;

namespace Gateway.Dtos;

public class BookAddReq
{
  public string Title { get; set; } = string.Empty;
  public string? Isbn { get; set; }

  public IEnumerable<string>? AuthorIds { get; set; }

  public BookAddRequest ToBookAddRequest()
  {
    var grpcRequest = new BookAddRequest()
    {
      Title = Title,
      Isbn = Isbn,
    };
    grpcRequest.AuthorIds.AddRange(AuthorIds);
    return grpcRequest;
  }
}