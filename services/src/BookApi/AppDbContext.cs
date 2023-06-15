using Microsoft.EntityFrameworkCore;

namespace BookApi;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Domains.Author> Authors => Set<Domains.Author>();
    public DbSet<Domains.Book> Books => Set<Domains.Book>();
}