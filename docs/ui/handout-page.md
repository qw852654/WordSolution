# HandoutPage 开发文档

本文档记录 CMS V2 `HandoutPage` 第一版上线范围、页面结构、组件边界、API 接入和生成规则。

依据：`CMS-V2-Handout-完整上线开发规格与Codex执行提示词.md`。如本文档与该规格或后续用户确认冲突，以最新确认规则为准，并先做冲突审计，不得静默改口径。

## 1. 开发边界

Handout 第一版只允许基于：

```text
WordSolution.CmsV2.sln
src-v2/
frontend-v2/
docs/cms-v2/
docs/ui/
```

禁止作为实现基础：

```text
V1 前端
V1 后端
题库本地服务/wwwroot
/api/题库实例/...
question-bank.db
旧 Handout API
旧 Word 生成链路
```

## 2. 第一版上线目标

Handout 第一版必须形成真实可用闭环：

```text
创建 Handout
创建 HandoutVersion
向 HandoutVersion 编排 SectionVariant / AtomicSection / ContentBlock
调整 HandoutVersionItem 顺序
编辑 TitleOverride / Note
创建或选择 OutputTemplate
创建 OutputForm
生成 Word
记录 GeneratedFile
查看历史生成记录
下载或打开生成的 Word
查看 VersionManifestJson
```

第一版是可用于真实讲义生产的工作流，不是 Mock UI。

## 3. 第一版明确不做

第一版不做：

- PDF / WordAndPdf 输出。
- 学生版 / 教师版实际内容过滤。
- 答案自动隐藏。
- 变量替换。
- 复杂条件内容。
- OutputTemplate 在线编辑器。
- OutputTemplate 上传服务。
- OutputTemplate 版本管理。
- RenderConfigJson 数据库字段。
- 复杂拖拽排序。
- 多选批量移动。
- 跨分组拖动。
- Handout 自动组装。
- 按 Difficulty 自动创建整份讲义。
- 复杂讲义分页预览。
- 完整 Word-like 浏览器预览。
- 生成失败记录表。
- 失败历史。
- 失败重试队列。
- GeneratedFile 重命名。
- GeneratedFile 对比。
- HandoutVersionItem 内容版本锁定。
- AtomicSection 内部局部选择。
- 复杂 Undo / Redo。
- 多人协作、权限、云同步。

## 4. 核心领域关系

```text
Handout
  -> HandoutVersion
    -> HandoutVersionItem
    -> OutputForm
      -> GeneratedFile
```

### Handout

`Handout` 表示一组相关讲义的总入口，回答“这组讲义属于什么材料”。它不直接保存编排项。

### HandoutVersion

`HandoutVersion` 是 `HandoutPage` 第一版的核心工作对象，回答“这份具体讲义当前按什么结构输出”。页面路由为：

```text
/handouts/:handoutVersionId
```

### HandoutVersionItem

`HandoutVersionItem` 是 `HandoutVersion` 中的一次顶层引用出现。第一版只允许直接引用：

```text
SectionVariant
AtomicSection
ContentBlock
```

第一版禁止直接引用：

```text
Section
SectionItem
AtomicSectionItem
TeachingTopic
ContentBlockVersion
```

## 5. HandoutVersionItem 目标类型

### SectionVariant

保存：

```text
TargetType = SectionVariant
TargetId = SectionVariant.Id
```

添加时不展开、不复制 `SectionItem`、不复制 `ContentBlock`。生成时展开：

```text
SectionVariant
  -> SectionVariantItem
    -> SectionItem
      -> ContentBlock
      -> AtomicSection
        -> AtomicSectionItem
          -> ContentBlock
```

### AtomicSection

保存：

```text
TargetType = AtomicSection
TargetId = AtomicSection.Id
```

直接引用 `AtomicSection` 是讲义级引用：

- 不要求它已被某个 `Section` 使用。
- 不创建 `SectionItem`。
- 不创建 `SectionVariantItem`。
- 不反向修改 `Section`。
- 不复制 `AtomicSection`。
- 不复制 `AtomicSectionItem`。

第一版生成时整体展开：

```text
AtomicSection
  -> AtomicSectionItem
    -> ContentBlock
      -> ContentBlockVersion
```

