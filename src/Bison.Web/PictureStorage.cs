using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;


namespace Bison.Web;

public interface IPictureStorage 
{
    // Return the public path or an error message if the file is not accepted 
    Task<(string? Path, string? Error)> SaveAsync(IFormFile file);

    Task DeleteAsync(string? picturePath);

}


public class PictureStorage : IPictureStorage
{
    private const long MaxBytes = 5 * 1024 * 1024;


    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif"
    };

    private readonly string _uploadDir;

    public PictureStorage(IWebHostEnvironment env)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _uploadDir = Path.Combine(webRoot, "uploads");
    }
     public async Task<(string? Path, string? Error)> SaveAsync(IFormFile file)
    {
        if (file.Length == 0)
            return (null, "The Dragon Warrior Scroll (empty......)");
        if (file.Length > MaxBytes)
            return (null, "Its too bigg chill out!!!!!! I can only take 5 MB or smaller. plsss");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedTypes.TryGetValue(extension, out var contentType)
            || !string.Equals(file.ContentType, contentType, StringComparison.OrdinalIgnoreCase))
            return (null, "wowowow!!! Hey Im picky I can only take JPG, PNG, GIF or WebP.");

        Directory.CreateDirectory(_uploadDir);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await using var stream = File.Create(Path.Combine(_uploadDir, fileName));
        await file.CopyToAsync(stream);

        return ($"/uploads/{fileName}", null);
    }
    public Task DeleteAsync(string? picturePath)
    {
        if(string.IsNullOrWhiteSpace(picturePath))
        return Task.CompletedTask;
        
        const string uploadPrefix= "/uploads/";
        if(!picturePath.StartsWith(uploadPrefix, StringComparison.Ordinal))
        throw new ArgumentException("Invalid upload path", nameof(picturePath));

        var fileName = picturePath.Substring(uploadPrefix.Length);
        if(Path.GetFileName(fileName)!= fileName)
        throw new ArgumentException("Invalid upload path", nameof(picturePath));

        var filePath = Path.Combine(_uploadDir, fileName);
        if(File.Exists(filePath))
            File.Delete(filePath);
        
        return Task.CompletedTask;
    }

}