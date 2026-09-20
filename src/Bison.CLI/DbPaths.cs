using System;
using System.IO;
using System.Linq;
using System.Collections.Generic; // For Dictionary - not sure why I need this, but it seems necessary for the _overrides dictionary.


namespace Bison.CLI;

public static class DbPaths
{
    private static readonly string _base = FindLocalFolder();

    /// <summary>Local folder the database files are resolved against.</summary>
    public static string BaseFolder => _base;

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


    // / Overrides for testing purposes. Allows tests to point the database paths to temporary files.
    private static Dictionary<string, string> _overrides = new();

    public static void Override(string name, string path)
    {
        _overrides[name] = path;
    }

    public static void Reset()
    {
        _overrides.Clear();
    }
    // Resolves the full path of a database file, considering any overrides for testing.
    public static string Resolve(string name)
    {
        if (_overrides.TryGetValue(name, out var overridden))
            return overridden;

        return Path.GetFullPath(Path.Combine(_base, name));
    }


}
