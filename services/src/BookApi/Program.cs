using BookApi;
using BookApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var fileStorageUri = builder.Configuration.GetSection("FileStorageUri").Value ?? throw new ArgumentException("The FileStorageUrl argument is empty");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=book_api.db"));
builder.Services.AddGrpc();
builder.Services.AddGrpcClient<Fss.FileStorageService.FileStorageServiceClient>(o =>
{
    o.Address = new Uri(fileStorageUri);
});

var app = builder.Build();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
if (dbContext.Database.EnsureCreated())
{
    dbContext.Database.Migrate();
}

app.MapGrpcService<BookService>();
app.MapGrpcService<AuthorService>();

app.Run();
