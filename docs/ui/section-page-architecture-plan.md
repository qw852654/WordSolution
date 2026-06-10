# SectionPage UI Architecture Plan

## 1. 已阅读文档列表

已阅读：

- `AGENTS.md`
- `CONTRIBUTING.md`
- `.codex/内容管理系统详细架构.md`
- `.codex/内容管理系统升级路线.md`
- `docs/ui/ui-architecture.md`
- `docs/ui/component-rules.md`
- `docs/ui/section-page.md`
- `docs/ui/focus-tree.md`
- `docs/ui/i18n.md`
- `docs/ui/codex-workflow.md`
- `docs/ui/小节页面-需求文档.md`

## 2. 当前仓库前端现状扫描

前提：

- V1 前端已经废弃。
- V1 后端也已经废弃。
- 本文档中的现有旧前端扫描只用于说明历史包袱，不表示应继续在其上增量开发。

### 2.1 目录现状

文档要求的目录：

```text
frontend-v2/
```

当前仓库实际情况：

- 不存在 `frontend-v2/`
- 存在 `src-v2/`
- `src-v2/` 当前只包含：
  - `WordSolution.CmsV2.Api`
  - `WordSolution.CmsV2.Application`
  - `WordSolution.CmsV2.Domain`
  - `WordSolution.CmsV2.Infrastructure`
  - `WordSolution.CmsV2.Tests`

结论：

- V2 后端已经开始建设。
- V2 Vue 前端工作区尚未落地。
- `ComponentLabPage`、`src/mocks/`、`src/labs/`、`src/locales/`、`src/apis/` 等前端基础目录当前均不存在。

### 2.2 前端技术栈落地情况

文档要求：

```text
Vue 3
Vite
Tailwind CSS
shadcn-vue
Pinia
Vue Router
Vue I18n
lucide-vue-next
```

当前仓库扫描结果：

- 未发现 `frontend-v2/package.json`
- 未发现 `vite.config.ts`
- 未发现 `tailwind.config.ts`
- 未发现 V2 前端 `router.ts`
- 未发现 V2 前端 `pinia.ts`
- 未发现 V2 前端 `i18n.ts`
- 未发现 V2 前端 `labs/` 路由或页面

结论：

- 当前任务必须先把前端骨架缺口写入开发计划。
- SectionPage 不具备“在现有 V2 前端上直接增量实现”的条件。

### 2.3 现有旧前端关系

当前仓库中的现有浏览器页主要仍是 V1 静态页：

```text
题库本地服务/wwwroot/
```

根据 `docs/ui/ui-architecture.md` 和 `docs/ui/codex-workflow.md`：

- V2 不兼容 V1 静态页面结构。
- 不应复用 V1 CSS 组件约定。
- 不应把 SectionPage 建在旧静态页基础上。
- 旧静态页扫描结果仅用于识别需要废弃的历史路径。

## 3. SectionPage 页面组件树

建议页面树：

```text
SectionPage
├── SectionTopToolbar
├── SectionMainLayout
│   ├── SectionStructurePanel
│   │   ├── SectionStructurePanelHeader
│   │   ├── SectionTree
│   │   │   └── FocusTree
│   │   └── SectionStructurePanelFooter
│   ├── SectionWorkspace
│   │   ├── SectionWorkspaceHeader
│   │   ├── SectionDocumentFlow
│   │   │   ├── SectionFlowItem
│   │   │   │   ├── ContentBlockDisplay
│   │   │   │   ├── AtomicSectionBlock
│   │   │   │   └── CompositeBlock
│   │   │   └── InsertPoint
│   │   └── TeachingNoteColumn
│   └── SectionInspector
│       └── SectionInspectorPanel
└── SecondaryWorkflowLayer
    ├── ContentBlockPicker
    ├── AtomicSectionPicker
    ├── PendingAtomicSectionDrawer
    └── TemporaryQuestionStagingDrawer
```

## 4. 组件分层表

