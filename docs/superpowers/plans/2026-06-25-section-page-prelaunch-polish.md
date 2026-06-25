# SectionPage Prelaunch Polish Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking. This repository/user workflow forbids staging, committing, pushing, resetting, or checking out branches during this task.

**Goal:** Complete the confirmed SectionPage prelaunch polish: contextual AS panel insert points, default AS panels, editable AS status, derived incomplete marker, and compact Inspector.

**Architecture:** Keep server-confirmed updates. Backend owns creation invariants for new `AtomicSection` default panels and status changes. Frontend uses existing `InsertPoint`, `AtomicSectionBlock`, `SectionInspector`, `SectionPage`, and `useSectionPageData` patterns without creating a second interaction system.

**Tech Stack:** .NET / EF Core / xUnit for CMS V2 backend; Vue 3 / TypeScript / Vite / Tailwind / shadcn-vue / vue-i18n for `frontend-v2`.

---

## Source Spec

Use this document as the product source:

```text
docs/ui/section-page-prelaunch-polish-development-doc.md
```

Before implementation, read:

```text
AGENTS.md
CONTRIBUTING.md
.codex/内容管理系统详细架构.md
.codex/内容管理系统升级路线.md
docs/ui/ui-architecture.md
docs/ui/component-rules.md
docs/ui/section-page.md
docs/ui/focus-tree.md
docs/ui/i18n.md
docs/ui/codex-workflow.md
docs/cms-v2/backend/后端重建阶段计划.md
docs/cms-v2/backend/后端数据模型开发文档.md
docs/cms-v2/backend/领域模型结构说明.md
```

## Hard Boundaries

- Do not modify V1, VSTO, `Word本地文件操作核心库`, or `题库本地服务/wwwroot`.
- Do not stage, commit, push, reset, checkout, or create branches.
- Do not create a new UI system beside `InsertPoint`.
- Do not move Tags or TeachingNotes out of Inspector in this round.
- Do not add new `AtomicSectionStatus` enum values.
- Do not add the global “incomplete AS” filter page in this round.
- If backend/API changes are made, the final report must say the backend should be restarted by the control thread.

## Expected Worktree Start

Run:

```powershell
git branch --show-current
git status --short
```

Expected:

```text
feature/rebuildUI
```

`git status --short` may include this plan and the development document. If unrelated user changes exist, do not overwrite them.

## File Map

Backend files:

- `src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionCommands.cs`
  - Add `ChangeAtomicSectionStatusCommand`.
- `src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionUseCases.cs`
  - Create default panels after AS creation.
  - Add `ChangeAtomicSectionStatusAsync`.
  - Add private helper for default panel creation.
- `src-v2/WordSolution.CmsV2.Application/Sections/SectionUseCases.cs`
  - Create default panels in `WrapSectionItemsAsAtomicSectionAsync` after wrapped AS creation.
- `src-v2/WordSolution.CmsV2.Api/CmsV2ApiRequests.cs`
  - Add `ChangeAtomicSectionStatusRequest`.
- `src-v2/WordSolution.CmsV2.Api/CmsV2ApiEndpointExtensions.cs`
  - Add `POST /api/cms-v2/atomic-sections/{id}/status`.
- `src-v2/WordSolution.CmsV2.Tests/Application/CmsV2ApplicationUseCaseTests.cs`
  - Update AS creation test and add wrap-as-AS panel assertions.
- `src-v2/WordSolution.CmsV2.Tests/Api/CmsV2ApiIntegrationTests.cs`
  - Add default panel and AS status API coverage.

Frontend files:

- `frontend-v2/src/types/index.ts`
  - Add `CreateAtomicSectionPanel` to `InsertActionType`.
  - Add `AtomicSectionPanelList` parent type.
  - Add `AtomicSectionStatusValue`.
  - Add `hasEmptyPanel` to `StructuredBlockModel`.
  - Add `hasEmptyPanel` to `SectionTreeNodeModel`.
- `frontend-v2/src/components/presentation/InsertPoint.vue`
  - Render a `CreateAtomicSectionPanel` button when allowed.
- `frontend-v2/src/components/business/AtomicSectionBlock.vue`
  - Replace constant panel create buttons with panel-list `InsertPoint`.
  - Show compact “待完善” marker when `block.hasEmptyPanel`.
- `frontend-v2/src/components/containers/SectionWorkspace.vue`
  - Pass panel insert actions through to `SectionPage`.
- `frontend-v2/src/composables/useSectionPageData.ts`
  - Derive `hasEmptyPanel` from panels with zero children.
- `frontend-v2/src/components/business/SectionInspector.vue`
  - Compact layout.
  - Add AS status selector.
  - Add AS completeness row.
- `frontend-v2/src/pages/SectionPage.vue`
  - Handle `CreateAtomicSectionPanel` insert action.
  - Handle AS status update.
