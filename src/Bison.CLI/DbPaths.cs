using System;
using System.IO;
using System.Linq;

namespace Bison.CLI;

public static class DbPaths
{
    private static readonly string _base = FindLocalFolder();

    /// <summary>Local folder the database files are resolved against.</summary>
    public static string BaseFolder => _base;

    /// <summary>Resolve a database file name to a path under the local folder.</summary>
    public static string Resolve(string file) =>
        Path.GetFullPath(Path.Combine(_base, file));

    private static string FindLocalFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (dir.EnumerateFiles("*.csproj").Any())
                return dir.FullName;
            dir = dir.Parent;
        }

        // No .csproj found (e.g. a published layout) -> fall back to CWD.
        return Environment.CurrentDirectory;
    }
}
