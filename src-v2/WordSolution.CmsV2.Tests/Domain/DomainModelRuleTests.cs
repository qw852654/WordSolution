using System.Reflection;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Exceptions;

namespace WordSolution.CmsV2.Tests.Domain;

public sealed class DomainModelRuleTests
{
    [Theory]
    [InlineData("section-item")]
    [InlineData("atomic-section-item")]
    [InlineData("content-block-relation")]
    public void Locked_reference_requires_locked_content_block_version_id(string relationKind)
    {
        Action act = relationKind switch
        {
            "section-item" => () => new SectionItem(
                sectionId: 1,
                targetType: SectionItemTargetType.ContentBlock,
                targetId: 2,
                referenceMode: ReferenceMode.LockedVersion,
                lockedContentBlockVersionId: null,
                sortOrder: 1),
            "atomic-section-item" => () => new AtomicSectionItem(
                atomicSectionId: 1,
                contentBlockId: 2,
                referenceMode: ReferenceMode.LockedVersion,
                lockedContentBlockVersionId: null,
                sortOrder: 1),
            "content-block-relation" => () => new ContentBlockRelation(
                parentBlockId: 1,
                childBlockId: 2,
                referenceMode: ReferenceMode.LockedVersion,
                lockedContentBlockVersionId: null,
                sortOrder: 1),
            _ => throw new ArgumentOutOfRangeException(nameof(relationKind), relationKind, null)
        };

        var exception = Assert.Throws<DomainException>(act);
        Assert.Contains("LockedContentBlockVersionId", exception.Message);
    }

    [Fact]
    public void Section_item_rejects_target_types_outside_content_block_and_atomic_section()
    {
        var invalidTargetType = (SectionItemTargetType)999;

        var exception = Assert.Throws<DomainException>(() => new SectionItem(
            sectionId: 1,
            targetType: invalidTargetType,
            targetId: 2,
            referenceMode: ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1));

        Assert.Contains("SectionItem.TargetType", exception.Message);
    }

    [Fact]
    public void Atomic_section_item_only_exposes_content_block_reference_shape()
    {
        var item = new AtomicSectionItem(
            atomicSectionId: 1,
            contentBlockId: 2,
            referenceMode: ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1);

        var members = typeof(AtomicSectionItem)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Select(member => member.Name)
            .ToArray();

        Assert.Equal(2, item.ContentBlockId);
        Assert.DoesNotContain("TargetType", members);
        Assert.DoesNotContain("AtomicSectionTargetId", members);
    }

    [Fact]
    public void Atomic_section_panel_requires_valid_title_role_and_sort_order()
    {
        var updatedTime = new DateTimeOffset(2026, 6, 23, 8, 0, 0, TimeSpan.Zero);

        var panel = new AtomicSectionPanel(
            atomicSectionId: 1,
            title: " Knowledge ",
            teachingRole: AtomicSectionTeachingRole.Knowledge,
            difficulty: Difficulty.Basic,
            sortOrder: 2,
            updatedTime: updatedTime);

        Assert.Equal(1, panel.AtomicSectionId);
        Assert.Equal("Knowledge", panel.Title);
        Assert.Equal(AtomicSectionTeachingRole.Knowledge, panel.TeachingRole);
        Assert.Equal(Difficulty.Basic, panel.Difficulty);
        Assert.Equal(2, panel.SortOrder);
        Assert.Equal(updatedTime, panel.UpdatedTime);

        Assert.Throws<DomainException>(() => new AtomicSectionPanel(
            atomicSectionId: 1,
            title: " ",
            teachingRole: AtomicSectionTeachingRole.Knowledge,
            difficulty: Difficulty.Basic,
            sortOrder: 1));
        Assert.Throws<DomainException>(() => new AtomicSectionPanel(
            atomicSectionId: 1,
            title: "Knowledge",
            teachingRole: AtomicSectionTeachingRole.Unclassified,
            difficulty: Difficulty.Basic,
            sortOrder: 1));
        Assert.Throws<DomainException>(() => new AtomicSectionPanel(
            atomicSectionId: 1,
            title: "Knowledge",
            teachingRole: AtomicSectionTeachingRole.Knowledge,
            difficulty: Difficulty.Basic,
            sortOrder: -1));
    }

