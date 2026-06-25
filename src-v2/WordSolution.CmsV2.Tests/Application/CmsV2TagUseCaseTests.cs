using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Tags;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2TagUseCaseTests
{
    [Fact]
    public async Task Tag_use_cases_create_reuse_archive_restore_and_search_active_tags()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var tags = new TagUseCases(unitOfWork);

        var created = await tags.CreateTagAsync(new CreateTagCommand("  Energy  "));
        var duplicate = await tags.CreateTagAsync(new CreateTagCommand("energy"));

        Assert.Equal(created.Id, duplicate.Id);
        Assert.Equal("Energy", created.Name);
        Assert.Equal("energy", created.NormalizedName);
        Assert.Contains(created.Color, TagUseCases.AllowedColorTokens);

        var renamed = await tags.UpdateTagAsync(new UpdateTagCommand(created.Id, "  机械能守恒  "));
        await tags.ArchiveTagAsync(new ArchiveTagCommand(created.Id));
        var activeSearch = await tags.SearchTagsAsync("机械能");
        var archived = await unitOfWork.Tags.GetByIdAsync(created.Id);

        Assert.Equal("机械能守恒", renamed.Name);
        Assert.Empty(activeSearch);
        Assert.Equal(TagStatus.Archived, archived?.Status);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => tags.CreateTagAsync(new CreateTagCommand("机械能守恒")));

        var restored = await tags.RestoreTagAsync(new RestoreTagCommand(created.Id));
        var restoredSearch = await tags.SearchTagsAsync("机械能");

        Assert.Equal(TagStatus.Active, restored.Status);
        Assert.Equal([created.Id], restoredSearch.Select(tag => tag.Id));
    }

    [Fact]
    public async Task Updating_tag_name_rejects_duplicate_normalized_name()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var tags = new TagUseCases(unitOfWork);

        await tags.CreateTagAsync(new CreateTagCommand("Energy"));
        var second = await tags.CreateTagAsync(new CreateTagCommand("动量"));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => tags.UpdateTagAsync(new UpdateTagCommand(second.Id, " energy ")));
    }

    [Fact]
    public async Task Tag_use_cases_create_with_default_or_specified_color_and_update_color()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var tags = new TagUseCases(unitOfWork);

        var defaultColor = await tags.CreateTagAsync(new CreateTagCommand("DefaultColor"));
        var specifiedColor = await tags.CreateTagAsync(new CreateTagCommand("SpecifiedColor", Color: "tag-purple"));
        var specifiedInitialColor = specifiedColor.Color;
        var recolored = await tags.UpdateTagAsync(new UpdateTagCommand(specifiedColor.Id, Color: "tag-red"));
        var recoloredValue = recolored.Color;
        var renamedAndRecolored = await tags.UpdateTagAsync(
            new UpdateTagCommand(specifiedColor.Id, Name: "SpecifiedColorRenamed", Color: "tag-blue"));

        Assert.Equal("tag-gray", defaultColor.Color);
        Assert.Equal("tag-purple", specifiedInitialColor);
        Assert.Equal("tag-red", recoloredValue);
        Assert.Equal("SpecifiedColorRenamed", renamedAndRecolored.Name);
        Assert.Equal("tag-blue", renamedAndRecolored.Color);
    }

    [Fact]
    public async Task Tag_use_cases_reject_invalid_color_tokens()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var tags = new TagUseCases(unitOfWork);
        var tag = await tags.CreateTagAsync(new CreateTagCommand("ValidColor"));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => tags.CreateTagAsync(new CreateTagCommand("InvalidColor", Color: "tag-cyan")));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => tags.UpdateTagAsync(new UpdateTagCommand(tag.Id, Color: "tag-cyan")));
    }

    [Fact]
    public async Task Tag_bindings_replace_single_target_without_touching_other_targets()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var tags = new TagUseCases(unitOfWork);
        var bindings = new TagBindingUseCases(unitOfWork);
        var blockUseCases = new ContentBlockUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var contentBlock = await blockUseCases.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "例题", ContentBlockType.Question));
        var atomicSection = new AtomicSection(sectionId, "原子小节");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();
        var mechanics = await tags.CreateTagAsync(new CreateTagCommand("力学"));
        var energy = await tags.CreateTagAsync(new CreateTagCommand("机械能守恒"));

        await bindings.SetTargetTagsAsync(
            new SetTargetTagsCommand(TagBindingTargetType.ContentBlock, contentBlock.Id, [mechanics.Id, energy.Id, energy.Id]));
        await bindings.SetTargetTagsAsync(
            new SetTargetTagsCommand(TagBindingTargetType.Section, sectionId, [mechanics.Id]));
        await bindings.SetTargetTagsAsync(
            new SetTargetTagsCommand(TagBindingTargetType.AtomicSection, atomicSection.Id, [mechanics.Id]));

        var initialBlockBindings = await bindings.GetTargetTagsAsync(
            new GetTargetTagsCommand(TagBindingTargetType.ContentBlock, contentBlock.Id));
        var atomicBindings = await bindings.GetTargetTagsAsync(
            new GetTargetTagsCommand(TagBindingTargetType.AtomicSection, atomicSection.Id));
        Assert.Equal([mechanics.Id, energy.Id], initialBlockBindings.Select(binding => binding.Tag.Id));
        Assert.Equal([mechanics.Id], atomicBindings.Select(binding => binding.Tag.Id));

        await bindings.SetTargetTagsAsync(
            new SetTargetTagsCommand(TagBindingTargetType.ContentBlock, contentBlock.Id, [energy.Id]));

        var updatedBlockBindings = await bindings.GetTargetTagsAsync(
            new GetTargetTagsCommand(TagBindingTargetType.ContentBlock, contentBlock.Id));
        var sectionBindings = await bindings.GetTargetTagsAsync(
            new GetTargetTagsCommand(TagBindingTargetType.Section, sectionId));

        Assert.Equal([energy.Id], updatedBlockBindings.Select(binding => binding.Tag.Id));
        Assert.Equal([mechanics.Id], sectionBindings.Select(binding => binding.Tag.Id));

        await tags.ArchiveTagAsync(new ArchiveTagCommand(energy.Id));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => bindings.SetTargetTagsAsync(
                new SetTargetTagsCommand(TagBindingTargetType.AtomicSection, atomicSection.Id, [energy.Id])));
    }

    [Fact]
    public async Task Tag_bindings_reject_missing_targets_and_missing_tags()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var tags = new TagUseCases(unitOfWork);
        var bindings = new TagBindingUseCases(unitOfWork);
        var tag = await tags.CreateTagAsync(new CreateTagCommand("力学"));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => bindings.SetTargetTagsAsync(
                new SetTargetTagsCommand(TagBindingTargetType.ContentBlock, 999_999, [tag.Id])));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => bindings.SetTargetTagsAsync(
                new SetTargetTagsCommand(TagBindingTargetType.Section, 999_999, [999_999])));
    }

    [Fact]
    public async Task ContentBlock_list_filters_by_multiple_tag_ids_with_and_semantics()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var tags = new TagUseCases(unitOfWork);
        var bindings = new TagBindingUseCases(unitOfWork);
        var contentBlocks = new ContentBlockUseCases(unitOfWork);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var mechanics = await tags.CreateTagAsync(new CreateTagCommand("力学"));
        var energy = await tags.CreateTagAsync(new CreateTagCommand("机械能守恒"));
        var both = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "同时包含两个标签", ContentBlockType.Question));
        var mechanicsOnly = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "只有力学", ContentBlockType.Question));
        var energyOnly = await contentBlocks.CreateContentBlockAsync(
            new CreateContentBlockCommand(sectionId, "只有机械能", ContentBlockType.Question));

        await bindings.SetTargetTagsAsync(
            new SetTargetTagsCommand(TagBindingTargetType.ContentBlock, both.Id, [mechanics.Id, energy.Id]));
        await bindings.SetTargetTagsAsync(
            new SetTargetTagsCommand(TagBindingTargetType.ContentBlock, mechanicsOnly.Id, [mechanics.Id]));
        await bindings.SetTargetTagsAsync(
            new SetTargetTagsCommand(TagBindingTargetType.ContentBlock, energyOnly.Id, [energy.Id]));

        var singleTag = await contentBlocks.ListContentBlocksAsync(
            new SearchContentBlocksCommand(TagIds: [mechanics.Id]));
        var bothTags = await contentBlocks.ListContentBlocksAsync(
            new SearchContentBlocksCommand(TagIds: [mechanics.Id, energy.Id]));

        Assert.Equal([both.Id, mechanicsOnly.Id], singleTag.Select(block => block.Id));
        Assert.Equal([both.Id], bothTags.Select(block => block.Id));
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-tag-application-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static async Task<int> CreateSectionAsync(EfCmsV2UnitOfWork unitOfWork)
    {
        var topic = new TeachingTopic("默认主题");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "默认 Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        return section.Id;
    }
}
