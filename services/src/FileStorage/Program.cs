using FileStorage;
using FileStorage.Config;
using FileStorage.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection(FileStorageOptions.FileStorage));
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=saved_files.db"));
builder.Services.AddGrpc();


var app = builder.Build();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
if (dbContext.Database.EnsureCreated())
{
  dbContext.Database.Migrate();
}

app.MapGrpcService<StorageService>();
app.Run();
