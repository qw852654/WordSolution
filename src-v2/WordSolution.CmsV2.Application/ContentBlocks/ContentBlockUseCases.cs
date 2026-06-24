using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed class ContentBlockUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public ContentBlockUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CreatedEntityResult> CreateContentBlockAsync(
        CreateContentBlockCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireSectionAsync(command.SectionId, transactionCancellationToken);

            var contentBlock = new ContentBlock(
                command.SectionId,
                command.Title,
                command.BlockType,
                command.Summary,
                command.Difficulty,
                command.QuestionType,
                command.Status);

            await _unitOfWork.ContentBlocks.AddAsync(contentBlock, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            result = new CreatedEntityResult(contentBlock.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<CreatedEntityResult> CreateContentBlockWithInitialVersionAsync(
        CreateContentBlockWithInitialVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            await RequireSectionAsync(command.SectionId, transactionCancellationToken);

            var contentBlock = new ContentBlock(
                command.SectionId,
                command.Title,
                command.BlockType,
                command.Summary,
                command.Difficulty,
                command.QuestionType,
                command.Status);

            await _unitOfWork.ContentBlocks.AddAsync(contentBlock, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            var version = new ContentBlockVersion(
                contentBlock.Id,
                versionNumber: 1,
                command.DocxPath,
                command.HtmlPreviewPath,
                command.PlainText,
                isCurrent: true);

            await _unitOfWork.ContentBlockVersions.AddAsync(version, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            contentBlock.SetCurrentVersion(version.Id);
            version.MarkCurrent();
            _unitOfWork.ContentBlocks.Update(contentBlock);
            _unitOfWork.ContentBlockVersions.Update(version);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            result = new CreatedEntityResult(contentBlock.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task<CreatedEntityResult> CreateContentBlockVersionAsync(
        CreateContentBlockVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var contentBlock = await RequireContentBlockAsync(command.ContentBlockId, transactionCancellationToken);
            var existingVersions = await _unitOfWork.ContentBlockVersions.ListByContentBlockAsync(
                command.ContentBlockId,
                transactionCancellationToken);
            var nextVersionNumber = existingVersions.Count == 0
                ? 1
                : existingVersions.Max(version => version.VersionNumber) + 1;

            var version = new ContentBlockVersion(
                command.ContentBlockId,
                nextVersionNumber,
                command.DocxPath,
                command.HtmlPreviewPath,
                command.PlainText,
                isCurrent: false);

            await _unitOfWork.ContentBlockVersions.AddAsync(version, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

            if (command.SetAsCurrent)
            {
                foreach (var existingVersion in existingVersions)
                {
                    existingVersion.MarkNotCurrent();
                    _unitOfWork.ContentBlockVersions.Update(existingVersion);
                }

                version.MarkCurrent();
                contentBlock.SetCurrentVersion(version.Id);
                _unitOfWork.ContentBlocks.Update(contentBlock);
                _unitOfWork.ContentBlockVersions.Update(version);
                await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            }

            result = new CreatedEntityResult(version.Id);
        }, cancellationToken);

        return result!;
    }

    public async Task SetCurrentContentBlockVersionAsync(
        SetCurrentContentBlockVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                await SetCurrentContentBlockVersionCoreAsync(
                    command.ContentBlockId,
                    command.ContentBlockVersionId,
                    transactionCancellationToken);
            },
            cancellationToken);
    }

    public async Task<ContentBlock> ChangeContentBlockDifficultyAsync(
        ChangeContentBlockDifficultyCommand command,
        CancellationToken cancellationToken = default)
    {
        ContentBlock? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            var contentBlock = await RequireContentBlockAsync(command.ContentBlockId, transactionCancellationToken);
            contentBlock.ChangeDifficulty(command.Difficulty);
            _unitOfWork.ContentBlocks.Update(contentBlock);

            await ReassignAtomicSectionItemsForContentBlockAsync(contentBlock, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = contentBlock;
        }, cancellationToken);

        return result!;
    }

    private async Task SetCurrentContentBlockVersionCoreAsync(
        int contentBlockId,
        int contentBlockVersionId,
        CancellationToken cancellationToken)
    {
        var contentBlock = await RequireContentBlockAsync(contentBlockId, cancellationToken);
        var targetVersion = await _unitOfWork.ContentBlockVersions.GetByIdAsync(contentBlockVersionId, cancellationToken);

        if (targetVersion is null)
        {
            throw new CmsV2ApplicationException($"ContentBlockVersion {contentBlockVersionId} was not found.");
        }

        if (targetVersion.ContentBlockId != contentBlockId)
        {
            throw new CmsV2ApplicationException("ContentBlockVersion does not belong to the specified ContentBlock.");
        }

        var versions = await _unitOfWork.ContentBlockVersions.ListByContentBlockAsync(contentBlockId, cancellationToken);
        foreach (var version in versions)
        {
            if (version.Id == contentBlockVersionId)
            {
                version.MarkCurrent();
            }
            else
            {
                version.MarkNotCurrent();
            }

            _unitOfWork.ContentBlockVersions.Update(version);
        }

        contentBlock.SetCurrentVersion(contentBlockVersionId);
        _unitOfWork.ContentBlocks.Update(contentBlock);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ContentBlock> RequireContentBlockAsync(int contentBlockId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken)
            ?? throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
    }

    private async Task RequireSectionAsync(int sectionId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.Sections.GetByIdAsync(sectionId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"Section {sectionId} was not found.");
        }
    }

    private async Task ReassignAtomicSectionItemsForContentBlockAsync(
        ContentBlock contentBlock,
        CancellationToken cancellationToken)
    {
        var items = (await _unitOfWork.AtomicSectionItems.ListByContentBlockAsync(
                contentBlock.Id,
                cancellationToken))
            .OrderBy(item => item.AtomicSectionId)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();
        var affectedScopes = new HashSet<(int AtomicSectionId, int? PanelId)>();

        foreach (var item in items)
        {
            var sourcePanelId = item.AtomicSectionPanelId;
            var targetPanelId = await ResolvePanelIdAsync(
                item.AtomicSectionId,
                item.TeachingRole,
                contentBlock.Difficulty,
                cancellationToken);

            if (sourcePanelId == targetPanelId)
            {
                continue;
            }

            var sortOrder = await NextAtomicSectionItemSortOrderAsync(
                item.AtomicSectionId,
                targetPanelId,
                cancellationToken);
            item.ChangeClassification(targetPanelId, item.TeachingRole, sortOrder);
            _unitOfWork.AtomicSectionItems.Update(item);

            affectedScopes.Add((item.AtomicSectionId, sourcePanelId));
            affectedScopes.Add((item.AtomicSectionId, targetPanelId));
        }

        if (affectedScopes.Count == 0)
        {
            return;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var (atomicSectionId, panelId) in affectedScopes)
        {
            await NormalizeAtomicSectionItemSortOrdersAsync(atomicSectionId, panelId, cancellationToken);
        }
    }

    private async Task<int?> ResolvePanelIdAsync(
        int atomicSectionId,
        AtomicSectionTeachingRole teachingRole,
        Difficulty difficulty,
        CancellationToken cancellationToken)
    {
        if (teachingRole == AtomicSectionTeachingRole.Unclassified)
        {
            return null;
        }

        var panels = (await _unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .OrderBy(panel => panel.SortOrder)
            .ThenBy(panel => panel.Id)
            .ToList();
        var exact = panels.FirstOrDefault(panel =>
            panel.TeachingRole == teachingRole && panel.Difficulty == difficulty);
        if (exact is not null)
        {
            return exact.Id;
        }

        var fallback = panels.FirstOrDefault(panel =>
            panel.TeachingRole == teachingRole && panel.Difficulty == Difficulty.Unset);
        return fallback?.Id;
    }

    private async Task<int> NextAtomicSectionItemSortOrderAsync(
        int atomicSectionId,
        int? panelId,
        CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken);
        var maxSortOrder = items
            .Where(item => item.AtomicSectionPanelId == panelId)
            .Select(item => item.SortOrder)
            .DefaultIfEmpty(0)
            .Max();

        return maxSortOrder + 10;
    }

    private async Task NormalizeAtomicSectionItemSortOrdersAsync(
        int atomicSectionId,
        int? panelId,
        CancellationToken cancellationToken)
    {
        var items = (await _unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSectionId, cancellationToken))
            .Where(item => item.AtomicSectionPanelId == panelId)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        for (var index = 0; index < items.Count; index++)
        {
            items[index].ChangeSortOrder((index + 1) * 10);
            _unitOfWork.AtomicSectionItems.Update(items[index]);
        }
    }
}