第一版不对直接引用的 `AtomicSection` 做内部局部选择或 Difficulty 过滤。

### ContentBlock

保存：

```text
TargetType = ContentBlock
TargetId = ContentBlock.Id
```

直接引用 `ContentBlock` 用于讲义级补充，例如封面说明、课前提示、临时补充题、总结页、作业说明等。

第一版规则：

- 不创建 `SectionItem`。
- 不属于 `SectionVariant`。
- 不反向进入 `Section`。
- 始终使用当前 `ContentBlockVersion`。
- 不增加 `ReferenceMode`。
- 不增加 `LockedContentBlockVersionId`。

## 6. 路由与页面

需要两个页面：

```text
/handouts
  Handout 列表、创建 Handout、查看和创建 HandoutVersion。

/handouts/:handoutVersionId
  HandoutVersion 编排页。
```

`/handouts/:handoutVersionId` 必须从 Placeholder 替换为真实 `HandoutPage`。

## 7. HandoutPage 布局

`HandoutPage` 采用类似 `SectionPage` 的三栏工作台：

```text
HandoutPage
├── HandoutStructurePanel
├── HandoutWorkspace
└── HandoutInspectorAndOutput
```

布局语义：

```text
左侧：讲义结构树
中间：讲义结构展开与编排
右侧：Inspector、OutputForm、GeneratedFile
```

第一版不做完整 Word-like 浏览器预览器。

## 8. 左侧 HandoutStructurePanel

### 根节点

根节点是当前 `HandoutVersion`。

### 顶层节点

每个顶层节点对应一条真实 `HandoutVersionItem`。节点必须使用 `HandoutVersionItem.Id` 作为出现身份，不能只使用 `targetId`，因为同一个目标允许重复添加。

建议节点 ID：

```text
handout-item:{handoutVersionItemId}
```

### 派生内部节点

内部节点只读，用于查看、定位、展示来源和 Inspector 查看。

`SectionVariant` 顶层项展开：

```text
HandoutVersionItem: SectionVariant
  -> SectionVariant
    -> SectionVariantItem
      -> SectionItem
        -> ContentBlock
        -> AtomicSection
          -> AtomicSectionItem
            -> ContentBlock
```

`AtomicSection` 顶层项展开：

```text
HandoutVersionItem: AtomicSection
  -> AtomicSectionItem
    -> ContentBlock
```

`ContentBlock` 顶层项展开：

```text
HandoutVersionItem: ContentBlock
  -> ContentBlockRelation children
```

派生节点 ID 必须包含顶层出现路径，避免相同源对象重复出现时节点冲突。

### 树能力

第一版必须支持：

- 展开 / 收起。
- 选中。
- 点击定位 Workspace。
- Inspector 联动。
- 顶层 item 上移 / 下移。
- 顶层 item 删除引用。
- 根节点添加到末尾。
- 顶层 item 在此后添加。

第一版不强制：

- Promote to Root。
- Breadcrumb 焦点根。
- Focused 模式。
- 跨层拖拽。

## 9. 添加与插入

第一版不在 `HandoutWorkspace` 顶部提供统一添加按钮。添加入口只位于左侧 `HandoutStructurePanel`。

### HandoutVersion 根节点菜单

提供：

```text
添加到末尾
```

随后选择：

```text
添加 SectionVariant
添加 AtomicSection
添加 ContentBlock
```

### 顶层 HandoutVersionItem 节点菜单

提供：

```text
在此后添加
```

随后选择目标类型。

### 内部派生节点

内部派生节点不显示添加入口，不允许插入顶层 `HandoutVersionItem`，也不修改源 `SectionVariant` / `AtomicSection` / `ContentBlockRelation`。

## 10. Picker

第一版添加内容分两类：

```text
SectionVariantSelectionDialog
AtomicSectionPicker
ContentBlockPicker
```

### SectionVariantSelectionDialog

`SectionVariantSelectionDialog` 是 Handout 创建链路中的批量追加选择器，不是同步编辑器。

层级固定为：

```text
TeachingTopic
  -> Section
    -> SectionVariant
```

规则：

