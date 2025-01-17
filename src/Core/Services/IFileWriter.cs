using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Muzonia.Core.Common;

namespace Muzonia.Core.Services;

public interface IFileWriter
{
    Task<Uri?> WriteAsync(IFormFile file, string prefix, string filePrefix);
    Task<Uri?> WriteAsync(
        Stream stream,
        string filename,
        string prefix,
        string filePrefix
    );

    void CreateDirectory(string existingPrefix, string createPrefix);

    Uri Resolve(string filename, string prefix, string filePrefix);
}

public sealed class StaticFileWriter(
    IOptions<ApiConfig> apiConfig,
    IWebHostEnvironment env
) : IFileWriter, IScoped
{
    private ApiConfig config = apiConfig.Value;

    public async Task<Uri?> WriteAsync(
        IFormFile file,
        string prefix,
        string filePrefix
    )
    {
        (var path, var uniqueFileName) = CreatePath(
            file.FileName,
            prefix,
            filePrefix
        );
        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return new Uri(
            $"{config.ApiProtocol}://{config.ApiDomain}:{config.ApiPort}/static/{prefix}{uniqueFileName}"
        );
    }

    private (string path, string uniqueFileName) CreatePath(
        string filename,
        string prefix,
        string filePrefix
    )
    {
        var uniqueFileName = $"{filePrefix}{filename}";
        var path = Path.Combine(
            env.WebRootPath,
            "muzonia",
            prefix,
            uniqueFileName
        );
        return (path, uniqueFileName);
    }

    public async Task<Uri?> WriteAsync(
        Stream stream,
        string filename,
        string prefix,
        string filePrefix
    )
    {
        (var path, var uniqueFileName) = CreatePath(
            filename,
            prefix,
            filePrefix
        );

        await using var fileStream = new FileStream(path, FileMode.Create);
        await stream.CopyToAsync(fileStream);
        return new Uri(
            $"{config.ApiProtocol}://{config.ApiDomain}:{config.ApiPort}/static/{prefix}{uniqueFileName}"
        );
    }

    public void CreateDirectory(string existingPrefix, string createPrefix)
    {
        var path = Path.Combine(
            env.WebRootPath,
            "muzonia",
            existingPrefix,
            createPrefix
        );
        Directory.CreateDirectory(path);
    }

    public Uri Resolve(string filename, string prefix, string filePrefix)
    {
        return new Uri(
            $"{config.ApiProtocol}://{config.ApiDomain}:{config.ApiPort}/static/{prefix}{filePrefix}_{filename}"
        );
    }
}

public sealed class NoopFileWriter : IFileWriter, ISingleton
{
    public Task<Uri?> WriteAsync(
        IFormFile file,
        string prefix,
        string filePrefix
    )
    {
        return Task.FromResult<Uri?>(
            new($"noop://{prefix}_{filePrefix}{file.FileName}")
        );
    }

    public Task<Uri?> WriteAsync(
        Stream stream,
        string filename,
        string prefix,
        string filePrefix
    )
    {
        return Task.FromResult<Uri?>(
            new($"noop://{prefix}_{filePrefix}{filename}")
        );
    }

    public void CreateDirectory(string existingPrefix, string createPrefix) { }

    public Uri Resolve(string filename, string prefix, string filePrefix)
    {
        return new($"noop://{prefix}_{filePrefix}{filename}");
    }
}