| 组件 | 分层 | 原因 |
|---|---|---|
| `InsertPoint` | Presentation | 只表达“这里可以插入”，不理解业务，不调 API |
| `InlineBorderHeader` | Presentation | 只负责结构容器标题线和 actions slot |
| `StructuredContainer` | Presentation | 只负责弱边框容器和插槽布局 |
| `ToolbarButton` | Presentation | 纯按钮表达 |
| `EmptyState` / `LoadingState` / `ErrorState` | Presentation | 纯状态展示 |
| `ContentBlockDisplay` | Business | 理解 ContentBlock 预览、引用模式、难度、版本 |
| `AtomicSectionBlock` | Business | 理解 AtomicSection 语义和内部结构 |
| `CompositeBlock` | Business | 理解 CompositeBlock 语义 |
| `SectionTreeNode` | Business | 理解 Section 结构树节点语义 |
| `SectionInspectorPanel` | Business | 理解当前选中对象的上下文属性 |
| `SectionDocumentFlow` | Business | 理解 SectionItem 顺序流 |
| `FocusTree` | Business | 通用业务交互能力，不理解 CMS 业务对象 |
| `ContentBlockPicker` | Container | 负责加载候选数据、管理筛选与选择 |
| `AtomicSectionPicker` | Container | 负责加载可选 AtomicSection |
| `SectionStructurePanel` | Container | 负责装配树、面包屑、模式切换 |
| `SectionWorkspace` | Container | 负责装配文档流、插入上下文和工作区模式 |
| `SectionInspector` | Container | 负责选中对象上下文读取与动作触发 |
| `SectionPage` | Page | 负责页面级数据、联动、布局和状态 |

## 5. 可复用组件清单

当前仓库中尚无 V2 Vue 组件可直接复用。第一轮应优先沉淀以下可复用组件：

- `InsertPoint`
- `StructuredContainer`
- `InlineBorderHeader`
- `FocusTree`
- `ContentBlockDisplay`
- `SectionInspectorPanel`

基础 UI 优先复用未来 `shadcn-vue` 能力：

- `Button`
- `Input`
- `Textarea`
- `Select`
- `Dialog`
- `Drawer`
- `DropdownMenu`
- `Tabs`
- `Tooltip`
- `Badge`
- `Sheet`

图标优先复用：

- `lucide-vue-next`

## 6. 页面专属组件清单

Page-only 组件建议：

- `SectionPage`
- `SectionTopToolbar`
- `SectionMainLayout`
- `SectionStructurePanel`
- `SectionTree`
- `SectionWorkspace`
- `SectionDocumentFlow`
- `SectionInspector`

这些组件虽然只服务 SectionPage，但内部仍应尽量由可复用业务组件组成。

## 7. DTO / ViewModel 草案

### 7.1 API DTO 草案

#### SectionDto

```ts
type SectionDto = {
  id: number
  teachingTopicId: number
  title: string
  description?: string | null
  type: "NormalCourse" | "FirstRoundReview" | "SpecialTopic" | "ExamTraining" | "Custom"
  difficulty: "Unset" | "Basic" | "Medium" | "Advanced" | "Top"
  status: "Draft" | "Active" | "Archived"
  sortOrder: number
  updatedTime: string
}
```

#### SectionItemDto

```ts
type SectionItemDto = {
  id: number
  sectionId: number
  targetType: "ContentBlock" | "AtomicSection"
  targetId: number
  referenceMode: "FollowLatest" | "LockedVersion"
  lockedContentBlockVersionId?: number | null
  titleOverride?: string | null
  parentItemId?: number | null
  sortOrder: number
  selectionLayer?: "BasicRequired" | "AdvancedSupplement" | "TopExtension" | "ClassroomBackup" | "Homework" | "FirstRoundReview" | "SpecialTopic" | "Custom" | null
  teachingUseOverride?: "Lecture" | "Exercise" | "Homework" | "Review" | "ExamTraining" | "Custom" | null
  status: "Draft" | "Active" | "Archived"
  note?: string | null
  updatedTime: string
}
```

#### ContentBlockDto

