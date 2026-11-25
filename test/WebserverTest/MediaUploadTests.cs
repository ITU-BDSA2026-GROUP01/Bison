using Bison.Core;
using Bison.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace WebserverTest;

public class MediaUploadTests : IDisposable
{
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), "bison-media-" + Guid.NewGuid().ToString("N"));
    private readonly PictureStorage _storage;

    public MediaUploadTests()
    {
        Directory.CreateDirectory(_webRoot);
        _storage = new PictureStorage(new FakeWebHostEnvironment(_webRoot));
    }

    public void Dispose()
    {
        if (Directory.Exists(_webRoot)) Directory.Delete(_webRoot, true);
    }

    private static IFormFile MakeFile(string fileName, string contentType, long length) =>
        new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, length, "Picture", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

    [Theory]
    [InlineData("heron.jpg", "image/jpeg")]
    [InlineData("clip.mp4", "video/mp4")]
    [InlineData("clip.webm", "video/webm")]
    [InlineData("song.mp3", "audio/mpeg")]
    [InlineData("song.m4a", "audio/mp4")]
    [InlineData("song.ogg", "audio/ogg")]
    public async Task AllowedTypes_AreSaved(string name, string contentType)
    {
        var (path, error) = await _storage.SaveAsync(MakeFile(name, contentType, 3));

        Assert.Null(error);
        Assert.StartsWith("/uploads/", path);
        Assert.EndsWith(Path.GetExtension(name), path);
        Assert.True(File.Exists(Path.Combine(_webRoot, "uploads", Path.GetFileName(path!))));
    }

    [Theory]
    [InlineData("big.jpg", "image/jpeg", 5L * 1024 * 1024 + 1)]
    [InlineData("big.mp4", "video/mp4", 150L * 1024 * 1024 + 1)]
    [InlineData("big.mp3", "audio/mpeg", 30L * 1024 * 1024 + 1)]
    public async Task TooBigFiles_AreRejected(string name, string contentType, long length)
    {
        var (path, error) = await _storage.SaveAsync(MakeFile(name, contentType, length));

        Assert.Null(path);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("virus.exe", "application/octet-stream")]
    [InlineData("clip.mp4", "image/png")]          // extension and content type must match
    [InlineData("page.html", "text/html")]
    public async Task OtherTypes_AreRejected(string name, string contentType)
    {
        var (path, error) = await _storage.SaveAsync(MakeFile(name, contentType, 3));

        Assert.Null(path);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task EmptyFile_IsRejected()
    {
        var (path, error) = await _storage.SaveAsync(MakeFile("clip.mp4", "video/mp4", 0));

        Assert.Null(path);
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("/uploads/a.mp4", "video")]
    [InlineData("/uploads/a.webm", "video")]
    [InlineData("/uploads/a.mp3", "audio")]
    [InlineData("/uploads/a.ogg", "audio")]
    [InlineData("/uploads/a.jpg", "image")]
    [InlineData("/uploads/a.gif", "image")]
    [InlineData(null, "image")]
    public void TypeOfMedia_IsDecidedByExtension(string? path, string expected)
    {
        Assert.Equal(expected, TypeOfMedia.For(path));
    }

    [Fact]
    public async Task Delete_RemovesTheFile()
    {
        var (path, _) = await _storage.SaveAsync(MakeFile("clip.mp4", "video/mp4", 3));
        var fullPath = Path.Combine(_webRoot, "uploads", Path.GetFileName(path!));
        Assert.True(File.Exists(fullPath));

        await _storage.DeleteAsync(path);

        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public async Task Delete_NullOrMissingFile_DoesNothing()
    {
        await _storage.DeleteAsync(null);
        await _storage.DeleteAsync("/uploads/does-not-exist.mp4");
    }

    [Theory]
    [InlineData("/etc/passwd")]
    [InlineData("/uploads/../secret.txt")]
    public async Task Delete_InvalidPath_Throws(string path)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.DeleteAsync(path));
    }
}

file class FakeWebHostEnvironment : IWebHostEnvironment
{
    public FakeWebHostEnvironment(string webRoot)
    {
        WebRootPath = webRoot;
        ContentRootPath = webRoot;
    }

    public string WebRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "test";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; } = "Testing";
}