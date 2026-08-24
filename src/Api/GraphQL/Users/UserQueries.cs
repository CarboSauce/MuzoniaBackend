using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Users;

[QueryType]
public static partial class UserQueries
{
    [UseSingleOrDefault]
    public static IQueryable<AppUser> GetMe(
        ApiDbContext dbContext,
        QueryContext<AppUser> query,
        ClaimsPrincipal user
    )
    {
        var curId = user.UserId;
        return dbContext.Users.With(query).Where(u => u.Id == curId);
    }

    public static IQueryable<AppUser> GetUserById(
        EntityId id,
        [Service] ApiDbContext dbContext,
        HttpContext context
    )
    {
        return dbContext.Users.Where(u => u.Id == id);
    }

    public static async Task<IEnumerable<AppUser>> SearchUsersAsync(
        string text,
        ApiDbContext dbContext,
        QueryContext<AppUser> queryContext,
        CancellationToken ct
    )
    {
        return await dbContext
            .Users.Where(u =>
                EF.Functions.ToTsVector("english", u.Name).Matches(text)
            )
            .With(queryContext)
            .ToListAsync(ct);
    }
}