- `frontend-v2/src/apis/cmsV2Client.ts`
  - Add `changeAtomicSectionStatus`.
- `frontend-v2/src/locales/zh-CN.ts`
  - Add Chinese UI copy.
- `frontend-v2/src/locales/en.ts`
  - Add English fallback copy.
- `docs/ui/component-rules.md`
  - Record final component rules after implementation.
- `docs/ui/section-page.md`
  - Record final SectionPage behavior after implementation.
- `docs/cms-v2/backend/后端数据模型开发文档.md`
  - Update AS creation default panel rule.
- `docs/cms-v2/backend/领域模型结构说明.md`
  - Update AS creation default panel rule.

---

## Task 0: Baseline Audit

**Files:**
- Read only.

- [ ] **Step 1: Confirm current branch and worktree**

Run:

```powershell
git branch --show-current
git status --short
```

Expected:

```text
feature/rebuildUI
```

Proceed only if current branch is not `master`.

- [ ] **Step 2: Locate current AS creation and status endpoints**

Run:

```powershell
rg -n "CreateAtomicSectionAsync|WrapSectionItemsAsAtomicSectionAsync|RenameAtomicSectionAsync|ChangeAtomicSectionDifficultyAsync|atomic-sections/.+difficulty|atomic-sections/.+title" src-v2
```

Expected findings:

```text
src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionUseCases.cs
src-v2/WordSolution.CmsV2.Application/Sections/SectionUseCases.cs
src-v2/WordSolution.CmsV2.Api/CmsV2ApiEndpointExtensions.cs
```

- [ ] **Step 3: Locate current frontend insert and inspector paths**

Run:

```powershell
rg -n "InsertActionType|InsertPointModel|requestInsert|createAtomicSectionPanel|changeAtomicSectionDifficulty|SectionInspector" frontend-v2/src
```

Expected findings:

```text
frontend-v2/src/types/index.ts
frontend-v2/src/components/presentation/InsertPoint.vue
frontend-v2/src/components/business/AtomicSectionBlock.vue
frontend-v2/src/components/containers/SectionWorkspace.vue
frontend-v2/src/pages/SectionPage.vue
frontend-v2/src/components/business/SectionInspector.vue
```

---

## Task 1: Backend Default Panels And AS Status

**Files:**
- Modify: `src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionCommands.cs`
- Modify: `src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionUseCases.cs`
- Modify: `src-v2/WordSolution.CmsV2.Application/Sections/SectionUseCases.cs`
- Modify: `src-v2/WordSolution.CmsV2.Api/CmsV2ApiRequests.cs`
- Modify: `src-v2/WordSolution.CmsV2.Api/CmsV2ApiEndpointExtensions.cs`
- Modify: `src-v2/WordSolution.CmsV2.Tests/Application/CmsV2ApplicationUseCaseTests.cs`
- Modify: `src-v2/WordSolution.CmsV2.Tests/Api/CmsV2ApiIntegrationTests.cs`

### Task 1.1: Write Failing Application Tests

- [ ] **Step 1: Replace the old AS creation expectation**

In `src-v2/WordSolution.CmsV2.Tests/Application/CmsV2ApplicationUseCaseTests.cs`, update test `CreateAtomicSection_creates_empty_atomic_section_without_default_child_blocks` so it becomes:

```csharp
[Fact]
public async Task CreateAtomicSection_creates_default_panels_without_default_content_blocks()
{
    await using var context = await CreateMigratedContextAsync();
    var unitOfWork = new EfCmsV2UnitOfWork(context);
    var atomicSections = new AtomicSectionUseCases(unitOfWork);
    var sectionId = await CreateSectionAsync(unitOfWork);

    var atomicSection = await atomicSections.CreateAtomicSectionAsync(
        new CreateAtomicSectionCommand(
            sectionId,
            "AS Alpha",
            "AS note",
            AtomicSectionType.Custom,
            Difficulty.Advanced,
            AtomicSectionStatus.Draft));

    var panels = await unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(atomicSection.Id);
    var items = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(atomicSection.Id);
    var contentBlocks = await unitOfWork.ContentBlocks.ListAsync();
    var versions = await unitOfWork.ContentBlockVersions.ListAsync();

    Assert.Equal(sectionId, atomicSection.SectionId);
    Assert.Equal("AS Alpha", atomicSection.Title);
    Assert.Equal("AS note", atomicSection.Description);
    Assert.Equal(Difficulty.Advanced, atomicSection.Difficulty);
    Assert.Empty(items);
    Assert.Empty(contentBlocks);
    Assert.Empty(versions);
    Assert.Equal(
        [AtomicSectionTeachingRole.Knowledge, AtomicSectionTeachingRole.Example, AtomicSectionTeachingRole.Variant],
        panels.Select(panel => panel.TeachingRole));
    Assert.All(panels, panel => Assert.Equal("AS Alpha", panel.Title));
    Assert.All(panels, panel => Assert.Equal(Difficulty.Advanced, panel.Difficulty));
    Assert.Equal([10, 20, 30], panels.Select(panel => panel.SortOrder));
}
```

