namespace AdReport.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResult(string error, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error }
        };
    }

    public static ApiResponse<T> ErrorResult(List<string> errors, string? message = null)
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
    public static ApiResponse<object> Success(string? message = null)
    {
        return ApiResponse<object>.SuccessResult(new { }, message);
    }

    public static ApiResponse<object> Error(string error, string? message = null)
    {
        return ApiResponse<object>.ErrorResult(error, message);
    }

    public static ApiResponse<object> Error(List<string> errors, string? message = null)
    {
        return ApiResponse<object>.ErrorResult(errors, message);
    }
}