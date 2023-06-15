using Gateway.Filters;


var builder = WebApplication.CreateBuilder(args);
var bookApiUrl = builder.Configuration.GetSection("BooksApiUri").Value ?? throw new ArgumentException("The BooksApiUri argument is empty.");

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(RpcExceptionFilter));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpcClient<Books.BookService.BookServiceClient>(o =>
{
    o.Address = new Uri(bookApiUrl);
});
builder.Services.AddGrpcClient<Author.AuthorService.AuthorServiceClient>(o =>
{
    o.Address = new Uri(bookApiUrl);
});


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
    policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:5070").AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHsts();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () =>
{
    return Results.Redirect(url: "http://localhost:4200");
});

app.Run();
