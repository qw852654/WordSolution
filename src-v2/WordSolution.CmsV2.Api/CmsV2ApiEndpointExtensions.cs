using Microsoft.Extensions.Options;
using WordSolution.CmsV2.Application.AtomicSections;
using WordSolution.CmsV2.Application.ContentBlocks;
using WordSolution.CmsV2.Application.Handouts;
using WordSolution.CmsV2.Application.SectionVariants;
using WordSolution.CmsV2.Application.Sections;
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

        group.MapGet("/health", (IOptions<CmsV2ApiOptions> options) => Results.Ok(new
        {
            status = "ok",
            bankRootDirectory = options.Value.BankRootDirectory
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
            teachingNoteTargetType = EnumNames<TeachingNoteTargetType>(),
            teachingNoteType = EnumNames<TeachingNoteType>(),
            teachingNoteStatus = EnumNames<TeachingNoteStatus>()
        }));

        MapTeachingTopics(group);
        MapContentBlocks(group);
        MapSections(group);
        MapAtomicSections(group);
        MapSectionVariants(group);
        MapHandouts(group);
        MapOutputs(group);
        MapTeachingNotes(group);

        return app;
    }

    private static void MapTeachingTopics(RouteGroupBuilder group)
    {
        group.MapGet("/teaching-topics", async (ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.TeachingTopics.ListAsync(cancellationToken)));

        group.MapGet("/teaching-topics/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.TeachingTopics.GetByIdAsync(id, cancellationToken)));

        group.MapGet("/teaching-topics/{id:int}/children", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.TeachingTopics.ListChildrenAsync(id, cancellationToken)));

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
        group.MapGet("/content-blocks", async (ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.ContentBlocks.ListAsync(cancellationToken)));

        group.MapGet("/content-blocks/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.ContentBlocks.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/content-blocks/blank-document", async (
            CreateContentBlockWithBlankDocumentRequest request,
            ContentBlockDocumentUseCases useCases,
            IOptions<CmsV2ApiOptions> options,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.CreateContentBlockWithBlankDocumentAsync(
                new CreateContentBlockWithBlankDocumentCommand(
                    options.Value.BankRootDirectory,
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
            IOptions<CmsV2ApiOptions> options,
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
                    options.Value.BankRootDirectory,
                    id,
                    stream,
                    setAsCurrent),
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
    }

    private static void MapSections(RouteGroupBuilder group)
    {
        group.MapGet("/sections", async (int? teachingTopicId, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(teachingTopicId.HasValue
                ? await unitOfWork.Sections.ListByTeachingTopicAsync(teachingTopicId.Value, cancellationToken)
                : await unitOfWork.Sections.ListAsync(cancellationToken)));

        group.MapGet("/sections/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.Sections.GetByIdAsync(id, cancellationToken)));

        group.MapPost("/sections", async (
            CreateSectionRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.TeachingTopics.GetByIdAsync(request.TeachingTopicId, cancellationToken) is null)
            {
                return NotFoundProblem($"TeachingTopic {request.TeachingTopicId} was not found.");
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
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (await unitOfWork.Sections.GetByIdAsync(request.SectionId, cancellationToken) is null)
            {
                return NotFoundProblem($"Section {request.SectionId} was not found.");
            }

            var atomicSection = new AtomicSection(
                request.SectionId,
                request.Title,
                request.Description,
                request.Type,
                request.Difficulty,
                request.Status);
            await unitOfWork.AtomicSections.AddAsync(atomicSection, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

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

        group.MapGet("/atomic-sections/{id:int}/items", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(id, cancellationToken)));

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
                    request.Note),
                cancellationToken);

            return Results.Ok(result);
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

    private static void MapSectionVariants(RouteGroupBuilder group)
    {
        group.MapGet("/section-variants", async (int? sectionId, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(sectionId.HasValue
                ? await unitOfWork.SectionVariants.ListBySectionAsync(sectionId.Value, cancellationToken)
                : await unitOfWork.SectionVariants.ListAsync(cancellationToken)));

        group.MapGet("/section-variants/{id:int}", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            await OkOrNotFoundAsync(unitOfWork.SectionVariants.GetByIdAsync(id, cancellationToken)));

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
                    request.Status,
                    request.SortOrder),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/section-variants/{id:int}/items", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.SectionVariantItems.ListBySectionVariantAsync(id, cancellationToken)));

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
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var handout = new Handout(request.Title, request.Description, request.Status);
            await unitOfWork.Handouts.AddAsync(handout, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(handout);
        });

        group.MapGet("/handouts/{id:int}/versions", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.HandoutVersions.ListByHandoutAsync(id, cancellationToken)));

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

        group.MapGet("/handout-versions/{id:int}/items", async (int id, ICmsV2UnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            Results.Ok(await unitOfWork.HandoutVersionItems.ListByHandoutVersionAsync(id, cancellationToken)));

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
                    request.Note),
                cancellationToken);

            return Results.Ok(result);
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
            IOptions<CmsV2ApiOptions> options,
            CancellationToken cancellationToken) =>
        {
            var result = await useCases.GenerateHandoutWordAsync(
                new GenerateHandoutWordCommand(options.Value.BankRootDirectory, id, request.GeneratedTime),
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
    }

    private static void MapTeachingNotes(RouteGroupBuilder group)
    {
        group.MapGet("/teaching-notes", async (
            TeachingNoteTargetType? targetType,
            int? targetId,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (targetType.HasValue && targetId.HasValue)
            {
                return Results.Ok(await unitOfWork.TeachingNotes.ListByTargetAsync(targetType.Value, targetId.Value, cancellationToken));
            }

            return Results.Ok(await unitOfWork.TeachingNotes.ListAsync(cancellationToken));
        });

        group.MapPost("/teaching-notes", async (
            CreateTeachingNoteRequest request,
            ICmsV2UnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var note = new TeachingNote(
                request.TargetType,
                request.TargetId,
                request.NoteType,
                request.Title,
                request.Content,
                request.Status);
            await unitOfWork.TeachingNotes.AddAsync(note, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Results.Ok(note);
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

    private static string[] EnumNames<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>();
    }
}
