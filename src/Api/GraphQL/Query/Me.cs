using HotChocolate.Authorization;
using Muzonia.Core.Services.Api;

namespace Muzonia.Api.GraphQL.Query;

public class MeDto
{
    public EntityId Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public Uri? AvatarUri { get; set; }
    public DateTime CreationDate { get; set; }
}

// public partial class Query
// {
//     [UseFirstOrDefault]
//     [UseProjection]
//     [Authorize]
//     public async Task<IQueryable<MeDto>> GetMe(
//         [Service] ApiDbContext dbContext,
//         [Service] UserService userService
//     )
//     {
//         var user = await userService.GetUser();
//
//         return dbContext
//             .Users.Where(u => u.Id == user.Id)
//             .Select(u => new MeDto
//             {
//                 Id = u.Id,
//                 Username = u.UserName!,
//                 AvatarUri = u.AvatarUri,
//                 CreationDate = u.CreationDate,
//                 Email = u.Email!,
//             });
//     }
// }
