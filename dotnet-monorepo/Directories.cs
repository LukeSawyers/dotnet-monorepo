namespace DotnetMonorepo;

public static class Directories
{
    /// <summary>
    /// The git repository resolved from the current directory 
    /// </summary>
    public static string? RepositoryRoot { get; } = Environment.CurrentDirectory.Let(d =>
    {
        var rootDirectory = new DirectoryInfo(d);
        while (true)
        {
            if (rootDirectory is null)
            {
                return null;
            }

            if (rootDirectory.EnumerateDirectories().Any(d => d.Name == ".git"))
            {
                break;
            }

            rootDirectory = rootDirectory.Parent;
        }

        return rootDirectory.FullName;
    });

    public static string TempFolder { get; } = Path.Combine(
        Path.GetTempPath(),
        "dotnet-monorepo",
        Guid.NewGuid().ToString()
    ).Apply(d => Directory.CreateDirectory(d));
}