using Microsoft.EntityFrameworkCore;

namespace Muzonia.Api.GraphQL.Users;

public static class UserDataLoaders
{
    [DataLoader]
    public static async Task<
        IReadOnlyDictionary<EntityId, bool>
    > IsAdminByUserIdAsync(
        IReadOnlyList<EntityId> ids,
        ApiDbContext dbContext,
        CancellationToken ct
    )
    {
        var adminIds = await dbContext
            .UserRoles.Where(ur =>
                ur.RoleId == ApiDbContext.AdminRoleId && ids.Contains(ur.UserId)
            )
            .Select(ur => ur.UserId)
            .ToHashSetAsync(ct);

        return ids.ToDictionary(id => id, adminIds.Contains);
    }
}
