using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Muzonia.Core.Dto.Request;
using Muzonia.Core.Dto.Response;
using Muzonia.Core.Services;
using Muzonia.Core.Services.Api;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserController(UserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<Results<Ok<UserResponse>, NotFound>> GetUserInfo()
    {
        var user = await userService.GetUserInfo();

        return user is not null
            ? TypedResults.Ok(
                UserResponse.From(
                    user,
                    user.Artist is null ? null : new(user.Artist)
                )
            )
            : TypedResults.NotFound();
    }

    [HttpPut("edit")]
    public async Task<Results<Ok<UserResponse>, BadRequest>> EditUserInfo(
        [FromForm] UserRequest request
    )
    {
        var user = await userService.EditUserInfo(request);

        ArtistResponse? artist = user?.Artist is null ? null : new(user.Artist);

        return user is not null
            ? TypedResults.Ok(UserResponse.From(user, artist))
            : TypedResults.BadRequest();
    }
}
