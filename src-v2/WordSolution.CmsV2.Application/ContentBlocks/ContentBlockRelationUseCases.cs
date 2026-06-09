using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Application.ContentBlocks;

public sealed class ContentBlockRelationUseCases
{
    private readonly ICmsV2UnitOfWork _unitOfWork;

    public ContentBlockRelationUseCases(ICmsV2UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<CreatedEntityResult> AddContentBlockRelationAsync(
        AddContentBlockRelationCommand command,
        CancellationToken cancellationToken = default)
    {
        CreatedEntityResult? result = null;

        await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
        {
            if (command.ParentBlockId == command.ChildBlockId)
            {
                throw new CmsV2ApplicationException("ContentBlockRelation cannot directly include itself.");
            }

            await RequireContentBlockAsync(command.ParentBlockId, transactionCancellationToken);
            await RequireContentBlockAsync(command.ChildBlockId, transactionCancellationToken);
            await EnsureLockedVersionBelongsToContentBlockAsync(
                command.LockedContentBlockVersionId,
                command.ChildBlockId,
                transactionCancellationToken);

            if (await ReachesBlockAsync(command.ChildBlockId, command.ParentBlockId, [], transactionCancellationToken))
            {
                throw new CmsV2ApplicationException("ContentBlockRelation would create a recursive cycle.");
            }

            var relation = new ContentBlockRelation(
                command.ParentBlockId,
                command.ChildBlockId,
                command.ReferenceMode,
                command.LockedContentBlockVersionId,
                command.SortOrder,
                command.TitleOverride,
                command.Note);

            await _unitOfWork.ContentBlockRelations.AddAsync(relation, transactionCancellationToken);
            await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
            result = new CreatedEntityResult(relation.Id);
        }, cancellationToken);

        return result!;
    }

    private async Task<bool> ReachesBlockAsync(
        int startBlockId,
        int targetBlockId,
        HashSet<int> visited,
        CancellationToken cancellationToken)
    {
        if (!visited.Add(startBlockId))
        {
            return false;
        }

        var children = await _unitOfWork.ContentBlockRelations.ListChildrenAsync(startBlockId, cancellationToken);
        foreach (var child in children)
        {
            if (child.ChildBlockId == targetBlockId)
            {
                return true;
            }

            if (await ReachesBlockAsync(child.ChildBlockId, targetBlockId, visited, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    private async Task RequireContentBlockAsync(int contentBlockId, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.ContentBlocks.GetByIdAsync(contentBlockId, cancellationToken) is null)
        {
            throw new CmsV2ApplicationException($"ContentBlock {contentBlockId} was not found.");
        }
    }

    private async Task EnsureLockedVersionBelongsToContentBlockAsync(
        int? contentBlockVersionId,
        int contentBlockId,
        CancellationToken cancellationToken)
    {
        if (!contentBlockVersionId.HasValue)
        {
            return;
        }

        var version = await _unitOfWork.ContentBlockVersions.GetByIdAsync(contentBlockVersionId.Value, cancellationToken);
        if (version is null || version.ContentBlockId != contentBlockId)
        {
            throw new CmsV2ApplicationException("Locked content block version does not belong to the referenced ContentBlock.");
        }
    }
}
