using GoldbApi.Data;
using GoldbApi.Models;
using GoldbApi.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
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

        using var image = await Image.LoadAsync(sourcePath);

        foreach (var size in sizes)
        {
            var subFolder = Path.Combine(targetFolder, size.Key);
            if (!Directory.Exists(subFolder))
            {
                Directory.CreateDirectory(subFolder);
            }

            using var clone = image.Clone(x => x.Resize(new ResizeOptions
            {
                Size = new Size(size.Value, 0),
                Mode = ResizeMode.Max
            }));

            var outputPath = Path.Combine(subFolder, $"{fileName}{extension}");
            await clone.SaveAsync(outputPath);
        }
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
