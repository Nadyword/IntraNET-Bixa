namespace Bixa.Backend.Models;

public class ApiResponse<T>
{
    private ApiResponse(bool success, string message, T? data, int statusCode)
    {
        Success = success;
        Message = message;
        Data = data ?? default!;
        StatusCode = statusCode;
    }

    public T Data { get; private set; }
    public string Message { get; private set; }
    public int StatusCode { get; set; }
    public bool Success { get; private set; }

    public static ApiResponse<T> BadRequest(T? data, string message, int statusCode = 400)
        => new ApiResponse<T>(false, message, data, statusCode);

    public static ApiResponse<T> ConflictResponse(string message, int statusCode = 409)
        => new ApiResponse<T>(false, message, default, statusCode);

    public static ApiResponse<T> Created(T data, string message = "", int statusCode = 201)
        => new ApiResponse<T>(true, message, data, statusCode);

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 500)
        => new ApiResponse<T>(false, message, default, statusCode);

    public static ApiResponse<T> Forbidden(T data, string message, int statusCode = 403)
        => new ApiResponse<T>(false, message, data, statusCode);

    public static ApiResponse<T> NotFoundResponse(string message, int statusCode = 404)
        => new ApiResponse<T>(false, message, default, statusCode);

    public static ApiResponse<T> SuccessResponse(T data, string message = "", int statusCode = 200)
                                => new ApiResponse<T>(true, message, data, statusCode);

    public static ApiResponse<T> UnauthorizedResponse(string message, int statusCode = 401)
        => new ApiResponse<T>(false, message, default, statusCode);
}