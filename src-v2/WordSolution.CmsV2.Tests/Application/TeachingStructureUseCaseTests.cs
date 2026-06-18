using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.TeachingStructure;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class TeachingStructureUseCaseTests
{
    [Fact]
    public async Task Create_child_and_next_sibling_topics_keep_stable_sibling_order()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new TeachingStructureUseCases(unitOfWork);
        var parent = new TeachingTopic("Parent");
        await unitOfWork.TeachingTopics.AddAsync(parent);
        await unitOfWork.SaveChangesAsync();

        var first = await useCases.CreateChildTopicAsync(new CreateTeachingTopicChildCommand(parent.Id, "A"));
        await useCases.CreateChildTopicAsync(new CreateTeachingTopicChildCommand(parent.Id, "C"));
        await useCases.CreateNextSiblingTopicAsync(new CreateTeachingTopicNextSiblingCommand(first.Id, "B"));

        var children = await unitOfWork.TeachingTopics.ListChildrenAsync(parent.Id);

        Assert.Equal(["A", "B", "C"], children.Select(topic => topic.Name));
        Assert.Equal([10, 20, 30], children.Select(topic => topic.SortOrder));
    }

    [Fact]
    public async Task Rename_and_delete_topic_apply_empty_topic_rules()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new TeachingStructureUseCases(unitOfWork);
        var root = new TeachingTopic("Root");
        var empty = new TeachingTopic("Empty");
        await unitOfWork.TeachingTopics.AddAsync(root);
        await unitOfWork.TeachingTopics.AddAsync(empty);
        await unitOfWork.SaveChangesAsync();
        var child = await useCases.CreateChildTopicAsync(new CreateTeachingTopicChildCommand(root.Id, "Child"));

        var renamed = await useCases.RenameTopicAsync(new RenameTeachingTopicCommand(child.Id, " Renamed child ", " note "));
        await useCases.DeleteTopicAsync(new DeleteTeachingTopicCommand(empty.Id));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.DeleteTopicAsync(new DeleteTeachingTopicCommand(root.Id)));

        await useCases.CreateSectionForTopicAsync(new CreateSectionForTeachingTopicCommand(child.Id));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.DeleteTopicAsync(new DeleteTeachingTopicCommand(child.Id)));

        Assert.Equal("Renamed child", renamed.Name);
        Assert.Equal("note", renamed.Description);
        Assert.Null(await unitOfWork.TeachingTopics.GetByIdAsync(empty.Id));
        Assert.NotNull(await unitOfWork.TeachingTopics.GetByIdAsync(root.Id));
        Assert.NotNull(await unitOfWork.TeachingTopics.GetByIdAsync(child.Id));
    }

    [Fact]
    public async Task Create_section_for_topic_uses_topic_title_and_rejects_duplicate_binding()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new TeachingStructureUseCases(unitOfWork);
        var topic = new TeachingTopic("Energy");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = await useCases.CreateSectionForTopicAsync(new CreateSectionForTeachingTopicCommand(topic.Id));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.CreateSectionForTopicAsync(new CreateSectionForTeachingTopicCommand(topic.Id, "Another Section")));
        var sections = await unitOfWork.Sections.ListByTeachingTopicAsync(topic.Id);

        Assert.Equal("Energy", section.Title);
        Assert.Single(sections);
        Assert.Equal(section.Id, sections[0].Id);
    }

    [Fact]
    public async Task Get_teaching_structure_returns_topic_tree_section_variants_and_ui_flags()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new TeachingStructureUseCases(unitOfWork);
        var root = new TeachingTopic("Energy", sortOrder: 20);
        var empty = new TeachingTopic("Empty", sortOrder: 10);
        await unitOfWork.TeachingTopics.AddAsync(root);
        await unitOfWork.TeachingTopics.AddAsync(empty);
        await unitOfWork.SaveChangesAsync();
        await useCases.CreateChildTopicAsync(new CreateTeachingTopicChildCommand(root.Id, "Circular track"));
        var section = await useCases.CreateSectionForTopicAsync(new CreateSectionForTeachingTopicCommand(root.Id, "Energy section"));
        var variant = new SectionVariant(section.Id, "Basic lecture", sortOrder: 1);
        await unitOfWork.SectionVariants.AddAsync(variant);
        await unitOfWork.SaveChangesAsync();

        var tree = await useCases.GetTeachingStructureAsync();

        Assert.Equal(["Empty", "Energy"], tree.Select(node => node.TeachingTopic.Name));
        Assert.True(tree[0].IsEmptyTopic);
        Assert.True(tree[0].CanDelete);
        Assert.False(tree[0].CanSetDisplayRoot);
        Assert.False(tree[1].IsEmptyTopic);
        Assert.False(tree[1].CanDelete);
        Assert.True(tree[1].CanSetDisplayRoot);
        Assert.NotNull(tree[1].Section);
        Assert.Equal(section.Id, tree[1].Section!.Id);
        Assert.Equal([variant.Id], tree[1].SectionVariants.Select(item => item.Id));
        Assert.Single(tree[1].Children);
        Assert.Equal("Circular track", tree[1].Children[0].TeachingTopic.Name);
        Assert.True(tree[1].Children[0].IsEmptyTopic);
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-teaching-structure-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }
}
