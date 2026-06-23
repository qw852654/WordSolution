# SectionPage Open Questions

说明：

- 这些问题属于阶段 2 之后的 V2 前端与 SectionPage 设计问题。
- 当前全局阶段 1 只做文档清理，不进入这些实现决策。

## 0. 已确认共识，不再作为开放问题

### Section 与 SectionVariant

```text
TeachingTopic
↓
Section（上帝小节 / 完整知识池 / 完整教学结构）
├── SectionVariant（基础讲解版）
├── SectionVariant（提高版）
├── SectionVariant（一轮复习版）
└── SectionVariant（冲刺版）
```

已确认：

- Section 本身就是上帝小节。
- SectionVariant 是从 Section 派生出的教学用途方案。
- SectionVariant 是 Section 的子级。
- 不再把“上帝小节”理解为一个 SectionVariant。

### SectionItemView

已确认统一使用：

```text
SectionItemView
```

说明：

- SectionItemView 表示 SectionItem 在 SectionWorkspace 中的可视化表现。
- SectionItemView 是上层概念。
- `ContentBlockDisplay`、`AtomicSectionBlock`、`CompositeBlock` 是具体组件。
- 后续文档、组件树、开发计划和命名说明不再使用 `SectionItemCard` 表示工作区里的 SectionItem。

### ComponentLab

已确认：

- ComponentLab 是当前开发轮次的验收入口。
- ComponentLab 不是永久组件展览馆。
- 每一轮开发结束后，只保留本轮需要验收的组件。
- 上一轮无关组件应移出当前 ComponentLab 视图。
- ComponentLab 应作为独立验收页面，不应包在主应用 AppShell、主导航或左侧导航中。
- 页面级开发可以把完整页面 mock 放入 ComponentLab，让用户直接确认本轮结果。
- 每轮交付时，必须说明本轮开发内容和 ComponentLab 中的具体验收区域。

## 1. 新前端工程目录如何落地？

当前结论：

- 不存在 `frontend-v2/`
- 当前只有 `src-v2/` 后端项目

需确认：

- 是否按 `docs/ui/ui-architecture.md` 新建独立 V2 前端工程？
- 目录名是否继续采用建议名 `frontend-v2/`？

## 2. ComponentLabPage 是否独立于主应用外壳？

当前结论：

- `/lab`、`src/labs/` 和 `ComponentLabPage` 已作为 V2 前端基础能力存在。
- 后续需要确保 `/lab` 是独立验收页面，不包在 AppShell 主导航中。

需确认：

- 是否在下一轮把 `/lab` 从主应用 AppShell 中拆出，作为独立验收路由？

## 3. ContentBlockDisplay 与 ContentBlockCard 的命名边界是否要补入 component-rules？

当前结论：

- 文档存在边界差异
- 资源卡片和文档流展示组件语义不同

需确认：

- 后续是否更新 `docs/ui/component-rules.md`，明确：
  - `ContentBlockCard` 只用于资源库/选择器
  - `ContentBlockDisplay` 只用于文档流工作区

## 4. InsertPoint 应归类为 Presentation 还是 Business？

当前建议：

- `Presentation`

理由：

- 只表达插入位置
- 不理解业务
- 不调 API

需确认：

- 是否接受这一分类？

## 5. AtomicSectionBlock 是否允许创建空 AtomicSection？

当前冲突：

- `docs/ui/section-page.md` 暗示 Toolbar 可直接新建 AtomicSection
- `docs/ui/小节页面-需求文档.md` 更偏向第一版先支持“连续块升级”

需确认：

- 第一版是否允许直接创建空 AtomicSection？
- 还是只支持从连续块升级？

## 6. Root Promotion 规则由什么提供？

候选方案：

- 节点 metadata
- 回调 `canPromoteToRoot(node)`
- 业务配置表

当前建议：

- 业务树提供 `canPromoteToRoot(node)`

需确认：

- 是否采用 callback 方案作为主接口？

## 7. difficulty 当前是否已有后端字段？（已关闭）

当前定稿：

- `Section`
- `SectionVariant`
- `AtomicSection`
- `ContentBlock`

都已有或应使用统一 `Difficulty`。

已确认：

- 当前 V2 `AtomicSection` 已有 `Difficulty` 字段。
- 后续文档不应再把 `AtomicSection.Difficulty` 写成缺口。

## 8. Word 编辑入口第一版只显示按钮，还是要接现有编辑链路？

