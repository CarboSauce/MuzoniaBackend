using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.User;

public class GetUserInfo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/", Handle)
            .WithName("GetUserInfo")
            .WithDescription("Get user info");
    }

    public record Response(
        EntityId Id,
        string Username,
        string Email,
        DateTime CreationDate,
        Uri? Avatar,
        ArtistResponse? Artist,
        bool IsAdmin
    );

    public static async Task<Results<Ok<Response>, NotFound>> Handle(
        UserService userService,
        ApiDbContext dbContext
    )
    {
        var user = await userService.GetUserInfo();
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        var response = await dbContext
            .Users.Where(u => u.Id == user.Id)
            .Select(u => new Response(
                u.Id,
                u.UserName!,
                u.Email!,
                u.CreationDate,
                u.AvatarUri,
                u.Artist != null
                    ? new ArtistResponse(
                        u.Artist.Id,
                        u.Artist.UserId,
                        u.Artist.Name,
                        u.Artist.Description,
                        u.Artist.ImageUri,
                        u.Artist.CreationDate
                    )
                    : null,
                dbContext.UserRoles.Any(ur =>
                    ur.RoleId == ApiDbContext.AdminRoleId
                    && ur.UserId == user.Id
                )
            ))
            .FirstOrDefaultAsync();

        if (response is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(response);
    }
}
