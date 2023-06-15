using Books;
namespace BookApi.Domains;
public sealed class Book
{
  public Guid Id { get; set; }
  public required string Title { get; set; }
  public string? ISBN { get; set; }
  public Guid FileStorageId { get; set; } = Guid.Empty;
  public List<Author> Authors { get; set; } = new();
  public BookReply ToBookReply()
  {
    var reply = new BookReply()
    {
      Id = Id.ToString(),
      Title = Title,
      Isbn = ISBN,
      HasFile = FileStorageId != Guid.Empty
    };
    reply.Authors.AddRange(Authors.Select(a => a.ToAuthorMessage()));
    return reply;
  }
}