    [Fact]
    public void Atomic_section_item_defaults_to_unassigned_and_can_change_panel_classification()
    {
        var updatedTime = new DateTimeOffset(2026, 6, 23, 8, 0, 0, TimeSpan.Zero);
        var item = new AtomicSectionItem(
            atomicSectionId: 1,
            contentBlockId: 2,
            referenceMode: ReferenceMode.FollowLatest,
            lockedContentBlockVersionId: null,
            sortOrder: 1);

        Assert.Null(item.AtomicSectionPanelId);
        Assert.Equal(AtomicSectionTeachingRole.Unclassified, item.TeachingRole);

        item.ChangeClassification(
            atomicSectionPanelId: 3,
            teachingRole: AtomicSectionTeachingRole.Example,
            sortOrder: 5,
            updatedTime: updatedTime);

        Assert.Equal(3, item.AtomicSectionPanelId);
        Assert.Equal(AtomicSectionTeachingRole.Example, item.TeachingRole);
        Assert.Equal(5, item.SortOrder);
        Assert.Equal(updatedTime, item.UpdatedTime);
    }

    [Fact]
    public void Tag_normalizes_name_records_color_and_supports_archive_restore()
    {
        var createdTime = new DateTimeOffset(2026, 6, 25, 8, 0, 0, TimeSpan.Zero);
        var archivedTime = new DateTimeOffset(2026, 6, 25, 9, 0, 0, TimeSpan.Zero);
        var restoredTime = new DateTimeOffset(2026, 6, 25, 10, 0, 0, TimeSpan.Zero);
        var tag = new Tag(
            name: " Energy ",
            color: "tag-blue",
            createdTime: createdTime,
            updatedTime: createdTime);

        Assert.Equal("Energy", tag.Name);
        Assert.Equal("energy", tag.NormalizedName);
        Assert.Equal("tag-blue", tag.Color);
        Assert.Equal(TagStatus.Active, tag.Status);
        Assert.Equal(createdTime, tag.CreatedTime);
        Assert.Equal(createdTime, tag.UpdatedTime);

        tag.Archive(archivedTime);

        Assert.Equal(TagStatus.Archived, tag.Status);
        Assert.Equal(archivedTime, tag.UpdatedTime);

        tag.Restore(restoredTime);

        Assert.Equal(TagStatus.Active, tag.Status);
        Assert.Equal(restoredTime, tag.UpdatedTime);
    }

    [Theory]
    [InlineData(" Energy ", "energy")]
    [InlineData("机械能守恒", "机械能守恒")]
    [InlineData("机械 能 守恒", "机械 能 守恒")]
    public void Tag_normalized_name_trims_and_lowercases_english_without_collapsing_middle_spaces(
        string name,
        string expectedNormalizedName)
    {
        var tag = new Tag(name, "tag-gray");

        Assert.Equal(expectedNormalizedName, tag.NormalizedName);
    }

    [Fact]
    public void Tag_rejects_empty_name_and_color()
    {
        Assert.Throws<DomainException>(() => new Tag(" ", "tag-gray"));
        Assert.Throws<DomainException>(() => new Tag("Energy", " "));
    }

    [Theory]
    [InlineData(TagBindingTargetType.ContentBlock)]
    [InlineData(TagBindingTargetType.AtomicSection)]
    [InlineData(TagBindingTargetType.Section)]
    public void Tag_binding_allows_first_version_target_types(TagBindingTargetType targetType)
    {
        var binding = new TagBinding(tagId: 1, targetType: targetType, targetId: 2);

        Assert.Equal(1, binding.TagId);
        Assert.Equal(targetType, binding.TargetType);
        Assert.Equal(2, binding.TargetId);
    }

    [Fact]
    public void Tag_binding_rejects_invalid_target_type()
    {
        var exception = Assert.Throws<DomainException>(() => new TagBinding(
            tagId: 1,
            targetType: (TagBindingTargetType)999,
            targetId: 2));

        Assert.Contains("TagBinding.TargetType", exception.Message);
    }

    [Fact]
    public void Teaching_note_keeps_lightweight_note_fields_without_old_task_contract()
    {
        var occurredAt = new DateTimeOffset(2026, 6, 24, 9, 0, 0, TimeSpan.Zero);
        var createdTime = new DateTimeOffset(2026, 6, 24, 10, 0, 0, TimeSpan.Zero);
        var updatedTime = new DateTimeOffset(2026, 6, 24, 11, 0, 0, TimeSpan.Zero);

        var note = new TeachingNote(
            TeachingNoteType.RevisionSuggestion,
            "  Add a simpler bridge question first.  ",
            TeachingNoteEffectLevel.Weak,
            occurredAt,
            createdTime,
            updatedTime);

        Assert.Equal(TeachingNoteType.RevisionSuggestion, note.NoteType);
        Assert.Equal("Add a simpler bridge question first.", note.Content);
        Assert.Equal(TeachingNoteEffectLevel.Weak, note.EffectLevel);
        Assert.Equal(occurredAt, note.OccurredAt);
        Assert.Equal(createdTime, note.CreatedTime);
        Assert.Equal(updatedTime, note.UpdatedTime);

        var propertyNames = typeof(TeachingNote)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("Title", propertyNames);
        Assert.DoesNotContain("Status", propertyNames);
        Assert.DoesNotContain("NextAction", propertyNames);
        Assert.DoesNotContain("SortOrder", propertyNames);
        Assert.DoesNotContain("TargetType", propertyNames);
        Assert.DoesNotContain("TargetId", propertyNames);
    }

