namespace WordSolution.CmsV2.Application.Handouts;

public sealed record GenerateHandoutWordCommand(
    string BankRootDirectory,
    int OutputFormId,
    DateTimeOffset? GeneratedTime = null);

public sealed record ValidateHandoutWordGenerationCommand(
    string BankRootDirectory,
    int OutputFormId);

public sealed record GeneratedHandoutFileResult(
    int GeneratedFileId,
    int OutputFormId,
    int HandoutVersionId,
    string FilePath,
    string VersionManifestJson);

public sealed record HandoutWordGenerationValidationResult(
    bool IsValid,
    IReadOnlyList<WordSolution.CmsV2.Domain.Documents.HandoutDocumentGenerationIssue> Issues);
