using Books;

namespace Gateway.Dtos;

public class BookUpdReq : BookAddReq
{
  public Guid Id { get; set; }

  public BookUpdateRequest ToBookUpdateRequest()
  {
    var grpcRequest = new BookUpdateRequest()
    {
      Id = Id.ToString(),
      Title = Title,
      Isbn = Isbn,
    };
    grpcRequest.AuthorIds.AddRange(AuthorIds);
    return grpcRequest;
  }
}