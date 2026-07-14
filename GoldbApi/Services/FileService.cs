using SkiaSharp;
using GoldbApi.Data;
using GoldbApi.Models;
using GoldbApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldbApi.Services;

public interface IFileService
{

    Task<Attachment> UploadImageAsync(IFormFile file, string subDirectory);

    Task<string> SaveFileAsync(IFormFile file, string subDirectory);

    void DeleteFile(string fileUrl);

    Task DeleteAttachmentAsync(int attachmentId);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IRepository<Attachment> _attachmentRepository;
    private readonly string _storageRoot;

    public FileService(IWebHostEnvironment environment, IRepository<Attachment> attachmentRepository, IConfiguration configuration)
    {
        _environment = environment;
        _attachmentRepository = attachmentRepository;

        var configPath = configuration["Storage:LocalPath"];
        _storageRoot = !string.IsNullOrEmpty(configPath) 
            ? Path.Combine(configPath, "uploads") 
            : Path.Combine(_environment.ContentRootPath, "wwwroot/uploads");

        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
        }
    }

    public async Task<Attachment> UploadImageAsync(IFormFile file, string subDirectory)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("File is empty");

        var extension = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"{Guid.NewGuid()}";
        var originalFileName = file.FileName;
        var mimeType = file.ContentType;
        var fileSize = file.Length;

        var folderPath = Path.Combine(_storageRoot, subDirectory);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var originalPath = Path.Combine(folderPath, $"{fileName}{extension}");
        using (var stream = new FileStream(originalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (mimeType.StartsWith("image/"))
        {
            await ProcessImageResizing(originalPath, folderPath, fileName, extension);
        }

        var attachment = new Attachment
        {
            FileName = fileName,
            OriginalName = originalFileName,
            Extension = extension,
            MimeType = mimeType,
            FileSize = fileSize,
            FilePath = $"/uploads/{subDirectory}/{fileName}{extension}",
            SubDirectory = subDirectory
        };

        await _attachmentRepository.AddAsync(attachment);
        await _attachmentRepository.SaveChangesAsync();

        return attachment;
    }

    private async Task ProcessImageResizing(string sourcePath, string targetFolder, string fileName, string extension)
    {
        var sizes = new Dictionary<string, int>
        {
            { "800", 800 },
            { "medium", 400 },
            { "small", 200 },
            { "thumb", 100 },
            { "avatar", 80 }
        };

        // SkiaSharp decode logic needs to run synchronously or wrapped in Task.Run if needed, but it's fast enough
        using var originalBitmap = SKBitmap.Decode(sourcePath);
        if (originalBitmap == null) return;

        foreach (var size in sizes)
        {
            var subFolder = Path.Combine(targetFolder, size.Key);
            if (!Directory.Exists(subFolder))
            {
                Directory.CreateDirectory(subFolder);
            }

            int targetWidth = size.Value;
            int targetHeight = (int)((float)targetWidth / originalBitmap.Width * originalBitmap.Height);

            if (originalBitmap.Width <= targetWidth)
            {
                targetWidth = originalBitmap.Width;
                targetHeight = originalBitmap.Height;
            }

            using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(targetWidth, targetHeight), new SKSamplingOptions(SKFilterMode.Linear));
            using var image = SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(GetEncodedImageFormat(extension), 90);
            
            var outputPath = Path.Combine(subFolder, $"{fileName}{extension}");
            using var stream = new FileStream(outputPath, FileMode.Create);
            data.SaveTo(stream);
        }
        await Task.CompletedTask; // Keep signature async as it was before, or you can just remove async if not needed by interface
    }

    private SKEncodedImageFormat GetEncodedImageFormat(string extension)
    {
        return extension.ToLower() switch
        {
            ".png" => SKEncodedImageFormat.Png,
            ".webp" => SKEncodedImageFormat.Webp,
            ".gif" => SKEncodedImageFormat.Gif,
            _ => SKEncodedImageFormat.Jpeg
        };
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subDirectory)
    {
        if (file == null || file.Length == 0) return string.Empty;

        var folderPath = Path.Combine(_storageRoot, subDirectory);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subDirectory}/{fileName}";
    }

    public void DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        var relativePath = fileUrl.Replace("/uploads/", "");
        var filePath = Path.Combine(_storageRoot, relativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public async Task DeleteAttachmentAsync(int attachmentId)
    {
        var attachment = await _attachmentRepository.GetByIdAsync(attachmentId);
        if (attachment == null) return;

        var folderPath = Path.Combine(_storageRoot, attachment.SubDirectory ?? "general");

        var originalFile = Path.Combine(folderPath, $"{attachment.FileName}{attachment.Extension}");
        if (File.Exists(originalFile)) File.Delete(originalFile);

        string[] versions = { "800", "medium", "small", "thumb", "avatar" };
        foreach (var version in versions)
        {
            var versionFile = Path.Combine(folderPath, version, $"{attachment.FileName}{attachment.Extension}");
            if (File.Exists(versionFile)) File.Delete(versionFile);
        }

        _attachmentRepository.Delete(attachment);
        await _attachmentRepository.SaveChangesAsync();
    }
}