    [Fact]
    public void Teaching_note_allows_null_effect_level()
    {
        var note = new TeachingNote(
            TeachingNoteType.General,
            "Works as a plain observation.",
            effectLevel: null);

        Assert.Null(note.EffectLevel);
    }

    [Fact]
    public void Teaching_note_rejects_empty_content_and_invalid_effect_level()
    {
        Assert.Throws<DomainException>(() => new TeachingNote(TeachingNoteType.General, " "));

        var exception = Assert.Throws<DomainException>(() => new TeachingNote(
            TeachingNoteType.General,
            "Invalid effect.",
            (TeachingNoteEffectLevel)999));

        Assert.Contains(nameof(TeachingNote.EffectLevel), exception.Message);
    }

    [Fact]
    public void Teaching_note_type_uses_lightweight_note_categories()
    {
        Assert.Equal(1, (int)TeachingNoteType.General);
        Assert.Equal(2, (int)TeachingNoteType.ClassroomRecord);
        Assert.Equal(3, (int)TeachingNoteType.LearningEffect);
        Assert.Equal(4, (int)TeachingNoteType.TeachingReflection);
        Assert.Equal(5, (int)TeachingNoteType.RevisionSuggestion);
        Assert.Equal(6, (int)TeachingNoteType.QuestionReplacement);
        Assert.Equal(7, (int)TeachingNoteType.CommonMistake);

        var names = Enum.GetNames<TeachingNoteType>();
        Assert.DoesNotContain("TeachingLogic", names);
        Assert.DoesNotContain("ExampleAdvice", names);
    }

