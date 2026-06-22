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
- GeneratedFile 删除。
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

第一版使用三个轻量 Picker：

```text
SectionVariantPicker
AtomicSectionPicker
ContentBlockPicker
```

Picker 职责：

- 查询候选。
- 简单搜索。
- 简单筛选。
- 展示必要元数据。
- 单选一个目标。
- 确认。

Picker 不负责：

- 创建源对象。
- 编辑源对象。
- 多选。
- 批量添加。
- 高级查询器。
- 跨页面拖入。
- 修改 `SectionVariant` / `AtomicSection` / `ContentBlock`。

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

## 19. GeneratedFile

每次成功生成都写入一条 `GeneratedFile`。历史记录按生成时间倒序显示。

支持：

- 查看历史。
- 下载生成 DOCX。
- 打开生成 DOCX。
- 查看 `VersionManifestJson`。

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
HandoutIndexPage.vue
HandoutPage.vue
```

Containers：

```text
HandoutStructurePanel.vue
HandoutWorkspace.vue
HandoutInspector.vue
HandoutOutputPanel.vue
GeneratedFilePanel.vue
SectionVariantPicker.vue
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
```

如当前 API 路径风格与本文不同，开发前必须列为冲突或 API 设计差异，不得无声改变。

## 24. 开发阶段

### Phase H0：冲突审计

只读，不改代码。输出冲突审计并等待确认。

### Phase H1：正式文档完善

完善后端文档、UI 架构、HandoutPage 文档、开发计划和 Open Questions。不写实现代码。

### Phase H2：技术调研与编号 Spike

围绕 Aspose 编号 API、跨 DOCX 列表定义统一、最小 fixture 和替代方案做验证。

### Phase H3：Domain 与 Persistence

完成 `AtomicSection` target、mutation 方法、check constraint、migration 和测试。

### Phase H4：Application 编排用例

完成 add after、move、remove、update、sort normalization 和测试。

### Phase H5：Workspace Aggregate 与 API

完成 workspace query、item endpoints、template validation 和 API tests。

### Phase H6：Render Plan 与 Word 生成重构

完成 Render Plan、结构标题、模板保留、编号统一、生成器测试和 generation API tests。

### Phase H7：前端基础和 HandoutIndexPage

完成 routes、DTO、APIs、Handout / Version 创建、mock / lab、typecheck / build。

### Phase H8：HandoutPage 三栏与树

完成 `HandoutStructurePanel`、`HandoutWorkspace`、Inspector 和 mock 验收。

### Phase H9：真实编排接入

完成 aggregate、picker、add after、duplicate warning、move、delete、edit。

### Phase H10：Output 与 GeneratedFile

完成 template、output form、generate、history、download、manifest。

### Phase H11：端到端上线验收

完成空库、真实数据、Word 编号、build/test 和文档状态更新。

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

