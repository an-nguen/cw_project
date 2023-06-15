using System.Net.Mime;
using BookApi.Config;
using BookApi.Domains;
using Books;
using Common;
using Fss;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BookApi.Services;

public class BookService : Books.BookService.BookServiceBase
{

    private readonly AppDbContext _dbContext;

    private readonly BookApiOptions _options;

    private readonly FileStorageService.FileStorageServiceClient _fssClient;

    public BookService(AppDbContext context, IOptions<BookApiOptions> options, FileStorageService.FileStorageServiceClient fssClient)
    {
        _dbContext = context;
        _options = options.Value;
        _fssClient = fssClient;
    }

    public override async Task GetBooks(Empty request, IServerStreamWriter<BookReply> responseStream, ServerCallContext context)
    {
        await foreach (var book in _dbContext.Books.Include(b => b.Authors).AsAsyncEnumerable())
            await responseStream.WriteAsync(book.ToBookReply());
    }


    public async Task<Book> GetBookById(string id)
    {
        if (!Guid.TryParse(id, out Guid guid)) throw new RpcException(new Status(StatusCode.InvalidArgument, $"The provided id is not guid."));
        return await _dbContext.Books.FindAsync(guid) ?? throw new RpcException(new Status(StatusCode.NotFound, $"The book with id = {id} not found"));
    }
    public override Task<BookReply> AddBook(BookAddRequest request, ServerCallContext context)
    {
        var newBook = new Book()
        {
            Title = request.Title,
            ISBN = request.Isbn,
        };
        var authors = _dbContext.Authors.Where((a) => request.AuthorIds.Contains(a.Id.ToString()));
        newBook.Authors.AddRange(authors);
        _dbContext.Books.Add(newBook);
        _dbContext.SaveChanges();
        var response = new BookReply()
        {
            Id = newBook.Id.ToString(),
            Title = newBook.Title,
            Isbn = newBook.ISBN,
        };
        response.Authors.AddRange(authors.Select(a => a.ToAuthorMessage()));
        return Task.FromResult(response);
    }

    public override async Task<BookReply> UpdateBook(BookUpdateRequest request, ServerCallContext context)
    {
        var found = await GetBookById(request.Id);

        found.Title = request.Title;
        found.ISBN = request.Isbn;
        found.Authors.Clear();
        found.Authors.AddRange(found.Authors.Where(a => request.AuthorIds.Contains(a.Id.ToString())));
        await _dbContext.SaveChangesAsync();

        return found.ToBookReply();
    }

    public override async Task<Empty> RemoveBook(RemoveRequest request, ServerCallContext context)
    {
        var found = await GetBookById(request.Id);

        _dbContext.Books.Remove(found);
        await _dbContext.SaveChangesAsync();
        return new Empty();
    }

    public override async Task<HasFileReply> HasFile(HasFileRequest request, ServerCallContext context)
    {
        var found = await GetBookById(request.Id);
        return new HasFileReply()
        {
            File = found.FileStorageId == Guid.Empty
        };
    }

    public override async Task<Empty> UploadBookFile(IAsyncStreamReader<UploadBookFileChunkRequest> requestStream, ServerCallContext context)
    {
        await requestStream.MoveNext();
        var it = requestStream.Current;
        var found = await GetBookById(it.Id);

        using var call = _fssClient.Upload();
        do
        {
            it = requestStream.Current;
            await call.RequestStream.WriteAsync(new FileChunkMessage()
            {
                ChunkSize = it.ChunkSize,
                FileSize = it.FileSize,
                FileName = it.Filename,
                Chunk = it.Chunk,
            });
        } while (await requestStream.MoveNext());
        await call.RequestStream.CompleteAsync();
        var reply = await call.ResponseAsync;
        found.FileStorageId = Guid.Parse(reply.Uuid);
        await _dbContext.SaveChangesAsync();
        return new Empty();
    }

    public override async Task DownloadBookFile(DownloadBookFileRequest request, IServerStreamWriter<FileChunkMessage> responseStream, ServerCallContext context)
    {
        var found = await GetBookById(request.Id);

        using var call = _fssClient.Download(new DownloadRequest()
        {
            Uuid = found.FileStorageId.ToString(),
        });

        await foreach (var message in call.ResponseStream.ReadAllAsync())
            await responseStream.WriteAsync(message);
    }
}