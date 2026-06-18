using WordSolution.CmsV2.Domain.Documents;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed record CreateContentBlockEditSessionCommand(
    string BankRootDirectory,
    int ContentBlockId,
    bool OpenWord);

public sealed record GetContentBlockEditSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record SyncContentBlockEditSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record SyncActiveContentBlockEditSessionsCommand(
    string BankRootDirectory,
    TimeSpan MinimumSessionAge);

public sealed record CancelContentBlockEditSessionCommand(
    string BankRootDirectory,
    string SessionId);

public sealed record SyncContentBlockEditSessionResult(
    string SessionId,
    int ContentBlockId,
    bool Changed,
    int? NewContentBlockVersionId,
    int? CurrentVersionNumber,
    ContentBlockEditSessionStatus Status,
    string? Message = null);
