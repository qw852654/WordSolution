using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;
using WordSolution.CmsV2.Infrastructure.Persistence;

namespace WordSolution.CmsV2.Infrastructure.Repositories;

public sealed class EfTeachingTopicRepository(CmsV2DbContext context)
    : EfRepository<TeachingTopic>(context), ITeachingTopicRepository
{
    public Task<IReadOnlyList<TeachingTopic>> ListChildrenAsync(int? parentId, CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(topic => topic.ParentId == parentId)
                .OrderBy(topic => topic.SortOrder)
                .ThenBy(topic => topic.Id),
            cancellationToken);
    }
}

public sealed class EfSectionRepository(CmsV2DbContext context)
    : EfRepository<Section>(context), ISectionRepository
{
    public Task<IReadOnlyList<Section>> ListByTeachingTopicAsync(
        int teachingTopicId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(section => section.TeachingTopicId == teachingTopicId)
                .OrderBy(section => section.SortOrder)
                .ThenBy(section => section.Id),
            cancellationToken);
    }
}

public sealed class EfSectionItemRepository(CmsV2DbContext context)
    : EfRepository<SectionItem>(context), ISectionItemRepository
{
    public Task<IReadOnlyList<SectionItem>> ListBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.SectionId == sectionId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<SectionItem>> ListByTargetAsync(
        SectionItemTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.TargetType == targetType && item.TargetId == targetId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }
}

public sealed class EfAtomicSectionRepository(CmsV2DbContext context)
    : EfRepository<AtomicSection>(context), IAtomicSectionRepository
{
}

public sealed class EfAtomicSectionPanelRepository(CmsV2DbContext context)
    : EfRepository<AtomicSectionPanel>(context), IAtomicSectionPanelRepository
{
    public Task<IReadOnlyList<AtomicSectionPanel>> ListByAtomicSectionAsync(
        int atomicSectionId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(panel => panel.AtomicSectionId == atomicSectionId)
                .OrderBy(panel => panel.SortOrder)
                .ThenBy(panel => panel.Id),
            cancellationToken);
    }
}

public sealed class EfAtomicSectionItemRepository(CmsV2DbContext context)
    : EfRepository<AtomicSectionItem>(context), IAtomicSectionItemRepository
{
    public Task<IReadOnlyList<AtomicSectionItem>> ListByAtomicSectionAsync(
        int atomicSectionId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.AtomicSectionId == atomicSectionId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<AtomicSectionItem>> ListByContentBlockAsync(
        int contentBlockId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.ContentBlockId == contentBlockId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }
}

public sealed class EfSectionVariantRepository(CmsV2DbContext context)
    : EfRepository<SectionVariant>(context), ISectionVariantRepository
{
    public Task<IReadOnlyList<SectionVariant>> ListBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(variant => variant.SectionId == sectionId)
                .OrderBy(variant => variant.SortOrder)
                .ThenBy(variant => variant.Id),
            cancellationToken);
    }
}

