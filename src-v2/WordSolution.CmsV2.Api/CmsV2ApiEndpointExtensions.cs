using System.Text.Json;
using WordSolution.CmsV2.Application.Common;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Application.Sections;
using WordSolution.CmsV2.Application.Tags;
using WordSolution.CmsV2.Application.TeachingNotes;
using WordSolution.CmsV2.Application.TeachingStructure;
using WordSolution.CmsV2.Domain.Documents;
using WordSolution.CmsV2.Domain.Entities;
using WordSolution.CmsV2.Domain.Enums;
using WordSolution.CmsV2.Domain.Repositories;

namespace WordSolution.CmsV2.Api;

public static class CmsV2ApiEndpointExtensions
{
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public static IEndpointRouteBuilder MapCmsV2Api(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cms-v2");

        group.MapGet("/health", (CmsV2CurrentBank currentBank) => Results.Ok(new
        {
            status = "ok",
            bankKey = currentBank.BankKey,
            bankDisplayName = currentBank.DisplayName,
            bankKind = currentBank.Kind,
            bankRootDirectory = currentBank.RootDirectory
        }));

        group.MapGet("/meta/enums", () => Results.Ok(new
        {
            teachingTopicStatus = EnumNames<TeachingTopicStatus>(),
            sectionType = EnumNames<SectionType>(),
            difficulty = EnumNames<Difficulty>(),
            sectionStatus = EnumNames<SectionStatus>(),
            sectionItemTargetType = EnumNames<SectionItemTargetType>(),
            referenceMode = EnumNames<ReferenceMode>(),
            selectionLayer = EnumNames<SelectionLayer>(),
            teachingUse = EnumNames<TeachingUse>(),
            atomicSectionType = EnumNames<AtomicSectionType>(),
            atomicSectionStatus = EnumNames<AtomicSectionStatus>(),
            atomicSectionTeachingRole = EnumNames<AtomicSectionTeachingRole>(),
            contentBlockType = EnumNames<ContentBlockType>(),
            questionType = EnumNames<QuestionType>(),
            contentBlockStatus = EnumNames<ContentBlockStatus>(),
            sectionVariantType = EnumNames<SectionVariantType>(),
            sectionVariantStatus = EnumNames<SectionVariantStatus>(),
            handoutStatus = EnumNames<HandoutStatus>(),
            handoutVersionType = EnumNames<HandoutVersionType>(),
            handoutVersionStatus = EnumNames<HandoutVersionStatus>(),
            handoutVersionItemTargetType = EnumNames<HandoutVersionItemTargetType>(),
            outputTemplateStatus = EnumNames<OutputTemplateStatus>(),
            outputAudience = EnumNames<OutputAudience>(),
            outputFormat = EnumNames<OutputFormat>(),
            visibilityMode = EnumNames<VisibilityMode>(),
            outputFormStatus = EnumNames<OutputFormStatus>(),
            tagStatus = EnumNames<TagStatus>(),
            tagBindingTargetType = EnumNames<TagBindingTargetType>(),
            teachingNoteType = EnumNames<TeachingNoteType>(),
            teachingNoteEffectLevel = EnumNames<TeachingNoteEffectLevel>(),
            teachingNoteBindingTargetType = EnumNames<TeachingNoteBindingTargetType>()
        }));

        MapTeachingStructure(group);
        MapTeachingTopics(group);
        MapContentBlocks(group);
        MapSections(group);
        MapAtomicSections(group);
        MapTags(group);
        MapTeachingNotes(group);
        MapSectionVariants(group);
        MapHandouts(group);
        MapOutputs(group);

        return app;
    }

    private static void MapTeachingStructure(RouteGroupBuilder group)
    {
        group.MapGet("/teaching-structure", async (
            TeachingStructureUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.GetTeachingStructureAsync(cancellationToken)));
    }

