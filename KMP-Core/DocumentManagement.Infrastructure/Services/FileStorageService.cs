using DocumentManagement.Domain.Interfaces;

namespace DocumentManagement.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _storageBasePath;
    
    public FileStorageService(string? storageBasePath = null)
    {
        // Use provided path or default to AppData/Documents
        var basePath = storageBasePath ?? Path.Combine("AppData", "Documents");
        
        // Convert to absolute path if it's relative
        if (!Path.IsPathRooted(basePath))
        {
            _storageBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath);
        }
        else
        {
            _storageBasePath = basePath;
        }
        
        // Ensure directory exists
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }
    
    public Task<string> GetFilePathAsync(string fileName)
    {
        try
        {
            var fullPath = Path.Combine(_storageBasePath, fileName);
            return Task.FromResult(fullPath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting file path for '{fileName}': {ex.Message}", ex);
        }
    }
    
    public Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            return Task.FromResult(File.Exists(filePath));
        }
        catch (Exception ex)
        {
            throw new Exception($"Error checking file existence at '{filePath}': {ex.Message}", ex);
        }
    }
    
    public Task<Stream> GetFileStreamAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }
            
            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<Stream>(stream);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting file stream for '{filePath}': {ex.Message}", ex);
        }
    }
}