- `TeachingTopic` / `Section` 只作为分组节点。
- 只有 `SectionVariant` 叶子会写入 `HandoutVersionItem`。
- 可一次选择多个 `SectionVariant`。
- 当前 `HandoutVersion` 已经存在的 `SectionVariant` 默认勾选并锁定。
- 选择权威状态只保存 `selectedVariantIds: Set<number>` 与 `existingVariantIds: Set<number>`。
- 父节点半选、全选、取消必须由纯函数推导，不能散落在 Vue template 或 watcher。
- 搜索支持匹配 `TeachingTopic.Title`、`Section.Title`、`SectionVariant.Title`，过滤后仍保留祖先路径且不丢选择状态。

第一版建议建立纯函数模块：

```text
frontend-v2/src/utils/sectionVariantTreeSelection.ts
```

至少包含：

```text
collectSelectableVariantIds
deriveNodeCheckState
toggleVariant
toggleGroup
buildExistingVariantIds
getNewVariantIds
filterTree
```

### AtomicSectionPicker / ContentBlockPicker

`AtomicSectionPicker` 和 `ContentBlockPicker` 第一版仍是轻量单选选择器。

职责：

- 查询候选。
- 简单搜索。
- 简单筛选。
- 展示必要元数据。
- 单选一个目标。
- 确认。

### Picker 不负责

- 创建源对象。
- 编辑源对象。
- 高级查询器。
- 跨页面拖入。
- 修改 `SectionVariant` / `AtomicSection` / `ContentBlock`。

说明：`SectionVariantSelectionDialog` 是上述“批量添加”限制的唯一例外，因为 Handout 初始内容和追加 `SectionVariant` 已确认采用批量追加树。

## 11. 添加 API 语义

建议请求：

```ts
type AddHandoutVersionItemRequest = {
  targetType: 'SectionVariant' | 'AtomicSection' | 'ContentBlock'
  targetId: number
  afterHandoutVersionItemId?: number | null
  titleOverride?: string | null
  note?: string | null
}
```

语义：

```text
afterHandoutVersionItemId = null
  -> 添加到末尾

afterHandoutVersionItemId = 某顶层 HandoutVersionItem.Id
  -> 插入到该项之后
```

前端不得自行计算 `SortOrder`。后端必须在事务中插入并规整同级 `SortOrder`。

## 12. 重复添加

同一个 `HandoutVersion` 允许同一个 `targetType + targetId` 出现多次。

重复添加由前端提醒，但不禁止：

```text
这项内容已经在当前讲义中出现过，是否仍然添加？
```

第一版：

- 后端不禁止重复。
- 数据库不增加唯一约束。
- 后端不需要 warning 响应。
- 不展示重复位置列表。
- 不自动跳转到已有项。
- 不智能合并。

## 13. 排序、删除和编辑

### 上移 / 下移

第一版必须支持按钮式上移 / 下移，不做拖拽。

建议 API：

```http
POST /api/cms-v2/handout-version-items/{id}/move-up
POST /api/cms-v2/handout-version-items/{id}/move-down
```

完成后重新规整 `SortOrder`，当前 item 保持选中且尽量保持可见。

### 删除引用

删除 `HandoutVersionItem` 只删除讲义中的引用，不删除源 `SectionVariant`、`AtomicSection`、`ContentBlock`、`Section` 或 DOCX。

建议 API：

```http
DELETE /api/cms-v2/handout-version-items/{id}
```

删除当前选中项后：

1. 优先选中后一个。
2. 没有后一个则选中前一个。
3. 列表为空时清空选择。
4. Inspector 显示空状态。

### TitleOverride / Note

第一版允许简单编辑：

```text
TitleOverride
Note
```

它们只作用于当前 `HandoutVersionItem`，不反向修改源对象。

建议 API：

```http
PATCH /api/cms-v2/handout-version-items/{id}
```

## 14. 标题显示优先级

```text
TitleOverride 非空
  -> 显示 TitleOverride

否则：
  SectionVariant -> SectionVariant.Title
  AtomicSection  -> AtomicSection.Title
  ContentBlock   -> ContentBlock.Title
```

目标对象异常缺失时，显示“目标对象不存在”，标记为异常，并禁止生成或在生成时明确失败。

## 15. HandoutWorkspace

`HandoutWorkspace` 显示当前 `HandoutVersion` 的展开结构。它不是完整 Word 预览器。