- [ ] **Step 2: Add wrap-as-AS panel test**

In the same test file, add assertions to an existing successful wrap test or add a new test:

```csharp
[Fact]
public async Task WrapSectionItemsAsAtomicSection_creates_default_panels_without_assigning_existing_items()
{
    await using var context = await CreateMigratedContextAsync();
    var unitOfWork = new EfCmsV2UnitOfWork(context);
    var contentBlocks = new ContentBlockUseCases(unitOfWork);
    var sectionUseCases = new SectionUseCases(unitOfWork);
    var sectionId = await CreateSectionAsync(unitOfWork);

    var firstBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
        new CreateContentBlockWithInitialVersionCommand(sectionId, "first", ContentBlockType.Question, "wrap-first/v1.docx"));
    var secondBlock = await contentBlocks.CreateContentBlockWithInitialVersionAsync(
        new CreateContentBlockWithInitialVersionCommand(sectionId, "second", ContentBlockType.Question, "wrap-second/v1.docx"));

    var firstItem = new SectionItem(sectionId, SectionItemTargetType.ContentBlock, firstBlock.Id, ReferenceMode.FollowLatest, null, 10);
    var secondItem = new SectionItem(sectionId, SectionItemTargetType.ContentBlock, secondBlock.Id, ReferenceMode.FollowLatest, null, 20);
    await unitOfWork.SectionItems.AddAsync(firstItem);
    await unitOfWork.SectionItems.AddAsync(secondItem);
    await unitOfWork.SaveChangesAsync();

    var result = await sectionUseCases.WrapSectionItemsAsAtomicSectionAsync(
        new WrapSectionItemsAsAtomicSectionCommand(
            sectionId,
            [firstItem.Id, secondItem.Id],
            "Wrapped AS",
            null,
            AtomicSectionType.Custom,
            Difficulty.Medium,
            AtomicSectionStatus.Draft));

    var panels = await unitOfWork.AtomicSectionPanels.ListByAtomicSectionAsync(result.AtomicSectionId);
    var atomicItems = await unitOfWork.AtomicSectionItems.ListByAtomicSectionAsync(result.AtomicSectionId);

    Assert.Equal(
        [AtomicSectionTeachingRole.Knowledge, AtomicSectionTeachingRole.Example, AtomicSectionTeachingRole.Variant],
        panels.Select(panel => panel.TeachingRole));
    Assert.All(panels, panel => Assert.Equal("Wrapped AS", panel.Title));
    Assert.All(panels, panel => Assert.Equal(Difficulty.Medium, panel.Difficulty));
    Assert.All(atomicItems, item => Assert.Null(item.AtomicSectionPanelId));
}
```

- [ ] **Step 3: Add AS status update application test**

Add:

```csharp
[Fact]
public async Task ChangeAtomicSectionStatus_updates_status()
{
    await using var context = await CreateMigratedContextAsync();
    var unitOfWork = new EfCmsV2UnitOfWork(context);
    var atomicSections = new AtomicSectionUseCases(unitOfWork);
    var sectionId = await CreateSectionAsync(unitOfWork);
    var atomicSection = await atomicSections.CreateAtomicSectionAsync(
        new CreateAtomicSectionCommand(sectionId, "Status AS"));

    var updated = await atomicSections.ChangeAtomicSectionStatusAsync(
        new ChangeAtomicSectionStatusCommand(atomicSection.Id, AtomicSectionStatus.Active));

    Assert.Equal(AtomicSectionStatus.Active, updated.Status);
}
```

- [ ] **Step 4: Run tests and confirm expected failure**

Run:

```powershell
dotnet test src-v2/WordSolution.CmsV2.Tests/WordSolution.CmsV2.Tests.csproj --filter "CreateAtomicSection_creates_default_panels_without_default_content_blocks|WrapSectionItemsAsAtomicSection_creates_default_panels_without_assigning_existing_items|ChangeAtomicSectionStatus_updates_status"
```

Expected before implementation:

```text
Failed
```

One failure should show no default panels. One compile failure may show `ChangeAtomicSectionStatusAsync` or `ChangeAtomicSectionStatusCommand` does not exist.

### Task 1.2: Implement Default Panel Creation

- [ ] **Step 1: Add command**

Append to `src-v2/WordSolution.CmsV2.Application/AtomicSections/AtomicSectionCommands.cs`:

```csharp
public sealed record ChangeAtomicSectionStatusCommand(
    int AtomicSectionId,
    AtomicSectionStatus Status);
```

