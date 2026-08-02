using System.Security.Claims;

namespace Muzonia.Api.Common;

public static class HttpContextUtilsExt
{
    extension(HttpContext context)
    {
        public bool IsLoggedIn() =>
            context.User.Identity?.IsAuthenticated ?? false;

        public EntityId GetUserId() => context.User.UserId;
    }

    extension(ClaimsPrincipal principal)
    {
        public EntityId UserId
        {
            get
            {
                var stringId = principal.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );
                if (stringId is null)
                    throw new UnauthorizedAccessException();

                return EntityId.Parse(stringId);
            }
        }
    }
}
