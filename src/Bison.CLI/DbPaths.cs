using System;
using System.IO;
using System.Linq;

namespace Bison.CLI;

/// <summary>
/// Resolves database file paths relative to the project's local folder
/// (the directory containing the nearest .csproj) rather than the current
/// working directory.
///
/// Because it is resolved per-process, the same call site behaves correctly
/// in both entry points:
///   • run as the CLI  -> resolves to the Bison.CLI (/src) project folder
///   • invoked by the test project -> resolves to the /test project folder
/// </summary>
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