- [ ] **Step 2: Add default panel helper**

In `AtomicSectionUseCases`, add a private helper near other private helpers:

```csharp
private static IReadOnlyList<(AtomicSectionTeachingRole TeachingRole, int SortOrder)> DefaultPanelDefinitions { get; } =
[
    (AtomicSectionTeachingRole.Knowledge, 10),
    (AtomicSectionTeachingRole.Example, 20),
    (AtomicSectionTeachingRole.Variant, 30),
];

private async Task CreateDefaultPanelsForAtomicSectionAsync(
    AtomicSection atomicSection,
    CancellationToken cancellationToken)
{
    foreach (var definition in DefaultPanelDefinitions)
    {
        await _unitOfWork.AtomicSectionPanels.AddAsync(
            new AtomicSectionPanel(
                atomicSection.Id,
                atomicSection.Title,
                definition.TeachingRole,
                atomicSection.Difficulty,
                definition.SortOrder),
            cancellationToken);
    }
}
```

- [ ] **Step 3: Call helper in `CreateAtomicSectionAsync`**

After `await _unitOfWork.SaveChangesAsync(transactionCancellationToken);` that assigns `atomicSection.Id`, call:

```csharp
await CreateDefaultPanelsForAtomicSectionAsync(atomicSection, transactionCancellationToken);
await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
```

Set `result = atomicSection;` after the second save.

- [ ] **Step 4: Call helper in wrap-as-AS**

Because `SectionUseCases` owns `WrapSectionItemsAsAtomicSectionAsync`, add an equivalent private helper to `SectionUseCases` and call it after wrapped AS is saved and before creating `AtomicSectionItem`.

Use the same default definitions:

```csharp
private async Task CreateDefaultPanelsForAtomicSectionAsync(
    AtomicSection atomicSection,
    CancellationToken cancellationToken)
{
    foreach (var definition in new[]
    {
        (AtomicSectionTeachingRole.Knowledge, SortOrder: 10),
        (AtomicSectionTeachingRole.Example, SortOrder: 20),
        (AtomicSectionTeachingRole.Variant, SortOrder: 30),
    })
    {
        await _unitOfWork.AtomicSectionPanels.AddAsync(
            new AtomicSectionPanel(
                atomicSection.Id,
                atomicSection.Title,
                definition.Item1,
                atomicSection.Difficulty,
                definition.SortOrder),
            cancellationToken);
    }
}
```

Call:

```csharp
await CreateDefaultPanelsForAtomicSectionAsync(atomicSection, transactionCancellationToken);
await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
```

Then continue creating wrapped `AtomicSectionItem` with `AtomicSectionPanelId = null`.

### Task 1.3: Implement AS Status API

- [ ] **Step 1: Add use case method**

In `AtomicSectionUseCases`, add:

```csharp
public async Task<AtomicSection> ChangeAtomicSectionStatusAsync(
    ChangeAtomicSectionStatusCommand command,
    CancellationToken cancellationToken = default)
{
    AtomicSection? result = null;

    await _unitOfWork.ExecuteInTransactionAsync(async transactionCancellationToken =>
    {
        var atomicSection = await GetAtomicSectionForCommandAsync(
            command.AtomicSectionId,
            transactionCancellationToken);

        atomicSection.ChangeStatus(command.Status);
        _unitOfWork.AtomicSections.Update(atomicSection);
        await _unitOfWork.SaveChangesAsync(transactionCancellationToken);
        result = atomicSection;
    }, cancellationToken);

    return result!;
}
```

Add `ChangeStatus` to `src-v2/WordSolution.CmsV2.Domain/Entities/AtomicSection.cs` beside `ChangeDifficulty`:

```csharp
public void ChangeStatus(AtomicSectionStatus status, DateTimeOffset? updatedTime = null)
{
    DomainGuard.ValidEnum(status, nameof(Status));

    Status = status;
    UpdatedTime = DomainGuard.UpdatedNow(updatedTime);
}
```

- [ ] **Step 2: Add API request**

In `src-v2/WordSolution.CmsV2.Api/CmsV2ApiRequests.cs`, add:

```csharp
public sealed record ChangeAtomicSectionStatusRequest(
    AtomicSectionStatus Status);
```

- [ ] **Step 3: Add endpoint**

In `CmsV2ApiEndpointExtensions.cs`, place after `/atomic-sections/{id:int}/difficulty`:

```csharp
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
```

- [ ] **Step 4: Add API integration test**

In `CmsV2ApiIntegrationTests`, add a test following existing `atomic-sections/{id}/difficulty` style:

