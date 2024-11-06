using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Services;

public interface IFileWriter
{
    Task<Uri?> WriteAsync(IFormFile file, string prefix);
}

public sealed class StaticFileWriter(
    IWebHostEnvironment env,
    IHttpContextAccessor contextAccessor
) : IFileWriter, IScoped
{
    public async Task<Uri?> WriteAsync(IFormFile file, string prefix = "")
    {
        var context = contextAccessor.HttpContext;
        ArgumentNullException.ThrowIfNull(context);
        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var path = Path.Combine(
            env.WebRootPath,
            "muzonia",
            prefix,
            uniqueFileName
        );
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        return new Uri(
            $"{context.Request.Scheme}://{context.Request.Host}/static/{prefix}{uniqueFileName}"
        );
    }
}

public sealed class NoopFileWriter : IFileWriter, ISingleton
{
    public Task<Uri?> WriteAsync(IFormFile file, string prefix = "") =>
        Task.FromResult<Uri?>(
            new($"noop://{prefix}{{file.Name}}_{file.ContentType}")
        );
}
