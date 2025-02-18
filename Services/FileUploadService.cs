using System.IO;
using Microsoft.Extensions.Configuration;

namespace server.Services;

public class FileUploadService
{
    private readonly string _uploadsPath;
    private readonly string _backendUrl;

    public FileUploadService(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _uploadsPath = Path.Combine(environment.ContentRootPath, "uploads", "products");
        _backendUrl = configuration["AppSettings:BackendUrl"]?.TrimEnd('/') ?? "http://localhost:5000";

        // Ensure directory exists
        if (!Directory.Exists(_uploadsPath))
        {
            Directory.CreateDirectory(_uploadsPath);
        }
    }

    public async Task<string> SaveProductImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was provided");

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(_uploadsPath, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return full URL path for storage in database
        return $"{_backendUrl}/uploads/products/{fileName}";
    }

    public void DeleteProductImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        // Extract filename from full URL
        var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
        var filePath = Path.Combine(_uploadsPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}