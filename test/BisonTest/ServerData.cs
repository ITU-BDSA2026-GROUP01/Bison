namespace test;

/// <summary>
/// Resolves the data folder of the running <c>Bison.CSVDBService</c> instance.
/// The server is started from <c>src/Bison.CSVDBService</c> and reads/writes its
/// CSVs under <c>src/Bison.CSVDBService/data/</c> (relative to its CWD).
///
/// The E2E tests mutate that shared store over HTTP, so they back up these files
/// before running and restore them afterwards (see the tests).
/// </summary>
internal static class ServerData
{
    public static string Folder
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (dir.EnumerateFiles("*.sln").Any())
                    return Path.Combine(dir.FullName, "src", "Bison.CSVDBService", "data");
                dir = dir.Parent;
            }

            throw new InvalidOperationException(
                "Could not locate the solution root, so the server data folder is unknown.");
        }
    }

    public static string Observations => Path.Combine(Folder, "observations.csv");
    public static string Comments => Path.Combine(Folder, "comments.csv");
}
