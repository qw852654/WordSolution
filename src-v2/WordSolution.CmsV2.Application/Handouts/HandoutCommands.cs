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
    int SortOrder = 0,
    string? TitleOverride = null,
    string? Note = null,
    int? AfterHandoutVersionItemId = null);

public enum HandoutVersionItemMoveDirection
{
    Up,
    Down
}

public sealed record MoveHandoutVersionItemCommand(
    int HandoutVersionId,
    int HandoutVersionItemId,
    HandoutVersionItemMoveDirection Direction);

public sealed record UpdateHandoutVersionItemCommand(
    int HandoutVersionId,
    int HandoutVersionItemId,
    string? TitleOverride = null,
    string? Note = null);

public sealed record RemoveHandoutVersionItemCommand(
    int HandoutVersionId,
    int HandoutVersionItemId);
