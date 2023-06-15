using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gateway.Filters;

public class RpcExceptionFilter : IExceptionFilter
{
  private static int GetStatusCodeByRpcException(RpcException e)
  {
    return e.StatusCode switch
    {
      StatusCode.NotFound => (int)HttpStatusCode.BadRequest,
      StatusCode.InvalidArgument => (int)HttpStatusCode.BadRequest,
      _ => (int)HttpStatusCode.InternalServerError,
    };
  }

  public void OnException(ExceptionContext context)
  {
    if (context.Exception is RpcException e)
    {
      context.Result = new ObjectResult(e.Message)
      {
        StatusCode = GetStatusCodeByRpcException(e),
      };
    }
  }
}