    private static void MapTeachingTopics(RouteGroupBuilder group)
    {
        group.MapGet("/teaching-topics", async (ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.TeachingTopics.ListAsync(cancellationToken)));

        group.MapGet("/teaching-topics/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.TeachingTopics.GetByIdAsync(id, cancellationToken)));

        group.MapGet("/teaching-topics/{id:int}/children", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.TeachingTopics.ListChildrenAsync(id, cancellationToken)));

        group.MapPost("/teaching-topics/{id:int}/children", async (
            int id,
            CreateTeachingTopicChildRequest request,
            TeachingStructureUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateChildTopicAsync(
                new CreateTeachingTopicChildCommand(id, request.Name, request.Description, request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/teaching-topics/{id:int}/next-sibling", async (
            int id,
            CreateTeachingTopicNextSiblingRequest request,
            TeachingStructureUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateNextSiblingTopicAsync(
                new CreateTeachingTopicNextSiblingCommand(id, request.Name, request.Description, request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/teaching-topics/{id:int}/rename", async (
            int id,
            RenameTeachingTopicRequest request,
            TeachingStructureUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.RenameTopicAsync(
                new RenameTeachingTopicCommand(id, request.Name, request.Description),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapDelete("/teaching-topics/{id:int}", async (
            int id,
            TeachingStructureUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.DeleteTopicAsync(new DeleteTeachingTopicCommand(id), cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/teaching-topics/{id:int}/section", async (
            int id,
            CreateSectionForTeachingTopicRequest request,
            TeachingStructureUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateSectionForTopicAsync(
                new CreateSectionForTeachingTopicCommand(
                    id,
                    request.Title,
                    request.Description,
                    request.Type,
                    request.Difficulty,
                    request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/teaching-topics", async (
            CreateTeachingTopicRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (request.ParentId.HasValue && await unitOfWork.TeachingTopics.GetByIdAsync(request.ParentId.Value, cancellationToken) is null)
            {
                return NotFoundProblem($"TeachingTopic {request.ParentId.Value} was not found.");
            }

            var topic = new TeachingTopic(
                request.Name,
                request.Description,
                request.ParentId,
                request.SortOrder,
                request.Status);
            await unitOfWork.TeachingTopics.AddAsync(topic, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(topic);
        });
    }

    private static void MapContentBlocks(RouteGroupBuilder group)
    {
        group.MapGet("/content-blocks", async (
            int[]? tagIds,
            ContentBlockUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.ListContentBlocksAsync(
                new SearchContentBlocksCommand(tagIds),
                cancellationToken)));

        group.MapGet("/content-blocks/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.ContentBlocks.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/content-blocks/{id:int}/title", async (
            int id,
            RenameContentBlockRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var contentBlock = await unitOfWork.ContentBlocks.GetByIdAsync(id, cancellationToken);
            if (contentBlock is null)
            {
                return NotFoundProblem($"ContentBlock {id} was not found.");
            }

            contentBlock.Rename(request.Title);
            unitOfWork.ContentBlocks.Update(contentBlock);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(contentBlock);
        });

        group.MapPost("/content-blocks", async (
            CreateContentBlockRequest request,
            ContentBlockUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateContentBlockAsync(
                new CreateContentBlockCommand(
                    request.SectionId,
                    request.Title,
                    request.BlockType,
                    request.Difficulty,
                    request.Summary,
                    request.QuestionType,
                    request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/content-blocks/blank-document", async (
            CreateContentBlockWithBlankDocumentRequest request,
            ContentBlockDocumentUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateContentBlockWithBlankDocumentAsync(
                new CreateContentBlockWithBlankDocumentCommand(
                    currentBank.RootDirectory,
                    request.SectionId,
                    request.Title,
                    request.BlockType,
                    request.Summary,
                    request.Difficulty,
                    request.QuestionType,
                    request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/content-blocks/{id:int}/versions/import", async (
            int id,
            HttpRequest request,
            ContentBlockDocumentUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");
            if (file is null)
            {
                return Results.BadRequest(new { message = "A DOCX file field named 'file' is required." });
            }

            var setAsCurrent = !bool.TryParse(form["setAsCurrent"], out var parsedSetAsCurrent) || parsedSetAsCurrent;
            await using var stream = file.OpenReadStream();
            var result = await useCases.ImportContentBlockDocxVersionAsync(
                new ImportContentBlockDocxVersionCommand(
                    currentBank.RootDirectory,
                    id,
                    stream,
                    setAsCurrent),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/content-blocks/{id:int}/edit-session", async (
            int id,
            CreateContentBlockEditSessionRequest request,
            ContentBlockEditSessionUseCases useCases,
            ICmsV2UnitOfWork unitOfWork,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.ContentBlocks.GetByIdAsync(id, cancellationToken) is null)
            {
                return NotFoundProblem($"ContentBlock {id} was not found.");
            }

            var session = await useCases.CreateAsync(
                new CreateContentBlockEditSessionCommand(
                    currentBank.RootDirectory,
                    id,
                    request.OpenWord),
                cancellationToken);

            return Results.Ok(ToContentBlockEditSessionResponse(session));
        });

        group.MapPost("/content-blocks/{id:int}/delete-cascade", async (
            int id,
            ContentBlockDeletionUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.DeleteContentBlockCascadeAsync(
                new DeleteContentBlockCascadeCommand(currentBank.RootDirectory, id),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/question-import-sessions", async (
            CreateQuestionImportSessionRequest request,
            QuestionImportUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateSessionAsync(
                new CreateQuestionImportSessionCommand(
                    currentBank.RootDirectory,
                    request.Context,
                    request.OpenWord),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/question-import-sessions/{sessionId}", async (
            string sessionId,
            QuestionImportUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.GetSessionAsync(
                new GetQuestionImportSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/question-import-sessions/{sessionId}/candidates", async (
            string sessionId,
            QuestionImportUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.GetCandidatesAsync(
                new GetQuestionImportSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/question-import-sessions/{sessionId}/confirm", async (
            string sessionId,
            ConfirmQuestionImportRequest request,
            QuestionImportUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.ConfirmAsync(
                new ConfirmQuestionImportCommand(
                    currentBank.RootDirectory,
                    sessionId,
                    request.Candidates),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/question-import-sessions/{sessionId}/cancel", async (
            string sessionId,
            QuestionImportUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CancelSessionAsync(
                new CancelQuestionImportSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/question-import-sessions/{sessionId}/reopen", async (
            string sessionId,
            QuestionImportUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.ReopenSessionAsync(
                new ReopenQuestionImportSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/content-blocks/{id:int}/versions", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.ContentBlocks.GetByIdAsync(id, cancellationToken) is null)
            {
                return NotFoundProblem($"ContentBlock {id} was not found.");
            }

            return Results.Ok(await unitOfWork.ContentBlockVersions.ListByContentBlockAsync(id, cancellationToken));
        });

        group.MapPost("/content-blocks/{id:int}/current-version", async (
            int id,
            SetCurrentContentBlockVersionRequest request,
            ContentBlockUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.SetCurrentContentBlockVersionAsync(
                new SetCurrentContentBlockVersionCommand(id, request.ContentBlockVersionId),
                cancellationToken);

            return Results.Ok(new { contentBlockId = id, contentBlockVersionId = request.ContentBlockVersionId });
        });

        group.MapPost("/content-blocks/{id:int}/difficulty", async (
            int id,
            ChangeContentBlockDifficultyRequest request,
            ContentBlockUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.ChangeContentBlockDifficultyAsync(
                new ChangeContentBlockDifficultyCommand(id, request.Difficulty),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/content-blocks/{id:int}/docx", async (
            int id,
            ICmsV2UnitOfWork unitOfWork,
            IContentBlockFileStore fileStore,
            CancellationToken cancellationToken) =>
        {
            var block = await unitOfWork.ContentBlocks.GetByIdAsync(id, cancellationToken);
            if (block is null)
            {
                return NotFoundProblem($"ContentBlock {id} was not found.");
            }

            var version = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(id, cancellationToken);
            return await DownloadContentBlockVersionAsync(block, version, fileStore, cancellationToken);
        });

        group.MapGet("/content-blocks/{id:int}/versions/{versionId:int}/docx", async (
            int id,
            int versionId,
            ICmsV2UnitOfWork unitOfWork,
            IContentBlockFileStore fileStore,
            CancellationToken cancellationToken) =>
        {
            var block = await unitOfWork.ContentBlocks.GetByIdAsync(id, cancellationToken);
            if (block is null)
            {
                return NotFoundProblem($"ContentBlock {id} was not found.");
            }

            var version = await unitOfWork.ContentBlockVersions.GetByIdAsync(versionId, cancellationToken);
            if (version is not null && version.ContentBlockId != id)
            {
                return NotFoundProblem($"ContentBlockVersion {versionId} was not found.");
            }

            return await DownloadContentBlockVersionAsync(block, version, fileStore, cancellationToken);
        });

        group.MapGet("/content-blocks/{id:int}/html-preview", async (
            int id,
            ICmsV2UnitOfWork unitOfWork,
            IContentBlockFileStore fileStore,
            CancellationToken cancellationToken) =>
        {
            var version = await unitOfWork.ContentBlockVersions.GetCurrentByContentBlockAsync(id, cancellationToken);
            return await ReadHtmlPreviewAsync(version, fileStore, cancellationToken);
        });

        group.MapGet("/content-blocks/{id:int}/versions/{versionId:int}/html-preview", async (
            int id,
            int versionId,
            ICmsV2UnitOfWork unitOfWork,
            IContentBlockFileStore fileStore,
            CancellationToken cancellationToken) =>
        {
            var version = await unitOfWork.ContentBlockVersions.GetByIdAsync(versionId, cancellationToken);
            if (version is not null && version.ContentBlockId != id)
            {
                return NotFoundProblem($"ContentBlockVersion {versionId} was not found.");
            }

            return await ReadHtmlPreviewAsync(version, fileStore, cancellationToken);
        });

        group.MapGet("/content-blocks/{id:int}/versions/{versionId:int}/parts", async (
            int id,
            int versionId,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var version = await unitOfWork.ContentBlockVersions.GetByIdAsync(versionId, cancellationToken);
            if (version is null || version.ContentBlockId != id)
            {
                return NotFoundProblem($"ContentBlockVersion {versionId} was not found.");
            }

            var parts = await unitOfWork.ContentBlockVersionParts.ListByContentBlockVersionAsync(
                versionId,
                cancellationToken);
            return Results.Ok(parts);
        });

        group.MapGet("/content-blocks/{id:int}/relations/children", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.ContentBlockRelations.ListChildrenAsync(id, cancellationToken)));

        group.MapGet("/content-blocks/{id:int}/relations/parents", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.ContentBlockRelations.ListParentsAsync(id, cancellationToken)));

        group.MapPost("/content-blocks/{id:int}/relations/children", async (
            int id,
            AddContentBlockRelationRequest request,
            ContentBlockRelationUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.AddContentBlockRelationAsync(
                new AddContentBlockRelationCommand(
                    id,
                    request.ChildBlockId,
                    request.ReferenceMode,
                    request.LockedContentBlockVersionId,
                    request.SortOrder,
                    request.TitleOverride,
                    request.Note),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/content-blocks/{id:int}/relations/children/{relationId:int}/move", async (
            int id,
            int relationId,
            MoveContentBlockRelationRequest request,
            ContentBlockRelationUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<ContentBlockRelationMoveDirection>(request.Direction, ignoreCase: true, out var direction))
            {
                return Results.BadRequest(new { message = "Direction must be Up or Down." });
            }

            await useCases.MoveContentBlockRelationAsync(
                new MoveContentBlockRelationCommand(id, relationId, direction),
                cancellationToken);

            return Results.Ok(new { parentBlockId = id, relationId, direction = direction.ToString() });
        });

        group.MapDelete("/content-blocks/{id:int}/relations/children/{relationId:int}", async (
            int id,
            int relationId,
            ContentBlockRelationUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.RemoveContentBlockRelationAsync(
                new RemoveContentBlockRelationCommand(id, relationId),
                cancellationToken);

            return Results.NoContent();
        });

        group.MapGet("/content-block-edit-sessions/{sessionId}", async (
            string sessionId,
            ContentBlockEditSessionUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var session = await useCases.GetAsync(
                new GetContentBlockEditSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);

            return session is null
                ? NotFoundProblem($"ContentBlockEditSession {sessionId} was not found.")
                : Results.Ok(ToContentBlockEditSessionResponse(session));
        });

        group.MapPost("/content-block-edit-sessions/{sessionId}/sync", async (
            string sessionId,
            ContentBlockEditSessionUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var session = await useCases.GetAsync(
                new GetContentBlockEditSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);
            if (session is null)
            {
                return NotFoundProblem($"ContentBlockEditSession {sessionId} was not found.");
            }

            var result = await useCases.SyncAsync(
                new SyncContentBlockEditSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);

            return Results.Ok(ToSyncContentBlockEditSessionResponse(result));
        });

        group.MapPost("/content-block-edit-sessions/{sessionId}/cancel", async (
            string sessionId,
            ContentBlockEditSessionUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var session = await useCases.GetAsync(
                new GetContentBlockEditSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);
            if (session is null)
            {
                return NotFoundProblem($"ContentBlockEditSession {sessionId} was not found.");
            }

            var cancelled = await useCases.CancelAsync(
                new CancelContentBlockEditSessionCommand(currentBank.RootDirectory, sessionId),
                cancellationToken);

            return Results.Ok(ToContentBlockEditSessionResponse(cancelled));
        });
    }

    private static void MapSections(RouteGroupBuilder group)
    {
        group.MapGet("/sections", async (int? teachingTopicId, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(teachingTopicId.HasValue
                ? await unitOfWork.Sections.ListByTeachingTopicAsync(teachingTopicId.Value, cancellationToken)
                : await unitOfWork.Sections.ListAsync(cancellationToken)));

        group.MapGet("/sections/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.Sections.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/sections/{id:int}/title", async (
            int id,
            RenameSectionRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var section = await unitOfWork.Sections.GetByIdAsync(id, cancellationToken);
            if (section is null)
            {
                return NotFoundProblem($"Section {id} was not found.");
            }

            section.Rename(request.Title);
            unitOfWork.Sections.Update(section);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(section);
        });

        group.MapPost("/sections/{sectionId:int}/generate-word", async (
            int sectionId,
            HandoutGenerationUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.GenerateSectionWordAsync(
                new GenerateSectionWordCommand(currentBank.RootDirectory, sectionId),
                cancellationToken);

            return Results.File(result.FileBytes, result.ContentType, result.FileName);
        });

        group.MapPost("/sections/{sectionId:int}/validate-word-generation", async (
            int sectionId,
            HandoutGenerationUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.ValidateSectionWordGenerationAsync(
                new ValidateSectionWordGenerationCommand(currentBank.RootDirectory, sectionId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/sections", async (
            CreateSectionRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.TeachingTopics.GetByIdAsync(request.TeachingTopicId, cancellationToken) is null)
            {
                return NotFoundProblem($"TeachingTopic {request.TeachingTopicId} was not found.");
            }

            var existingSections = await unitOfWork.Sections.ListByTeachingTopicAsync(request.TeachingTopicId, cancellationToken);
            if (existingSections.Count > 0)
            {
                throw new CmsV2ApplicationException("TeachingTopic already has a bound Section.");
            }

            var section = new Section(
                request.TeachingTopicId,
                request.Title,
                request.Description,
                request.Type,
                request.Difficulty,
                request.Status,
                request.SortOrder);
            await unitOfWork.Sections.AddAsync(section, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(section);
        });

        group.MapGet("/sections/{id:int}/items", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.SectionItems.ListBySectionAsync(id, cancellationToken)));

        group.MapPost("/sections/{id:int}/items", async (
            int id,
            AddSectionItemRequest request,
            SectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.AddSectionItemAsync(
                new AddSectionItemCommand(
                    id,
                    request.TargetType,
                    request.TargetId,
                    request.ReferenceMode,
                    request.LockedContentBlockVersionId,
                    request.SortOrder,
                    request.TitleOverride,
                    request.ParentItemId,
                    request.SelectionLayer,
                    request.TeachingUseOverride,
                    request.Status,
                    request.Note),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/sections/{id:int}/items/wrap-as-atomic-section", async (
            int id,
            WrapSectionItemsAsAtomicSectionRequest request,
            SectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.WrapSectionItemsAsAtomicSectionAsync(
                new WrapSectionItemsAsAtomicSectionCommand(
                    id,
                    request.SectionItemIds,
                    request.Title,
                    request.Description,
                    request.Type,
                    request.Difficulty,
                    request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/sections/{id:int}/items/{itemId:int}/move", async (
            int id,
            int itemId,
            MoveSectionItemRequest request,
            SectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<SectionItemMoveDirection>(request.Direction, ignoreCase: true, out var direction))
            {
                return Results.BadRequest(new { message = "Direction must be Up or Down." });
            }

            await useCases.MoveSectionItemAsync(
                new MoveSectionItemCommand(id, itemId, direction),
                cancellationToken);

            return Results.Ok(new { sectionId = id, sectionItemId = itemId, direction = direction.ToString() });
        });

        group.MapDelete("/sections/{id:int}/items/{itemId:int}", async (
            int id,
            int itemId,
            SectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.RemoveSectionItemAsync(
                new RemoveSectionItemCommand(id, itemId),
                cancellationToken);

            return Results.NoContent();
        });
    }

    private static void MapAtomicSections(RouteGroupBuilder group)
    {
        group.MapGet("/atomic-sections", async (ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.AtomicSections.ListAsync(cancellationToken)));

        group.MapGet("/atomic-sections/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.AtomicSections.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/atomic-sections", async (
            CreateAtomicSectionRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var atomicSection = await useCases.CreateAtomicSectionAsync(
                new CreateAtomicSectionCommand(
                    request.SectionId,
                    request.Title,
                    request.Description,
                    request.Type,
                    request.Difficulty,
                    request.Status),
                cancellationToken);

            return Results.Ok(atomicSection);
        });

        group.MapPost("/atomic-sections/{id:int}/title", async (
            int id,
            RenameAtomicSectionRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.RenameAtomicSectionAsync(
                new RenameAtomicSectionCommand(id, request.Title),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/atomic-sections/{id:int}/difficulty", async (
            int id,
            ChangeAtomicSectionDifficultyRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.ChangeAtomicSectionDifficultyAsync(
                new ChangeAtomicSectionDifficultyCommand(id, request.Difficulty),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/atomic-sections/{id:int}/status", async (
            int id,
            ChangeAtomicSectionStatusRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.ChangeAtomicSectionStatusAsync(
                new ChangeAtomicSectionStatusCommand(id, request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/atomic-sections/{id:int}/items", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(id, cancellationToken)));

        group.MapGet("/atomic-sections/{id:int}/panels", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(id, cancellationToken)));

        group.MapPost("/atomic-sections/{id:int}/panels", async (
            int id,
            CreateAtomicSectionPanelRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateAtomicSectionPanelAsync(
                new CreateAtomicSectionPanelCommand(
                    id,
                    request.Title,
                    request.TeachingRole,
                    request.Difficulty,
                    request.BeforeAtomicSectionPanelId,
                    request.AfterAtomicSectionPanelId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPut("/atomic-sections/{id:int}/panels/{panelId:int}", async (
            int id,
            int panelId,
            UpdateAtomicSectionPanelRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.UpdateAtomicSectionPanelAsync(
                new UpdateAtomicSectionPanelCommand(
                    id,
                    panelId,
                    request.Title,
                    request.TeachingRole,
                    request.Difficulty),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/atomic-sections/{id:int}/panels/{panelId:int}/move", async (
            int id,
            int panelId,
            MoveAtomicSectionPanelRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<AtomicSectionPanelMoveDirection>(request.Direction, ignoreCase: true, out var direction))
            {
                return Results.BadRequest(new { message = "Direction must be Up or Down." });
            }

            await useCases.MoveAtomicSectionPanelAsync(
                new MoveAtomicSectionPanelCommand(id, panelId, direction),
                cancellationToken);

            return Results.Ok(new { atomicSectionId = id, atomicSectionPanelId = panelId, direction = direction.ToString() });
        });

        group.MapDelete("/atomic-sections/{id:int}/panels/{panelId:int}", async (
            int id,
            int panelId,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.DeleteAtomicSectionPanelAsync(
                new DeleteAtomicSectionPanelCommand(id, panelId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/atomic-sections/{id:int}/items", async (
            int id,
            AddAtomicSectionItemRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.AddAtomicSectionItemAsync(
                new AddAtomicSectionItemCommand(
                    id,
                    request.ContentBlockId,
                    request.ReferenceMode,
                    request.LockedContentBlockVersionId,
                    request.SortOrder,
                    request.TitleOverride,
                    request.Note,
                    request.AtomicSectionPanelId,
                    request.TeachingRole,
                    request.BeforeAtomicSectionItemId,
                    request.AfterAtomicSectionItemId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/atomic-sections/{id:int}/items/{itemId:int}/classification", async (
            int id,
            int itemId,
            ChangeAtomicSectionItemClassificationRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.ChangeAtomicSectionItemClassificationAsync(
                new ChangeAtomicSectionItemClassificationCommand(
                    id,
                    itemId,
                    request.TeachingRole,
                    request.Difficulty),
                cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/atomic-sections/{id:int}/items/{itemId:int}/move", async (
            int id,
            int itemId,
            MoveAtomicSectionItemRequest request,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<AtomicSectionItemMoveDirection>(request.Direction, ignoreCase: true, out var direction))
            {
                return Results.BadRequest(new { message = "Direction must be Up or Down." });
            }

            await useCases.MoveAtomicSectionItemAsync(
                new MoveAtomicSectionItemCommand(id, itemId, direction),
                cancellationToken);

            return Results.Ok(new { atomicSectionId = id, atomicSectionItemId = itemId, direction = direction.ToString() });
        });

        group.MapDelete("/atomic-sections/{id:int}/items/{itemId:int}", async (
            int id,
            int itemId,
            AtomicSectionUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.RemoveAtomicSectionItemAsync(
                new RemoveAtomicSectionItemCommand(id, itemId),
                cancellationToken);

            return Results.NoContent();
        });
    }

    private static void MapTags(RouteGroupBuilder group)
    {
        group.MapGet("/tags", async (
            string? keyword,
            TagUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.SearchTagsAsync(keyword, cancellationToken)));

        group.MapPost("/tags", async (
            CreateTagRequest request,
            TagUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.CreateTagAsync(
                new CreateTagCommand(request.Name, request.Color),
                cancellationToken)));

        group.MapPatch("/tags/{id:int}", async (
            int id,
            UpdateTagRequest request,
            TagUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.UpdateTagAsync(
                new UpdateTagCommand(id, request.Name, request.Color),
                cancellationToken)));

        group.MapPost("/tags/{id:int}/archive", async (
            int id,
            TagUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.ArchiveTagAsync(
                new ArchiveTagCommand(id),
                cancellationToken)));

        group.MapPost("/tags/{id:int}/restore", async (
            int id,
            TagUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.RestoreTagAsync(
                new RestoreTagCommand(id),
                cancellationToken)));

        group.MapGet("/tag-bindings", async (
            TagBindingTargetType targetType,
            int targetId,
            TagBindingUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.GetTargetTagsAsync(
                new GetTargetTagsCommand(targetType, targetId),
                cancellationToken)));

        group.MapPut("/tag-bindings", async (
            SetTargetTagsRequest request,
            TagBindingUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.SetTargetTagsAsync(
                new SetTargetTagsCommand(request.TargetType, request.TargetId, request.TagIds),
                cancellationToken)));
    }

    private static void MapTeachingNotes(RouteGroupBuilder group)
    {
        group.MapGet("/teaching-notes/{id:int}", async (
            int id,
            TeachingNoteUseCases useCases,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.TeachingNotes.GetByIdAsync(id, cancellationToken) is null)
            {
                return NotFoundProblem($"TeachingNote {id} was not found.");
            }

            return Results.Ok(await useCases.GetTeachingNoteAsync(id, cancellationToken));
        });

        group.MapGet("/teaching-notes", async (
            string? keyword,
            TeachingNoteType? noteType,
            TeachingNoteEffectLevel? effectLevel,
            TeachingNoteBindingTargetType? targetType,
            int? targetId,
            DateTimeOffset? occurredFrom,
            DateTimeOffset? occurredTo,
            TeachingNoteUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.ListTeachingNotesAsync(
                new SearchTeachingNotesCommand(
                    keyword,
                    noteType,
                    effectLevel,
                    targetType,
                    targetId,
                    occurredFrom,
                    occurredTo),
                cancellationToken)));

        group.MapGet("/teaching-note-bindings", async (
            TeachingNoteBindingTargetType targetType,
            int targetId,
            TeachingNoteUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.ListTeachingNotesAsync(
                new SearchTeachingNotesCommand(TargetType: targetType, TargetId: targetId),
                cancellationToken)));

        group.MapPost("/teaching-notes", async (
            CreateTeachingNoteRequest request,
            TeachingNoteUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateTeachingNoteAsync(
                new CreateTeachingNoteCommand(
                    request.NoteType,
                    request.Content,
                    request.EffectLevel,
                    request.OccurredAt,
                    ToBindingCommands(request.Bindings)),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPatch("/teaching-notes/{id:int}", async (
            int id,
            JsonElement request,
            TeachingNoteUseCases useCases,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.TeachingNotes.GetByIdAsync(id, cancellationToken) is null)
            {
                return NotFoundProblem($"TeachingNote {id} was not found.");
            }

            var current = await useCases.GetTeachingNoteAsync(id, cancellationToken);
            var result = await useCases.UpdateTeachingNoteAsync(
                new UpdateTeachingNoteCommand(
                    id,
                    ReadEnumProperty(request, "noteType", current.NoteType),
                    ReadStringProperty(request, "content", current.Content),
                    ReadNullableEnumProperty(request, "effectLevel", current.EffectLevel),
                    ReadNullableDateTimeOffsetProperty(request, "occurredAt", current.OccurredAt),
                    ReadBindingsProperty(request, "bindings", current.Bindings)),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapDelete("/teaching-notes/{id:int}", async (
            int id,
            TeachingNoteUseCases useCases,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.TeachingNotes.GetByIdAsync(id, cancellationToken) is null)
            {
                return NotFoundProblem($"TeachingNote {id} was not found.");
            }

            await useCases.DeleteTeachingNoteAsync(new DeleteTeachingNoteCommand(id), cancellationToken);

            return Results.NoContent();
        });
    }

    private static void MapSectionVariants(RouteGroupBuilder group)
    {
        group.MapGet("/section-variants", async (int? sectionId, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(sectionId.HasValue
                ? await unitOfWork.SectionVariants.ListBySectionAsync(sectionId.Value, cancellationToken)
                : await unitOfWork.SectionVariants.ListAsync(cancellationToken)));

        group.MapGet("/section-variants/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.SectionVariants.GetByIdAsync(id, cancellationToken)));

        group.MapGet("/section-variants/tree", async (
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
            Results.Ok(await useCases.GetSectionVariantSelectionTreeAsync(cancellationToken)));

        group.MapPost("/section-variants/selection-preview", async (
            PreviewSectionVariantSelectionRequest request,
            SectionVariantUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.PreviewSectionVariantSelectionAsync(
                new PreviewSectionVariantSelectionCommand(request.SectionId, request.Difficulty),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/section-variants", async (
            CreateSectionVariantRequest request,
            SectionVariantUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateSectionVariantAsync(
                new CreateSectionVariantCommand(
                    request.SectionId,
                    request.Title,
                    request.Description,
                    request.Type,
                    request.Difficulty,
                    request.SelectedSectionItemIds),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/section-variants/{id:int}/items", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(id, cancellationToken)));

        group.MapDelete("/section-variants/{id:int}", async (
            int id,
            SectionVariantUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.DeleteSectionVariantAsync(
                new DeleteSectionVariantCommand(id),
                cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/section-variants/{id:int}/items", async (
            int id,
            AddSectionVariantItemRequest request,
            SectionVariantUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.AddSectionVariantItemAsync(
                new AddSectionVariantItemCommand(id, request.SectionItemId, request.SortOrder, request.Note),
                cancellationToken);

            return Results.Ok(result);
        });
    }

    private static void MapHandouts(RouteGroupBuilder group)
    {
        group.MapGet("/handouts", async (ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.Handouts.ListAsync(cancellationToken)));

        group.MapGet("/handouts/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.Handouts.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/handouts", async (
            CreateHandoutRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateHandoutAsync(
                new CreateHandoutCommand(request.Title, request.Description, request.Status),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPatch("/handouts/{id:int}", async (
            int id,
            UpdateHandoutRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.UpdateHandoutAsync(
                new UpdateHandoutCommand(id, request.Title, request.Description, request.Status),
                cancellationToken);

            return Results.Ok(new { handoutId = id });
        });

        group.MapGet("/handouts/{id:int}/versions", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.HandoutVersions.ListByHandoutAsync(id, cancellationToken)));

        group.MapGet("/handout-versions/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.HandoutVersions.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/handouts/{id:int}/versions", async (
            int id,
            CreateHandoutVersionRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateHandoutVersionAsync(
                new CreateHandoutVersionCommand(
                    id,
                    request.Title,
                    request.Description,
                    request.Type,
                    request.Status,
                    request.SortOrder),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPatch("/handout-versions/{id:int}", async (
            int id,
            UpdateHandoutVersionRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.UpdateHandoutVersionAsync(
                new UpdateHandoutVersionCommand(
                    id,
                    request.Title,
                    request.Description,
                    request.Type,
                    request.Status,
                    request.SortOrder),
                cancellationToken);

            return Results.Ok(new { handoutVersionId = id });
        });

        group.MapGet("/handout-versions/{id:int}/items", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(id, cancellationToken)));

        group.MapGet("/handout-versions/{id:int}/workspace", async (
            int id,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var workspace = await useCases.GetHandoutVersionWorkspaceAsync(
                new GetHandoutVersionWorkspaceCommand(id),
                cancellationToken);

            return Results.Ok(workspace);
        });

        group.MapPost("/handout-versions/{id:int}/items", async (
            int id,
            AddHandoutVersionItemRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.AddHandoutVersionItemAsync(
                new AddHandoutVersionItemCommand(
                    id,
                    request.TargetType,
                    request.TargetId,
                    request.SortOrder,
                    request.TitleOverride,
                    request.Note,
                    request.AfterHandoutVersionItemId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/handout-versions/{id:int}/items/batch-add-section-variants", async (
            int id,
            BatchAddSectionVariantsToHandoutVersionRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.BatchAddSectionVariantsAsync(
                new BatchAddSectionVariantsCommand(
                    id,
                    request.SectionVariantIds ?? [],
                    request.InsertAfterHandoutVersionItemId),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPatch("/handout-versions/{id:int}/items/{itemId:int}", async (
            int id,
            int itemId,
            UpdateHandoutVersionItemRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.UpdateHandoutVersionItemAsync(
                new UpdateHandoutVersionItemCommand(
                    id,
                    itemId,
                    request.TitleOverride,
                    request.Note),
                cancellationToken);

            return Results.Ok(new { handoutVersionId = id, handoutVersionItemId = itemId });
        });

        group.MapPost("/handout-versions/{id:int}/items/{itemId:int}/move", async (
            int id,
            int itemId,
            MoveHandoutVersionItemRequest request,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<HandoutVersionItemMoveDirection>(request.Direction, ignoreCase: true, out var direction))
            {
                return Results.BadRequest(new { message = "Direction must be Up or Down." });
            }

            await useCases.MoveHandoutVersionItemAsync(
                new MoveHandoutVersionItemCommand(id, itemId, direction),
                cancellationToken);

            return Results.Ok(new { handoutVersionId = id, handoutVersionItemId = itemId, direction = direction.ToString() });
        });

        group.MapDelete("/handout-versions/{id:int}/items/{itemId:int}", async (
            int id,
            int itemId,
            HandoutUseCases useCases,
            CancellationToken cancellationToken) =>
        {
            await useCases.RemoveHandoutVersionItemAsync(
                new RemoveHandoutVersionItemCommand(id, itemId),
                cancellationToken);

            return Results.NoContent();
        });
    }

    private static void MapOutputs(RouteGroupBuilder group)
    {
        group.MapGet("/output-templates", async (ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.OutputTemplates.ListAsync(cancellationToken)));

        group.MapGet("/output-templates/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.OutputTemplates.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/output-templates", async (
            CreateOutputTemplateRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var template = new OutputTemplate(request.Title, request.TemplateDocxPath, request.Description, request.Status);
            await unitOfWork.OutputTemplates.AddAsync(template, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(template);
        });

        group.MapPost("/output-templates/validate", (
            ValidateOutputTemplateRequest request,
            IOutputTemplatePathResolver outputTemplatePathResolver) =>
        {
            var templateDocxPath = request.TemplateDocxPath.Trim();
            if (string.IsNullOrWhiteSpace(templateDocxPath))
            {
                return Results.Ok(new ValidateOutputTemplateResponse(false, "OutputTemplate DOCX path is required."));
            }

            if (!string.Equals(Path.GetExtension(templateDocxPath), ".docx", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok(new ValidateOutputTemplateResponse(false, "OutputTemplate path must point to a .docx file."));
            }

            var resolvedTemplateDocxPath = outputTemplatePathResolver.ResolveTemplateDocxPath(templateDocxPath);
            if (!File.Exists(resolvedTemplateDocxPath))
            {
                return Results.Ok(new ValidateOutputTemplateResponse(false, $"OutputTemplate file was not found: {templateDocxPath}"));
            }

            return Results.Ok(new ValidateOutputTemplateResponse(true, "OutputTemplate DOCX is ready."));
        });

        group.MapGet("/output-forms", async (
            int? handoutVersionId,
            int? templateId,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (handoutVersionId.HasValue)
            {
                return Results.Ok(await unitOfWork.OutputForms.ListByHandoutVersionAsync(handoutVersionId.Value, cancellationToken));
            }

            if (templateId.HasValue)
            {
                return Results.Ok(await unitOfWork.OutputForms.ListByTemplateAsync(templateId.Value, cancellationToken));
            }

            return Results.Ok(await unitOfWork.OutputForms.ListAsync(cancellationToken));
        });

        group.MapGet("/output-forms/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.OutputForms.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/output-forms", async (
            CreateOutputFormRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.HandoutVersions.GetByIdAsync(request.HandoutVersionId, cancellationToken) is null)
            {
                return NotFoundProblem($"HandoutVersion {request.HandoutVersionId} was not found.");
            }

            if (await unitOfWork.OutputTemplates.GetByIdAsync(request.OutputTemplateId, cancellationToken) is null)
            {
                return NotFoundProblem($"OutputTemplate {request.OutputTemplateId} was not found.");
            }

            var outputForm = new OutputForm(
                request.HandoutVersionId,
                request.OutputTemplateId,
                request.Title,
                request.Audience,
                request.OutputFormat,
                request.VisibilityMode,
                request.Status,
                request.SortOrder);
            await unitOfWork.OutputForms.AddAsync(outputForm, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(outputForm);
        });

        group.MapPost("/output-forms/{id:int}/generate-word", async (
            int id,
            GenerateHandoutWordRequest request,
            HandoutGenerationUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.GenerateHandoutWordAsync(
                new GenerateHandoutWordCommand(currentBank.RootDirectory, id, request.GeneratedTime),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapPost("/output-forms/{id:int}/validate-word-generation", async (
            int id,
            HandoutGenerationUseCases useCases,
            CmsV2CurrentBank currentBank,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.ValidateHandoutWordGenerationAsync(
                new ValidateHandoutWordGenerationCommand(currentBank.RootDirectory, id),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/output-forms/{id:int}/generated-files", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.GeneratedFiles.ListByOutputFormAsync(id, cancellationToken)));

        group.MapGet("/generated-files/{id:int}/manifest", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
        {
            var generatedFile = await unitOfWork.GeneratedFiles.GetByIdAsync(id, cancellationToken);
            if (generatedFile is null)
            {
                return NotFoundProblem($"GeneratedFile {id} was not found.");
            }

            return Results.Text(generatedFile.VersionManifestJson, "application/json");
        });

        group.MapGet("/generated-files/{id:int}/download", async (
            int id,
            ICmsV2UnitOfWork unitOfWork,
            IContentBlockFileStore fileStore,
            CancellationToken cancellationToken) =>
        {
            var generatedFile = await unitOfWork.GeneratedFiles.GetByIdAsync(id, cancellationToken);
            if (generatedFile is null)
            {
                return NotFoundProblem($"GeneratedFile {id} was not found.");
            }

            var bytes = await fileStore.ReadContentBlockDocxAsync(generatedFile.FilePath, cancellationToken);
            if (bytes is null)
            {
                return NotFoundProblem($"Generated file was not found: {generatedFile.FilePath}");
            }

            return Results.File(bytes, DocxContentType, Path.GetFileName(generatedFile.FilePath));
        });

        group.MapDelete("/generated-files/{id:int}", async (
            int id,
            ICmsV2UnitOfWork unitOfWork,
            IContentBlockFileStore fileStore,
            CancellationToken cancellationToken) =>
        {
            var generatedFile = await unitOfWork.GeneratedFiles.GetByIdAsync(id, cancellationToken);
            if (generatedFile is null)
            {
                return NotFoundProblem($"GeneratedFile {id} was not found.");
            }

            unitOfWork.GeneratedFiles.Remove(generatedFile);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await fileStore.DeleteIfExistsAsync(generatedFile.FilePath, cancellationToken);

            return Results.NoContent();
        });
    }

    private static async Task<IResult> DownloadContentBlockVersionAsync(
        ContentBlock block,
        ContentBlockVersion? version,
        IContentBlockFileStore fileStore,
        CancellationToken cancellationToken)
    {
        if (version is null)
        {
            return NotFoundProblem($"ContentBlock {block.Id} does not have the requested version.");
        }

        var bytes = await fileStore.ReadContentBlockDocxAsync(version.DocxPath, cancellationToken);
        if (bytes is null)
        {
            return NotFoundProblem($"ContentBlockVersion DOCX file was not found: {version.DocxPath}");
        }

        return Results.File(bytes, DocxContentType, $"{block.Title}-v{version.VersionNumber}.docx");
    }

    private static async Task<IResult> ReadHtmlPreviewAsync(
        ContentBlockVersion? version,
        IContentBlockFileStore fileStore,
        CancellationToken cancellationToken)
    {
        if (version?.HtmlPreviewPath is null)
        {
            return NotFoundProblem("ContentBlockVersion HTML preview was not found.");
        }

        var html = await fileStore.ReadHtmlPreviewAsync(version.HtmlPreviewPath, cancellationToken);
        if (html is null)
        {
            return NotFoundProblem($"HTML preview file was not found: {version.HtmlPreviewPath}");
        }

        return Results.Text(html, "text/html; charset=utf-8");
    }

    private static ContentBlockEditSessionResponse ToContentBlockEditSessionResponse(ContentBlockEditSession session)
    {
        return new ContentBlockEditSessionResponse(
            session.SessionId,
            session.ContentBlockId,
            session.SourceContentBlockVersionId,
            session.Status.ToString(),
            session.LaunchMode.ToString(),
            session.OpenedByServer,
            session.Message,
            session.CreatedTime,
            session.UpdatedTime);
    }

    private static SyncContentBlockEditSessionResponse ToSyncContentBlockEditSessionResponse(
        SyncContentBlockEditSessionResult result)
    {
        return new SyncContentBlockEditSessionResponse(
            result.SessionId,
            result.ContentBlockId,
            result.Changed,
            result.NewContentBlockVersionId,
            result.CurrentVersionNumber,
            result.Status.ToString(),
            result.Message);
    }

    private static IReadOnlyList<TeachingNoteBindingCommand>? ToBindingCommands(
        IReadOnlyList<TeachingNoteBindingRequest>? bindings)
    {
        return bindings?
            .Select(binding => new TeachingNoteBindingCommand(binding.TargetType, binding.TargetId))
            .ToArray();
    }

    private static string ReadStringProperty(JsonElement request, string propertyName, string currentValue)
    {
        if (!request.TryGetProperty(propertyName, out var property))
        {
            return currentValue;
        }

        return property.ValueKind == JsonValueKind.Null
            ? string.Empty
            : property.GetString() ?? string.Empty;
    }

    private static TEnum ReadEnumProperty<TEnum>(JsonElement request, string propertyName, TEnum currentValue)
        where TEnum : struct, Enum
    {
        if (!request.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return currentValue;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String when Enum.TryParse<TEnum>(property.GetString(), ignoreCase: true, out var parsed) => parsed,
            JsonValueKind.Number when property.TryGetInt32(out var value) && Enum.IsDefined(typeof(TEnum), value) => (TEnum)Enum.ToObject(typeof(TEnum), value),
            _ => throw new CmsV2ApplicationException($"{propertyName} is invalid.")
        };
    }

    private static TEnum? ReadNullableEnumProperty<TEnum>(JsonElement request, string propertyName, TEnum? currentValue)
        where TEnum : struct, Enum
    {
        if (!request.TryGetProperty(propertyName, out var property))
        {
            return currentValue;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return ReadEnumValue<TEnum>(property, propertyName);
    }

    private static DateTimeOffset? ReadNullableDateTimeOffsetProperty(
        JsonElement request,
        string propertyName,
        DateTimeOffset? currentValue)
    {
        if (!request.TryGetProperty(propertyName, out var property))
        {
            return currentValue;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            && DateTimeOffset.TryParse(property.GetString(), out var parsed)
                ? parsed
                : throw new CmsV2ApplicationException($"{propertyName} is invalid.");
    }

    private static IReadOnlyList<TeachingNoteBindingCommand> ReadBindingsProperty(
        JsonElement request,
        string propertyName,
        IReadOnlyList<TeachingNoteBindingDto> currentBindings)
    {
        if (!request.TryGetProperty(propertyName, out var property))
        {
            return currentBindings
                .Select(binding => new TeachingNoteBindingCommand(binding.TargetType, binding.TargetId))
                .ToArray();
        }

        if (property.ValueKind != JsonValueKind.Array)
        {
            throw new CmsV2ApplicationException($"{propertyName} must be an array.");
        }

        var bindings = new List<TeachingNoteBindingCommand>();
        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                throw new CmsV2ApplicationException("Teaching note binding must be an object.");
            }

            if (!item.TryGetProperty("targetType", out var targetTypeProperty))
            {
                throw new CmsV2ApplicationException("Teaching note binding targetType is required.");
            }

            if (!item.TryGetProperty("targetId", out var targetIdProperty)
                || !targetIdProperty.TryGetInt32(out var targetId))
            {
                throw new CmsV2ApplicationException("Teaching note binding targetId is required.");
            }

            bindings.Add(new TeachingNoteBindingCommand(
                ReadEnumValue<TeachingNoteBindingTargetType>(targetTypeProperty, "targetType"),
                targetId));
        }

        return bindings;
    }

    private static TEnum ReadEnumValue<TEnum>(JsonElement property, string propertyName)
        where TEnum : struct, Enum
    {
        return property.ValueKind switch
        {
            JsonValueKind.String when Enum.TryParse<TEnum>(property.GetString(), ignoreCase: true, out var parsed) => parsed,
            JsonValueKind.Number when property.TryGetInt32(out var value) && Enum.IsDefined(typeof(TEnum), value) => (TEnum)Enum.ToObject(typeof(TEnum), value),
            _ => throw new CmsV2ApplicationException($"{propertyName} is invalid.")
        };
    }

    private static async Task<IResult> OkOrNotFoundAsync<T>(Task<T?> entityTask)
        where T : class
    {
        var entity = await entityTask;
        return entity is null ? NotFoundProblem($"{typeof(T).Name} was not found.") : Results.Ok(entity);
    }

    private static IResult NotFoundProblem(string detail)
    {
        return Results.Problem(detail: detail, title: "Resource not found.", statusCode: StatusCodes.Status404NotFound);
    }

    private static IResult BadRequestProblem(string detail)
    {
        return Results.Problem(detail: detail, title: "Bad request.", statusCode: StatusCodes.Status400BadRequest);
    }

    private static string[] EnumNames<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>();
    }
}
