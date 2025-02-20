using Microsoft.AspNetCore.Http.HttpResults;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.Features.User;

public class DeleteUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/", Handle);

    public static async Task<Results<Ok, BadRequest>> Handle(
        UserService userService
    )
    {
        await userService.DeleteUser();
        return TypedResults.Ok();
    }
}
