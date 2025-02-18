using System.IO;

namespace server.Services;

public class FileUploadService
{
    private readonly string _uploadsPath;

    public FileUploadService(IWebHostEnvironment environment)
    {
        _uploadsPath = Path.Combine(environment.ContentRootPath, "uploads", "products");
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

        // Return relative path for storage in database
        return $"/uploads/products/{fileName}";  // This URL will work directly in <img> tags
    }

    public void DeleteProductImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        var fileName = Path.GetFileName(imageUrl);
        var filePath = Path.Combine(_uploadsPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}