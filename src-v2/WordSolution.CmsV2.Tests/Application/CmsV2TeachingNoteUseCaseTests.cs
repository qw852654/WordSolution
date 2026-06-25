using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.TeachingNotes;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2TeachingNoteUseCaseTests
{
    [Fact]
    public async Task Teaching_note_use_cases_create_search_update_and_delete_notes_with_bindings()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new TeachingNoteUseCases(unitOfWork);
        var targets = await CreateAllTargetsAsync(unitOfWork);

        var created = await useCases.CreateTeachingNoteAsync(new CreateTeachingNoteCommand(
            TeachingNoteType.ClassroomRecord,
            "  pacing was too fast  ",
            null,
            new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero),
            [
                new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.ContentBlock, targets.ContentBlockId),
                new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.Section, targets.SectionId),
                new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.AtomicSection, targets.AtomicSectionId),
                new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.AtomicSectionPanel, targets.AtomicSectionPanelId),
                new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.AtomicSectionItem, targets.AtomicSectionItemId),
                new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.SectionItem, targets.SectionItemId)
            ]));

        Assert.Equal("pacing was too fast", created.Content);
        Assert.Null(created.EffectLevel);
        Assert.Equal(6, created.Bindings.Count);

        var byTarget = await useCases.ListTeachingNotesAsync(new SearchTeachingNotesCommand(
            TargetType: TeachingNoteBindingTargetType.AtomicSectionItem,
            TargetId: targets.AtomicSectionItemId));
        var search = await useCases.ListTeachingNotesAsync(new SearchTeachingNotesCommand(
            Keyword: "pacing",
            TargetType: TeachingNoteBindingTargetType.ContentBlock,
            EffectLevel: null));

        Assert.Equal([created.Id], byTarget.Select(note => note.Id));
        Assert.Equal([created.Id], search.Select(note => note.Id));

        var updated = await useCases.UpdateTeachingNoteAsync(new UpdateTeachingNoteCommand(
            created.Id,
            TeachingNoteType.RevisionSuggestion,
            "add one warmup problem next time",
            TeachingNoteEffectLevel.Weak,
            null,
            [new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.SectionItem, targets.SectionItemId)]));

        Assert.Equal(TeachingNoteType.RevisionSuggestion, updated.NoteType);
        Assert.Equal(TeachingNoteEffectLevel.Weak, updated.EffectLevel);
        Assert.Null(updated.OccurredAt);
        Assert.Equal([TeachingNoteBindingTargetType.SectionItem], updated.Bindings.Select(binding => binding.TargetType));

        var noLongerOnContentBlock = await useCases.ListTeachingNotesAsync(new SearchTeachingNotesCommand(
            TargetType: TeachingNoteBindingTargetType.ContentBlock,
            TargetId: targets.ContentBlockId));
        Assert.Empty(noLongerOnContentBlock);

        await useCases.DeleteTeachingNoteAsync(new DeleteTeachingNoteCommand(created.Id));

        Assert.Empty(await unitOfWork.TeachingNotes.ListAsync());
        Assert.Empty(await unitOfWork.TeachingNoteBindings.ListByTeachingNoteAsync(created.Id));
    }

    [Fact]
    public async Task Teaching_note_use_cases_return_notes_by_updated_time_descending()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new TeachingNoteUseCases(unitOfWork);
        var targets = await CreateAllTargetsAsync(unitOfWork);

        var older = await useCases.CreateTeachingNoteAsync(new CreateTeachingNoteCommand(
            TeachingNoteType.General,
            "older note",
            TeachingNoteEffectLevel.Normal,
            null,
            [new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.ContentBlock, targets.ContentBlockId)]));
        await Task.Delay(5);
        var newer = await useCases.CreateTeachingNoteAsync(new CreateTeachingNoteCommand(
            TeachingNoteType.General,
            "newer note",
            TeachingNoteEffectLevel.Normal,
            null,
            [new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.ContentBlock, targets.ContentBlockId)]));

        var byTarget = await useCases.ListTeachingNotesAsync(new SearchTeachingNotesCommand(
            TargetType: TeachingNoteBindingTargetType.ContentBlock,
            TargetId: targets.ContentBlockId));
        var search = await useCases.ListTeachingNotesAsync(new SearchTeachingNotesCommand(
            TargetType: TeachingNoteBindingTargetType.ContentBlock,
            EffectLevel: TeachingNoteEffectLevel.Normal));

        Assert.Equal([newer.Id, older.Id], byTarget.Select(note => note.Id));
        Assert.Equal([newer.Id, older.Id], search.Select(note => note.Id));
    }

    [Fact]
    public async Task Teaching_note_use_cases_reject_empty_content_missing_targets_and_duplicate_bindings()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var useCases = new TeachingNoteUseCases(unitOfWork);
        var targets = await CreateAllTargetsAsync(unitOfWork);

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.CreateTeachingNoteAsync(new CreateTeachingNoteCommand(
                TeachingNoteType.General,
                " ",
                null,
                null,
                [new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.ContentBlock, targets.ContentBlockId)])));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.CreateTeachingNoteAsync(new CreateTeachingNoteCommand(
                TeachingNoteType.General,
                "missing bindings",
                null,
                null,
                [])));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.CreateTeachingNoteAsync(new CreateTeachingNoteCommand(
                TeachingNoteType.General,
                "duplicate bindings",
                null,
                null,
                [
                    new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.ContentBlock, targets.ContentBlockId),
                    new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.ContentBlock, targets.ContentBlockId)
                ])));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(
            () => useCases.CreateTeachingNoteAsync(new CreateTeachingNoteCommand(
                TeachingNoteType.General,
                "missing target",
                null,
                null,
                [new TeachingNoteBindingCommand(TeachingNoteBindingTargetType.AtomicSectionPanel, 999_999)])));
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-teaching-note-application-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static async Task<TeachingNoteTargets> CreateAllTargetsAsync(EfCmsV2UnitOfWork unitOfWork)
    {
        var topic = new TeachingTopic("Default topic");
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new Section(topic.Id, "Default Section");
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        var contentBlock = new ContentBlock(section.Id, "Default ContentBlock", ContentBlockType.Question);
        await unitOfWork.ContentBlocks.AddAsync(contentBlock);
        await unitOfWork.SaveChangesAsync();

        var atomicSection = new AtomicSection(section.Id, "Default AtomicSection");
        await unitOfWork.AtomicSections.AddAsync(atomicSection);
        await unitOfWork.SaveChangesAsync();

        var panel = new AtomicSectionPanel(
            atomicSection.Id,
            "Example panel",
            AtomicSectionTeachingRole.Example,
            Difficulty.Medium);
        await unitOfWork.AtomicSectionPanels.AddAsync(panel);
        await unitOfWork.SaveChangesAsync();

        var atomicItem = new AtomicSectionItem(
            atomicSection.Id,
            contentBlock.Id,
            ReferenceMode.FollowLatest,
            null,
            0,
            atomicSectionPanelId: panel.Id,
            teachingRole: AtomicSectionTeachingRole.Example);
        await unitOfWork.AtomicSectionItems.AddAsync(atomicItem);

        var sectionItem = new SectionItem(
            section.Id,
            SectionItemTargetType.ContentBlock,
            contentBlock.Id,
            ReferenceMode.FollowLatest,
            null,
            0);
        await unitOfWork.SectionItems.AddAsync(sectionItem);
        await unitOfWork.SaveChangesAsync();

        return new TeachingNoteTargets(
            section.Id,
            contentBlock.Id,
            atomicSection.Id,
            panel.Id,
            atomicItem.Id,
            sectionItem.Id);
    }

    private sealed record TeachingNoteTargets(
        int SectionId,
        int ContentBlockId,
        int AtomicSectionId,
        int AtomicSectionPanelId,
        int AtomicSectionItemId,
        int SectionItemId);
}