    [Theory]
    [InlineData(TeachingNoteBindingTargetType.ContentBlock)]
    [InlineData(TeachingNoteBindingTargetType.Section)]
    [InlineData(TeachingNoteBindingTargetType.AtomicSection)]
    [InlineData(TeachingNoteBindingTargetType.AtomicSectionPanel)]
    [InlineData(TeachingNoteBindingTargetType.AtomicSectionItem)]
    [InlineData(TeachingNoteBindingTargetType.SectionItem)]
    public void Teaching_note_binding_allows_first_version_target_types(TeachingNoteBindingTargetType targetType)
    {
        var binding = new TeachingNoteBinding(
            teachingNoteId: 1,
            targetType: targetType,
            targetId: 2);

        Assert.Equal(1, binding.TeachingNoteId);
        Assert.Equal(targetType, binding.TargetType);
        Assert.Equal(2, binding.TargetId);
        Assert.True(binding.CreatedTime <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Teaching_note_binding_rejects_invalid_target_type()
    {
        var exception = Assert.Throws<DomainException>(() => new TeachingNoteBinding(
            teachingNoteId: 1,
            targetType: (TeachingNoteBindingTargetType)999,
            targetId: 2));

        Assert.Contains("TeachingNoteBinding.TargetType", exception.Message);
    }

    [Fact]
    public void Handout_version_item_rejects_section_target()
    {
        var exception = Assert.Throws<DomainException>(() => new HandoutVersionItem(
            handoutVersionId: 1,
            targetType: HandoutVersionItemTargetType.Section,
            targetId: 2,
            sortOrder: 1));

        Assert.Contains("HandoutVersionItem.TargetType", exception.Message);
    }

    [Theory]
    [InlineData(HandoutVersionItemTargetType.SectionVariant)]
    [InlineData(HandoutVersionItemTargetType.ContentBlock)]
    [InlineData(HandoutVersionItemTargetType.AtomicSection)]
    public void Handout_version_item_allows_section_variant_content_block_and_atomic_section_targets(HandoutVersionItemTargetType targetType)
    {
        var item = new HandoutVersionItem(
            handoutVersionId: 1,
            targetType: targetType,
            targetId: 2,
            sortOrder: 1);

        Assert.Equal(targetType, item.TargetType);
    }

    [Fact]
    public void Content_block_version_keeps_only_body_version_fields()
    {
        var propertyNames = typeof(ContentBlockVersion)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("Difficulty", propertyNames);
        Assert.DoesNotContain("BlockType", propertyNames);
        Assert.DoesNotContain("QuestionType", propertyNames);
    }

    [Fact]
    public void Content_block_version_records_question_part_parse_status()
    {
        var updatedTime = new DateTimeOffset(2026, 6, 24, 8, 0, 0, TimeSpan.Zero);
        var version = new ContentBlockVersion(
            contentBlockId: 1,
            versionNumber: 1,
            docxPath: "content-blocks/source/1/v1.docx");

        Assert.Equal(ContentBlockPartParseStatus.NotApplicable, version.PartParseStatus);
        Assert.Null(version.PartParseMessage);

        version.MarkPartParsed(
            ContentBlockPartParseStatus.ParsedWithWarnings,
            "Unknown style: 自定义样式",
            updatedTime);

        Assert.Equal(ContentBlockPartParseStatus.ParsedWithWarnings, version.PartParseStatus);
        Assert.Equal("Unknown style: 自定义样式", version.PartParseMessage);
        Assert.Equal(updatedTime, version.UpdatedTime);
    }

    [Fact]
    public void Content_block_version_part_requires_valid_version_type_and_sort_order()
    {
        var part = new ContentBlockVersionPart(
            contentBlockVersionId: 1,
            partType: ContentBlockPartType.Stem,
            sortOrder: 0,
            plainText: " 题干 ",
            sourceStyleNamesJson: """["例题","正文"]""",
            warningMessage: " warning ");

        Assert.Equal(1, part.ContentBlockVersionId);
        Assert.Equal(ContentBlockPartType.Stem, part.PartType);
        Assert.Equal(0, part.SortOrder);
        Assert.Equal(" 题干 ", part.PlainText);
        Assert.Equal("""["例题","正文"]""", part.SourceStyleNamesJson);
        Assert.Equal("warning", part.WarningMessage);

        Assert.Throws<DomainException>(() => new ContentBlockVersionPart(
            contentBlockVersionId: 0,
            partType: ContentBlockPartType.Stem,
            sortOrder: 0));
        Assert.Throws<DomainException>(() => new ContentBlockVersionPart(
            contentBlockVersionId: 1,
            partType: (ContentBlockPartType)999,
            sortOrder: 0));
        Assert.Throws<DomainException>(() => new ContentBlockVersionPart(
            contentBlockVersionId: 1,
            partType: ContentBlockPartType.Stem,
            sortOrder: -1));
    }

    [Fact]
    public void Generated_file_records_generated_time()
    {
        var generatedTime = new DateTimeOffset(2026, 6, 9, 8, 30, 0, TimeSpan.Zero);

        var generatedFile = new GeneratedFile(
            outputFormId: 1,
            filePath: "handouts/generated/1/student.docx",
            versionManifestJson: """{"contentBlocks":[{"id":1,"version":3}]}""",
            generatedTime: generatedTime);

        Assert.Equal(generatedTime, generatedFile.GeneratedTime);
    }

    [Fact]
    public void Teaching_topic_rename_trims_name_and_updates_time()
    {
        var initialTime = new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero);
        var updatedTime = new DateTimeOffset(2026, 6, 2, 8, 0, 0, TimeSpan.Zero);
        var topic = new TeachingTopic(" Mechanics ", " old ", sortOrder: 1, updatedTime: initialTime);

        topic.Rename(" Energy ", " new ", updatedTime);

        Assert.Equal("Energy", topic.Name);
        Assert.Equal("new", topic.Description);
        Assert.Equal(updatedTime, topic.UpdatedTime);
    }

    [Fact]
    public void Teaching_topic_rename_rejects_empty_name()
    {
        var topic = new TeachingTopic("Mechanics");

        var exception = Assert.Throws<DomainException>(() => topic.Rename(" "));

        Assert.Contains("Name", exception.Message);
    }

    [Fact]
    public void Teaching_topic_move_updates_parent_sort_order_and_time()
    {
        var updatedTime = new DateTimeOffset(2026, 6, 2, 8, 0, 0, TimeSpan.Zero);
        var topic = new TeachingTopic("Mechanics", parentId: 1, sortOrder: 1);

        topic.MoveTo(parentId: 2, sortOrder: 20, updatedTime);

        Assert.Equal(2, topic.ParentId);
        Assert.Equal(20, topic.SortOrder);
        Assert.Equal(updatedTime, topic.UpdatedTime);
    }
}
