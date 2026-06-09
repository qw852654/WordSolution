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

    [Theory]
    [InlineData(HandoutVersionItemTargetType.Section)]
    [InlineData(HandoutVersionItemTargetType.AtomicSection)]
    public void Handout_version_item_rejects_section_and_atomic_section_targets(HandoutVersionItemTargetType targetType)
    {
        var exception = Assert.Throws<DomainException>(() => new HandoutVersionItem(
            handoutVersionId: 1,
            targetType: targetType,
            targetId: 2,
            sortOrder: 1));

        Assert.Contains("HandoutVersionItem.TargetType", exception.Message);
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
}
