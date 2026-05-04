using System.Security.Claims;

namespace ExamManagement.Api.Helpers;

public static class UserClaimsPrincipalExtensions
{
    public static int GetTeacherId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var teacherId)
            ? teacherId
            : throw ApiException.Unauthorized("Invalid authentication token.");
    }
}