```ts
type ContentBlockDto = {
  id: number
  title: string
  summary?: string | null
  blockType:
    | "KnowledgePoint"
    | "Explanation"
    | "Question"
    | "Answer"
    | "Analysis"
    | "MethodSummary"
    | "CommonMistake"
    | "Analogy"
    | "DiagramNote"
    | "ExampleGroup"
    | "ExerciseGroup"
    | "VariantGroup"
    | "GeneralText"
  difficulty: "Unset" | "Basic" | "Medium" | "Advanced" | "Top"
  questionType?: "Unset" | "Choice" | "Blank" | "Calculation" | "Experiment" | "Diagram" | "Composite" | null
  status: "Draft" | "Active" | "Archived"
  currentVersionId?: number | null
  updatedTime: string
}
```

#### AtomicSectionDto

```ts
type AtomicSectionDto = {
  id: number
  title: string
  description?: string | null
  type: "ConceptBuild" | "MethodExplain" | "ExampleExplain" | "MistakeAnalysis" | "ExerciseArrange" | "Custom"
  status: "Draft" | "Active" | "Archived"
  updatedTime: string
}
```

#### AtomicSectionItemDto

```ts
type AtomicSectionItemDto = {
  id: number
  atomicSectionId: number
  contentBlockId: number
  referenceMode: "FollowLatest" | "LockedVersion"
  lockedContentBlockVersionId?: number | null
  sortOrder: number
  titleOverride?: string | null
  note?: string | null
  updatedTime: string
}
```

#### SectionVariantDto

```ts
type SectionVariantDto = {
  id: number
  sectionId: number
  title: string
  description?: string | null
  type: "Lecture" | "Exercise" | "Homework" | "Review" | "ExamTraining" | "Custom"
  difficulty: "Unset" | "Basic" | "Medium" | "Advanced" | "Top"
  status: "Draft" | "Active" | "Archived"
  sortOrder: number
  updatedTime: string
}
```

#### TeachingNoteDto

```ts
type TeachingNoteDto = {
  id: number
  targetType: "TeachingTopic" | "Section" | "SectionVariant" | "SectionItem" | "AtomicSection" | "ContentBlock" | "HandoutVersion"
  targetId: number
  noteType: "TeachingReflection" | "RevisionSuggestion" | "CommonMistake" | "TeachingLogic" | "ExampleAdvice" | "QuestionReplacement" | "General"
  title: string
  content: string
  status: "Active" | "Resolved" | "Archived"
}
```

### 7.2 ViewModel 草案

#### FocusTreeNodeVm

```ts
type FocusTreeNodeVm = {
  id: string
  nodeType: "teachingTopic" | "section" | "atomicSection" | "sectionItem" | "contentBlock" | "compositeBlock" | "stats"
  title: string
  subtitle?: string
  badges?: string[]
  meta?: string[]
  children?: FocusTreeNodeVm[]
  isLeaf: boolean
  isExpanded: boolean
  isSelected: boolean
  canPromoteToRoot: boolean
  linkedWorkspaceItemId?: string | null
}
```

#### SectionFlowItemVm

```ts
type SectionFlowItemVm = {
  id: string
  itemType: "contentBlock" | "atomicSection" | "compositeBlock"
  sectionItemId?: number | null
  targetType: "ContentBlock" | "AtomicSection"
  targetId: number
  title: string
  displayTitle: string
  description?: string | null
  difficulty?: "Unset" | "Basic" | "Medium" | "Advanced" | "Top" | null
  status: string
  referenceMode?: "FollowLatest" | "LockedVersion" | null
  lockedContentBlockVersionId?: number | null
  currentVersionId?: number | null
  htmlPreviewState: "ready" | "loading" | "empty" | "error"
  htmlPreview?: string | null
  children?: SectionFlowItemVm[]
  note?: string | null
}
```

#### SectionInspectorVm

```ts
type SectionInspectorVm = {
  selectedNodeId: string | null
  selectedNodeType: "section" | "sectionItem" | "contentBlock" | "atomicSection" | "compositeBlock" | null
  title: string
  referenceMode?: "FollowLatest" | "LockedVersion" | null
  lockedVersionLabel?: string | null
  difficulty?: string | null
  status?: string | null
  note?: string | null
  teachingNotes?: TeachingNoteDto[]
  availableActions: string[]
}
```

#### InsertContextVm

