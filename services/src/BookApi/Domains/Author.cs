using Author;

namespace BookApi.Domains;

public sealed class Author
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }

    public List<Book> Books { get; set; } = new();

    public AuthorMessage ToAuthorMessage()
    {
        return new AuthorMessage()
        {
            Id = Id.ToString(),
            FirstName = FirstName,
            LastName = LastName,
            MidName = MiddleName,
        };
    }
}