using WordSolution.CmsV2.Domain.Rules;

namespace WordSolution.CmsV2.Domain.Entities;

public sealed class GeneratedFile
{
    public GeneratedFile(
        int outputFormId,
        string filePath,
        string versionManifestJson,
        DateTimeOffset generatedTime)
    {
        DomainGuard.Positive(outputFormId, nameof(OutputFormId));
        DomainGuard.NotWhiteSpace(filePath, nameof(FilePath));
        DomainGuard.NotWhiteSpace(versionManifestJson, nameof(VersionManifestJson));

        OutputFormId = outputFormId;
        FilePath = filePath.Trim();
        VersionManifestJson = versionManifestJson;
        GeneratedTime = generatedTime;
    }

    public int Id { get; private set; }

    public int OutputFormId { get; private set; }

    public string FilePath { get; private set; }

    public string VersionManifestJson { get; private set; }

    public DateTimeOffset GeneratedTime { get; private set; }
}

