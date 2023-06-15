using System;
using FileStorage.Config;
using Fss;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Options;

namespace FileStorage.Services;

public sealed class StorageService : FileStorageService.FileStorageServiceBase
{
    private const int DefaultBufferSize = 1048576;

    private readonly ILogger<StorageService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly FileStorageOptions _options;

    public StorageService(ILogger<StorageService> logger, IOptions<FileStorageOptions> options, AppDbContext dbContext)
    {
        _logger = logger;
        _options = options.Value;
        _dbContext = dbContext;
    }

    public override async Task<UploadReply> Upload(IAsyncStreamReader<FileChunkMessage> requestStream, ServerCallContext context)
    {
        Guid uuid;
        try
        {
            await requestStream.MoveNext();
            var it = requestStream.Current;
            if (string.IsNullOrEmpty(it.FileName) || it.FileSize <= 0)
                throw new ArgumentException("The filename or filesize is not provided.");

            uuid = Guid.NewGuid();
            var date = new DateTimeOffset();
            var fileExtension = Path.GetExtension(it.FileName);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(it.FileName);
            if (fileNameWithoutExt.Length > 100)
                fileNameWithoutExt = fileNameWithoutExt[..100];
            var fileName = $"[{date.Year}-{date.Month}-{date.Date}] {fileNameWithoutExt}.{fileExtension}";
            var filePath = Path.Combine(_options.StoragePath, fileName);
            await using var writeStream = File.Create(filePath);

            do
            {
                it = requestStream.Current;
                await writeStream.WriteAsync(it.Chunk.Memory);
            } while (await requestStream.MoveNext());
            await _dbContext.FileEntries.AddAsync(new Models.FileEntry()
            {
                Id = uuid,
                FilePath = filePath,
            });
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Upload error");
            return new UploadReply()
            {
                Success = false
            };
        }

        return new UploadReply()
        {
            Success = true,
            Uuid = uuid.ToString()
        };
    }

    public override async Task Download(DownloadRequest request, IServerStreamWriter<FileChunkMessage> responseStream, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Uuid, out Guid id))
            throw new RpcException(new Status(StatusCode.NotFound, $"The provided id = {request.Uuid} is not Guid."));
        var found = await _dbContext.FileEntries.FindAsync(id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"File with id = {id} not found"));

        await using var readStream = File.OpenRead(found.FilePath);

        var fileInfo = new FileInfo(found.FilePath);
        var fileName = Path.GetFileName(found.FilePath);

        var buffer = new byte[DefaultBufferSize];
        int bytesRead = 0;
        while ((bytesRead = await readStream.ReadAsync(buffer.AsMemory(0, DefaultBufferSize))) > 0)
        {
            await responseStream.WriteAsync(new FileChunkMessage()
            {
                FileSize = (int)fileInfo.Length,
                FileName = fileName,
                ChunkSize = bytesRead,
                Chunk = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, bytesRead))
            });
        }

    }
}