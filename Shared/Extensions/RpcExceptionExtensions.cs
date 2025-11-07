using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using InvalidOperationException = Shared.Exceptions.InvalidOperationException;

namespace Shared.Extensions;

public static class RpcExceptionExtensions
{
    public static RpcException ToRpcException(this Exception ex, string errorCode = "unknown", string? traceId = null)
    {
        if (ex is RpcException rpc)
        {
            return rpc;
        }

        var status = ex switch
        {
            OperationCanceledException => new Status(StatusCode.Cancelled, "Request was cancelled"),
            TimeoutException => new Status(StatusCode.DeadlineExceeded, "Operation timed out"),
            ArgumentException or FormatException or ValidationException
                => new Status(StatusCode.InvalidArgument, ex.Message),
            KeyNotFoundException => new Status(StatusCode.NotFound, ex.Message),
            InvalidOperationException => new Status(StatusCode.FailedPrecondition, ex.Message),
            DbUpdateConcurrencyException => new Status(StatusCode.Aborted, "Concurrency conflict"),
            DbUpdateException => new Status(StatusCode.Internal, "Database error"),

            _ => new Status(StatusCode.Internal, "Unhandled server error")
        };

        var meta = new Metadata
        {
            { "err-code", errorCode },
        };
        if (!string.IsNullOrWhiteSpace(traceId))
        {
            meta.Add("trace-id", traceId);
        }

        return new RpcException(status, meta);
    }
}