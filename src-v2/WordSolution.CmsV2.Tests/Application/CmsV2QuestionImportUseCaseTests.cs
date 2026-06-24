using Aspose.Words;
using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Infrastructure.Documents;
using WordSolution.CmsV2.Infrastructure.Persistence;
using WordSolution.CmsV2.Infrastructure.Repositories;
using DomainSection = WordSolution.CmsV2.Domain.Entities.Section;

namespace WordSolution.CmsV2.Tests.Application;

public sealed class CmsV2QuestionImportUseCaseTests
{
    [Fact]
    public async Task CreateSession_splits_candidates_by_question_start_styles_and_discards_intro()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var sourceDocxPath = Path.Combine(bankRootDirectory, "imports", "multi-question.docx");
        CreateMultiQuestionDocx(sourceDocxPath);
        var useCases = CreateUseCases(unitOfWork);

        await using var stream = File.OpenRead(sourceDocxPath);
        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(bankRootDirectory, sectionId, stream));

        Assert.Equal(sectionId, session.SectionId);
        Assert.Equal(3, session.Candidates.Count);
        Assert.All(session.Candidates, candidate =>
        {
            Assert.Equal(ContentBlockPartParseStatus.Parsed, candidate.ParseStatus);
            Assert.NotNull(candidate.HtmlPreview);
            Assert.Contains("data-question-part=\"Stem\"", candidate.HtmlPreview);
        });
        Assert.DoesNotContain("导入前说明", string.Join("\n", session.Candidates.Select(candidate => candidate.HtmlPreview)));
        Assert.Contains("典型例题题干", session.Candidates[1].HtmlPreview);
        Assert.Contains("练习题题干", session.Candidates[2].HtmlPreview);
    }

    [Fact]
    public async Task ConfirmCandidate_creates_formal_version_parts_and_neutralizes_first_stem_style()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var bankRootDirectory = CreateTempRoot();
        var sourceDocxPath = Path.Combine(bankRootDirectory, "imports", "multi-question.docx");
        CreateMultiQuestionDocx(sourceDocxPath);
        var useCases = CreateUseCases(unitOfWork);

        await using var stream = File.OpenRead(sourceDocxPath);
        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(bankRootDirectory, sectionId, stream));
        var result = await useCases.ConfirmCandidateAsync(
            new ConfirmQuestionImportCandidateCommand(
                bankRootDirectory,
                session.SessionId,
                session.Candidates[0].CandidateId,
                sectionId,
                "导入题目 1",
                "候选题目确认导入",
                ContentBlockType.Question,
                Difficulty.Medium,
                QuestionType.Calculation));

        var block = await unitOfWork.ContentBlocks.GetByIdAsync(result.ContentBlockId);
        var version = await unitOfWork.ContentBlockVersions.GetByIdAsync(result.ContentBlockVersionId);
        var parts = await unitOfWork.ContentBlockVersionParts.ListByContentBlockVersionAsync(result.ContentBlockVersionId);
        var document = new Document(result.DocxPath);
        var firstEffectiveParagraph = document
            .GetChildNodes(NodeType.Paragraph, true)
            .OfType<Paragraph>()
            .First(paragraph => !string.IsNullOrWhiteSpace(paragraph.GetText())
                && !paragraph.GetText().Contains("Created with an evaluation copy of Aspose.Words", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(block);
        Assert.NotNull(version);
        Assert.Equal(result.ContentBlockVersionId, block.CurrentVersionId);
        Assert.Equal(ContentBlockPartParseStatus.Parsed, version.PartParseStatus);
        Assert.Equal(
            [ContentBlockPartType.Stem, ContentBlockPartType.Answer],
            parts.Select(part => part.PartType));
        Assert.Equal("正文", firstEffectiveParagraph.ParagraphFormat.StyleName);
        Assert.Contains("第一题答案", parts.Single(part => part.PartType == ContentBlockPartType.Answer).PlainText);
    }

    [Fact]
    public async Task ConfirmCandidate_rejects_wrong_section_without_creating_content_block()
    {
        await using var context = await CreateMigratedContextAsync();
        var unitOfWork = new EfCmsV2UnitOfWork(context);
        var sectionId = await CreateSectionAsync(unitOfWork);
        var otherSectionId = await CreateSectionAsync(unitOfWork, "另一个主题", "另一个 Section");
        var bankRootDirectory = CreateTempRoot();
        var sourceDocxPath = Path.Combine(bankRootDirectory, "imports", "multi-question.docx");
        CreateMultiQuestionDocx(sourceDocxPath);
        var useCases = CreateUseCases(unitOfWork);

        await using var stream = File.OpenRead(sourceDocxPath);
        var session = await useCases.CreateSessionAsync(
            new CreateQuestionImportSessionCommand(bankRootDirectory, sectionId, stream));

        await Assert.ThrowsAsync<CmsV2ApplicationException>(() => useCases.ConfirmCandidateAsync(
            new ConfirmQuestionImportCandidateCommand(
                bankRootDirectory,
                session.SessionId,
                session.Candidates[0].CandidateId,
                otherSectionId,
                "错误 Section",
                null,
                ContentBlockType.Question,
                Difficulty.Basic,
                QuestionType.Calculation)));

        Assert.Empty(await unitOfWork.ContentBlocks.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersions.ListAsync());
        Assert.Empty(await unitOfWork.ContentBlockVersionParts.ListAsync());
    }

    private static QuestionImportUseCases CreateUseCases(EfCmsV2UnitOfWork unitOfWork)
    {
        return new QuestionImportUseCases(
            unitOfWork,
            new CmsV2FileAssetPathProvider(),
            new LocalContentBlockFileStore(),
            new AsposeContentBlockDocumentProcessor(),
            new AsposeQuestionImportDocumentProcessor());
    }

    private static async Task<int> CreateSectionAsync(
        EfCmsV2UnitOfWork unitOfWork,
        string topicName = "测试主题",
        string sectionTitle = "测试 Section")
    {
        var topic = new TeachingTopic(topicName);
        await unitOfWork.TeachingTopics.AddAsync(topic);
        await unitOfWork.SaveChangesAsync();

        var section = new DomainSection(topic.Id, sectionTitle);
        await unitOfWork.Sections.AddAsync(section);
        await unitOfWork.SaveChangesAsync();

        return section.Id;
    }

    private static async Task<CmsV2DbContext> CreateMigratedContextAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            "cms-v2-question-import-tests",
            Guid.NewGuid().ToString("N"),
            "cms-v2.db");

        var context = CmsV2DbContextFactory.CreateForDatabase(databasePath);
        await context.Database.MigrateAsync();

        return context;
    }

    private static string CreateTempRoot()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "cms-v2-question-import-tests",
            Guid.NewGuid().ToString("N"));
    }

    private static void CreateMultiQuestionDocx(string docxPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(docxPath)!);

        var document = new Document();
        var body = document.FirstSection.Body;
        body.RemoveAllChildren();

        AddStyledParagraph(document, body, "正文", "导入前说明，应该被丢弃。");
        AddStyledParagraph(document, body, "例题", "第一题题干");
        AddStyledParagraph(document, body, "答案", "第一题答案");
        AddStyledParagraph(document, body, "典型例题", "典型例题题干");
        AddStyledParagraph(document, body, "解析", "典型例题解析");
        AddStyledParagraph(document, body, "练习题", "练习题题干");
        AddStyledParagraph(document, body, "答案", "练习题答案");

        document.Save(docxPath);
    }

    private static void AddStyledParagraph(Document document, Body body, string styleName, string text)
    {
        EnsureParagraphStyle(document, styleName);
        var paragraph = new Paragraph(document);
        paragraph.ParagraphFormat.StyleName = styleName;
        paragraph.AppendChild(new Run(document, text));
        body.AppendChild(paragraph);
    }

    private static void EnsureParagraphStyle(Document document, string styleName)
    {
        if (document.Styles[styleName] is not null)
        {
            return;
        }

        document.Styles.Add(StyleType.Paragraph, styleName);
    }
}