```ts
type InsertContextVm = {
  sectionId: number
  parentType: "Section" | "AtomicSection" | "CompositeBlock"
  parentId?: number | null
  beforeItemId?: number | null
  afterItemId?: number | null
  insertPosition: "before" | "after" | "append" | "asChild"
  allowedTargetTypes: ("ContentBlock" | "AtomicSection" | "CompositeBlock")[]
}
```

## 8. Mock Data 需求清单

需要 mock：

- `mockTeachingTopics`
- `mockSections`
- `mockSectionItemsFlat`
- `mockSectionVariants`
- `mockAtomicSections`
- `mockAtomicSectionItems`
- `mockContentBlocks`
- `mockContentBlockHtmlPreviews`
- `mockTeachingNotes`
- `mockFocusTreeNodes`
- `mockSectionFlowItems`
- `mockInspectorStates`
- `mockInsertContexts`
- `mockEditSessionsByContentBlockId`

必须覆盖的场景：

- 空 Section
- 单个 ContentBlock
- 多个连续 ContentBlock
- AtomicSection 包含多个块
- CompositeBlock 包含多个块
- 长标题
- 长备注
- 长 HTML 预览
- 无版本预览
- LockedVersion
- FollowLatest
- Difficulty 不同档位
- 内容加载中
- 内容加载失败
- Inspector 无选中项
- Root Promotion 前后树变化

## 9. 状态归属表

| 状态 | 建议归属 | 理由 |
|---|---|---|
| `selectedNodeId` | `SectionPage` / `useSectionPage` | 同时驱动树、工作区、Inspector |
| `selectedNodeIds` | `SectionPage` / `useSectionPage` | 为连续块升级 AtomicSection 预留 |
| `currentSection` | `SectionPage` / `useSectionPage` | 页面级主数据 |
| `sectionItems` | `SectionPage` / `useSectionPage` | 页面级主数据 |
| `sectionVariants` | `SectionPage` / `useSectionPage` | 页面级切换数据 |
| `activeVariantId` | `SectionPage` / `useSectionPage` | 页面级切换状态 |
| `expandedNodeIds` | `useFocusTree` + page state | 树和工作区共享 |
| `focusedRootNodeId` | `useFocusTree` | FocusTree 机制状态 |
| `insertContext` | `SectionPage` / `SectionWorkspace` 容器 | 只服务当前页面流程 |
| `leftPanelMode` | `SectionPage` | 页面布局状态 |
| `rightInspectorVisible` | `SectionPage` | 页面布局状态 |
| `teachingNoteMode` | `SectionPage` | 工作区视图模式 |
| `dirtyState` | `SectionPage` / `useSectionPage` | 页面级保存提示 |
| `loadingState` | `SectionPage` / containers | 页面或容器加载 |
| `errorState` | `SectionPage` / containers | 页面或容器错误 |
| `editSessionsByContentBlockId` | `SectionPage` / composable | 当前阶段只在 SectionPage 使用，但要预留多会话 |
| `sectionStateBySectionId` | `SectionPage` / composable | 预留未来多 Section 工作区 |
| `currentTeachingTopicId` | 未来 Pinia | 只有确认被多个页面共享后再进入 store |
| `uiPreferences` | 未来 Pinia | 多页面共享时才建立 |

当前不建议直接进 Pinia 的状态：

- `currentSection`
- `selectedNodeId`
- `expandedNodeIds`
- `insertContext`
- `editSessionsByContentBlockId`

## 10. 事件清单

- `selectNode(nodeId)`
- `selectMultipleNodes(nodeIds)`
- `locateWorkspaceItem(itemId)`
- `toggleNodeExpanded(nodeId)`
- `promoteNodeToRoot(nodeId)`
- `returnFocusRoot()`
- `setLeftPanelMode(mode)`
- `toggleInspector()`
- `toggleTeachingNoteMode()`
- `openInsertMenu(context)`
- `insertBlankContentBlock(context, blockType)`
- `insertBlankCompositeBlock(context, blockType)`
- `insertExistingContentBlock(context, contentBlockId)`
- `createAtomicSectionFromSelection(itemIds)`
- `moveItemUp(itemId)`
- `moveItemDown(itemId)`
- `changeItemIndent(itemId)`
- `changeItemOutdent(itemId)`
- `removeSectionItem(itemId)`
- `openContentBlockInWord(contentBlockId)`
- `refreshContentBlockPreview(contentBlockId)`
- `updateContentBlockDifficulty(contentBlockId, difficulty)`
- `updateInspectorReferenceMode(itemId, referenceMode)`
- `updateInspectorLockedVersion(itemId, versionId)`
- `openPendingAtomicSectionDrawer()`
- `openTemporaryQuestionStagingDrawer()`

