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
    private const long MaxImageBytes = 5L * 1024 * 1024; // 5 mb picture
    private const long MaxVideoBytes = 150L * 1024 * 1024; // around 8 min of 720p vid
    private const long MaxAudioBytes = 30L * 1024 * 1024; // around 20 min sound

    private static readonly Dictionary<string, (string[] ContentType, long MaxBytes)> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = (new[] { "image/jpeg" }, MaxImageBytes),
        [".jpeg"] = (new[] { "image/jpeg" }, MaxImageBytes),
        [".png"] = (new[] { "image/png" }, MaxImageBytes),
        [".webp"] = (new[] { "image/webp" }, MaxImageBytes),
        [".gif"] = (new[] { "image/gif" }, MaxImageBytes),
        [".mp4"] = (new[] { "video/mp4" }, MaxVideoBytes),
        [".webm"] = (new[] { "video/webm" }, MaxVideoBytes),
        [".mp3"] = (new[] { "audio/mpeg", "audio/mp3" }, MaxAudioBytes),
        [".m4a"] = (new[] { "audio/mp4", "audio/x-m4a", "audio/m4a" }, MaxAudioBytes),
        [".ogg"] = (new[] { "audio/ogg", "application/ogg" }, MaxAudioBytes),

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
        

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedTypes.TryGetValue(extension, out var allowed)
            || !allowed.ContentType.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return (null, "wowowow!!! Hey Im picky I can only take JPG, PNG, GIF, WebP, MP4, WebM, MP3, M4A or OGG.");

        if (file.Length > allowed.MaxBytes)
            return (null, "Its too bigg chill out!!!!!! I can only take 5 MB, video max 150 MB and audio max 30 Mb. plssss");

        
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