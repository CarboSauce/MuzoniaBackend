using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services;

public interface IDbFileService
{
    Task<FileModel?> GetFile(EntityId id);
    Task<byte[]?> GetFileData(EntityId id);
    Task<FileModel> SaveFile(IFormFile file);
}

public class NoopDbFileService : IDbFileService, IScoped
{
    public Task<FileModel?> GetFile(EntityId id) =>
        Task.FromResult<FileModel?>(null);

    public Task<byte[]?> GetFileData(EntityId id) =>
        Task.FromResult<byte[]?>(null);

    public Task<FileModel> SaveFile(IFormFile file) =>
        Task.FromResult(
            new FileModel
            {
                Id = Guid.NewGuid(),
                Name = "dummy",
                Data = [],
                Path = "dummy",
                ContentType = "dummy",
                Length = 0,
            }
        );
}

public class DbFileService(ApiDbContext dbContext) : IDbFileService, IScoped
{
    public async Task<FileModel?> GetFile(EntityId id)
    {
        var file = await dbContext.Files.FindAsync(id);
        return file;
    }

    public async Task<byte[]?> GetFileData(EntityId id)
    {
        var file = await dbContext
            .Files.Where(f => f.Id == id)
            .Select(f => f.Data)
            .FirstOrDefaultAsync();
        return file;
    }

    public async Task<FileModel> SaveFile(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        var uniqueId = NewId.Create();

        var entity = new FileModel
        {
            Id = uniqueId,
            Name = file.Name,
            Data = stream.GetBuffer(),
            Path = Path.Combine(uniqueId.ToString(), file.FileName),
            ContentType = file.ContentType,
            Length = (int)stream.Length,
        };

        return entity;
    }
}
