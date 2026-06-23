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
