using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace BookApi;

public static class Utils
{
  public static async Task<TEntity> GetItemByStringGuid<TEntity>(DbContext context, string id) where TEntity : class
  {
    if (!Guid.TryParse(id, out Guid guid))
      throw new RpcException(new Status(StatusCode.InvalidArgument, $"The provided id = {id} is not valid Guid value."));
    var found = await context.Set<TEntity>().FindAsync(guid)
        ?? throw new RpcException(new Status(StatusCode.NotFound, $"The {nameof(TEntity)} with id = {id} not found."));
    return found;
  }
}