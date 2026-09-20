using System;
using System.IO;

public sealed class TempFile
{
    public string Path { get; }

    private TempFile(string path)
    {
        Path = path;
    }

    public static TempFile Create(string fileName, string contents)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}_{fileName}");
        File.WriteAllText(path, contents);
        return new TempFile(path);
    }
}
