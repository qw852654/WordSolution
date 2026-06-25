using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Infrastructure.Documents;

public sealed class OutputTemplatePathResolver : IOutputTemplatePathResolver
{
    private readonly string _appBaseDirectory;
    private readonly string _currentDirectory;

    public OutputTemplatePathResolver()
        : this(AppContext.BaseDirectory, Directory.GetCurrentDirectory())
    {
    }

    public OutputTemplatePathResolver(string appBaseDirectory, string currentDirectory)
    {
        if (string.IsNullOrWhiteSpace(appBaseDirectory))
        {
            throw new ArgumentException("App base directory cannot be empty.", nameof(appBaseDirectory));
        }

        if (string.IsNullOrWhiteSpace(currentDirectory))
        {
            throw new ArgumentException("Current directory cannot be empty.", nameof(currentDirectory));
        }

        _appBaseDirectory = Path.GetFullPath(appBaseDirectory);
        _currentDirectory = Path.GetFullPath(currentDirectory);
    }

    public string ResolveTemplateDocxPath(string templateDocxPath)
    {
        if (string.IsNullOrWhiteSpace(templateDocxPath))
        {
            throw new ArgumentException("Template DOCX path cannot be empty.", nameof(templateDocxPath));
        }

        var trimmedPath = templateDocxPath.Trim();
        if (Path.IsPathRooted(trimmedPath))
        {
            return Path.GetFullPath(trimmedPath);
        }

        if (OutputTemplatePaths.IsDefaultTemplatePath(trimmedPath))
        {
            return Path.GetFullPath(
                OutputTemplatePaths.RuntimeDefaultTemplateDocxPath,
                _appBaseDirectory);
        }

        var currentDirectoryPath = Path.GetFullPath(trimmedPath, _currentDirectory);
        if (File.Exists(currentDirectoryPath))
        {
            return currentDirectoryPath;
        }

        return Path.GetFullPath(trimmedPath, _appBaseDirectory);
    }
}