## 11. API 需求清单

| 用途 | 方法 | 路径 | 请求 DTO | 返回 DTO | 第一版必须 | 可先 mock |
|---|---|---|---|---|---|---|
| 健康检查/启动确认 | `GET` | `/api/cms-v2/health` | 无 | health payload | 否 | 是 |
| 枚举字典 | `GET` | `/api/cms-v2/meta/enums` | 无 | enums payload | 是 | 否 |
| 教学主题列表 | `GET` | `/api/cms-v2/teaching-topics` | 无 | `TeachingTopicDto[]` | 是 | 可先 mock，但最终应接真实 |
| 单个教学主题 | `GET` | `/api/cms-v2/teaching-topics/{id}` | 无 | `TeachingTopicDto` | 否 | 是 |
| 教学主题子节点 | `GET` | `/api/cms-v2/teaching-topics/{id}/children` | 无 | `TeachingTopicDto[]` | 否 | 是 |
| Section 列表 | `GET` | `/api/cms-v2/sections?teachingTopicId=` | 无 | `SectionDto[]` | 是 | 否 |
| Section 详情 | `GET` | `/api/cms-v2/sections/{id}` | 无 | `SectionDto` | 是 | 否 |
| 创建 Section | `POST` | `/api/cms-v2/sections` | `CreateSectionRequest` | `SectionDto` | 否 | 是 |
| SectionItem 列表 | `GET` | `/api/cms-v2/sections/{id}/items` | 无 | `SectionItemDto[]` | 是 | 否 |
| 创建 SectionItem 引用 | `POST` | `/api/cms-v2/sections/{id}/items` | `AddSectionItemRequest` | `SectionItemDto` | 否 | 是 |
| SectionVariant 列表 | `GET` | `/api/cms-v2/section-variants?sectionId=` | 无 | `SectionVariantDto[]` | 是 | 否 |
| SectionVariant 详情 | `GET` | `/api/cms-v2/section-variants/{id}` | 无 | `SectionVariantDto` | 否 | 是 |
| SectionVariant items | `GET` | `/api/cms-v2/section-variants/{id}/items` | 无 | `SectionVariantItemDto[]` | 否 | 是 |
| AtomicSection 列表 | `GET` | `/api/cms-v2/atomic-sections` | 无 | `AtomicSectionDto[]` | 是 | 否 |
| AtomicSection 详情 | `GET` | `/api/cms-v2/atomic-sections/{id}` | 无 | `AtomicSectionDto` | 否 | 是 |
| AtomicSection items | `GET` | `/api/cms-v2/atomic-sections/{id}/items` | 无 | `AtomicSectionItemDto[]` | 是 | 否 |
| 创建 AtomicSection | `POST` | `/api/cms-v2/atomic-sections` | `CreateAtomicSectionRequest` | `AtomicSectionDto` | 否 | 是 |
| 给 AtomicSection 添加块 | `POST` | `/api/cms-v2/atomic-sections/{id}/items` | `AddAtomicSectionItemRequest` | `AtomicSectionItemDto` | 否 | 是 |
| ContentBlock 列表 | `GET` | `/api/cms-v2/content-blocks` | 无 | `ContentBlockDto[]` | 是 | 否 |
| ContentBlock 详情 | `GET` | `/api/cms-v2/content-blocks/{id}` | 无 | `ContentBlockDto` | 是 | 否 |
| 创建空白 ContentBlock | `POST` | `/api/cms-v2/content-blocks/blank-document` | `CreateContentBlockWithBlankDocumentRequest` | created payload | 否 | 是 |
| 当前版本 HTML 预览 | `GET` | `/api/cms-v2/content-blocks/{id}/html-preview` | 无 | HTML string | 是 | 否 |
| 版本列表 | `GET` | `/api/cms-v2/content-blocks/{id}/versions` | 无 | version dto array | 否 | 是 |
| 设置当前版本 | `POST` | `/api/cms-v2/content-blocks/{id}/current-version` | `SetCurrentContentBlockVersionRequest` | simple result | 否 | 是 |
| 子块关系列表 | `GET` | `/api/cms-v2/content-blocks/{id}/relations/children` | 无 | relation dto array | 是 | 否 |
| 添加子块关系 | `POST` | `/api/cms-v2/content-blocks/{id}/relations/children` | `AddContentBlockRelationRequest` | relation dto | 否 | 是 |
| TeachingNote 列表 | `GET` | `/api/cms-v2/teaching-notes?targetType=&targetId=` | 无 | `TeachingNoteDto[]` | 否 | 是 |
| 创建 TeachingNote | `POST` | `/api/cms-v2/teaching-notes` | `CreateTeachingNoteRequest` | `TeachingNoteDto` | 否 | 是 |

