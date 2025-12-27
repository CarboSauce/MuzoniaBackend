using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;

namespace Muzonia.Api.GraphQL.Query;

public class UserDto
{
    public required EntityId Id { get; set; }
    public required string Username { get; set; } = null!;
    public required string? Email { get; set; }
    public required Uri? AvatarUri { get; set; }
    public required DateTime CreationDate { get; set; }
}

[QueryType]
public static class UserQueries
{
    [UseFirstOrDefault]
    [UseProjection]
    [Authorize]
    public static IQueryable<UserDto> GetUserById(
        EntityId id,
        [Service] ApiDbContext dbContext,
        HttpContext context
    )
    {
        var curId = context.GetUserId();

        return dbContext
            .Users.Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.UserName!,
                AvatarUri = u.AvatarUri,
                CreationDate = u.CreationDate,
                Email = u.Id == curId ? u.Email : null,
            });
    }

    [UseFirstOrDefault]
    [UseProjection]
    [Authorize]
    public static IQueryable<UserDto> GetMe(
        [Service] ApiDbContext dbContext,
        HttpContext context
    )
    {
        var curId = context.GetUserId();

        return dbContext
            .Users.Where(u => u.Id == curId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.UserName!,
                AvatarUri = u.AvatarUri,
                CreationDate = u.CreationDate,
                Email = u.Email,
            });
    }
}

[ExtendObjectType(typeof(UserDto))]
public class UserDtoResolvers : ITypeResolver
{
    public async Task<bool> GetIsAdmin(
        [Parent] UserDto user,
        [Service] ApiDbContext dbContext
    ) =>
        await dbContext.UserRoles.AnyAsync(ur =>
            ur.RoleId == ApiDbContext.AdminRoleId && ur.UserId == user.Id
        );
}
