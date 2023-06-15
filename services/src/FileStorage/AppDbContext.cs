using FileStorage.Models;
using Microsoft.EntityFrameworkCore;

namespace FileStorage;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<FileEntry> FileEntries => Set<FileEntry>();
}