当前 API 缺口：

- 未看到 SectionItem 排序更新接口
- 未看到 SectionItem 删除接口
- 未看到 SectionItem 缩进/层级调整接口
- 未看到连续块升级为 AtomicSection 的后端用例接口
- 未看到 Word 编辑会话接口
- 未看到 ContentBlock difficulty 快速更新接口
- 未看到 SectionPage 专用聚合读取接口

结论：

- SectionPage v0.1 若要真实写入，后端接口尚未齐。
- 准备阶段应默认按 `mock first + 只读真实读取接口` 规划。

## 12. i18n key 草案

### common

- `common.save`
- `common.cancel`
- `common.close`
- `common.create`
- `common.delete`
- `common.edit`
- `common.refresh`
- `common.preview`
- `common.loading`
- `common.empty`
- `common.error`
- `common.retry`
- `common.more`
- `common.open`
- `common.back`

### navigation

- `navigation.sections`
- `navigation.contentBlocks`
- `navigation.handouts`
- `navigation.topics`
- `navigation.lab`

### section

- `section.pageTitle`
- `section.toolbar.preview`
- `section.toolbar.structure`
- `section.toolbar.inspector`
- `section.toolbar.teachingNoteMode`
- `section.toolbar.pendingAtomicSections`
- `section.toolbar.questionStaging`
- `section.toolbar.more`
- `section.structure.title`
- `section.structure.hidden`
- `section.structure.docked`
- `section.structure.focused`
- `section.structure.promoteRoot`
- `section.structure.returnRoot`
- `section.workspace.title`
- `section.workspace.cleanMode`
- `section.workspace.teachingNoteMode`
- `section.workspace.insertBefore`
- `section.workspace.insertAfter`
- `section.workspace.append`
- `section.workspace.asChild`
- `section.workspace.wrapAsAtomicSection`
- `section.inspector.title`
- `section.inspector.referenceMode`
- `section.inspector.lockedVersion`
- `section.inspector.note`
- `section.inspector.teachingNote`
- `section.empty.noItems`
- `section.error.loadFailed`

### contentBlock

- `contentBlock.title`
- `contentBlock.summary`
- `contentBlock.type`
- `contentBlock.status`
- `contentBlock.difficulty`
- `contentBlock.questionType`
- `contentBlock.currentVersion`
- `contentBlock.referenceMode`
- `contentBlock.followLatest`
- `contentBlock.lockedVersion`
- `contentBlock.openInWord`
- `contentBlock.refreshPreview`
- `contentBlock.moveUp`
- `contentBlock.moveDown`

### atomicSection

- `atomicSection.title`
- `atomicSection.description`
- `atomicSection.type`
- `atomicSection.status`
- `atomicSection.create`
- `atomicSection.empty`

### focusTree

- `focusTree.breadcrumb`
- `focusTree.focusRoot`
- `focusTree.backToParent`
- `focusTree.expand`
- `focusTree.collapse`
- `focusTree.notPromotable`

### lab

- `lab.pageTitle`
- `lab.defaultState`
- `lab.selectedState`
- `lab.disabledState`
- `lab.loadingState`
- `lab.errorState`