public sealed class EfSectionVariantItemRepository(CmsV2DbContext context)
    : EfRepository<SectionVariantItem>(context), ISectionVariantItemRepository
{
    public Task<IReadOnlyList<SectionVariantItem>> ListBySectionVariantAsync(
        int sectionVariantId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.SectionVariantId == sectionVariantId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<SectionVariantItem>> ListBySectionItemAsync(
        int sectionItemId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.SectionItemId == sectionItemId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }
}

public sealed class EfContentBlockRepository(CmsV2DbContext context)
    : EfRepository<ContentBlock>(context), IContentBlockRepository
{
}

public sealed class EfContentBlockVersionRepository(CmsV2DbContext context)
    : EfRepository<ContentBlockVersion>(context), IContentBlockVersionRepository
{
    public Task<IReadOnlyList<ContentBlockVersion>> ListByContentBlockAsync(
        int contentBlockId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(version => version.ContentBlockId == contentBlockId)
                .OrderBy(version => version.VersionNumber)
                .ThenBy(version => version.Id),
            cancellationToken);
    }

    public async Task<ContentBlockVersion?> GetByContentBlockAndVersionNumberAsync(
        int contentBlockId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        return await Set
            .AsNoTracking()
            .FirstOrDefaultAsync(
                version => version.ContentBlockId == contentBlockId && version.VersionNumber == versionNumber,
                cancellationToken);
    }

    public async Task<ContentBlockVersion?> GetCurrentByContentBlockAsync(
        int contentBlockId,
        CancellationToken cancellationToken = default)
    {
        return await Set
            .AsNoTracking()
            .Where(version => version.ContentBlockId == contentBlockId && version.IsCurrent)
            .OrderByDescending(version => version.VersionNumber)
            .ThenByDescending(version => version.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public sealed class EfContentBlockVersionPartRepository(CmsV2DbContext context)
    : EfRepository<ContentBlockVersionPart>(context), IContentBlockVersionPartRepository
{
    public Task<IReadOnlyList<ContentBlockVersionPart>> ListByContentBlockVersionAsync(
        int contentBlockVersionId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(part => part.ContentBlockVersionId == contentBlockVersionId)
                .OrderBy(part => part.SortOrder)
                .ThenBy(part => part.Id),
            cancellationToken);
    }
}

public sealed class EfContentBlockRelationRepository(CmsV2DbContext context)
    : EfRepository<ContentBlockRelation>(context), IContentBlockRelationRepository
{
    public Task<IReadOnlyList<ContentBlockRelation>> ListChildrenAsync(
        int parentBlockId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(relation => relation.ParentBlockId == parentBlockId)
                .OrderBy(relation => relation.SortOrder)
                .ThenBy(relation => relation.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<ContentBlockRelation>> ListParentsAsync(
        int childBlockId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(relation => relation.ChildBlockId == childBlockId)
                .OrderBy(relation => relation.SortOrder)
                .ThenBy(relation => relation.Id),
            cancellationToken);
    }
}

public sealed class EfHandoutRepository(CmsV2DbContext context)
    : EfRepository<Handout>(context), IHandoutRepository
{
}

public sealed class EfHandoutVersionRepository(CmsV2DbContext context)
    : EfRepository<HandoutVersion>(context), IHandoutVersionRepository
{
    public Task<IReadOnlyList<HandoutVersion>> ListByHandoutAsync(int handoutId, CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(version => version.HandoutId == handoutId)
                .OrderBy(version => version.SortOrder)
                .ThenBy(version => version.Id),
            cancellationToken);
    }
}

public sealed class EfHandoutVersionItemRepository(CmsV2DbContext context)
    : EfRepository<HandoutVersionItem>(context), IHandoutVersionItemRepository
{
    public Task<IReadOnlyList<HandoutVersionItem>> ListByHandoutVersionAsync(
        int handoutVersionId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.HandoutVersionId == handoutVersionId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<HandoutVersionItem>> ListByTargetAsync(
        HandoutVersionItemTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(item => item.TargetType == targetType && item.TargetId == targetId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id),
            cancellationToken);
    }
}

public sealed class EfOutputTemplateRepository(CmsV2DbContext context)
    : EfRepository<OutputTemplate>(context), IOutputTemplateRepository
{
}

public sealed class EfOutputFormRepository(CmsV2DbContext context)
    : EfRepository<OutputForm>(context), IOutputFormRepository
{
    public Task<IReadOnlyList<OutputForm>> ListByHandoutVersionAsync(
        int handoutVersionId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(form => form.HandoutVersionId == handoutVersionId)
                .OrderBy(form => form.SortOrder)
                .ThenBy(form => form.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<OutputForm>> ListByTemplateAsync(
        int outputTemplateId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(form => form.OutputTemplateId == outputTemplateId)
                .OrderBy(form => form.SortOrder)
                .ThenBy(form => form.Id),
            cancellationToken);
    }
}

public sealed class EfGeneratedFileRepository(CmsV2DbContext context)
    : EfRepository<GeneratedFile>(context), IGeneratedFileRepository
{
    public Task<IReadOnlyList<GeneratedFile>> ListByOutputFormAsync(
        int outputFormId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(file => file.OutputFormId == outputFormId)
                .OrderBy(file => file.GeneratedTime)
                .ThenBy(file => file.Id),
            cancellationToken);
    }
}

public sealed class EfTagRepository(CmsV2DbContext context)
    : EfRepository<Tag>(context), ITagRepository
{
    public async Task<Tag?> GetByNormalizedNameAsync(
        string normalizedName,
        CancellationToken cancellationToken = default)
    {
        var normalized = Tag.NormalizeName(normalizedName);

        return await Set
            .AsNoTracking()
            .FirstOrDefaultAsync(tag => tag.NormalizedName == normalized, cancellationToken);
    }

    public Task<IReadOnlyList<Tag>> SearchActiveAsync(
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var query = Set
            .AsNoTracking()
            .Where(tag => tag.Status == TagStatus.Active);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = Tag.NormalizeName(keyword);
            query = query.Where(tag => tag.NormalizedName.Contains(normalizedKeyword));
        }

        return ToReadOnlyListAsync(
            query
                .OrderBy(tag => tag.NormalizedName)
                .ThenBy(tag => tag.Id),
            cancellationToken);
    }
}

public sealed class EfTagBindingRepository(CmsV2DbContext context)
    : EfRepository<TagBinding>(context), ITagBindingRepository
{
    public Task<IReadOnlyList<TagBinding>> ListByTargetAsync(
        TagBindingTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(binding => binding.TargetType == targetType && binding.TargetId == targetId)
                .OrderBy(binding => binding.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<TagBinding>> ListByTagAsync(
        int tagId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(binding => binding.TagId == tagId)
                .OrderBy(binding => binding.Id),
            cancellationToken);
    }
}

public sealed class EfTeachingNoteRepository(CmsV2DbContext context)
    : EfRepository<TeachingNote>(context), ITeachingNoteRepository
{
    public override void Remove(TeachingNote entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.Id > 0)
        {
            Context.Set<TeachingNoteBinding>()
                .RemoveRange(Context.Set<TeachingNoteBinding>().Where(binding => binding.TeachingNoteId == entity.Id));
        }

        base.Remove(entity);
    }

    public Task<IReadOnlyList<TeachingNote>> ListByTargetAsync(
        TeachingNoteBindingTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default)
    {
        var query =
            from note in Set.AsNoTracking()
            join binding in Context.Set<TeachingNoteBinding>().AsNoTracking()
                on note.Id equals binding.TeachingNoteId
            where binding.TargetType == targetType && binding.TargetId == targetId
            select note;

        return ToReadOnlyListAsync(OrderByUpdatedTimeDescending(query), cancellationToken);
    }

    public Task<IReadOnlyList<TeachingNote>> SearchAsync(
        string? keyword,
        TeachingNoteType? noteType,
        TeachingNoteEffectLevel? effectLevel,
        DateTimeOffset? occurredFrom,
        DateTimeOffset? occurredTo,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmedKeyword = keyword.Trim();
            query = query.Where(note => note.Content.Contains(trimmedKeyword));
        }

        if (noteType.HasValue)
        {
            query = query.Where(note => note.NoteType == noteType.Value);
        }

        if (effectLevel.HasValue)
        {
            query = query.Where(note => note.EffectLevel == effectLevel.Value);
        }

        if (occurredFrom.HasValue)
        {
            query = query.Where(note => note.OccurredAt.HasValue && note.OccurredAt.Value >= occurredFrom.Value);
        }

        if (occurredTo.HasValue)
        {
            query = query.Where(note => note.OccurredAt.HasValue && note.OccurredAt.Value <= occurredTo.Value);
        }

        return ToReadOnlyListAsync(OrderByUpdatedTimeDescending(query), cancellationToken);
    }

    private static IOrderedQueryable<TeachingNote> OrderByUpdatedTimeDescending(IQueryable<TeachingNote> query)
    {
        return query
            .OrderByDescending(note => note.UpdatedTime)
            .ThenByDescending(note => note.Id);
    }
}

public sealed class EfTeachingNoteBindingRepository(CmsV2DbContext context)
    : EfRepository<TeachingNoteBinding>(context), ITeachingNoteBindingRepository
{
    public Task<IReadOnlyList<TeachingNoteBinding>> ListByTargetAsync(
        TeachingNoteBindingTargetType targetType,
        int targetId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(binding => binding.TargetType == targetType && binding.TargetId == targetId)
                .OrderBy(binding => binding.Id),
            cancellationToken);
    }

    public Task<IReadOnlyList<TeachingNoteBinding>> ListByTeachingNoteAsync(
        int teachingNoteId,
        CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking()
                .Where(binding => binding.TeachingNoteId == teachingNoteId)
                .OrderBy(binding => binding.Id),
            cancellationToken);
    }
}
