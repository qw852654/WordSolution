# Section Action Composables Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract real `SectionItem`, `AtomicSection`, and `ContentBlock` actions from page/component event handlers into reusable SectionPage action composables.

**Architecture:** UI components emit intent only. `SectionPage` receives events and delegates real behavior to composables such as `useSectionItemActions`, `useAtomicSectionActions`, and later `useContentBlockActions`. The composables call `cmsV2Client`, trigger server-confirmed refresh, and expose loading/error feedback without owning visual layout.

**Tech Stack:** Vue 3 Composition API, TypeScript, existing `frontend-v2` composables, existing `cmsV2Client`, CMS V2 API, Viteless project scripts (`npm run typecheck`, `npm run build`).

---

## Scope

In scope:

- Extract action orchestration out of `SectionPage.vue` and business components.
- Keep `SectionItemView`, `AtomicSectionBlock`, `ContentBlockDisplay`, `SectionTree`, and Inspector components emit-only.
- Make the same action methods callable from Workspace, SectionTree context menu, Inspector, and future keyboard shortcuts.
- Preserve server-confirmed update behavior.

Out of scope:

- New backend API.
- V1 frontend/backend changes.
- `VSTO/` changes.
- `Word本地文件操作核心库/` changes.
- UI redesign.
- New state management library.

## Naming

Use composable names with business intent:

```text
frontend-v2/src/composables/useSectionItemActions.ts
frontend-v2/src/composables/useAtomicSectionActions.ts
frontend-v2/src/composables/useContentBlockActions.ts
```

Recommended action method names:

```text
removeSectionItemReference
moveSectionItemUp
moveSectionItemDown
renameAtomicSection
createContentBlockInsideAtomicSection
startContentBlockWordEdit
removeContentBlockReference
```

Do not use ambiguous names such as `deleteAtomicSection` for Workspace removal. Workspace removal currently means removing a `SectionItem` reference, not deleting the `AtomicSection` entity.

## Task 1: Add SectionItem action composable

**Files:**

- Create: `frontend-v2/src/composables/useSectionItemActions.ts`
- Modify: `frontend-v2/src/pages/SectionPage.vue`

- [ ] **Step 1: Move SectionItem move/remove orchestration into `useSectionItemActions`**

Expose methods:

```ts
type SectionRefresh = () => Promise<void>

type SectionActionFeedback = (message: string) => void

export function useSectionItemActions(options: {
  refreshSection: SectionRefresh
  setFeedback: SectionActionFeedback
}) {
  async function moveSectionItemUp(sectionId: number, sectionItemId: number) {
    await cmsV2Client.moveSectionItem(sectionId, sectionItemId, "up")
    await options.refreshSection()
    options.setFeedback("已上移 SectionItem")
  }

  async function moveSectionItemDown(sectionId: number, sectionItemId: number) {
    await cmsV2Client.moveSectionItem(sectionId, sectionItemId, "down")
    await options.refreshSection()
    options.setFeedback("已下移 SectionItem")
  }

  async function removeSectionItemReference(sectionId: number, sectionItemId: number) {
    await cmsV2Client.removeSectionItem(sectionId, sectionItemId)
    await options.refreshSection()
    options.setFeedback("已移除 SectionItem 引用")
  }

  return {
    moveSectionItemUp,
    moveSectionItemDown,
    removeSectionItemReference,
  }
}
```

- [ ] **Step 2: Replace direct SectionPage handlers with composable calls**

`SectionPage.vue` should keep event handlers thin:

```ts
async function handleMoveSectionItemUp(sectionItemId: number) {
  if (!currentSection.value) return
  await sectionItemActions.moveSectionItemUp(currentSection.value.id, sectionItemId)
}
```

- [ ] **Step 3: Run frontend checks**

```powershell
cd frontend-v2
npm run typecheck
npm run build
```

Expected: both commands pass.

## Task 2: Add AtomicSection action composable

**Files:**

- Create: `frontend-v2/src/composables/useAtomicSectionActions.ts`
- Modify: `frontend-v2/src/pages/SectionPage.vue`

- [ ] **Step 1: Move AtomicSection rename and child creation into `useAtomicSectionActions`**

Expose methods:

