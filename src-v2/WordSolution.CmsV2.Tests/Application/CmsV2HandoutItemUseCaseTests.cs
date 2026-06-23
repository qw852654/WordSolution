using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2HandoutItemUseCaseTests
{
    [Fact]
    public async Task AddHandoutVersionItemAsync_allows_atomic_section_and_rejects_section_target()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var handoutVersionId = await CreateHandoutVersionAsync(unitOfWork, handouts);

        var atomicSection = new AtomicSection(sectionId, "Atomic section");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.AddHandoutVersionItemAsync(
                new AddHandoutVersionItemCommand(
                    handoutVersionId,
                    HandoutVersionItemTargetType.Section,
                    sectionId)));

        var item = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersionId,
                HandoutVersionItemTargetType.AtomicSection,
                atomicSection.Id));

        var persisted = await unitOfWork.HandoutVersionItems.GetByIdAsync(item.Id);

        Assert.NotNull(persisted);
        Assert.Equal(HandoutVersionItemTargetType.AtomicSection, persisted.TargetType);
        Assert.Equal(atomicSection.Id, persisted.TargetId);
    }

    [Fact]
    public async Task AddHandoutVersionItemAsync_inserts_after_existing_item_and_normalizes_sort_order()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var handoutVersionId = await CreateHandoutVersionAsync(unitOfWork, handouts);
        var firstBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "First");
        var insertedBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "Inserted");
        var lastBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "Last");

        var first = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersionId,
                HandoutVersionItemTargetType.ContentBlock,
                firstBlock.Id));
        var last = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersionId,
                HandoutVersionItemTargetType.ContentBlock,
                lastBlock.Id));
        var inserted = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersionId,
                HandoutVersionItemTargetType.ContentBlock,
                insertedBlock.Id,
                AfterHandoutVersionItemId: first.Id));

        var items = await unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(handoutVersionId);

        Assert.Equal([first.Id, inserted.Id, last.Id], items.Select(item => item.Id));
        Assert.Equal([10, 20, 30], items.Select(item => item.SortOrder));
    }

    [Fact]
    public async Task MoveHandoutVersionItemAsync_moves_items_up_and_down()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var handoutVersionId = await CreateHandoutVersionAsync(unitOfWork, handouts);
        var firstBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "First");
        var secondBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "Second");
        var thirdBlock = await CreateContentBlockAsync(unitOfWork, sectionId, "Third");

        var first = await AddContentBlockItemAsync(handouts, handoutVersionId, firstBlock.Id);
        var second = await AddContentBlockItemAsync(handouts, handoutVersionId, secondBlock.Id);
        var third = await AddContentBlockItemAsync(handouts, handoutVersionId, thirdBlock.Id);

        await handouts.MoveHandoutVersionItemAsync(
            new MoveHandoutVersionItemCommand(
                handoutVersionId,
                third.Id,
                HandoutVersionItemMoveDirection.Up));
        await handouts.MoveHandoutVersionItemAsync(
            new MoveHandoutVersionItemCommand(
                handoutVersionId,
                first.Id,
                HandoutVersionItemMoveDirection.Down));

        var items = await unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(handoutVersionId);

        Assert.Equal([third.Id, first.Id, second.Id], items.Select(item => item.Id));
        Assert.Equal([10, 20, 30], items.Select(item => item.SortOrder));
    }

    [Fact]
    public async Task UpdateAndRemoveHandoutVersionItemAsync_updates_metadata_and_removes_reference_only()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var handoutVersionId = await CreateHandoutVersionAsync(unitOfWork, handouts);
        var block = await CreateContentBlockAsync(unitOfWork, sectionId, "Block");
        var item = await AddContentBlockItemAsync(handouts, handoutVersionId, block.Id);

        await handouts.UpdateHandoutVersionItemAsync(
            new UpdateHandoutVersionItemCommand(
                handoutVersionId,
                item.Id,
                " Title override ",
                " Note value "));

        var updated = await unitOfWork.HandoutVersionItems.GetByIdAsync(item.Id);

        Assert.NotNull(updated);
        Assert.Equal("Title override", updated.TitleOverride);
        Assert.Equal("Note value", updated.Note);

        await handouts.RemoveHandoutVersionItemAsync(
            new RemoveHandoutVersionItemCommand(handoutVersionId, item.Id));

        Assert.Null(await unitOfWork.HandoutVersionItems.GetByIdAsync(item.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(block.Id));
    }

    private static Task<CreatedEntityResult> AddContentBlockItemAsync(
        HandoutUseCases handouts,
        int handoutVersionId,
        int contentBlockId)
    {
        return handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                handoutVersionId,
                HandoutVersionItemTargetType.ContentBlock,
                contentBlockId));
    }

    private static async Task<int> CreateHandoutVersionAsync(
        EfCmsV2UnitOfWork unitOfWork,
        HandoutUseCases handouts)
    {
        var handout = new Handout("Handout");
        await unitOfWork.Handouts.AddAsync(handout);
        await unitOfWork.SaveChangesAsync();

        var handoutVersion = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "Handout version"));

        return handoutVersion.Id;
    }

    private static async Task<int> CreateSectionAsync(EfCmsV2UnitOfWork unitOfWork)
    {
        var topic = new TeachingTopic("Topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        return section.Id;
    }

    private static async Task<ContentBlock> CreateContentBlockAsync(
        EfCmsV2UnitOfWork unitOfWork,
        int sectionId,
        string title)
    {
        var block = new ContentBlock(sectionId, title, ContentBlockType.KnowledgePoint);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        return block;
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-handout-item-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }
}
