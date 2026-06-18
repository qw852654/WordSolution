namespace WordSolution.CmsV2.Domain.Documents;

public enum ContentBlockEditSessionStatus
{
    Created,
    Opening,
    Editing,
    Synced,
    Cancelled,
    Failed
}

public enum ContentBlockEditLaunchMode
{
    LocalShell,
    ExternalUri,
    Cloud,
    None
}

public sealed record ContentBlockEditLaunchResult(
    ContentBlockEditLaunchMode LaunchMode,
    bool OpenedByServer,
    string? Message = null);

public sealed record ContentBlockEditSession(
    string SessionId,
    int ContentBlockId,
    int SourceContentBlockVersionId,
    string EditableDocxPath,
    string OriginalDocxHash,
    ContentBlockEditSessionStatus Status,
    ContentBlockEditLaunchMode LaunchMode,
    bool OpenedByServer,
    string? Message,
    DateTimeOffset CreatedTime,
    DateTimeOffset UpdatedTime);

