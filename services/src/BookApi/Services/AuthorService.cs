using Author;
using Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Services;

public sealed class AuthorService : Author.AuthorService.AuthorServiceBase
{
    private readonly AppDbContext _dbContext;

    public AuthorService(AppDbContext context)
    {
        _dbContext = context;
    }

    public override async Task GetAuthors(Empty request, IServerStreamWriter<AuthorMessage> responseStream, ServerCallContext context)
    {
        await foreach (var a in _dbContext.Authors.AsAsyncEnumerable())
        {
            await responseStream.WriteAsync(new AuthorMessage()
            {
                Id = a.Id.ToString(),
                FirstName = a.FirstName,
                LastName = a.LastName,
                MidName = a.MiddleName
            });
        }
    }

    public override async Task<AuthorMessage> AddAuthor(AuthorAddRequest request, ServerCallContext context)
    {
        var newAuthor = new Domains.Author()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MidName,
        };
        await _dbContext.Authors.AddAsync(newAuthor);
        await _dbContext.SaveChangesAsync();
        return newAuthor.ToAuthorMessage();
    }

    public override async Task<AuthorMessage> UpdateAuthor(AuthorMessage request, ServerCallContext context)
    {
        var found = await Utils.GetItemByStringGuid<Domains.Author>(_dbContext, request.Id);
        found.FirstName = request.FirstName;
        found.LastName = request.LastName;
        found.MiddleName = request.MidName;
        await _dbContext.SaveChangesAsync();
        return request;
    }

    public override async Task<Empty> RemoveAuthor(RemoveRequest request, ServerCallContext context)
    {
        var found = await Utils.GetItemByStringGuid<Domains.Author>(_dbContext, request.Id);
        _dbContext.Remove(found);
        await _dbContext.SaveChangesAsync();
        return new Empty();
    }
}