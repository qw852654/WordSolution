using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2HandoutSectionVariantBatchUseCaseTests
{
    [Fact]
    public async Task GetSectionVariantSelectionTreeAsync_returns_topics_sections_and_variants_in_structure_order()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var root = new TeachingTopic("功能关系", sortOrder: 20);
        await unitOfWork.TeachingTopics.AddAsync(root);
        await unitOfWork.SaveChangesAsync();
        var child = new TeachingTopic("机械能守恒", parentId: root.Id, sortOrder: 10);
        await unitOfWork.TeachingTopics.AddAsync(child);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(child.Id, "机械能守恒");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var firstVariant = new SectionVariant(section.Id, "基础讲解版", difficulty: Difficulty.Basic, sortOrder: 20);
        var secondVariant = new SectionVariant(section.Id, "提高训练版", difficulty: Difficulty.Advanced, sortOrder: 10);
        await unitOfWork.SectionVariants.AddAsync(firstVariant);
        await unitOfWork.SectionVariants.AddAsync(secondVariant);
        await unitOfWork.SaveChangesAsync();

        var tree = await handouts.GetSectionVariantSelectionTreeAsync();

        Assert.Single(tree);
        Assert.Equal(root.Id, tree[0].TeachingTopic.Id);
        Assert.Single(tree[0].Children);
        Assert.Equal(child.Id, tree[0].Children[0].TeachingTopic.Id);
        Assert.Single(tree[0].Children[0].Sections);
        Assert.Equal(section.Id, tree[0].Children[0].Sections[0].Section.Id);
        Assert.Equal(
            [secondVariant.Id, firstVariant.Id],
            tree[0].Children[0].Sections[0].SectionVariants.Select(variant => variant.Id));
    }

    [Fact]
    public async Task BatchAddSectionVariantsAsync_inserts_in_tree_order_skips_existing_and_normalizes_sort_order()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var handout = await handouts.CreateHandoutAsync(new CreateHandoutCommand("圆周运动讲义"));
        var version = await handouts.CreateHandoutVersionAsync(new CreateHandoutVersionCommand(handout.Id, "基础班"));
        var firstVariant = await CreateVariantAsync(unitOfWork, "圆周运动", "圆周运动基础", "基础讲解版", topicSortOrder: 10, variantSortOrder: 10);
        var secondVariant = await CreateVariantAsync(unitOfWork, "功能关系", "机械能守恒", "提高训练版", topicSortOrder: 20, variantSortOrder: 10);
        var thirdVariant = await CreateVariantAsync(unitOfWork, "功能关系", "动能定理", "基础练习版", topicSortOrder: 20, variantSortOrder: 20);
        var existing = await handouts.AddHandoutVersionItemAsync(
            new AddHandoutVersionItemCommand(
                version.Id,
                HandoutVersionItemTargetType.SectionVariant,
                secondVariant.Id));

        var result = await handouts.BatchAddSectionVariantsAsync(
            new BatchAddSectionVariantsCommand(
                version.Id,
                [thirdVariant.Id, firstVariant.Id, secondVariant.Id],
                InsertAfterHandoutVersionItemId: existing.Id));
        var items = await unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(version.Id);

        Assert.Equal([secondVariant.Id], result.SkippedExistingVariantIds);
        Assert.Equal(2, result.CreatedItemIds.Count);
        Assert.Equal(
            [existing.Id, .. result.CreatedItemIds],
            items.Select(item => item.Id));
        Assert.Equal(
            [secondVariant.Id, firstVariant.Id, thirdVariant.Id],
            items.Select(item => item.TargetId));
        Assert.Equal([10, 20, 30], items.Select(item => item.SortOrder));
    }

    [Fact]
    public async Task BatchAddSectionVariantsAsync_rejects_duplicate_request_and_archived_variant()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var handout = await handouts.CreateHandoutAsync(new CreateHandoutCommand("专题讲义"));
        var version = await handouts.CreateHandoutVersionAsync(new CreateHandoutVersionCommand(handout.Id, "基础版"));
        var activeVariant = await CreateVariantAsync(unitOfWork, "主题 A", "小节 A", "基础讲解版", 10, 10);
        var archivedVariant = await CreateVariantAsync(
            unitOfWork,
            "主题 B",
            "小节 B",
            "归档版",
            20,
            10,
            SectionVariantStatus.Archived);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.BatchAddSectionVariantsAsync(
                new BatchAddSectionVariantsCommand(version.Id, [activeVariant.Id, activeVariant.Id])));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.BatchAddSectionVariantsAsync(
                new BatchAddSectionVariantsCommand(version.Id, [archivedVariant.Id])));

        Assert.Empty(await unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(version.Id));
    }

    private static async Task<SectionVariant> CreateVariantAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string topicName,
        string sectionTitle,
        string variantTitle,
        int topicSortOrder,
        int variantSortOrder,
        SectionVariantStatus status = SectionVariantStatus.Draft)
    {
        var topic = new TeachingTopic(topicName, sortOrder: topicSortOrder);
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();
        var section = new Section(topic.Id, sectionTitle);
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();
        var variant = new SectionVariant(
            section.Id,
            variantTitle,
            difficulty: Difficulty.Basic,
            status: status,
            sortOrder: variantSortOrder);
        await unitOfWork.SectionVariants.AddAsync(variant);
        await unitOfWork.SaveChangesAsync();

        return variant;
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-handout-section-variant-batch-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }
}
