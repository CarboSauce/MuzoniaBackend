using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Dto.Response;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    DateTime CreationDate,
    ArtistResponse? Artist,
    bool IsAdmin
)
{
    public static UserResponse From(
        AppUser user,
        ArtistResponse? artist,
        bool isAdmin
    ) =>
        new(
            user.Id,
            user.Name!,
            user.Email!,
            user.CreationDate,
            artist,
            isAdmin
        );
}