第一版展示原则：

- 顶层 `HandoutVersionItem` 是工作区主要单元。
- `SectionVariant` 展开为只读结构视图。
- 直接 `AtomicSection` 整体展开。
- `ContentBlock` 显示轻量预览入口。
- 组合 `ContentBlock` 显示自身内容和子块展开。
- 内部派生节点不提供编辑源结构的操作。

## 16. HandoutInspectorAndOutput

右侧区域包含：

- 当前选中节点 Inspector。
- OutputTemplate / OutputForm 区域。
- GeneratedFile 历史区域。
- VersionManifestJson 查看入口。

Inspector 只显示当前选中节点上下文，不做跨页面全局信息面板。

## 17. OutputTemplate

`OutputTemplate` 是本地 DOCX 模板，承载：

- 页面设置。
- 页眉页脚。
- Word 样式。
- 题干编号样式。

第一版手动指定本地 DOCX 模板文件，不做上传服务、在线编辑、版本管理或 RenderConfigJson。

生成时必须保留 `OutputTemplate` 的页眉页脚和页面设置。

## 18. OutputForm

第一版 `OutputForm` 只允许 `OutputFormat.Word`。

`Audience` / `VisibilityMode` 第一版只作为元数据，不实际做学生版 / 教师版内容过滤。

每个 `HandoutVersion` 拥有自己的 `OutputForm`。创建新的 `HandoutVersion` 时，系统应自动为该 version 创建一个默认 Word `OutputForm`，这样新 version 打开后可以直接进入生成 Word 流程。

这里的“共用”只指共用同一份默认 `OutputTemplate` DOCX 文件，不表示所有 `HandoutVersion` 共用同一个 `OutputForm` 记录。

新建默认 `OutputTemplate.TemplateDocxPath` 使用运行时相对路径：

```text
Documents/Templates/content-block-default.docx
```

后端在校验和生成时会解析为 API 输出目录下的实际模板文件。历史数据库中已经存在的旧默认路径 `src-v2/WordSolution.CmsV2.Infrastructure/Documents/Templates/content-block-default.docx` 仍视为同一份默认模板，不要求用户清库。

## 19. GeneratedFile

每次成功生成都写入一条 `GeneratedFile`。历史记录按生成时间倒序显示。

支持：

- 查看历史。
- 下载生成 DOCX。
- 打开生成 DOCX。
- 查看 `VersionManifestJson`。
- 删除 `GeneratedFile` 记录和本地生成文件。

失败不写 `GeneratedFile`，第一版也不做失败记录表、失败历史或重试队列。

## 20. Word 生成规则

第一版不能仅依赖：

```text
AppendDocument(..., ImportFormatMode.KeepSourceFormatting)
```

需要引入结构化 Render Plan，并处理：

- `TeachingTopic` -> 标题 1。
- `Section` -> 标题 2。
- `AtomicSection` -> 标题 3。
- 题干 -> 统一“例题”样式。
- 连续相同 `TeachingTopic` 去重。
- 连续相同 `Section` 去重。
- 模板页眉页脚保留。
- 源 ContentBlock DOCX 页眉页脚忽略或清除。
- 跨 DOCX 合并后的题干编号全文连续。

必须先做编号技术 Spike，验证多个源 DOCX 的同名“例题”样式在合并后能统一到目标模板列表定义，并通过 fixture 证明编号为 `1..n` 连续。

## 21. 前端组件建议

Pages：

```text
HandoutManagementPage.vue
HandoutPage.vue
```

Containers：

```text
HandoutStructurePanel.vue
HandoutWorkspace.vue
HandoutInspector.vue
HandoutOutputPanel.vue
GeneratedFilePanel.vue
SectionVariantSelectionDialog.vue
AtomicSectionPicker.vue
ContentBlockPicker.vue
```

Business Components：

```text
HandoutVersionTree.vue
HandoutVersionItemView.vue
HandoutSectionVariantBlock.vue
HandoutAtomicSectionBlock.vue
HandoutContentBlockItem.vue
OutputFormCard.vue
GeneratedFileRow.vue
VersionManifestViewer.vue
```

应复用：

