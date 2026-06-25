using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2HandoutManagementUseCaseTests
{
    private const string LegacyDefaultOutputTemplateDocxPath =
        "src-v2/WordSolution.CmsV2.Infrastructure/Documents/Templates/content-block-default.docx";
    private const string DefaultOutputTemplateDocxPath =
        "Documents/Templates/content-block-default.docx";

    [Fact]
    public async Task CreateHandoutAsync_rejects_duplicate_active_title_and_ignores_archived_title()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);

        var created = await handouts.CreateHandoutAsync(
            new CreateHandoutCommand(" 机械能守恒讲义 ", " 基础材料 "));

        var persisted = await unitOfWork.Handouts.GetByIdAsync(created.Id);
        Assert.NotNull(persisted);
        Assert.Equal("机械能守恒讲义", persisted.Title);
        Assert.Equal("基础材料", persisted.Description);
        Assert.Equal(HandoutStatus.Draft, persisted.Status);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.CreateHandoutAsync(new CreateHandoutCommand("机械能守恒讲义")));

        await handouts.UpdateHandoutAsync(
            new UpdateHandoutCommand(created.Id, "机械能守恒讲义", "归档", HandoutStatus.Archived));

        var recreated = await handouts.CreateHandoutAsync(
            new CreateHandoutCommand("机械能守恒讲义"));

        Assert.NotEqual(created.Id, recreated.Id);
    }

    [Fact]
    public async Task UpdateHandoutAsync_rejects_duplicate_title_and_archived_handout_disallows_new_version()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var first = await handouts.CreateHandoutAsync(new CreateHandoutCommand("第一份讲义"));
        var second = await handouts.CreateHandoutAsync(new CreateHandoutCommand("第二份讲义"));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.UpdateHandoutAsync(
                new UpdateHandoutCommand(second.Id, "第一份讲义", null, HandoutStatus.Active)));

        await handouts.UpdateHandoutAsync(
            new UpdateHandoutCommand(first.Id, "第一份讲义", null, HandoutStatus.Archived));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.CreateHandoutVersionAsync(
                new CreateHandoutVersionCommand(first.Id, "基础版")));
    }

    [Fact]
    public async Task CreateHandoutVersionAsync_uses_server_sort_order_and_rejects_duplicate_active_title()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var handout = await handouts.CreateHandoutAsync(new CreateHandoutCommand("圆周运动讲义"));

        var first = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(
                handout.Id,
                "基础班",
                SortOrder: 999));
        var second = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(
                handout.Id,
                "提高班",
                SortOrder: 999));

        var versions = await unitOfWork.HandoutVersions.ListByHandoutAsync(handout.Id);

        Assert.Equal([first.Id, second.Id], versions.Select(version => version.Id));
        Assert.Equal([10, 20], versions.Select(version => version.SortOrder));
        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.CreateHandoutVersionAsync(
                new CreateHandoutVersionCommand(handout.Id, " 基础班 ")));
    }

    [Fact]
    public async Task CreateHandoutVersionAsync_creates_default_word_output_form_with_shared_template()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var handout = await handouts.CreateHandoutAsync(new CreateHandoutCommand("Output form handout"));
        var template = new OutputTemplate("Shared template", LegacyDefaultOutputTemplateDocxPath);
        await unitOfWork.OutputTemplates.AddAsync(template);
        await unitOfWork.SaveChangesAsync();

        var version = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "Default output version"));

        var outputForms = await unitOfWork.OutputForms.ListByHandoutVersionAsync(version.Id);
        var outputForm = Assert.Single(outputForms);
        Assert.Equal(template.Id, outputForm.OutputTemplateId);
        Assert.Equal("课堂 Word", outputForm.Title);
        Assert.Equal(OutputAudience.Student, outputForm.Audience);
        Assert.Equal(OutputFormat.Word, outputForm.OutputFormat);
        Assert.Equal(VisibilityMode.Classroom, outputForm.VisibilityMode);
        Assert.Equal(OutputFormStatus.Active, outputForm.Status);
        Assert.Equal(1, outputForm.SortOrder);
    }

    [Fact]
    public async Task CreateHandoutVersionAsync_reuses_legacy_default_template_without_creating_duplicate()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var handout = await handouts.CreateHandoutAsync(new CreateHandoutCommand("Legacy template handout"));
        var legacyTemplate = new OutputTemplate("Legacy template", LegacyDefaultOutputTemplateDocxPath);
        await unitOfWork.OutputTemplates.AddAsync(legacyTemplate);
        await unitOfWork.SaveChangesAsync();

        var version = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "Default output version"));

        var templates = await unitOfWork.OutputTemplates.ListAsync();
        var outputForm = Assert.Single(await unitOfWork.OutputForms.ListByHandoutVersionAsync(version.Id));
        Assert.Single(templates);
        Assert.Equal(legacyTemplate.Id, outputForm.OutputTemplateId);
    }

    [Fact]
    public async Task CreateHandoutVersionAsync_creates_default_template_and_output_form_when_template_is_missing()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var handout = await handouts.CreateHandoutAsync(new CreateHandoutCommand("Missing template handout"));

        var version = await handouts.CreateHandoutVersionAsync(
            new CreateHandoutVersionCommand(handout.Id, "Default output version"));

        var templates = await unitOfWork.OutputTemplates.ListAsync();
        var template = Assert.Single(templates);
        Assert.Equal("默认 Word 模板", template.Title);
        Assert.Equal(DefaultOutputTemplateDocxPath, template.TemplateDocxPath);
        Assert.Equal(OutputTemplateStatus.Active, template.Status);

        var outputForms = await unitOfWork.OutputForms.ListByHandoutVersionAsync(version.Id);
        var outputForm = Assert.Single(outputForms);
        Assert.Equal(template.Id, outputForm.OutputTemplateId);
        Assert.Equal("课堂 Word", outputForm.Title);
        Assert.Equal(OutputAudience.Student, outputForm.Audience);
        Assert.Equal(OutputFormat.Word, outputForm.OutputFormat);
        Assert.Equal(VisibilityMode.Classroom, outputForm.VisibilityMode);
        Assert.Equal(OutputFormStatus.Active, outputForm.Status);
        Assert.Equal(1, outputForm.SortOrder);
    }

    [Fact]
    public async Task UpdateHandoutVersionAsync_rejects_duplicate_title_and_archived_version_blocks_item_writes()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var handouts = new HandoutUseCases(unitOfWork);
        var handout = await handouts.CreateHandoutAsync(new CreateHandoutCommand("功能关系讲义"));
        var first = await handouts.CreateHandoutVersionAsync(new CreateHandoutVersionCommand(handout.Id, "基础版"));
        var second = await handouts.CreateHandoutVersionAsync(new CreateHandoutVersionCommand(handout.Id, "提高版"));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.UpdateHandoutVersionAsync(
                new UpdateHandoutVersionCommand(second.Id, "基础版", null, HandoutVersionType.Normal, HandoutVersionStatus.Active, 20)));

        await handouts.UpdateHandoutVersionAsync(
            new UpdateHandoutVersionCommand(first.Id, "基础版", null, HandoutVersionType.Normal, HandoutVersionStatus.Archived, 10));
        var sectionId = await CreateSectionAsync(unitOfWork);
        var block = new ContentBlock(sectionId, "知识点", ContentBlockType.KnowledgePoint);
        await unitOfWork.ContentBlocks.AddAsync(block);
        await unitOfWork.SaveChangesAsync();

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => handouts.AddHandoutVersionItemAsync(
                new AddHandoutVersionItemCommand(
                    first.Id,
                    HandoutVersionItemTargetType.ContentBlock,
                    block.Id)));
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

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-handout-management-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }
}
