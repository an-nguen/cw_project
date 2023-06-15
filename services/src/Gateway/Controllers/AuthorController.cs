using Author;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

[EnableCors]
[ApiController]
[Route("/authors")]
public class AuthorController : ControllerBase
{
    private readonly AuthorService.AuthorServiceClient _client;

    public AuthorController(AuthorService.AuthorServiceClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var authors = new List<AuthorMessage>();
        using var call = _client.GetAuthors(new Empty());
        await foreach (var reply in call.ResponseStream.ReadAllAsync())
        {
            authors.Add(reply);
        }
        return Ok(authors);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AuthorAddRequest req) => Ok(await _client.AddAuthorAsync(req));

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] AuthorMessage req) => Ok(await _client.UpdateAuthorAsync(req));

    [HttpDelete("{id:Guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        await _client.RemoveAuthorAsync(new Common.RemoveRequest()
        {
            Id = id.ToString()
        });
        return Ok();
    }
}