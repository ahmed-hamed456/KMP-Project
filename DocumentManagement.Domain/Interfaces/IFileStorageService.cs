namespace DocumentManagement.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> GetFilePathAsync(string fileName);
    Task<bool> FileExistsAsync(string filePath);
    Task<Stream> GetFileStreamAsync(string filePath);
}
