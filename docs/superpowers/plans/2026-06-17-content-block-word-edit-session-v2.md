# ContentBlock Word Edit Session V2 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a CMS V2 backend edit-session API for opening and synchronizing `ContentBlock` Word editing, while keeping the Word launch strategy replaceable for a future cloud backend.

**Architecture:** The V2 frontend emits a `ContentBlock` Word edit intent. `SectionPage` or a page-level composable calls `WordSolution.CmsV2.Api`. The API delegates to `WordSolution.CmsV2.Application`, which creates a transient edit session, copies the current `ContentBlockVersion` DOCX to an editable session file, and asks an Infrastructure launcher to open it. Sync imports the edited DOCX as a new `ContentBlockVersion` through existing document processing capabilities. UI components never construct local file paths, never open Word directly, and never depend on V1 edit-session endpoints.

**Tech Stack:** ASP.NET Core Minimal APIs, `WordSolution.CmsV2.Application`, `WordSolution.CmsV2.Domain`, `WordSolution.CmsV2.Infrastructure`, EF Core-backed existing repositories, local filesystem session assets, existing Aspose DOCX processing, Vue 3 frontend API client in a later wiring step.

---

## Scope

This plan is for documentation and future implementation only. It does not implement code in the current documentation round.

In scope for the future implementation:

- V2-only API under `/api/cms-v2`.
- `ContentBlock` Word edit-session creation.
- Session status query.
- Session sync into a new `ContentBlockVersion`.
- Session cancel.
- Replaceable backend launch strategy.
- Local implementation that can open a DOCX through the host OS.
- Frontend API client and page-level hookup after backend endpoints exist.

Out of scope:

- V1 backend endpoints.
- `题库本地服务/wwwroot`.
- `VSTO/`.
- `Word本地文件操作核心库/`.
- Cloud Word editing implementation.
- Multi-user session locking.
- Long-running collaboration.
- New database table unless explicitly confirmed later.
- Rewriting existing content-block version import or preview generation.

## Confirmed Existing Capabilities

- V2 already has `ContentBlock` create/import/version/download/HTML preview endpoints.
- V2 already has `ContentBlockDocumentUseCases`.
- V2 already has `IContentBlockFileStore`.
- V2 already has `IContentBlockDocumentProcessor`.
- V2 already stores DOCX, HTML preview, and plain text assets under the CMS V2 bank root.
- V2 currently does not have a Word edit-session API.
- V1 has historical local Word edit-session behavior, but it is not a current implementation target.

## API Contract

Add these V2 endpoints:

```text
POST /api/cms-v2/content-blocks/{contentBlockId}/edit-session
GET  /api/cms-v2/content-block-edit-sessions/{sessionId}
POST /api/cms-v2/content-block-edit-sessions/{sessionId}/sync
POST /api/cms-v2/content-block-edit-sessions/{sessionId}/cancel
```

Create request:

```ts
type CreateContentBlockEditSessionRequest = {
  openWord: boolean
}
```

Session response:

```ts
type ContentBlockEditSessionDto = {
  sessionId: string
  contentBlockId: number
  sourceContentBlockVersionId: number
  status: "Created" | "Opening" | "Editing" | "Synced" | "Cancelled" | "Failed"
  launchMode: "LocalShell" | "ExternalUri" | "Cloud" | "None"
  openedByServer: boolean
  message?: string
  createdTime: string
  updatedTime: string
}
```

Sync response:

```ts
type SyncContentBlockEditSessionResult = {
  sessionId: string
  contentBlockId: number
  changed: boolean
  newContentBlockVersionId?: number
  currentVersionNumber?: number
  status: "Synced"
  message?: string
}
```

Do not expose local absolute editable file paths as a required frontend contract. If a local debug field is ever needed, keep it optional and clearly documented as non-portable.

## Domain Contracts

- [ ] Add `ContentBlockEditSessionStatus`.
- [ ] Add `ContentBlockEditLaunchMode`.
- [ ] Add `ContentBlockEditSession` as a transient domain/application model, not a persisted core entity unless a later decision explicitly requires persistence.
- [ ] Add `IContentBlockEditSessionStore` for session lookup by `sessionId`.
- [ ] Add `IContentBlockEditSessionFileStore` for editable session-file creation, cleanup, hashing, and stream reading.
- [ ] Add `IContentBlockEditSessionLauncher` for replaceable launch behavior.

The launcher contract should express intent, not implementation:

```text
Launch edit file for ContentBlock edit session.
Return launch mode, whether the server opened it, and a user-facing message.
```

## Application Use Cases