- `BasicTree` / 共享树节点行为。
- `SectionItemView` 的交互思想。
- `StructuredContainer`。
- `ContentBlockDisplay` 的轻量预览入口。
- `WeakScrollArea`。
- `EmptyState`。
- `StatusPill`。
- shadcn-vue 基础组件。
- Theme Token。
- Vue I18n。

不要为 Handout 复制一套视觉系统。

## 22. 页面状态建议

```ts
type HandoutPageState = {
  handout: HandoutDto | null
  version: HandoutVersionDto | null
  items: HandoutWorkspaceItemDto[]
  outputForms: OutputFormDto[]
  selectedNodeId: string | null
  selectedHandoutVersionItemId: number | null
  expandedNodeIds: Set<string>
  loading: boolean
  error?: string | null
  operationPending: boolean
}
```

状态默认放页面或 `useHandoutPage`。只有多个页面真实共享时才进入 Pinia。

第一版不做复杂 optimistic update。操作成功后刷新 workspace aggregate，并恢复必要的选中和展开状态。

## 23. API 规划

读取：

```http
GET /api/cms-v2/handouts
GET /api/cms-v2/handouts/{id}
GET /api/cms-v2/handouts/{id}/versions
GET /api/cms-v2/handout-versions/{id}
GET /api/cms-v2/handout-versions/{id}/workspace
GET /api/cms-v2/handout-versions/{id}/items
```

创建：

```http
POST /api/cms-v2/handouts
POST /api/cms-v2/handouts/{id}/versions
POST /api/cms-v2/handout-versions/{id}/items
```

Item 操作：

```http
PATCH  /api/cms-v2/handout-version-items/{id}
DELETE /api/cms-v2/handout-version-items/{id}
POST   /api/cms-v2/handout-version-items/{id}/move-up
POST   /api/cms-v2/handout-version-items/{id}/move-down
```

输出：

```http
GET  /api/cms-v2/output-templates
POST /api/cms-v2/output-templates/validate
POST /api/cms-v2/output-templates

GET  /api/cms-v2/output-forms?handoutVersionId=
POST /api/cms-v2/output-forms

POST /api/cms-v2/output-forms/{id}/generate-word
GET  /api/cms-v2/output-forms/{id}/generated-files
GET  /api/cms-v2/generated-files/{id}/manifest
GET  /api/cms-v2/generated-files/{id}/download
DELETE /api/cms-v2/generated-files/{id}
```

如当前 API 路径风格与本文不同，开发前必须列为冲突或 API 设计差异，不得无声改变。

## 24. 开发阶段

### Phase H0：冲突审计

只读，不改代码。输出冲突审计并等待确认。

### Phase H1：正式文档完善

完善后端文档、UI 架构、HandoutPage 文档、开发计划和 Open Questions。不写实现代码。

### Phase H2：后端 Handout / Version 管理

补齐 `Handout` / `HandoutVersion` 创建、编辑、归档、唯一性和 `SortOrder` 规则。

### Phase H3：SectionVariant 树与批量加入 API

补齐 `GET /api/cms-v2/section-variants/tree` 和 `POST /api/cms-v2/handout-versions/{id}/items/batch-add-section-variants`。

### Phase H4：`/handouts` 管理页

完成 Master–Detail 管理页、`Handout` 创建 / 编辑 / 归档、`HandoutVersion` 创建 / 编辑 / 归档。

### Phase H5：HandoutOverviewFlyout

在 `HandoutVersion` 编辑页复用 SectionPage 左边缘 hover 总览交互，显示所有 `Handout -> HandoutVersion`。

### Phase H6：空 Version 批量选择

空 `HandoutVersion` 的 `HandoutWorkspace` 显示添加内容按钮，打开 `SectionVariantSelectionDialog` 并批量加入。

### Phase H7：非空 Version 后插入

常驻结构树根节点支持添加到末尾，顶层 `HandoutVersionItem` 右键支持在该节点后添加。

### Phase H8：稳定编辑器衔接

以最小改动替换当前 `window.prompt` 临时入口，保留现有 `HandoutPage` 三栏、`OutputForm`、Word 生成和 `GeneratedFile` 能力。

### Phase H9：端到端上线验收

完成从已有 `SectionVariant` 到 `/handouts` 创建、创建 `HandoutVersion`、批量加入、生成 Word、查看 / 下载 `GeneratedFile` 的真实闭环。