```csharp
[Fact]
public async Task AtomicSection_status_endpoint_updates_status()
{
    await using var app = await CreateAppAsync();
    var client = app.CreateClient();
    var sectionId = await CreateSectionViaApiAsync(client);
    var atomic = await PostJsonAsync(client, "/api/cms-v2/atomic-sections", new
    {
        sectionId,
        title = "状态 AS",
        type = "Custom",
        difficulty = "Basic",
        status = "Draft"
    });

    var response = await PostJsonAsync(
        client,
        $"/api/cms-v2/atomic-sections/{atomic.GetProperty("id").GetInt32()}/status",
        new { status = "Active" });

    Assert.Equal("Active", response.GetProperty("status").GetString());
}
```

Use existing API test helpers rather than creating new helper conventions.

- [ ] **Step 5: Run backend focused tests**

Run:

```powershell
dotnet test src-v2/WordSolution.CmsV2.Tests/WordSolution.CmsV2.Tests.csproj --filter "CreateAtomicSection_creates_default_panels_without_default_content_blocks|WrapSectionItemsAsAtomicSection_creates_default_panels_without_assigning_existing_items|ChangeAtomicSectionStatus_updates_status|AtomicSection_status_endpoint_updates_status"
```

Expected:

```text
Passed
```

When the full backend suite is run later, update any additional tests that still assume a newly-created `AtomicSection` has zero panels. The new canonical invariant is: new AS has three empty panels and zero `AtomicSectionItem` / zero `ContentBlock`.

---

## Task 2: InsertPoint Panel Action

**Files:**
- Modify: `frontend-v2/src/types/index.ts`
- Modify: `frontend-v2/src/components/presentation/InsertPoint.vue`
- Modify: `frontend-v2/src/components/business/AtomicSectionBlock.vue`
- Modify: `frontend-v2/src/components/containers/SectionWorkspace.vue`
- Modify: `frontend-v2/src/pages/SectionPage.vue`
- Modify: `frontend-v2/src/locales/zh-CN.ts`
- Modify: `frontend-v2/src/locales/en.ts`

### Task 2.1: Extend Insert Types And Copy

- [ ] **Step 1: Extend type union**

In `frontend-v2/src/types/index.ts`, change:

```ts
export type InsertActionType = 'CreateContentBlock' | 'CreateAtomicSection' | 'SearchExistingBlock'
```

to:

```ts
export type InsertActionType =
  | 'CreateContentBlock'
  | 'CreateAtomicSection'
  | 'CreateAtomicSectionPanel'
  | 'SearchExistingBlock'
```

Change:

```ts
export type InsertParentType = 'Section' | 'AtomicSection' | 'CompositeBlock'
```

to:

```ts
export type InsertParentType = 'Section' | 'AtomicSection' | 'AtomicSectionPanelList' | 'CompositeBlock'
```

- [ ] **Step 2: Add i18n keys**

In `frontend-v2/src/locales/zh-CN.ts`, add under `components.insertPoint`:

```ts
createAtomicSectionPanel: '新建板块',
```

In `frontend-v2/src/locales/en.ts`, add:

```ts
createAtomicSectionPanel: 'Create panel',
```

Use the existing object structure and comma style.

### Task 2.2: Render InsertPoint Panel Button

- [ ] **Step 1: Import an icon**

In `InsertPoint.vue`, change:

```ts
import { Layers, Plus, Search } from 'lucide-vue-next'
```

to:

```ts
import { Layers, PanelTop, Plus, Search } from 'lucide-vue-next'
```

- [ ] **Step 2: Add button**

Add this block between `CreateAtomicSection` and `SearchExistingBlock`:

```vue
<Button
  v-if="isActionAllowed('CreateAtomicSectionPanel')"
  type="button"
  size="sm"
  variant="outline"
  class="h-6 px-2 text-xs"
  :aria-label="t('components.insertPoint.createAtomicSectionPanel')"
  :disabled="point.disabled"
  @click="emitAction('CreateAtomicSectionPanel')"
>
  <PanelTop class="size-3.5" />
  {{ t('components.insertPoint.createAtomicSectionPanel') }}
</Button>
```

### Task 2.3: Replace Constant AS Panel Buttons With InsertPoint

- [ ] **Step 1: Add panel-list insert point builder**

In `AtomicSectionBlock.vue`, add:

```ts
function createAtomicSectionPanelInsertPoint(
  beforePanel?: AtomicSectionPanelModel,
  afterPanel?: AtomicSectionPanelModel,
): InsertPointModel {
  const parentId = props.block.atomicSectionId
  const suffix = `${afterPanel?.panelId ?? 'start'}-${beforePanel?.panelId ?? 'end'}`

  return {
    id: `atomic-section-${props.block.id}-panel-insert-${suffix}`,
    label: t('components.insertPoint.createAtomicSectionPanel'),
    allowedActions: ['CreateAtomicSectionPanel'],
    disabled: props.block.disabled || !parentId,
    placement: parentId
      ? {
          parentType: 'AtomicSectionPanelList',
          parentId,
          beforeItemId: beforePanel?.panelId,
          afterItemId: afterPanel?.panelId,
          beforeSortOrder: beforePanel?.sortOrder,
          afterSortOrder: afterPanel?.sortOrder,
        }
      : undefined,
  }
}
```

