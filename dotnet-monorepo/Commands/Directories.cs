namespace DotnetMonorepo.Commands;

public static class Directories
{
    public static string RepositoryRoot { get; } = AppContext.BaseDirectory.Let(d =>
    {
        var rootDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (true)
        {
            if (rootDirectory is null)
            {
                throw new ArgumentNullException(nameof(rootDirectory));
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