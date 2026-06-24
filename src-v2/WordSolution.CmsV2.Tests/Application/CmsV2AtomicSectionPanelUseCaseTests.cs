using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2AtomicSectionPanelUseCaseTests
{
    [Fact]
    public async Task CreateAtomicSectionPanel_absorbs_matching_unassigned_items_only()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);

        var fallbackPanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Knowledge fallback",
            AtomicSectionTeachingRole.Knowledge,
            Difficulty.Unset,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(fallbackPanel);
        await unitOfWork.SaveChangesAsync();

        var basicBlock = await CreateContentBlockAsync(unitOfWork, "basic", ContentBlockType.KnowledgePoint, Difficulty.Basic);
        var mediumBlock = await CreateContentBlockAsync(unitOfWork, "medium", ContentBlockType.KnowledgePoint, Difficulty.Medium);
        var alreadyAssignedBlock = await CreateContentBlockAsync(unitOfWork, "assigned", ContentBlockType.KnowledgePoint, Difficulty.Basic);
        var basicItem = await CreateAtomicSectionItemAsync(
            unitOfWork,
            atomicSection.Id,
            basicBlock.Id,
            sortOrder: 10,
            teachingRole: AtomicSectionTeachingRole.Knowledge);
        var mediumItem = await CreateAtomicSectionItemAsync(
            unitOfWork,
            atomicSection.Id,
            mediumBlock.Id,
            sortOrder: 20,
            teachingRole: AtomicSectionTeachingRole.Knowledge);
        var alreadyAssignedItem = await CreateAtomicSectionItemAsync(
            unitOfWork,
            atomicSection.Id,
            alreadyAssignedBlock.Id,
            sortOrder: 10,
            teachingRole: AtomicSectionTeachingRole.Knowledge,
            panelId: fallbackPanel.Id);

        var panel = await atomicSections.CreateAtomicSectionPanelAsync(
            new CreateAtomicSectionPanelCommand(
                atomicSection.Id,
                "Knowledge basic",
                AtomicSectionTeachingRole.Knowledge,
                Difficulty.Basic));

        var reloadedBasic = await unitOfWork.AtomicSectionItems.GetByIdAsync(basicItem.Id);
        var reloadedMedium = await unitOfWork.AtomicSectionItems.GetByIdAsync(mediumItem.Id);
        var reloadedAssigned = await unitOfWork.AtomicSectionItems.GetByIdAsync(alreadyAssignedItem.Id);

        Assert.NotNull(reloadedBasic);
        Assert.NotNull(reloadedMedium);
        Assert.NotNull(reloadedAssigned);
        Assert.Equal(panel.Id, reloadedBasic.AtomicSectionPanelId);
        Assert.Null(reloadedMedium.AtomicSectionPanelId);
        Assert.Equal(fallbackPanel.Id, reloadedAssigned.AtomicSectionPanelId);
    }

    [Fact]
    public async Task CreateAtomicSectionPanel_inserts_by_anchor()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);

        var first = await atomicSections.CreateAtomicSectionPanelAsync(
            new CreateAtomicSectionPanelCommand(
                atomicSection.Id,
                "Knowledge",
                AtomicSectionTeachingRole.Knowledge,
                Difficulty.Basic));
        var second = await atomicSections.CreateAtomicSectionPanelAsync(
            new CreateAtomicSectionPanelCommand(
                atomicSection.Id,
                "Practice",
                AtomicSectionTeachingRole.Practice,
                Difficulty.Basic));
        var inserted = await atomicSections.CreateAtomicSectionPanelAsync(
            new CreateAtomicSectionPanelCommand(
                atomicSection.Id,
                "Example",
                AtomicSectionTeachingRole.Example,
                Difficulty.Basic,
                AfterAtomicSectionPanelId: first.Id));
        var beforeFirst = await atomicSections.CreateAtomicSectionPanelAsync(
            new CreateAtomicSectionPanelCommand(
                atomicSection.Id,
                "Homework",
                AtomicSectionTeachingRole.Homework,
                Difficulty.Basic,
                BeforeAtomicSectionPanelId: first.Id));

        var panels = await unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(atomicSection.Id);

        Assert.Equal([beforeFirst.Id, first.Id, inserted.Id, second.Id], panels.Select(panel => panel.Id));
        Assert.Equal([10, 20, 30, 40], panels.Select(panel => panel.SortOrder));
    }

    [Fact]
    public async Task ChangeAtomicSectionItemClassification_updates_content_block_difficulty_and_assigns_matching_panel()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);
        var panel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example medium",
            AtomicSectionTeachingRole.Example,
            Difficulty.Medium,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(panel);
        await unitOfWork.SaveChangesAsync();

        var block = await CreateContentBlockAsync(unitOfWork, "example", ContentBlockType.Question, Difficulty.Unset);
        var item = await CreateAtomicSectionItemAsync(unitOfWork, atomicSection.Id, block.Id, sortOrder: 10);

        await atomicSections.ChangeAtomicSectionItemClassificationAsync(
            new ChangeAtomicSectionItemClassificationCommand(
                atomicSection.Id,
                item.Id,
                AtomicSectionTeachingRole.Example,
                Difficulty.Medium));

        var reloadedItem = await unitOfWork.AtomicSectionItems.GetByIdAsync(item.Id);
        var reloadedBlock = await unitOfWork.ContentBlocks.GetByIdAsync(block.Id);

        Assert.NotNull(reloadedItem);
        Assert.NotNull(reloadedBlock);
        Assert.Equal(panel.Id, reloadedItem.AtomicSectionPanelId);
        Assert.Equal(AtomicSectionTeachingRole.Example, reloadedItem.TeachingRole);
        Assert.Equal(Difficulty.Medium, reloadedBlock.Difficulty);
    }

    [Fact]
    public async Task ChangeContentBlockDifficulty_reassigns_referencing_atomic_section_items()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);
        var basicPanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example basic",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 10);
        var mediumPanel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example medium",
            AtomicSectionTeachingRole.Example,
            Difficulty.Medium,
            sortOrder: 20);
        await unitOfWork.AtomicSectionPanels.AddAsync(basicPanel);
        await unitOfWork.AtomicSectionPanels.AddAsync(mediumPanel);
        await unitOfWork.SaveChangesAsync();

        var block = await CreateContentBlockAsync(unitOfWork, "example", ContentBlockType.Question, Difficulty.Basic);
        var item = await CreateAtomicSectionItemAsync(
            unitOfWork,
            atomicSection.Id,
            block.Id,
            sortOrder: 10,
            teachingRole: AtomicSectionTeachingRole.Example,
            panelId: basicPanel.Id);

        await contentBlocks.ChangeContentBlockDifficultyAsync(
            new ChangeContentBlockDifficultyCommand(block.Id, Difficulty.Medium));

        var reloadedBlock = await unitOfWork.ContentBlocks.GetByIdAsync(block.Id);
        var reloadedItem = await unitOfWork.AtomicSectionItems.GetByIdAsync(item.Id);

        Assert.NotNull(reloadedBlock);
        Assert.NotNull(reloadedItem);
        Assert.Equal(Difficulty.Medium, reloadedBlock.Difficulty);
        Assert.Equal(mediumPanel.Id, reloadedItem.AtomicSectionPanelId);
        Assert.Equal(10, reloadedItem.SortOrder);
    }

    [Fact]
    public async Task ChangeAtomicSectionDifficulty_updates_atomic_section_itself()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);

        await atomicSections.ChangeAtomicSectionDifficultyAsync(
            new ChangeAtomicSectionDifficultyCommand(atomicSection.Id, Difficulty.Top));

        var reloaded = await unitOfWork.AtomicSections.GetByIdAsync(atomicSection.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(Difficulty.Top, reloaded.Difficulty);
    }

    [Fact]
    public async Task MoveAtomicSectionItem_reorders_only_items_in_same_panel_scope()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);
        var panel = new AtomicSectionPanel(
            atomicSection.Id,
            "Practice basic",
            AtomicSectionTeachingRole.Practice,
            Difficulty.Basic,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(panel);
        await unitOfWork.SaveChangesAsync();

        var unassignedBlock = await CreateContentBlockAsync(unitOfWork, "unassigned", ContentBlockType.Question, Difficulty.Unset);
        var panelFirstBlock = await CreateContentBlockAsync(unitOfWork, "panel first", ContentBlockType.Question, Difficulty.Basic);
        var panelSecondBlock = await CreateContentBlockAsync(unitOfWork, "panel second", ContentBlockType.Question, Difficulty.Basic);
        var unassignedItem = await CreateAtomicSectionItemAsync(unitOfWork, atomicSection.Id, unassignedBlock.Id, sortOrder: 10);
        var panelFirstItem = await CreateAtomicSectionItemAsync(
            unitOfWork,
            atomicSection.Id,
            panelFirstBlock.Id,
            sortOrder: 10,
            teachingRole: AtomicSectionTeachingRole.Practice,
            panelId: panel.Id);
        var panelSecondItem = await CreateAtomicSectionItemAsync(
            unitOfWork,
            atomicSection.Id,
            panelSecondBlock.Id,
            sortOrder: 20,
            teachingRole: AtomicSectionTeachingRole.Practice,
            panelId: panel.Id);

        await atomicSections.MoveAtomicSectionItemAsync(
            new MoveAtomicSectionItemCommand(
                atomicSection.Id,
                panelSecondItem.Id,
                AtomicSectionItemMoveDirection.Up));

        var items = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
        var panelItems = items
            .Where(item => item.AtomicSectionPanelId == panel.Id)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();
        var reloadedUnassigned = await unitOfWork.AtomicSectionItems.GetByIdAsync(unassignedItem.Id);

        Assert.Equal([panelSecondItem.Id, panelFirstItem.Id], panelItems.Select(item => item.Id));
        Assert.NotNull(reloadedUnassigned);
        Assert.Equal(10, reloadedUnassigned.SortOrder);
        Assert.Null(reloadedUnassigned.AtomicSectionPanelId);
    }

    [Fact]
    public async Task AddAtomicSectionItem_inserts_by_anchor_inside_panel_scope()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);
        var panel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example basic",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(panel);
        await unitOfWork.SaveChangesAsync();

        var firstBlock = await CreateContentBlockAsync(unitOfWork, "first", ContentBlockType.Question, Difficulty.Basic);
        var secondBlock = await CreateContentBlockAsync(unitOfWork, "second", ContentBlockType.Question, Difficulty.Basic);
        var insertedBlock = await CreateContentBlockAsync(unitOfWork, "inserted", ContentBlockType.Question, Difficulty.Basic);
        var firstItem = await atomicSections.AddAtomicSectionItemAsync(
            new AddAtomicSectionItemCommand(
                atomicSection.Id,
                firstBlock.Id,
                ReferenceMode.FollowLatest,
                null,
                AtomicSectionPanelId: panel.Id,
                TeachingRole: AtomicSectionTeachingRole.Example));
        var secondItem = await atomicSections.AddAtomicSectionItemAsync(
            new AddAtomicSectionItemCommand(
                atomicSection.Id,
                secondBlock.Id,
                ReferenceMode.FollowLatest,
                null,
                AtomicSectionPanelId: panel.Id,
                TeachingRole: AtomicSectionTeachingRole.Example));

        var insertedItem = await atomicSections.AddAtomicSectionItemAsync(
            new AddAtomicSectionItemCommand(
                atomicSection.Id,
                insertedBlock.Id,
                ReferenceMode.FollowLatest,
                null,
                AtomicSectionPanelId: panel.Id,
                TeachingRole: AtomicSectionTeachingRole.Example,
                AfterAtomicSectionItemId: firstItem.Id));

        var items = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
        var panelItems = items
            .Where(item => item.AtomicSectionPanelId == panel.Id)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToList();

        Assert.Equal([firstItem.Id, insertedItem.Id, secondItem.Id], panelItems.Select(item => item.Id));
        Assert.Equal([10, 20, 30], panelItems.Select(item => item.SortOrder));
    }

    [Fact]
    public async Task AddAtomicSectionItem_rejects_anchor_from_different_panel_scope()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);
        var panel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example basic",
            AtomicSectionTeachingRole.Example,
            Difficulty.Basic,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(panel);
        await unitOfWork.SaveChangesAsync();

        var panelBlock = await CreateContentBlockAsync(unitOfWork, "panel", ContentBlockType.Question, Difficulty.Basic);
        var unassignedBlock = await CreateContentBlockAsync(unitOfWork, "unassigned", ContentBlockType.Question, Difficulty.Unset);
        var panelItem = await atomicSections.AddAtomicSectionItemAsync(
            new AddAtomicSectionItemCommand(
                atomicSection.Id,
                panelBlock.Id,
                ReferenceMode.FollowLatest,
                null,
                AtomicSectionPanelId: panel.Id,
                TeachingRole: AtomicSectionTeachingRole.Example));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => atomicSections.AddAtomicSectionItemAsync(
                new AddAtomicSectionItemCommand(
                    atomicSection.Id,
                    unassignedBlock.Id,
                    ReferenceMode.FollowLatest,
                    null,
                    AfterAtomicSectionItemId: panelItem.Id)));
    }

    [Fact]
    public async Task DeleteAtomicSectionPanel_removes_panel_items_without_deleting_content_blocks()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var atomicSections = new AtomicSectionUseCases(unitOfWork);
        var (_, atomicSection) = await CreateAtomicSectionAsync(unitOfWork);
        var panel = new AtomicSectionPanel(
            atomicSection.Id,
            "Variant top",
            AtomicSectionTeachingRole.Variant,
            Difficulty.Top,
            sortOrder: 10);
        await unitOfWork.AtomicSectionPanels.AddAsync(panel);
        await unitOfWork.SaveChangesAsync();

        var panelBlock = await CreateContentBlockAsync(unitOfWork, "panel block", ContentBlockType.Question, Difficulty.Top);
        var unassignedBlock = await CreateContentBlockAsync(unitOfWork, "unassigned", ContentBlockType.Question, Difficulty.Unset);
        var panelItem = await CreateAtomicSectionItemAsync(
            unitOfWork,
            atomicSection.Id,
            panelBlock.Id,
            sortOrder: 10,
            teachingRole: AtomicSectionTeachingRole.Variant,
            panelId: panel.Id);
        var unassignedItem = await CreateAtomicSectionItemAsync(unitOfWork, atomicSection.Id, unassignedBlock.Id, sortOrder: 20);

        var result = await atomicSections.DeleteAtomicSectionPanelAsync(
            new DeleteAtomicSectionPanelCommand(atomicSection.Id, panel.Id));

        Assert.Equal(panel.Id, result.AtomicSectionPanelId);
        Assert.Equal(1, result.RemovedAtomicSectionItemCount);
        Assert.Null(await unitOfWork.AtomicSectionPanels.GetByIdAsync(panel.Id));
        Assert.Null(await unitOfWork.AtomicSectionItems.GetByIdAsync(panelItem.Id));
        Assert.NotNull(await unitOfWork.AtomicSectionItems.GetByIdAsync(unassignedItem.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(panelBlock.Id));
        Assert.NotNull(await unitOfWork.ContentBlocks.GetByIdAsync(unassignedBlock.Id));
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-atomic-section-panel-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static async Task<(int SectionId, AtomicSection AtomicSection)> CreateAtomicSectionAsync(
        EfCmsV2UnitOfWork unitOfWork)
    {
        var topic = new TeachingTopic("topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        var atomicSection = new AtomicSection(section.Id, "atomic", difficulty: Difficulty.Basic);
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        return (section.Id, atomicSection);
    }

    private static async Task<ContentBlock> CreateContentBlockAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string title,
        ContentBlockType blockType,
        Difficulty difficulty)
    {
        var sectionId = (await unitOfWork.Sections.ListAsync()).Single().Id;
        var block = new ContentBlock(sectionId, title, blockType, difficulty: difficulty);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        return block;
    }

    private static async Task<AtomicSectionItem> CreateAtomicSectionItemAsync(
        EfCmsV2UnitOfWork unitOfWork,
        int atomicSectionId,
        int contentBlockId,
        int sortOrder,
        AtomicSectionTeachingRole teachingRole = AtomicSectionTeachingRole.Unclassified,
        int? panelId = null)
    {
        var item = new AtomicSectionItem(
            atomicSectionId,
            contentBlockId,
            ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder,
            atomicSectionPanelId: panelId,
            teachingRole: teachingRole);
        await unitOfWork.AtomicSectionItems.AddAsync(item);
        await unitOfWork.SaveChangesAsync();

        return item;
    }
}