当前结论：

- 前端不能再复刻 V1 编辑链路
- 必须等待或补齐 V2 API 对应能力

需确认：

- SectionPage 第一版：
  - 只显示按钮占位
  - 还是必须先补 V2 后端编辑会话 API

## 9. SectionPage 第一版是否真实写入？

当前建议：

- `mock first`
- `真实只读`
- `写入继续 mock 或占位`

原因：

- 写入相关 API 不完整
- 当前任务也明确禁止直接接真实写入 API

需确认：

- 后续正式实现第一轮是否接受“只读真实 + 写入 mock”？

## 10. 是否需要支持多个 Section 同时打开？

当前建议：

- UI 第一版只打开一个 Section
- 状态结构预留多 Section

需确认：

- 是否接受这种折中方案？

## 11. docs/ui/section-page.md 与 docs/ui/小节页面-需求文档.md 是否存在冲突？

当前结论：

- 存在冲突

主要包括：

- Toolbar 对 `SectionVariant` 的强调程度
- 空 AtomicSection 创建方式
- 插入能力第一版范围
- `ContentBlockCard` vs `ContentBlockDisplay` 命名边界

需确认：

- 这些冲突是否按准备文档中的建议处理？

## 12. SectionItem 第一版支持的 targetType 具体有哪些？

当前 V2 后端：

- `ContentBlock`
- `AtomicSection`

需求文档还提到：

- CompositeBlock / GroupBlock 作为工作区结构

需确认：

- 第一版 SectionItem.targetType 是否严格只保留：
  - `ContentBlock`
  - `AtomicSection`

如果是这样：

- CompositeBlock 只能作为 `ContentBlockType.ExampleGroup / ExerciseGroup / VariantGroup` 表达，而不是独立 targetType。

## 13. AtomicSection 内部是否允许 CompositeBlock / QuestionGroup？

当前需求倾向：

- 允许“其他允许的非 AtomicSection 块”

当前需确认：

- 第一版 AtomicSection 内是否允许引用：
  - `ExampleGroup`
  - `ExerciseGroup`
  - `VariantGroup`

## 14. QuestionGroup / ExampleGroup 是否只作为普通结构块，不允许 Root Promotion？

当前建议：

- 不允许 Root Promotion

依据：

- `docs/ui/小节页面-需求文档.md` 明确说 `QuestionGroup / ExampleGroup` 不应提升为根节点

需确认：

- 是否把这条写成 SectionPage 第一版固定规则？

## 15. SectionWorkspace 是否必须在第一版支持 Teaching Note Mode 双列？

当前文档差异：

- `docs/ui/section-page.md` 更强调三段式工作台
- `docs/ui/小节页面-需求文档.md` 里 Teaching Note Mode 是重要扩展方向，但当前任务是准备阶段

需确认：

- 第一版实现是否必须包含：
  - `MainContentColumn | TeachingNoteColumn`
  - 还是只做状态和布局预留

## 16. SectionStructurePanel 的三种模式第一版是否都要落地？

文档要求：

- `Hidden`
- `Docked`
- `Focused`

需确认：

- 第一版是否必须三种都实现？
- 还是先做 `Docked + Focused`，`Hidden` 作为简单折叠状态？

## 17. 真实读取接口是否直接使用当前 V2 后端现状？

当前结论：

- `/api/cms-v2` 已有读取基础
- 但缺少 SectionPage 聚合读取接口

需确认：

- 第一版是否接受前端自行聚合：
  - `sections`
  - `section items`
  - `content blocks`
  - `atomic sections`
  - `section variants`

还是需要后端先提供专用聚合接口？

## 18. 连续块升级 AtomicSection 是否必须有后端专用用例？

当前建议：

- 必须有后端专用用例

原因：

- 前端自行多步重排和重挂接风险太高

需确认：

- 后续实现阶段是否先补这个用例，再做前端交互？

## 当前补充结论：Word 编辑入口需要 V2 编辑会话 API

已确认：

- `ContentBlock` 操作区中的 Word 编辑不是前端本地打开文件。
- 后续后端可能迁移到云端，因此 Word 编辑启动方式不能固化在前端。
- CMS V2 后端需要提供稳定的 `ContentBlock` 编辑会话 API。
- 本地打开 Word、未来云端编辑、外部 URI 跳转等差异应由后端策略封装。

当前结论：

