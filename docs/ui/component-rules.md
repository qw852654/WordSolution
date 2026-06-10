# CMS V2 前端组件规则

本文档定义 V2 前端组件分层、职责边界、API 调用规则、mock 优先流程和组件验证要求。

## 1. 组件类型

### 1.1 Presentation Components

纯 UI 展示组件。

职责：

- 接收 props。
- 渲染视觉结构。
- 通过 emits 暴露事件。
- 不理解具体 CMS 业务含义。
- 不调用 API。
- 不读取 Pinia store。

示例：

```text
Badge
Breadcrumb
ToolbarButton
IconButton
EmptyState
LoadingState
FieldLabel
```

### 1.2 Business Components

业务展示组件。

职责：

- 理解某个业务对象的展示语义。
- 可以组合 presentation components。
- 可以使用 composables。
- 默认不直接调用 API。

示例：

```text
ContentBlockCard
AtomicSectionCard
SectionItemView
SectionVariantCard
HandoutItemCard
OutputFormCard
GeneratedFileRow
TeachingNotePanel
```

### 1.3 Business Container Components

可复用业务容器。

职责：

- 封装一段可复用业务流程。
- 可以加载数据。
- 可以调用 API 或 composables。
- 可以管理局部容器状态。

示例：

```text
ContentBlockPicker
AtomicSectionPicker
SectionItemPicker
OutputTemplatePicker
TeachingNoteDrawer
GeneratedFilePanel
```

## 2. API 调用规则

```text
Presentation Components
  No API calls

Business Components
  Prefer composables

Business Container Components
  API calls allowed

Pages
  API calls allowed

Composables
  API calls allowed
```

禁止：

```text
在普通展示组件中直接 fetch。
在多个组件中复制同一个 API URL。
把页面状态偷偷放进全局 store。
```

允许：

```text
页面加载主数据。
业务容器加载选择器数据。
composable 封装查询、选择、保存、展开等复用逻辑。
```

## 3. Mock Data First 工作流

所有可复用组件开发遵循：

```text
Define DTO
↓
Create Mock Data
↓
Build Component
↓
Connect API
```

要求：

- 没有代表性 mock 数据，不开始做可复用组件。
- mock 数据必须覆盖空状态、长文本、多层级、禁用状态、错误状态。
- 组件先在 ComponentLabPage 验证，再接入真实页面。

## 3.1 样式约束

在用户没有明确给出详细视觉修改要求之前：

- 允许组件先使用最简样式落地。
- 禁止自行发挥成装饰性方案。

必须遵守：

- 不在组件中随意写一次性颜色值。
- 不使用大面积阴影、渐变、装饰性背景。
- 不为相似组件重复写多套样式。
- 优先复用 shadcn-vue 基础组件与 Tailwind spacing、border、text、background token。
- 布局结构、组件层级、状态类必须稳定，不因 hover、选中、加载而临时改结构。
- 如果视觉细节不确定，先保持简洁中性，不自行补充装饰。
- 所有可复用组件必须先在 ComponentLabPage 中以 mock 数据验收，再进入真实页面。

### 3.1.1 Theme Token Rule

后续所有业务组件，包括：

- `ContentBlockDisplay`
- `AtomicSectionBlock`
- `CompositeBlock`
- `SectionTree`
- `SectionInspector`
- `Toolbar`
- `StatusTag`

禁止直接写死颜色。

统一通过 Theme Token 引用颜色。

如果当前缺少 Token：

- 先提出需要新增什么 Token。
- 不要直接写颜色值。

## 3.2 新组件开发前确认

每次新增或抽象 UI 组件前，必须先向用户做简要对齐，内容包括：

- 组件名称。
- 组件职责。
- 组件不负责什么。
- 输入数据或 mock 数据范围。
- 对外 emits / 事件边界。
- 需要放入 ComponentLabPage 的验收场景。

用户确认后才能开始实现该组件。

## 4. ComponentLabPage

建议路由：

```text
/lab
```

职责：

```text
Component Development
Mock Data Testing
UI Verification
```

每个可复用组件至少提供：

- 默认状态。
- 选中状态。
- 禁用状态。
- 长标题 / 长正文。
- 空数据状态。
- 加载状态。
- 错误状态。

优先验证组件：

```text
当前开发轮次相关组件
```

说明：

- ComponentLabPage 是当前开发轮次的验收入口，不是永久组件展览馆。
- 每一轮只保留本轮需要验收的组件和 mock 场景。
- 上一轮无关组件应从当前 ComponentLabPage 视图中移除。
- ComponentLabPage 必须作为独立页面渲染，不应包在主应用 AppShell、主导航或左侧导航中。
- 页面级功能验收时，可以在 ComponentLabPage 中放入完整页面 mock，而不只展示孤立组件。
- 每轮交付时必须说明 ComponentLabPage 中具体放入了什么、用户需要验收哪些区域、哪些按钮或数据仍是占位。

## 5. 核心业务组件职责

### ContentBlockCard

表示一个可复用内容资产。

必须展示：

- 标题。
- 内容类型。
- 难度。
- 状态。
- 当前版本信息。
- 摘要或纯文本预览入口。

允许操作：

- 选择。
- 打开详情。
- 打开 Word 编辑入口。
- 查看 HTML 预览。

### AtomicSectionCard

表示一个原子教学片段。

必须展示：

- 标题。
- 类型。
- 状态。
- 内部内容块数量。
- 简要说明。

语义：

- AtomicSection 组织 ContentBlock。
- AtomicSection 自身不承载可编辑正文。

### SectionItemView