```ts
type AtomicSectionActionsOptions = {
  refreshSection: () => Promise<void>
  setFeedback: (message: string) => void
}

export function useAtomicSectionActions(options: AtomicSectionActionsOptions) {
  async function renameAtomicSection(atomicSectionId: number, title: string) {
    await cmsV2Client.renameAtomicSection(atomicSectionId, title)
    await options.refreshSection()
    options.setFeedback("已重命名 AtomicSection")
  }

  async function createContentBlockInsideAtomicSection(input: {
    atomicSectionId: number
    sectionId: number
    title: string
    type: string
    difficulty: string
  }) {
    const created = await cmsV2Client.createContentBlockWithBlankDocument({
      sectionId: input.sectionId,
      title: input.title,
      blockType: input.type,
      difficulty: input.difficulty,
    })

    await cmsV2Client.addAtomicSectionItem(input.atomicSectionId, {
      contentBlockId: created.contentBlockId,
      referenceMode: "FollowLatest",
    })

    await options.refreshSection()
    options.setFeedback("已在 AtomicSection 中新建 ContentBlock")
  }

  return {
    renameAtomicSection,
    createContentBlockInsideAtomicSection,
  }
}
```

- [ ] **Step 2: Keep `AtomicSectionBlock` and `SectionItemView` emit-only**

Component event names may stay as-is, but their handlers must not call API inside the component.

- [ ] **Step 3: Run frontend checks**

```powershell
cd frontend-v2
npm run typecheck
npm run build
```

Expected: both commands pass.

## Task 3: Prepare ContentBlock action composable boundary

**Files:**

- Create: `frontend-v2/src/composables/useContentBlockActions.ts`
- Modify: `frontend-v2/src/pages/SectionPage.vue`

- [ ] **Step 1: Add composable shell for ContentBlock operations**

Expose method names, but do not fake backend behavior:

```ts
export function useContentBlockActions(options: {
  refreshSection: () => Promise<void>
  setFeedback: (message: string) => void
}) {
  async function startContentBlockWordEdit(contentBlockId: number) {
    await cmsV2Client.createContentBlockEditSession(contentBlockId, { openWord: true })
    options.setFeedback("已请求启动 ContentBlock Word 编辑")
  }

  return {
    startContentBlockWordEdit,
  }
}
```

This step depends on the V2 edit-session API plan:

```text
docs/superpowers/plans/2026-06-17-content-block-word-edit-session-v2.md
```

- [ ] **Step 2: If backend API is not implemented yet, keep SectionPage Word edit handler disabled with a clear message**

Use a Chinese user-facing message:

```text
ContentBlock Word 编辑 API 尚未接入
```

- [ ] **Step 3: Run frontend checks**

```powershell
cd frontend-v2
npm run typecheck
npm run build
```

Expected: both commands pass.

## Task 4: Verify action reuse boundaries

**Files:**

- Modify: `frontend-v2/src/pages/SectionPage.vue`
- Modify as needed later: `frontend-v2/src/components/business/SectionTree.vue`
- Modify as needed later: `frontend-v2/src/components/containers/SectionInspector.vue`

- [ ] **Step 1: Confirm Workspace uses composables**

Workspace-origin events should call the composables through `SectionPage`.

- [ ] **Step 2: Confirm SectionTree context menu can call the same methods later**

Do not duplicate delete/move/rename logic inside `SectionTree`.

- [ ] **Step 3: Confirm Inspector can call the same methods later**

Do not duplicate delete/move/rename logic inside `SectionInspector`.

- [ ] **Step 4: Run grep checks**

```powershell
rg -n "removeSectionItemReference|renameAtomicSection|createContentBlockInsideAtomicSection|startContentBlockWordEdit" frontend-v2/src
```

Expected:

- Method definitions are in composables.
- Components emit events.
- `SectionPage.vue` wires events to composables.

## Acceptance Criteria

- `SectionItemView` remains emit-only.
- `AtomicSectionBlock` remains emit-only.
- `ContentBlockDisplay` remains emit-only.
- Real actions are callable from a shared composable layer.
- Workspace, SectionTree context menu, Inspector, and future shortcuts can reuse the same methods.
- Deleting from Workspace uses `removeSectionItemReference`, not `deleteAtomicSectionEntity`.
- No V1 or legacy frontend code is touched.