- [ ] **Step 2: Add handler for panel insert action**

In `AtomicSectionBlock.vue`, add:

```ts
function handlePanelInsert(request: InsertRequestModel) {
  const placement = request.placement
  if (request.actionType !== 'CreateAtomicSectionPanel' || placement?.parentType !== 'AtomicSectionPanelList') {
    emit('requestInsert', request)
    return
  }

  const beforePanel = (props.block.panels ?? []).find(
    (panel) => panel.panelId === placement.beforeItemId,
  )
  const afterPanel = (props.block.panels ?? []).find(
    (panel) => panel.panelId === placement.afterItemId,
  )
  emitCreatePanel(beforePanel, afterPanel)
}
```

- [ ] **Step 3: Remove header create panel button**

Remove the non-icon `Button` in the `#actions` template that currently calls:

```vue
@click.stop="emitCreatePanel(undefined, getLastPanel())"
```

Keep the more menu icon button.

- [ ] **Step 4: Replace panel-list ghost buttons**

In panel list rendering, replace each constant “新建 panel” Button with:

```vue
<InsertPoint
  v-if="!readOnly"
  :point="createAtomicSectionPanelInsertPoint(panel)"
  @request-action="handlePanelInsert"
/>
```

For after-last-panel, use:

```vue
<InsertPoint
  v-if="!readOnly"
  :point="createAtomicSectionPanelInsertPoint(undefined, panel)"
  @request-action="handlePanelInsert"
/>
```

For no-panel empty state, use:

```vue
<InsertPoint
  v-if="!readOnly"
  :point="createAtomicSectionPanelInsertPoint()"
  @request-action="handlePanelInsert"
/>
```

Keep `AtomicSectionPanelBlock`, `AtomicSectionUnassignedArea`, and content item insert points unchanged.

- [ ] **Step 5: Run frontend typecheck**

Run:

```powershell
Set-Location frontend-v2
npm run typecheck
```

Expected:

```text
No TypeScript errors
```

---

## Task 3: AS Completeness And Inspector Compacting

**Files:**
- Modify: `frontend-v2/src/types/index.ts`
- Modify: `frontend-v2/src/composables/useSectionPageData.ts`
- Modify: `frontend-v2/src/components/business/AtomicSectionBlock.vue`
- Modify: `frontend-v2/src/components/business/SectionInspector.vue`
- Modify: `frontend-v2/src/pages/SectionPage.vue`
- Modify: `frontend-v2/src/apis/cmsV2Client.ts`
- Modify: `frontend-v2/src/locales/zh-CN.ts`
- Modify: `frontend-v2/src/locales/en.ts`

### Task 3.1: Derive `hasEmptyPanel`

- [ ] **Step 1: Add field**

In `StructuredBlockModel`, add:

```ts
hasEmptyPanel?: boolean
```

- [ ] **Step 2: Compute field**

In `useSectionPageData.ts`, after:

```ts
const panels = buildAtomicSectionPanelModels(atomicPanels, children)
```

add:

```ts
const hasEmptyPanel = panels.some((panel) => panel.children.length === 0)
```

In the returned `block`, add:

```ts
hasEmptyPanel,
```

- [ ] **Step 3: Add visual marker**

In `AtomicSectionBlock.vue`, use a compact `<span>` in the title/meta area:

```vue
<span
  v-if="block.hasEmptyPanel"
  class="rounded-sm border px-1.5 py-0.5 text-[11px] leading-none text-muted-foreground"
>
  {{ t('components.structuredBlock.incomplete') }}
</span>
```

Add `components.structuredBlock.incomplete`:

```ts
incomplete: '待完善',
```

English fallback:

```ts
incomplete: 'Incomplete',
```

Use existing token classes only.

Reuse existing `components.sectionInspector.status`. Add these `components.sectionInspector` keys:

```ts
completeness: '完善状态',
incomplete: '待完善',
complete: '已完善',
```

English fallback:

```ts
completeness: 'Completeness',
incomplete: 'Incomplete',
complete: 'Complete',
```

Add `sectionPage.workspace.statusActions.operationFailed`:

```ts
statusActions: {
  operationFailed: '状态更新失败，请稍后重试。',
},
```

English fallback:

```ts
statusActions: {
  operationFailed: 'Status update failed. Please try again.',
},
```

### Task 3.2: Add AS Status Client And Page Action

- [ ] **Step 1: Add API client method**

In `frontend-v2/src/types/index.ts`, add near other AS-related unions:

```ts
export type AtomicSectionStatusValue = 'Draft' | 'Active' | 'Archived'
```