- [ ] Add `ContentBlockEditSessionUseCases`.
- [ ] `CreateAsync` validates the `ContentBlock` exists and has a current version.
- [ ] `CreateAsync` reads the current version DOCX through existing file-store capabilities.
- [ ] `CreateAsync` creates an editable session copy under a session asset directory.
- [ ] `CreateAsync` records source `ContentBlockVersionId`, original hash, editable path, status, and timestamps.
- [ ] `CreateAsync` calls `IContentBlockEditSessionLauncher` only when `openWord = true`.
- [ ] `GetAsync` returns current session status.
- [ ] `SyncAsync` compares editable file hash with original hash.
- [ ] `SyncAsync` creates a new `ContentBlockVersion` only when the edited DOCX changed.
- [ ] `SyncAsync` uses existing DOCX import/version logic so HTML preview and plain text generation stay consistent.
- [ ] `CancelAsync` marks the session cancelled and releases temporary assets when safe.
- [ ] All failures return `CmsV2ApplicationException` or equivalent existing application error type.

## Infrastructure

- [ ] Add a local session asset directory under the V2 bank root, for example:

```text
{BankRootDirectory}
  edit-sessions/
    content-blocks/
      {SessionId}/
        edit.docx
        session.json
```

- [ ] Add `LocalContentBlockEditSessionStore`.
- [ ] Add `LocalContentBlockEditSessionFileStore`.
- [ ] Add `LocalWordEditSessionLauncher`.
- [ ] Local launcher can initially use Windows shell open behavior for local DOCX files.
- [ ] Keep launcher behind `IContentBlockEditSessionLauncher` so future cloud migration can replace it without changing frontend API semantics.
- [ ] Register the new services in `WordSolution.CmsV2.Api` dependency injection.

## API Implementation

- [ ] Extend CMS V2 Minimal API endpoint mapping with the four edit-session endpoints.
- [ ] Keep all routes under `/api/cms-v2`.
- [ ] Return `404` for missing `ContentBlock` or session.
- [ ] Return `400` for invalid session state transitions.
- [ ] Do not add compatibility with `/api/题库实例/...`.
- [ ] Do not call V1 controllers or V1 application services.

## Frontend Wiring After Backend Exists

- [ ] Add API client methods in `frontend-v2/src/apis/cmsV2Client.ts`.
- [ ] Add a page-level composable such as `useContentBlockWordEditor`.
- [ ] `ContentBlockDisplay` and `SectionItemView` continue to emit events only.
- [ ] `SectionPage` handles the event and calls the composable.
- [ ] Show loading/error/success feedback in Chinese.
- [ ] Do not build `ms-word:` or file URI launch logic in frontend components.
- [ ] Do not directly fetch V1 edit-session endpoints.

## Tests

Application tests:

- [ ] Creating a session for missing `ContentBlock` fails.
- [ ] Creating a session for a block without current version fails.
- [ ] Creating a session copies the current DOCX to session storage.
- [ ] `openWord = true` invokes the launcher once.
- [ ] `openWord = false` does not invoke the launcher.
- [ ] Sync without changes returns `changed = false` and does not create a new version.
- [ ] Sync with changes creates a new current `ContentBlockVersion`.
- [ ] Cancel marks the session cancelled.

API integration tests:

- [ ] `POST /content-blocks/{id}/edit-session` returns a session DTO.
- [ ] `GET /content-block-edit-sessions/{sessionId}` returns status.
- [ ] `POST /content-block-edit-sessions/{sessionId}/sync` returns sync result.
- [ ] `POST /content-block-edit-sessions/{sessionId}/cancel` returns cancelled status.

Verification commands:

```powershell
dotnet test src-v2/WordSolution.CmsV2.Tests/WordSolution.CmsV2.Tests.csproj --artifacts-path .artifacts-test
dotnet build WordSolution.Cms.slnf -v:minimal
```

Frontend verification after wiring:

```powershell
cd frontend-v2
npm run typecheck
npm run build
```

## Implementation Order

- [ ] Backend contracts and tests.
- [ ] Local infrastructure implementations.
- [ ] Application use cases.
- [ ] Minimal API endpoints.
- [ ] API integration tests.
- [ ] Frontend API client methods.
- [ ] SectionPage operation-area hookup.
- [ ] Manual test with TEST bank after backend and frontend both pass.

## Acceptance Criteria

- [ ] `ContentBlock` operation area can request Word editing through the V2 API.
- [ ] The frontend does not know whether Word was opened by local shell, URI, or future cloud logic.
- [ ] Sync creates a new `ContentBlockVersion` when the edited DOCX changed.
- [ ] Existing content-block download, preview, import, and handout generation behavior still works.
- [ ] No V1 frontend/backend code is touched.
- [ ] No `VSTO/` or `Word本地文件操作核心库/` changes are made.
