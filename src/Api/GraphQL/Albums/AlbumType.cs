using GreenDonut.Data;
using HotChocolate.Execution;
using Muzonia.Api.GraphQL.Tracks;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Albums;

[ObjectType<Album>]
public static partial class AlbumType
{
    [UsePaging]
    [UseSorting]
    [BindMember(nameof(Album.Tracks))]
    public static async Task<Page<Track>?> GetTracksAsync(
        [Parent(requires: nameof(Album.Id))] Album album,
        ITracksByAlbumIdDataLoader dataLoader,
        PagingArguments paging,
        QueryContext<Track>? query,
        CancellationToken ct
    ) => await dataLoader.With(paging, query).LoadAsync(album.Id, ct);
}
