using WordSolution.CmsV2.Domain.Entities;

namespace WordSolution.CmsV2.Application.Handouts;

public sealed record GetHandoutVersionWorkspaceCommand(int HandoutVersionId);

public sealed record HandoutVersionWorkspaceDto(
    Handout Handout,
    HandoutVersion Version,
    IReadOnlyList<HandoutWorkspaceItemDto> Items,
    IReadOnlyList<OutputForm> OutputForms,
    IReadOnlyList<GeneratedFile> GeneratedFiles);

public sealed record HandoutWorkspaceItemDto(
    string NodeId,
    int HandoutVersionItemId,
    string TargetType,
    int TargetId,
    string Title,
    string? TitleOverride,
    string? Note,
    int SortOrder,
    IReadOnlyList<HandoutWorkspaceNodeDto> Children);

public sealed record HandoutWorkspaceNodeDto(
    string NodeId,
    string NodeKind,
    int SourceId,
    string Title,
    IReadOnlyList<HandoutWorkspaceNodeDto> Children);
