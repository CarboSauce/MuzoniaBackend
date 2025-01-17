using System.Security.Claims;

namespace Muzonia.Api.Common;

public static class HttpContextUtilsExt
{
    public static bool IsLoggedIn(this HttpContext context)
    {
        return context.User.Identity?.IsAuthenticated ?? false;
    }

    public static EntityId GetUserId(this HttpContext context)
    {
        var stringId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (stringId is null)
            throw new UnauthorizedAccessException();

        return EntityId.Parse(stringId);
    }
}
