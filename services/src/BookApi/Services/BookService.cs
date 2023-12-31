using BookApi.Domains;
using Books;
using Common;
using Fss;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Services;

public class BookService : Books.BookService.BookServiceBase
{

  private readonly AppDbContext _dbContext;

  private readonly FileStorageService.FileStorageServiceClient _fssClient;

  public BookService(AppDbContext context, FileStorageService.FileStorageServiceClient fssClient)
  {
    _dbContext = context;
    _fssClient = fssClient;
  }

  public override async Task GetBooks(Empty request, IServerStreamWriter<BookReply> responseStream, ServerCallContext context)
  {
    await foreach (var book in _dbContext.Books.Include(b => b.Authors).AsAsyncEnumerable())
      await responseStream.WriteAsync(book.ToBookReply());
  }

  public override async Task<BookReply> AddBook(BookAddRequest request, ServerCallContext context)
  {
    var newBook = new Book()
    {
      Title = request.Title,
      ISBN = request.Isbn,
    };
    var authorIds = request.AuthorIds.Distinct().Select(id => id.ToUpper());
    var authors = _dbContext.Authors.Where((a) => authorIds.Contains(a.Id.ToString()));

    newBook.Authors = await authors.ToListAsync();
    _dbContext.Books.Add(newBook);
    await _dbContext.SaveChangesAsync();
    var response = new BookReply()
    {
      Id = newBook.Id.ToString(),
      Title = newBook.Title,
      Isbn = newBook.ISBN,
      HasFile = false,
    };
    response.Authors.AddRange(authors.Select(a => a.ToAuthorMessage()));
    return response;
  }

  public override async Task<BookReply> UpdateBook(BookUpdateRequest request, ServerCallContext context)
  {
    var found = await Utils.GetItemByStringGuid<Book>(_dbContext, request.Id);
    await _dbContext.Entry(found).Collection(b => b.Authors).LoadAsync();

    var authorIds = request.AuthorIds.Distinct().Select(id => id.ToUpper());

    found.Title = request.Title;
    found.ISBN = request.Isbn;

    found.Authors.Clear();
    var authors = _dbContext.Authors.Where((a) => authorIds.Contains(a.Id.ToString()));
    found.Authors = await authors.ToListAsync();

    await _dbContext.SaveChangesAsync();

    return found.ToBookReply();
  }

  public override async Task<Empty> RemoveBook(RemoveRequest request, ServerCallContext context)
  {
    var found = await Utils.GetItemByStringGuid<Book>(_dbContext, request.Id);

    if (found.FileStorageId != Guid.Empty)
    {
      await _fssClient.DeleteEntryAsync(new DeleteEntryRequest()
      {
        Uuid = found.FileStorageId.ToString(),
      });
    }
    _dbContext.Books.Remove(found);
    await _dbContext.SaveChangesAsync();
    return new Empty();
  }

  public override async Task<HasFileReply> HasFile(HasFileRequest request, ServerCallContext context)
  {
    var found = await Utils.GetItemByStringGuid<Book>(_dbContext, request.Id);
    return new HasFileReply()
    {
      File = found.FileStorageId == Guid.Empty
    };
  }

  public override async Task<Empty> UploadBookFile(IAsyncStreamReader<UploadBookFileChunkRequest> requestStream, ServerCallContext context)
  {
    await requestStream.MoveNext();
    var it = requestStream.Current;
    var found = await Utils.GetItemByStringGuid<Book>(_dbContext, it.Id);

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
    var found = await Utils.GetItemByStringGuid<Book>(_dbContext, request.Id);

    using var call = _fssClient.Download(new DownloadRequest()
    {
      Uuid = found.FileStorageId.ToString(),
    });

    await foreach (var message in call.ResponseStream.ReadAllAsync())
      await responseStream.WriteAsync(message);
  }
}