## 25. 集中验收场景

最小真实数据：

```text
TeachingTopic：圆周运动
Section：圆周运动基础
SectionVariant：基础讲解版
AtomicSection：圆锥摆
ContentBlock：补充题
Handout：圆周运动讲义
HandoutVersion：基础班版本
OutputTemplate：高中物理讲义模板.docx
OutputForm：课堂 Word
```

编排：

```text
1. SectionVariant：基础讲解版
2. AtomicSection：圆锥摆
3. ContentBlock：补充题
```

验收：

- 把补充题移动到第 2。
- 编辑 AtomicSection item 的 `TitleOverride`。
- 删除并重新添加。
- 重复添加 ContentBlock 并确认。
- 生成 Word。
- 查看历史。
- 下载生成文件。
- 查看 manifest。
- 检查模板页眉页脚、页面设置、结构标题、例题编号全文连续、图片和公式未丢失。
## 当前实现状态：HandoutPage 真实读取与基础闭环

本节记录当前 `frontend-v2` 与 CMS V2 后端已经接入的 HandoutPage 能力。

- `/handouts/:handoutVersionId` 已从占位页切换为独立 `HandoutPage`。
- 数字 `handoutVersionId` 读取 `GET /api/cms-v2/handout-versions/{id}/workspace`。
- `demo-handout` 仅用于 Mock Data 结构验收，不代表真实数据源。
- `HandoutVersionItem` 已接入上移、下移、编辑 `TitleOverride / Note`、移除引用。
- `OutputForm` 已接入 `POST /api/cms-v2/output-forms/{id}/generate-word`。
- `GeneratedFile` 已接入 manifest 查看、Word 下载和删除。
- “添加到末尾”当前使用按 `targetType + targetId` 输入的临时入口，只用于验证后端写入链路；后续必须替换为正式 `SectionVariantSelectionDialog`、`AtomicSectionPicker`、`ContentBlockPicker`。
- 仍未完成：`/handouts` 管理页、创建 / 编辑 / 归档 `Handout`、创建 / 编辑 / 归档 `HandoutVersion`、`HandoutOverviewFlyout`、`SectionVariantSelectionDialog`、批量加入 `SectionVariant` API、讲义页面正式右键菜单、完整 manifest 展示组件。

## 当前更新：Handout 创建链路上线口径

本节依据 `CMS-V2-Handout-创建链路上线开发规格-rebuildUI.md` 补充，覆盖本文档中仍残留的旧 Picker 和旧阶段口径。

### 目标链路

```text
已有 SectionVariant
↓
/handouts 创建 Handout
↓
在 Handout 下创建 HandoutVersion
↓
进入 /handouts/:handoutVersionId
↓
空 Version 通过 SectionVariantSelectionDialog 批量加入初始内容
↓
非空 Version 在根节点末尾或顶层 item 后继续追加内容
↓
沿用现有 OutputForm / Word 生成 / GeneratedFile 能力
```

### `/handouts` 管理页

`/handouts` 是 Master–Detail 管理页，不是独立 `Handout` 详情页。

左侧 `HandoutListPanel`：

- 创建 `Handout`。
- 标题搜索。
- 状态筛选。
- 默认隐藏 Archived，可切换显示。
- 选择当前 `Handout`。

右侧 `HandoutDetailPanel`：

- 显示当前 `Handout` 标题、描述、状态、更新时间。
- 编辑 / 归档当前 `Handout`。
- 显示 `HandoutVersion` 列表。
- 创建 / 编辑 / 归档 `HandoutVersion`。
- 点击版本进入 `/handouts/:handoutVersionId`。

创建 `Handout` 后：

- 只创建 `Handout`。
- 不自动创建 `HandoutVersion`。
- 留在 `/handouts`。
- 刷新列表并选中新 `Handout`。

创建 `HandoutVersion` 后：

- 刷新当前 `Handout` 的版本列表。
- 跳转到稳定路由 `/handouts/:handoutVersionId`。
- 不重命名该稳定路由。

### 唯一性和归档

