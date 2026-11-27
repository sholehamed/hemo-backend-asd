namespace Application.Common.Models;

public class ApiResponse<T>
{
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public T Result { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

    public static ApiResponse<T> Success(T result)
    {
        return new ApiResponse<T>
        {
            ErrorCode = 0,
            ErrorMessage = "",
            Result = result
        };
    }

    public static ApiResponse<T> Fail(int errorCode, string errorMessage)
    {
        return new ApiResponse<T>
        {
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            Result = default
        };
    }
}