表示 SectionItem 在 SectionWorkspace 中的可视化表现。

当前已确认口径：

- SectionItemView 是上层容器，不是资源卡片。
- SectionItemView 不展示标题、类型、状态、版本、备注、引用模式或摘要。
- SectionItemView 只负责承载未来的 ContentBlockDisplay / AtomicSectionBlock / CompositeBlock 等具体内容组件。
- SectionItemView 的宽度应弹性填满横向区域。
- SectionItemView 的高度由内部实际渲染内容自动撑开。
- SectionItemView 允许子级 SectionItemView，用于表达 SectionItem 的父子层级。
- SectionItemView 默认不显示边框。
- SectionItemView 的右侧纵向操作区默认隐藏。
- 鼠标 hover 到 SectionItemView 的正文区域时，不显示边框，也不显示操作图标。
- 只有鼠标进入右侧纵向操作热区，或键盘 focus 进入右侧操作区时，右侧操作图标和 SectionItemView 容器边框才一起显现。

语义：

- 修改它只修改小节结构引用。
- 不直接修改源 ContentBlock 或 AtomicSection。
- SectionItemView 是上层概念，具体内容由 ContentBlockDisplay / AtomicSectionBlock / CompositeBlock 承载。
- 实现必须先在 ComponentLabPage 中用 Mock Data 验收默认、选中、禁用、横向填满、内容自适应高度、子级结构和 hover 操作区显隐。
- 组件只通过 emits 暴露选择、前插、后插、上移、下移、缩进、反缩进、移除和 Word 编辑入口。
- 组件不调用 API，不读取 Pinia，不持有 SectionPage 页面状态。

### SectionPage Skeleton Components

当前最小骨架包含：

- `SectionTopToolbar`
- `SectionStructurePanel`
- `SectionWorkspace`
- `SectionInspector`

职责：

- `SectionTopToolbar` 只作为右侧顶部的紧凑工具控件区，不显示页面标题。
- `SectionStructurePanel` 只保留左侧结构树区域和空状态。
- `SectionWorkspace` 保留低高度 Section 信息条、SectionItemView 文档流主列、未来 TeachingNoteColumn 分栏预留和空状态，并在竖直方向占满页面主工作区高度。
- `SectionWorkspace` 文档流主滚动区使用 `WeakScrollArea`，避免默认粗滚动条抢占内容注意力。
- `SectionInspector` 只保留右侧选中对象检查区域和空状态。

边界：

- 不接 API。
- 不写入数据。
- 不实现 SectionTree、FocusTree 联动、真实 SectionItemView 列表、ContentBlockDisplay、AtomicSectionBlock 或真实 InsertPoint 交互。
- 本轮 ComponentLabPage 只展示这些骨架组件。

### WeakScrollArea

表示弱视觉滚动容器。

职责：

- 统一承载页面中需要竖向滚动的局部区域。
- 使用轻量轨道和弱视觉滑块，降低默认滚动条对内容区的视觉干扰。
- 优先用于 SectionWorkspace 文档流、TeachingNoteColumn、SectionStructurePanel、SectionInspector，以及后续 HandoutPage 的类似滚动区域。

边界：

- 不理解 CMS 业务语义。
- 不调用 API。
- 不读取 Pinia。
- 不管理滚动区域内部内容状态。
- 不替代页面布局容器，只负责滚动外壳。

### SectionInspector

表示 SectionPage 右侧当前选中节点检查面板。

必须展示：

- 当前选中标题。
- 目标类型。
- 状态。
- 排序和层级。
- 引用模式。
- 锁定版本。
- 摘要。
- 备注。

语义：

- 只显示当前选中的 SectionItem / AtomicSection / ContentBlock 引用信息。
- 不直接修改 Section 结构。
- 不直接修改源 ContentBlock 或 AtomicSection。
- 第一轮只提供预览和 Word 编辑入口事件，不调用 API。
- 必须在 ComponentLabPage 中同时展示空状态和选中状态。

### SectionVariantCard

表示同一 Section 下的一个教学用途变体。

必须展示：

- 标题。
- 类型。
- 难度。
- 状态。
- 已选 SectionItem 数量。

### HandoutItemCard

表示讲义版本中的一个输出编排项。

必须展示：

- 目标类型。
- 目标标题。
- 排序。
- 标题覆盖。
- 备注。

语义：

- 引用 SectionVariant 时，展示为展开预览。
- 调整讲义项不能反向修改源 Section 结构。

## 6. shadcn-vue 使用规则

优先使用 shadcn-vue 提供的基础组件：

```text
Button
Input
Textarea
Select
Dialog
Drawer
Sheet
DropdownMenu
Tabs
Tooltip
Badge
Card
Table
Sidebar
```

约束：

- 主导航优先基于 Sidebar 模式，不手写一套无语义 sidebar。
- 表单优先复用统一表单组件与校验模式。
- 不为每个页面临时创造相似按钮、Badge 和面板样式。
- 卡片只用于重复业务对象，不把整页 section 做成卡片套卡片。

## 7. 图标规则

- 图标优先使用 `lucide-vue-next`。
- 不使用 emoji 作为 UI 图标。
- 图标按钮必须有 Tooltip 或 `aria-label`。
- 图标尺寸保持稳定，常用 16px、18px、20px、24px 四档。

## 8. 文本和 i18n

所有可见文本必须通过 i18n key：

```vue
<Button>{{ t("common.save") }}</Button>
```

禁止：

```vue
<Button>Save</Button>
```

组件 props 中如果传入显示文案，调用方也应从 i18n 获取。