Add common status labels in `frontend-v2/src/locales/zh-CN.ts`:

```ts
common: {
  status: {
    Draft: '草稿',
    Active: '启用',
    Archived: '已归档',
  },
}
```

English fallback in `frontend-v2/src/locales/en.ts`:

```ts
common: {
  status: {
    Draft: 'Draft',
    Active: 'Active',
    Archived: 'Archived',
  },
}
```

In `frontend-v2/src/apis/cmsV2Client.ts`, add request type:

```ts
import type { AtomicSectionStatusValue } from '@/types'

export type CmsV2AtomicSectionStatus = AtomicSectionStatusValue

export interface CmsV2ChangeAtomicSectionStatusRequest {
  status: CmsV2AtomicSectionStatus
}
```

Change `CmsV2AtomicSectionDto.status` from `string` to `CmsV2AtomicSectionStatus`.

Near `changeAtomicSectionDifficulty`, add:

```ts
changeAtomicSectionStatus: (
  atomicSectionId: number,
  request: CmsV2ChangeAtomicSectionStatusRequest,
) =>
  cmsV2PostJson<CmsV2AtomicSectionDto>(
    `/atomic-sections/${atomicSectionId}/status`,
    request,
  ),
```

- [ ] **Step 2: Add SectionInspector props and emit**

In `SectionInspector.vue`, add props:

```ts
updatingAtomicSectionStatus?: boolean
```

Add emit:

```ts
changeAtomicSectionStatus: [payload: { atomicSectionId: number; status: AtomicSectionStatusValue }]
```

Add `AtomicSectionStatusValue` to the existing `@/types` type import.

Add computed:

```ts
const showAtomicSectionStatusEditor = computed(() =>
  props.node?.kind === 'AtomicSection' && typeof props.node.atomicSectionId === 'number',
)
const atomicSectionStatusOptions: AtomicSectionStatusValue[] = ['Draft', 'Active', 'Archived']
const atomicSectionCompletenessLabel = computed(() => {
  if (props.node?.kind !== 'AtomicSection') {
    return ''
  }

  return props.node.hasEmptyPanel
    ? t('components.sectionInspector.incomplete')
    : t('components.sectionInspector.complete')
})
```

Add `hasEmptyPanel?: boolean` to `SectionTreeNodeModel`.

In `useSectionPageData.ts`, when building the AS tree node in `buildSectionItemNode`, compute:

```ts
const hasEmptyPanel = panelNodes.some((panel) => (panel.children?.length ?? 0) === 0)
```

Add `hasEmptyPanel` to the returned `AtomicSection` tree node. Do not set it on non-AS nodes.

- [ ] **Step 3: Add compact status selector**

In the Inspector property area, show only for AS:

```vue
<label v-if="showAtomicSectionStatusEditor" class="grid gap-1 text-xs text-muted-foreground">
  <span>{{ t('components.sectionInspector.status') }}</span>
  <select
    class="h-8 rounded-md border bg-background px-2 text-sm text-foreground"
    :value="node.targetStatus ?? node.status"
    :disabled="updatingAtomicSectionStatus"
    @change="
      emit('changeAtomicSectionStatus', {
        atomicSectionId: node.atomicSectionId!,
        status: ($event.target as HTMLSelectElement).value as AtomicSectionStatusValue,
      })
    "
  >
    <option v-for="status in atomicSectionStatusOptions" :key="status" :value="status">
      {{ t(`common.status.${status}`) }}
    </option>
  </select>
</label>
```

Add a compact row:

```vue
<div v-if="node.kind === 'AtomicSection'" class="flex items-center justify-between gap-2 text-xs">
  <span class="text-muted-foreground">{{ t('components.sectionInspector.completeness') }}</span>
  <span class="font-medium">{{ atomicSectionCompletenessLabel }}</span>
</div>
```

- [ ] **Step 4: Wire `SectionPage`**

In `SectionPage.vue`, add state:

```ts
const updatingAtomicSectionStatus = ref(false)
```

Add `type CmsV2AtomicSectionStatus` to the existing `@/apis/cmsV2Client` import.

Add handler:

```ts
async function changeAtomicSectionStatus(payload: {
  atomicSectionId: number
  status: CmsV2AtomicSectionStatus
}) {
  updatingAtomicSectionStatus.value = true
  sectionPageError.value = ''
  try {
    const previousSelectedNodeId = selectedStructureNodeId.value
    await cmsV2Api.changeAtomicSectionStatus(payload.atomicSectionId, {
      status: payload.status,
    })
    await loadCurrentSectionPage()
    selectedStructureNodeId.value = previousSelectedNodeId
  } catch (error) {
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.statusActions.operationFailed')
  } finally {
    updatingAtomicSectionStatus.value = false
  }
}
```

Pass props and events into `SectionInspector`:

