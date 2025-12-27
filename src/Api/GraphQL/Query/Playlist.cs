using HotChocolate.Authorization;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Query;

public class PlaylistType : ObjectType<Playlist>
{
    protected override void Configure(
        IObjectTypeDescriptor<Playlist> descriptor
    )
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Description);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.TrackCount);
        descriptor.Field(x => x.User);
        descriptor.Field(x => x.ImageUri);
        descriptor.Field(x => x.IsPublic);
        descriptor.Field(x => x.CreationDate);
    }
}

public class PlaylistTrackType : ObjectType<PlaylistTrack>
{
    protected override void Configure(
        IObjectTypeDescriptor<PlaylistTrack> descriptor
    )
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Index);
        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Track);
    }
}

[ExtendObjectType<UserDto>]
public static class PlaylistUserNode
{
    [UseProjection]
    [Authorize]
    public static IQueryable<Playlist> GetPlaylists(
        [Parent(requires: nameof(UserDto.Id))] UserDto user,
        ApiDbContext dbContext,
        HttpContext context
    )
    {
        var userId = context.GetUserId();

        return dbContext.Playlists.Where(p => p.UserId == userId || p.IsPublic);
    }
}

[ExtendObjectType<Playlist>]
public static class PlaylistNode
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [UsePaging]
    [Authorize]
    public static IQueryable<PlaylistTrack> GetEntries(
        [Parent(requires: nameof(Playlist.Id))] Playlist playlist,
        ApiDbContext dbContext
    )
    {
        return dbContext.PlaylistTracks.Where(p => p.PlaylistId == playlist.Id);
    }
}
