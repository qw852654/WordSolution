using WordSolution.CmsV2.Domain.Enums;

namespace WordSolution.CmsV2.Application.Tags;

public sealed record CreateTagCommand(string Name, string? Color = null);

public sealed record UpdateTagCommand(int TagId, string? Name = null, string? Color = null);

public sealed record ArchiveTagCommand(int TagId);

public sealed record RestoreTagCommand(int TagId);

public sealed record GetTargetTagsCommand(
    TagBindingTargetType TargetType,
    int TargetId);

public sealed record SetTargetTagsCommand(
    TagBindingTargetType TargetType,
    int TargetId,
    IReadOnlyList<int>? TagIds);
