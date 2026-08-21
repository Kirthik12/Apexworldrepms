namespace ApexWorld.Core.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public Dictionary<string, string> AuthEndpoints { get; set; } = new Dictionary<string, string>
    {
        { "Login", "/api/v1/auth/login" },
        { "Register", "/api/v1/auth/register-buyer" },
        { "Refresh", "/api/v1/auth/refresh" }
    };

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }
}