- `Handout` 标题全局唯一，比较规则为 trim 后忽略英文大小写。
- 同一 `Handout` 下 `HandoutVersion` 标题唯一，比较规则为 trim 后忽略英文大小写。
- Archived 对象不参与当前名称冲突。
- 第一版只做归档，不物理删除 `Handout` 或 `HandoutVersion`。
- Archived `Handout` 默认从普通列表隐藏，不允许创建新 `HandoutVersion`。
- Archived `HandoutVersion` 可查看历史，不允许新增 / 移动 / 删除内容，也不允许新的生成操作。

### HandoutOverviewFlyout

`/handouts/:handoutVersionId` 需要左边缘 hover 总览树，必须复用或抽取 SectionPage 已有左边缘触发行为：

- 屏幕最左侧固定窄触发区。
- hover 约 2 秒打开。
- `Escape` 关闭。
- `Teleport` 到 `body`。
- 背景遮罩。
- 树节点视觉、展开收起、右键菜单和当前节点高亮与 SectionPage 统一。

树结构：

```text
Handout
  -> HandoutVersion
```

职责：

- 总览所有 `Handout` 和 `HandoutVersion`。
- 当前 `Handout` 默认展开。
- 当前 `HandoutVersion` 高亮。
- 点击其他 `HandoutVersion` 快速跳转。
- 提供轻量新建、重命名、归档入口。

### SectionVariant 批量选择树

第一版 `SectionVariant` 追加使用批量选择树，不使用单选 Picker。

数据接口目标：

```http
GET /api/cms-v2/section-variants/tree
```

H0 审计结论：当前 `/api/cms-v2/teaching-structure` 数据接近可复用，但它属于教学结构管理树，携带管理 UI 字段。为了让 Handout 批量选择稳定表达过滤、排序、只读叶子和选择语义，H1 后续开发采用专用只读接口 `/section-variants/tree`。

批量加入接口目标：

```http
POST /api/cms-v2/handout-versions/{id}/items/batch-add-section-variants
```

请求：

```ts
type BatchAddSectionVariantsRequest = {
  sectionVariantIds: number[]
  afterHandoutVersionItemId?: number | null
}
```

响应：

```ts
type BatchAddSectionVariantsResult = {
  createdItemIds: number[]
  skippedExistingVariantIds: number[]
}
```

语义：

- `afterHandoutVersionItemId = null` 表示添加到末尾。
- 指定顶层 `HandoutVersionItem.Id` 时，在该节点后连续插入。
- 请求内 ID 不允许重复。
- 已在当前版本中的 `SectionVariant` 由后端并发兜底跳过。
- 后端按教学结构顺序写入，并重新规整整个版本的 `SortOrder`。

### 稳定编辑页保护

后续阶段不得大幅重构当前已经可用的：

- `HandoutPage.vue` 三栏主体。
- `HandoutStructurePanel`。
- `HandoutWorkspace`。
- `HandoutInspector`。
- `HandoutOutputPanel`。
- `OutputForm`。
- Word 生成。
- `GeneratedFile` 下载 / manifest / 删除。

允许的最小改动只包括：空状态按钮、批量选择 Dialog、右键后插入、hover 总览、归档只读 guard、替换 `window.prompt` 临时入口和新增 API 对接。
## Current H6-H8 implementation: Handout creation chain

- Empty `HandoutVersion` workspaces expose an initial-content entry in `HandoutWorkspace`.
- Initial content opens `SectionVariantSelectionDialog` and batch-adds selected `SectionVariant` entries through `POST /api/cms-v2/handout-versions/{id}/items/batch-add-section-variants`.
- `HandoutStructurePanel` root context menu supports adding `SectionVariant`, `AtomicSection`, and `ContentBlock` to the end.
- Top-level `HandoutVersionItem` context menu supports adding `SectionVariant`, `AtomicSection`, and `ContentBlock` after that item.
- `SectionVariantSelectionDialog` uses `frontend-v2/src/utils/sectionVariantTreeSelection.ts` for selection state, group toggling, filtering, and locked existing entries.
- `HandoutTargetPicker` provides first-version single-select picking for direct `AtomicSection` and `ContentBlock` insertion.
- `HandoutOccurrenceEditDialog` replaces the temporary prompt-based `TitleOverride / Note` edit flow.
- Archived `Handout` or `HandoutVersion` entries are read-only for writes and generation.
