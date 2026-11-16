namespace EstudioContable.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, Guid estudioId, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType, string FileName)> DownloadFileAsync(string storagePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default);
}
