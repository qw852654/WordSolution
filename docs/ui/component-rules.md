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
SectionItemCard
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
ContentBlockCard
AtomicSectionCard
SectionItemCard
FocusTree
SectionTree
SectionStructurePanel
SectionInspector
ContentBlockPicker
```

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

### SectionItemCard

表示小节结构中的一个引用项。

必须展示：

- 目标标题。
- 目标类型。
- 引用模式。
- 锁定版本标识。
- 排序和层级。

语义：

- 修改它只修改小节结构引用。
- 不直接修改源 ContentBlock 或 AtomicSection。

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

