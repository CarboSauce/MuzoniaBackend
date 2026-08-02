using System.Security.Claims;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Users;

[QueryType]
public static partial class UserQueries
{
    public static IQueryable<AppUser> GetMe(
        ApiDbContext dbContext,
        ClaimsPrincipal user
    )
    {
        var curId = user.UserId;

        return dbContext.Users.Where(u => u.Id == curId);
    }

    public static IQueryable<AppUser> GetUserById(
        EntityId id,
        [Service] ApiDbContext dbContext,
        HttpContext context
    )
    {
        return dbContext.Users.Where(u => u.Id == id);
    }
}
