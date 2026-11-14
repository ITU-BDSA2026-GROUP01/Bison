namespace Bison.Web;

public interface IpictureStorage 
{
    // Return the public path or an error message if the file is not accepted 
    Task<(string? Path, string? Error)> SaveAsync(IFormFile file);

}


public class PicturePath : IpictureStorage
{
    private const long MaxBytes = 5 * 1024 * 1024;


    private static readonly Directory<string, string> AllowedTypes = new (StringComparison.OrderIgnoreCase);
    {
        [".jpg"] = "image/jpeg",
        ["jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif"
    }:

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

}