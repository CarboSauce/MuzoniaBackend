using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Dto.Response;

public record UserResponse(
    Guid Id,
    string UserName,
    string Email,
    DateTime CreationDate,
    ArtistResponse? Artist,
    Uri? Avatar
)
{
    public static UserResponse From(AppUser user, ArtistResponse? artist) =>
        new(
            user.Id,
            user.UserName!,
            user.Email!,
            user.CreationDate,
            artist,
            user.AvatarUri
        );
}