```vue
:updating-atomic-section-status="updatingAtomicSectionStatus"
@change-atomic-section-status="changeAtomicSectionStatus"
```

### Task 3.3: Compact Inspector Layout

- [ ] **Step 1: Keep the same component, reduce density**

In `SectionInspector.vue`, keep `Card` + `WeakScrollArea`, but change:

```vue
<CardHeader class="gap-2 px-4 py-3">
```

to:

```vue
<CardHeader class="shrink-0 gap-2 px-3 py-2">
```

Change detail row container from large bordered rows to two-column compact rows:

```vue
<dl class="grid gap-1.5 text-xs">
  <div
    v-for="row in detailRows"
    :key="row.id"
    class="grid grid-cols-[72px_minmax(0,1fr)] items-center gap-2"
  >
    <dt class="text-muted-foreground">{{ row.label }}</dt>
    <dd class="truncate font-medium">{{ row.value }}</dd>
  </div>
</dl>
```

- [ ] **Step 2: Reduce editor section padding**

Change section wrappers:

```vue
class="border-t px-4 py-3"
```

to:

```vue
class="border-t px-3 py-2"
```

Change large inner cards:

```vue
class="grid gap-2 rounded-md border bg-muted/20 p-2"
```

to:

```vue
class="grid gap-2 rounded-md border bg-muted/20 p-2 text-xs"
```

- [ ] **Step 3: Hide irrelevant controls**

Keep existing `showAtomicSectionItemClassification` condition requiring both numeric `atomicSectionId` and numeric `atomicSectionItemId`; do not show AS item classification for a plain top-level `ContentBlock`.

### Task 3.4: Run Frontend Verification

- [ ] **Step 1: Typecheck**

Run:

```powershell
Set-Location frontend-v2
npm run typecheck
```

Expected:

```text
No TypeScript errors
```

- [ ] **Step 2: Build**

Run:

```powershell
Set-Location frontend-v2
npm run build
```

Expected:

```text
build completed
```

---

## Task 4: Documentation Sync And Final Verification

**Files:**
- Modify: `docs/ui/component-rules.md`
- Modify: `docs/ui/section-page.md`
- Modify: `docs/cms-v2/backend/后端数据模型开发文档.md`
- Modify: `docs/cms-v2/backend/领域模型结构说明.md`

### Task 4.1: Sync Docs

- [ ] **Step 1: Update component rules**

In `docs/ui/component-rules.md`, update InsertPoint and AtomicSection sections to record:

```text
InsertPoint can be used as a contextual operation slot.
AtomicSection panel-list positions may expose only CreateAtomicSectionPanel.
AtomicSection header should not show a permanent create panel button.
```

Also record:

```text
AtomicSectionBlock may show a lightweight incomplete marker when any panel has zero children.
```

- [ ] **Step 2: Update SectionPage doc**

In `docs/ui/section-page.md`, update AS workflow section so it says:

```text
Creating a new AtomicSection creates three default empty panels: Knowledge, Example, Variant.
These panels inherit the AS title and difficulty.
This does not create default ContentBlocks.
```

- [ ] **Step 3: Update backend model docs**

In both backend docs, replace old AS initialization wording with:

```text
创建 AtomicSection 时自动创建 Knowledge / Example / Variant 三个默认 AtomicSectionPanel；默认 panel 继承 AtomicSection 的 Title 和 Difficulty；不创建默认 ContentBlock。
```

### Task 4.2: Full Verification

- [ ] **Step 1: Backend tests**

Run:

```powershell
dotnet test src-v2/WordSolution.CmsV2.sln
```

Expected:

```text
Passed
```

- [ ] **Step 2: Frontend checks**

Run:

```powershell
Set-Location frontend-v2
npm run typecheck
npm run build
```

Expected:

```text
typecheck passes
build passes
```

- [ ] **Step 3: Diff whitespace check**

Run:

```powershell
Set-Location C:\Users\BOX\source\repos\qw852654\WordSolution
git diff --check
```

Expected:

```text
no output
```

- [ ] **Step 4: Browser smoke**

If the dev server is available, verify in browser:

```text
SectionPage opens.
New AS displays Knowledge / Example / Variant panels.
AS panel creation uses InsertPoint.
AS incomplete marker displays while a panel is empty.
Inspector top summary remains visible for ContentBlock.
Inspector AS status selector updates after backend confirmation.
Tags and TeachingNotes still render.
```

If browser verification cannot run, state the reason in the final report.

## Final Report Requirements

The execution thread final report must include:

- Files changed.
- Backend tests run and result.
- Frontend checks run and result.
- Browser smoke result.
- Whether backend restart is needed.
- Any remaining manual验收 items.

Backend restart note:

```text
This task changes backend API/use cases, so the control thread should restart src-v2/WordSolution.CmsV2.Api and check /api/cms-v2/health before user验收.
```
