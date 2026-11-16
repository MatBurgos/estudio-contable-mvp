using Microsoft.Extensions.Configuration;
using EstudioContable.Application.Common.Interfaces;

namespace EstudioContable.Infrastructure.Services;

/// <summary>
/// Almacenamiento local en disco para desarrollo y bajo costo inicial.
/// Compatible por interfaz para cambiar a Cloudflare R2 / AWS S3 en producción.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _baseStoragePath = configuration["Storage:BasePath"] ?? Path.Combine(AppContext.BaseDirectory, "StorageFiles");
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, Guid estudioId, CancellationToken cancellationToken = default)
    {
        var tenantFolder = Path.Combine(_baseStoragePath, estudioId.ToString());
        if (!Directory.Exists(tenantFolder))
        {
            Directory.CreateDirectory(tenantFolder);
        }

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(tenantFolder, uniqueFileName);

        using (var destStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await fileStream.CopyToAsync(destStream, cancellationToken);
        }

        // Devolvemos el path relativo para portabilidad
        return Path.Combine(estudioId.ToString(), uniqueFileName);
    }

    public Task<(Stream Stream, string ContentType, string FileName)> DownloadFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseStoragePath, storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("El archivo solicitado no existe en el almacenamiento.", storagePath);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var fileName = Path.GetFileName(storagePath);
        return Task.FromResult<(Stream, string, string)>((stream, "application/octet-stream", fileName));
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_baseStoragePath, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }
}
