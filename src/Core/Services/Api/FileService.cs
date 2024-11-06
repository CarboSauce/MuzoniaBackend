using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf;
using Muzonia.DbEf.Entities;

namespace Muzonia.Core.Services.Api;

public class FileService(
    ApiDbContext dbContext,
    CancellationToken cancellationToken
) : ITransient
{
    public async Task<string?> CreateFile(string prefix, IFormFile file)
    {
        var key = $"{prefix}{Guid.NewGuid()}";

        var buffer = await CreateBuffer(file, cancellationToken);

        var newFile = new FileModel
        {
            Data = buffer,
            ContentType = file.ContentType,
            Name = file.FileName,
            Path = key,
        };

        dbContext.Files.Add(newFile);
        await dbContext.SaveChangesAsync(cancellationToken);

        return key;
        static async Task<byte[]> CreateBuffer(
            IFormFile file,
            CancellationToken token
        )
        {
            await using var stream = file.OpenReadStream();

            var bytes = new byte[stream.Length];
            await stream.ReadExactlyAsync(bytes, token);
            return bytes;
        }
    }

    public async Task<bool> DeleteFile(string key)
    {
        var res = await dbContext
            .Files.Where(f => f.Path == key)
            .ExecuteDeleteAsync();

        return res != 0;
    }

    public async Task<FileModel?> GetFile(string key)
    {
        var file = await dbContext
            .Files.Where(f => f.Path == key)
            .FirstOrDefaultAsync(cancellationToken);

        return file;
    }
}
