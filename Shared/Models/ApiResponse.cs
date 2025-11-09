namespace Shared.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T? data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return ApiResponse<T>.SuccessResponse(data, message);
    }

    public static ApiResponse<object> Success(string? message = null)
    {
        return ApiResponse<object>.SuccessResponse(null, message ?? "Success");
    }

    public static ApiResponse<T> Error<T>(string message, IEnumerable<string>? errors = null)
    {
        return ApiResponse<T>.ErrorResponse(message, errors);
    }

    public static ApiResponse<object> Error(string message, IEnumerable<string>? errors = null)
    {
        return ApiResponse<object>.ErrorResponse(message, errors);
    }
}