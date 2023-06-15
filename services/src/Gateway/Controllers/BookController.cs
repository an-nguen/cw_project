using Books;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[EnableCors]
[ApiController]
[Route("/books")]
public sealed class BookController : ControllerBase
{
    public const int DefaultChunkSize = 1_048_576; // 1 MiB
    private readonly BookService.BookServiceClient _client;


    public BookController(BookService.BookServiceClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var books = new List<BookReply>();
        using var call = _client.GetBooks(new Empty());
        await foreach (var book in call.ResponseStream.ReadAllAsync())
        {
            books.Add(book);
        }
        return Ok(books);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] BookAddRequest book) => Ok(await _client.AddBookAsync(book));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] BookUpdateRequest request) => Ok(await _client.UpdateBookAsync(request));

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id) => Ok(await _client.RemoveBookAsync(new Common.RemoveRequest() { Id = id.ToString() }));

    [HttpPost]
    [Route("upload/{id:Guid}")]
    public async Task<IActionResult> Upload([FromRoute] Guid id, [FromForm] IFormFile file)
    {

        using var stream = file.OpenReadStream();
        using var call = _client.UploadBookFile();

        var buffer = new byte[DefaultChunkSize];
        int bytesRead = 0;
        while ((bytesRead = stream.Read(buffer, 0, DefaultChunkSize)) > 0)
        {
            await call.RequestStream.WriteAsync(new UploadBookFileChunkRequest()
            {
                Id = id.ToString(),
                Filename = file.FileName,
                FileSize = (int)file.Length,
                ChunkSize = DefaultChunkSize,
                Chunk = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, DefaultChunkSize))
            });
        }
        await call.RequestStream.CompleteAsync();
        var reply = await call;

        return Ok(reply);


    }

    [HttpGet]
    [Route("download/{id:Guid}")]
    public async Task<IActionResult> Download([FromRoute] Guid id)
    {
        using var call = _client.DownloadBookFile(new DownloadBookFileRequest()
        {
            Id = id.ToString()
        });
        await call.ResponseStream.MoveNext();
        var it = call.ResponseStream.Current;

        var file = new byte[it.FileSize];
        var fileName = it.FileName;
        var offset = 0;

        do
        {
            it = call.ResponseStream.Current;
            it.Chunk.CopyTo(file, offset);
            offset += it.ChunkSize;
        } while (await call.ResponseStream.MoveNext());
        return File(file, GetMimeType(fileName), fileName);
    }

    private static string GetMimeType(string fileName) => Path.GetExtension(fileName) switch
    {
        ".pdf" => "application/pdf",
        ".epub" => "application/epub+zip",
        _ => "application/octet-stream"
    };
}