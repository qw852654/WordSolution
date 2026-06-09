namespace WordSolution.CmsV2.Application.Handouts;

public sealed record GenerateHandoutWordCommand(
    string BankRootDirectory,
    int OutputFormId,
    DateTimeOffset? GeneratedTime = null);

public sealed record GeneratedHandoutFileResult(
    int GeneratedFileId,
    int OutputFormId,
    int HandoutVersionId,
    string FilePath,
    string VersionManifestJson);