## 13. ComponentLab 验收组件清单

第一批必须进入 `/lab` 的组件：

- `InsertPoint`
- `StructuredContainer`
- `InlineBorderHeader`
- `ContentBlockDisplay`
- `AtomicSectionBlock`
- `CompositeBlock`
- `FocusTree`
- `SectionTree`
- `SectionWorkspace`
- `SectionInspector`

每个组件至少提供：

- 默认状态
- 选中状态
- 禁用状态
- 长标题
- 长正文
- 空数据
- 加载状态
- 错误状态
- 不同 `difficulty`
- `FollowLatest`
- `LockedVersion`

## 14. docs/ui/section-page.md 与 docs/ui/小节页面-需求文档.md 的差异或冲突

### 冲突 1：页面顶部语义

- `docs/ui/section-page.md` 更强调 `SectionVariant` 切换是 Toolbar 的一等入口。
- `docs/ui/小节页面-需求文档.md` 也保留 `SectionVariant`，但整体重心已经转向 `SectionPage` 作为结构编辑工作台，要求先把页面组件树、状态、事件和插入流程理清。

处理建议：

- 第一版准备文档保留 `SectionVariant` 在页面级状态中。
- 真正实现时，不把 Toolbar 塞满变体相关操作。

### 冲突 2：左侧树命名与职责

- `docs/ui/section-page.md` 使用 `SectionStructurePanel`。
- `docs/ui/小节页面-需求文档.md` 在目标结构里写 `SectionStructurePanel / SectionTree`，但正文对树职责又扩展到 `TeachingStructure Workspace` 级别的注意力管理。

处理建议：

- 命名上采用 `SectionStructurePanel` 包裹 `SectionTree`。
- `FocusTree` 只提供机制，不承载 Section 业务。

### 冲突 3：AtomicSection 创建方式

- `docs/ui/section-page.md` 允许 Toolbar 直接“新建 AtomicSection”。
- `docs/ui/小节页面-需求文档.md` 明确第一版真实流程优先是“从连续块升级为 AtomicSection”，并把“创建空 AtomicSection”列为待确认问题。

处理建议：

- 作为冲突保留，不在准备阶段自行定案。
- 第一版开发计划默认按“连续块升级必做，空 AtomicSection 单独确认”。

### 冲突 4：插入能力范围

- `docs/ui/section-page.md` 第一版建议主要是上移、下移、缩进、取消缩进。
- `docs/ui/小节页面-需求文档.md` 第一版明确要支持：
  - 插入空白 ContentBlock
  - 插入空白 CompositeBlock / QuestionGroup
  - 从连续块升级为 AtomicSection

处理建议：

- 把插入和创建流程列为第一版规划核心。
- 缩进/取消缩进保留在 API 缺口和后续能力中。

### 冲突 5：ContentBlock 命名

- `docs/ui/component-rules.md` 当前核心业务组件叫 `ContentBlockCard`。
- `docs/ui/小节页面-需求文档.md` 明确建议在工作区避免使用 `Card` 命名，偏向 `ContentBlockDisplay`。

处理建议：

- 资源库和选择器继续用 `ContentBlockCard`。
- 文档流工作区使用 `ContentBlockDisplay`。
- 后续应把这条边界补入 `component-rules.md`。

### 冲突 6：写入能力预期

- `docs/ui/section-page.md` 更接近“第一版最终要可用”的页面规格。
- 本任务输入明确禁止当前轮直接接真实写入 API。

处理建议：

- 准备任务只输出架构、mock、计划、问题，不进入实现。

## 15. 当前不允许实现的后续能力

- 直接实现 SectionPage 页面
- 直接实现复杂拖拽排序
- 直接实现 Word 编辑全链路
- 直接修改后端模型
- 直接修改数据库结构
- 直接接真实写入 API
- 兼容 V1 静态页面
- 复用 V1 CSS 组件约定
- 默认把页面状态塞进 Pinia
- 自行补完未确认业务规则
- 非连续块升级 AtomicSection
- 完整 HandoutPage 设计扩展
- 多人协作、权限、云端同步
