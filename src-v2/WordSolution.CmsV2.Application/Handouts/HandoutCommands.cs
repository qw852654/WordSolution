using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.Handouts;

public sealed record CreateHandoutVersionCommand(
    int HandoutId,
    string Title,
    string? Description = null,
    HandoutVersionType Type = HandoutVersionType.Normal,
    HandoutVersionStatus Status = HandoutVersionStatus.Draft,
    int SortOrder = 0);

public sealed record AddHandoutVersionItemCommand(
    int HandoutVersionId,
    HandoutVersionItemTargetType TargetType,
    int TargetId,
    int SortOrder,
    string? TitleOverride = null,
    string? Note = null);
