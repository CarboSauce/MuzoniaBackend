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
) : IFileWriter
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