- SectionPage 第一版接真实 Word 编辑前，先实现 CMS V2 后端编辑会话能力。
- `ContentBlockDisplay` / `SectionItemView` 只发出 `openWord` 事件。
- `SectionPage` 或页面级 composable 调用 `/api/cms-v2`。
- 不调用 V1 `编辑会话` 接口。
- 不在前端构造本地 DOCX 路径、`ms-word:` 或 `file://`。

该问题不再作为 UI 组件开放问题处理，转入后端前置计划：

```text
docs/superpowers/plans/2026-06-17-content-block-word-edit-session-v2.md
```

## 当前关闭问题：SectionVariant

以下问题已经通过 2026-06-22 的 SectionVariant 文档定稿关闭。

### 1. Section 与 SectionVariant 的关系

结论：

- `Section` 本身就是上帝小节 / 完整知识池 / 完整教学结构。
- `SectionVariant` 是 `Section` 的子级教学用途方案。
- 不存在“Section 下面再有一个上帝版本”的模型。

### 2. SectionVariant 选择什么对象

结论：

- 第一版只选择顶层 `SectionItem`。
- `SectionVariantItem` 保存 `SectionItemId`。
- 前端提交 `selectedSectionItemIds`。
- 不提交 `ContentBlockId`、`AtomicSectionId`、`AtomicSectionItemId` 或 `flowItemId`。

### 3. CompositeBlock 是否是 TargetType

结论：

- 不是。
- `CompositeBlock` 只是组类型 `ContentBlock` 在前端的展示组件。
- 后端 `SectionItem.TargetType` 第一版仍只有 `ContentBlock` 与 `AtomicSection`。

### 4. AtomicSection 是否有难度字段

结论：

- 当前 V2 模型中 `AtomicSection` 已有 `Difficulty` 字段。
- 后续文档不再把它列为缺口。

### 5. SectionVariant 创建入口

结论：

第一版唯一入口：

```text
SectionPage -> SectionTree -> Section 根节点右键菜单 -> 新建 SectionVariant
```

不从 Toolbar、Workspace、Inspector 或 TeachingStructureTree 创建。

### 6. 创建后是否立即打开

结论：

- 创建后不立即打开。
- 创建后刷新 `SectionTree` / Variant 列表。
- 显示创建成功提示。

### 7. 是否允许创建空 Variant

结论：

- 允许。
- 用户可以在第二步不勾选任何 `SectionItem`。

### 8. 后续 AtomicSection 部分选择

结论：

- 第一版不做。
- 后续必须以 `SectionVariantItem` 为锚点扩展 `SectionVariantAtomicItemSelection`。
- 不直接用 `SectionVariantId + AtomicSectionId` 建模。
## 当前已关闭问题：AtomicSection Panel 模型

以下问题已经确认，不再作为开放问题处理。

### 1. AtomicSection 创建时是否初始化默认内容

结论：

- 不初始化默认内容。
- 创建 `AtomicSection` 时只创建 `AtomicSection` 本体。
- 不自动创建知识点、例题组、练习题组。
- 空 `AtomicSection` 由前端显示插入入口。

说明：

当前数据库均为模拟数据，可以废弃重建，因此不需要考虑旧数据迁移或为历史 AS 自动补默认内容。

### 2. 是否需要 AtomicSectionPanel

结论：

- 需要新增 `AtomicSectionPanel`。
- 它是后端持久化模型，不只是前端分组。
- 它用于表达 `AtomicSection` 内部的知识点、例题、变式题、练习题、课后练习等教学板块。

### 3. CompositeBlock 是否是独立领域模型

结论：

- 不是。
- `CompositeBlock` 仍然是 `ContentBlock.BlockType` 为组类型时的前端展示组件。
- 后端不新增 `CompositeBlock` target type。

### 4. ContentBlock 是否记录所属 AS / Panel

结论：

- 不记录。
- `ContentBlock` 是可复用资产。
- 所属 AS、Panel、Section、Handout 都由引用关系表达。

### 5. 未归组区域是否存在

结论：

- 存在。
- `AtomicSectionItem.AtomicSectionPanelId = null` 表示未归组区域。
- 未归组区域在 Workspace 和 SectionTree 中都需要可见。

### 6. Panel 标题是否进入 Handout 输出

结论：

- 第一版不进入。
- Handout 展开 AS 时按 panel 顺序输出 item，但不输出 panel 标题。
