using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.Tags;

public sealed record TargetTagBindingDto(
    int Id,
    int TagId,
    TagBindingTargetType TargetType,
    int TargetId,
    Tag Tag);

public sealed class TagUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public TagUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public static IReadOnlyList<string> AllowedColorTokens => Tag.AllowedColorTokens;

    public async Task<Tag> CreateTagAsync(
        CreateTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = Tag.NormalizeName(command.Name);
        var existing = await _unitOfWork.Tags.GetByNormalizedNameAsync(normalizedName, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == TagStatus.Active)
            {
                return existing;
            }

            throw new CmsV2ApplicationException("Archived tag with the same name already exists. Restore it or use another name.");
        }

        var color = NormalizeColorToken(command.Color, useDefault: true);
        var tag = new Tag(command.Name, color);
        await _unitOfWork.Tags.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }

    public async Task<Tag> UpdateTagAsync(
        UpdateTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var tag = await RequireTagAsync(command.TagId, cancellationToken);

        if (command.Name is null && command.Color is null)
        {
            return tag;
        }

        if (command.Name is not null)
        {
            var normalizedName = Tag.NormalizeName(command.Name);
            var existing = await _unitOfWork.Tags.GetByNormalizedNameAsync(normalizedName, cancellationToken);
            if (existing is not null && existing.Id != tag.Id)
            {
                throw new CmsV2ApplicationException("Tag name already exists.");
            }

            tag.Rename(command.Name);
        }

        if (command.Color is not null)
        {
            tag.ChangeColor(NormalizeColorToken(command.Color, useDefault: false));
        }

        _unitOfWork.Tags.Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }

    public async Task<Tag> ArchiveTagAsync(
        ArchiveTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var tag = await RequireTagAsync(command.TagId, cancellationToken);
        tag.Archive();
        _unitOfWork.Tags.Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }

    public async Task<Tag> RestoreTagAsync(
        RestoreTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var tag = await RequireTagAsync(command.TagId, cancellationToken);
        var existing = await _unitOfWork.Tags.GetByNormalizedNameAsync(tag.NormalizedName, cancellationToken);
        if (existing is not null && existing.Id != tag.Id && existing.Status == TagStatus.Active)
        {
            throw new CmsV2ApplicationException("An active tag with the same name already exists.");
        }

        tag.Restore();
        _unitOfWork.Tags.Update(tag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag;
    }

    public Task<IReadOnlyList<Tag>> SearchTagsAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        return _unitOfWork.Tags.SearchActiveAsync(keyword, cancellationToken);
    }

    private async Task<Tag> RequireTagAsync(int tagId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Tags.GetByIdAsync(tagId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"Tag {tagId} was not found.");
    }

    private static string NormalizeColorToken(string? color, bool useDefault)
    {
        if (color is null)
        {
            return useDefault ? "tag-gray" : throw new CmsV2ApplicationException("Tag color is required.");
        }

        var normalizedColor = color.Trim();
        if (normalizedColor.Length == 0)
        {
            throw new CmsV2ApplicationException("Tag color is required.");
        }

        if (!Tag.AllowedColorTokens.Contains(normalizedColor, StringComparer.Ordinal))
        {
            throw new CmsV2ApplicationException($"Tag color must be one of: {string.Join(", ", Tag.AllowedColorTokens)}.");
        }

        return normalizedColor;
    }
}

public sealed class TagBindingUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public TagBindingUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IReadOnlyList<TargetTagBindingDto>> GetTargetTagsAsync(
        GetTargetTagsCommand command,
        CancellationToken cancellationToken = default)
    {
        await RequireTargetAsync(command.TargetType, command.TargetId, cancellationToken);

        var bindings = await _unitOfWork.TagBindings.ListByTargetAsync(
            command.TargetType,
            command.TargetId,
            cancellationToken);
        var results = new List<TargetTagBindingDto>(bindings.Count);

        foreach (var binding in bindings)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(binding.TagId, cancellationToken)
                ?? throw new CmsV2ApplicationException($"Tag {binding.TagId} was not found.");
            results.Add(new TargetTagBindingDto(
                binding.Id,
                binding.TagId,
                binding.TargetType,
                binding.TargetId,
                tag));
        }

        return results;
    }

    public async Task<IReadOnlyList<TargetTagBindingDto>> SetTargetTagsAsync(
        SetTargetTagsCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireTargetAsync(command.TargetType, command.TargetId, transactionCancellationToken);
            var tagIds = NormalizeTagIds(command.TagIds);
            var tags = new List<Tag>(tagIds.Count);

            foreach (var tagId in tagIds)
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(tagId, transactionCancellationToken)
                    ?? throw new CmsV2ApplicationException($"Tag {tagId} was not found.");
                if (tag.Status != TagStatus.Active)
                {
                    throw new CmsV2ApplicationException("Archived tags cannot be bound to targets.");
                }

                tags.Add(tag);
            }

            var existing = await _unitOfWork.TagBindings.ListByTargetAsync(
                command.TargetType,
                command.TargetId,
                transactionCancellationToken);
            var targetTagIds = tagIds.ToHashSet();
            var existingTagIds = existing.Select(binding => binding.TagId).ToHashSet();

            foreach (var binding in existing.Where(binding => !targetTagIds.Contains(binding.TagId)))
            {
                _unitOfWork.TagBindings.Remove(binding);
            }

            foreach (var tagId in targetTagIds.Where(tagId => !existingTagIds.Contains(tagId)))
            {
                await _unitOfWork.TagBindings.AddAsync(
                    new TagBinding(tagId, command.TargetType, command.TargetId),
                    transactionCancellationToken);
            }
        }, cancellationToken);

        return await GetTargetTagsAsync(
            new GetTargetTagsCommand(command.TargetType, command.TargetId),
            cancellationToken);
    }

    private static IReadOnlyList<int> NormalizeTagIds(IReadOnlyList<int>? tagIds)
    {
        if (tagIds is null)
        {
            return [];
        }

        var results = new List<int>();
        foreach (var tagId in tagIds)
        {
            if (tagId <= 0)
            {
                throw new CmsV2ApplicationException("TagId must be greater than 0.");
            }

            if (!results.Contains(tagId))
            {
                results.Add(tagId);
            }
        }

        return results;
    }

    private async Task RequireTargetAsync(
        TagBindingTargetType targetType,
        int targetId,
        CancellationToken cancellationToken)
    {
        if (targetId <= 0)
        {
            throw new CmsV2ApplicationException("TargetId must be greater than 0.");
        }

        var exists = targetType switch
        {
            TagBindingTargetType.ContentBlock => await _unitOfWork.ContentBlocks.GetByIdAsync(targetId, cancellationToken) is not null,
            TagBindingTargetType.AtomicSection => await _unitOfWork.AtomicSections.GetByIdAsync(targetId, cancellationToken) is not null,
            TagBindingTargetType.Section => await _unitOfWork.Sections.GetByIdAsync(targetId, cancellationToken) is not null,
            _ => throw new CmsV2ApplicationException("Unsupported tag binding target type.")
        };

        if (!exists)
        {
            throw new CmsV2ApplicationException($"{targetType} target {targetId} was not found.");
        }
    }
}
