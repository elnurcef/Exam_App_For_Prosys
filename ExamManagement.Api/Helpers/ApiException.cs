namespace ExamManagement.Api.Helpers;

public sealed class ApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public static ApiException BadRequest(string message) => new(message, StatusCodes.Status400BadRequest);
    public static ApiException Unauthorized(string message) => new(message, StatusCodes.Status401Unauthorized);
    public static ApiException Forbidden(string message) => new(message, StatusCodes.Status403Forbidden);
    public static ApiException NotFound(string message) => new(message, StatusCodes.Status404NotFound);
    public static ApiException Conflict(string message) => new(message, StatusCodes.Status409Conflict);
}
