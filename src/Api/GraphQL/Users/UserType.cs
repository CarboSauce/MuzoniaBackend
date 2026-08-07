using System.Security.Claims;
using GreenDonut.Data;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Muzonia.Api.Common;
using Muzonia.Api.GraphQL.Artists;
using Muzonia.Api.GraphQL.Playlists;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Users;

[ObjectType<Profile>]
public static partial class UserType
{
    static partial void Configure(IObjectTypeDescriptor<Profile> descriptor)
    {
        descriptor.Field(u => u.Artist).Ignore();
        descriptor.Field(u => u.Id).ID<Profile>();
    }

    public static async Task<Artist> GetArtistAsync(
        [Parent] Profile user,
        IArtistByUserIdDataLoader artistByUserIdDataLoader,
        ISelection selection,
        CancellationToken cancellationToken
    ) =>
        await artistByUserIdDataLoader
            .Select(selection)
            .LoadRequiredAsync(user.Id, cancellationToken);

    [UsePaging]
    [UseSorting]
    [BindMember(nameof(Profile.Playlists), Replace = true)]
    public static async Task<Page<Playlist>> GetPlaylistsAsync(
        [Parent(requires: nameof(Profile.Id))] Profile user,
        IPlaylistsByUserIdDataLoader dataLoader,
        PagingArguments paging,
        QueryContext<Playlist> query,
        ClaimsPrincipal claims,
        CancellationToken ct
    )
    {
        var curUserId = claims.UserId;
        return await dataLoader
            .With(paging, query)
            .SetState("userId", curUserId)
            .LoadRequiredAsync(user.Id, ct);
    }
}
