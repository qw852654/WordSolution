# CMS V2 前端组件规则

本文档定�?V2 前端组件分层、职责边界、API 调用规则、mock 优先流程和组件验证要求�?

## 1. 组件类型

### 1.1 Presentation Components

�?UI 展示组件�?

职责�?

- 接收 props�?
- 渲染视觉结构�?
- 通过 emits 暴露事件�?
- 不理解具�?CMS 业务含义�?
- 不调�?API�?
- 不读�?Pinia store�?

示例�?

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

业务展示组件�?

职责�?

- 理解某个业务对象的展示语义�?
- 可以组合 presentation components�?
- 可以使用 composables�?
- 默认不直接调�?API�?

示例�?

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

可复用业务容器�?

职责�?

- 封装一段可复用业务流程�?
- 可以加载数据�?
- 可以调用 API �?composables�?
- 可以管理局部容器状态�?

示例�?

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

禁止�?

```text
在普通展示组件中直接 fetch�?
在多个组件中复制同一�?API URL�?
把页面状态偷偷放进全局 store�?
```

允许�?

```text
页面加载主数据�?
业务容器加载选择器数据�?
composable 封装查询、选择、保存、展开等复用逻辑�?
```

## 3. Mock Data First 工作�?

所有可复用组件开发遵循：

```text
Define DTO
�?
Create Mock Data
�?
Build Component
�?
Connect API
```

要求�?

- 没有代表�?mock 数据，不开始做可复用组件�?
- mock 数据必须覆盖空状态、长文本、多层级、禁用状态、错误状态�?
- 组件先在 ComponentLabPage 验证，再接入真实页面�?

## 3.1 样式约束

在用户没有明确给出详细视觉修改要求之前：

- 允许组件先使用最简样式落地�?
- 禁止自行发挥成装饰性方案�?

必须遵守�?

- 不在组件中随意写一次性颜色值�?
- 不使用大面积阴影、渐变、装饰性背景�?
- 不为相似组件重复写多套样式�?
- 优先复用 shadcn-vue 基础组件�?Tailwind spacing、border、text、background token�?
- 布局结构、组件层级、状态类必须稳定，不�?hover、选中、加载而临时改结构�?
- 如果视觉细节不确定，先保持简洁中性，不自行补充装饰�?
- 所有可复用组件必须先在 ComponentLabPage 中以 mock 数据验收，再进入真实页面�?

### 3.1.1 Theme Token Rule

后续所有业务组件，包括�?

- `ContentBlockDisplay`
- `AtomicSectionBlock`
- `CompositeBlock`
- `SectionTree`
- `SectionInspector`
- `Toolbar`
- `StatusTag`

禁止直接写死颜色�?

统一通过 Theme Token 引用颜色�?

如果当前缺少 Token�?

- 先提出需要新增什�?Token�?
- 不要直接写颜色值�?

## 3.2 新组件开发前确认

每次新增或抽�?UI 组件前，必须先向用户做简要对齐，内容包括�?

- 组件名称�?
- 组件职责�?
- 组件不负责什么�?
- 输入数据�?mock 数据范围�?
- 对外 emits / 事件边界�?
- 需要放�?ComponentLabPage 的验收场景�?

用户确认后才能开始实现该组件�?

## 4. ComponentLabPage

建议路由�?

```text
/lab
```

职责�?

```text
Component Development
Mock Data Testing
UI Verification
```

每个可复用组件至少提供：

- 默认状态�?
- 选中状态�?
- 禁用状态�?
- 长标�?/ 长正文�?
- 空数据状态�?
- 加载状态�?
- 错误状态�?

优先验证组件�?

```text
当前开发轮次相关组�?
```

说明�?

- ComponentLabPage 是当前开发轮次的验收入口，不是永久组件展览馆�?
- 每一轮只保留本轮需要验收的组件�?mock 场景�?
- 上一轮无关组件应从当�?ComponentLabPage 视图中移除�?
- ComponentLabPage 必须作为独立页面渲染，不应包在主应用 AppShell、主导航或左侧导航中�?
- 页面级功能验收时，可以在 ComponentLabPage 中放入完整页�?mock，而不只展示孤立组件�?
- 每轮交付时必须说�?ComponentLabPage 中具体放入了什么、用户需要验收哪些区域、哪些按钮或数据仍是占位�?

## 5. 核心业务组件职责

### ContentBlockCard

表示一个可复用内容资产�?

使用范围�?

- 资源库�?
- 内容选择器�?
- 其他需要“选择内容资产”的列表或网格�?

不用于：

- SectionWorkspace 文档流正文展示�?
- SectionItemView 内部正文内容�?

必须展示�?

- 标题�?
- 内容类型�?
- 难度�?
- 状态�?
- 当前版本信息�?
- 摘要或纯文本预览入口�?

允许操作�?

- 选择�?
- 打开详情�?
- 打开 Word 编辑入口�?
- 查看 HTML 预览�?

### ContentBlockDisplay

表示 ContentBlock �?SectionWorkspace 文档流中的正文展示�?

职责�?

- 展示 ContentBlock 的正�?HTML 预览�?
- 不显�?ContentBlock 标题�?
- 不显示版本信息�?
- 正文预览区域不显示边框，尽量贴近文档流�?
- 不显�?ContentBlock 类型、可用状态、引用模式等文字元信息�?
- 只显示难度，难度只用左侧顶部的小颜色点表示；具体颜色值后续由用户确认后再固定�?
- 自身上下左右 padding �?0，间距由外层 SectionItemView 控制�?
- 鼠标 hover �?ContentBlockDisplay 时不显示边框�?
- 提供轻量动作入口：Word 编辑、刷新预览、更多�?
- 可以作为 SectionItemView �?slot 内容�?
- 可以作为 AtomicSectionBlock / CompositeBlock 的子内容�?

边界�?

- 不作为资源库卡片使用�?
- 不使�?`StructuredContainer`�?
- 不直接调�?API�?
- 不直接实�?Word 编辑会话轮询�?
- 不持�?SectionPage 页面状态�?

ComponentLabPage 验收�?

- 默认状态�?
- 选中状态�?
- LockedVersion�?
- �?HTML 预览�?
- 长正文�?
- 禁用状态�?
- 不显示标题和版本�?
- 不显�?ContentBlock 类型和状态�?

### InsertPoint

表示文档流中“这里可以插入”的交互位置�?

职责�?

- 出现在两�?flow item 之间�?
- 默认弱化显示�?
- 高度保持紧凑�?
- 鼠标停留�?0.5 秒后显示插入入口�?
- 键盘 focus 后应显示插入入口�?
- 中间提供 slot，用于展示当前位置允许插入的全部内容类型�?
- 通过 `insert` 事件把插入点 id 交给父组件�?

边界�?

- 不决定可以插入哪些业务对象�?
- 不调�?API�?
- 不写死插入菜单选项�?
- 不修�?Section 数据�?

### StructuredContainer / InlineBorderHeader

表示 AtomicSectionBlock �?CompositeBlock 共享的弱边框结构容器�?

职责�?

- `StructuredContainer` 负责弱边框容器和 body slot�?
- `InlineBorderHeader` 负责边框线上的标题和 actions slot�?
- 支持长标题和多个操作入口�?
- AtomicSectionBlock / CompositeBlock 内部子块不使用左侧竖线�?
- AtomicSectionBlock / CompositeBlock 内部的每�?ContentBlockDisplay 必须先由子级 SectionItemView 包裹，再承载正文展示�?

边界�?

- 不理�?CMS 业务语义�?
- 不调�?API�?
- 不用�?ContentBlockDisplay�?
- 不持有展开、选中或写入状态�?

### AtomicSectionCard

表示一个原子教学片段�?

必须展示�?

- 标题�?
- 类型�?
- 状态�?
- 内部内容块数量�?
- 简要说明�?

语义�?

- AtomicSection 组织 ContentBlock�?
- AtomicSection 自身不承载可编辑正文�?

### SectionItemView

表示 SectionItem �?SectionWorkspace 中的可视化表现�?

当前已确认口径：

- SectionItemView 是上层容器，不是资源卡片�?
- SectionItemView 不展示标题、类型、状态、版本、备注、引用模式或摘要�?
- SectionItemView 只负责承载未来的 ContentBlockDisplay / AtomicSectionBlock / CompositeBlock 等具体内容组件�?
- SectionItemView 的宽度应弹性填满横向区域�?
- SectionItemView 的高度由内部实际渲染内容自动撑开�?
- SectionItemView 允许子级 SectionItemView，用于表�?SectionItem 的父子层级�?
- SectionItemView 默认不显示边框�?
- SectionItemView 的右侧纵向操作区默认隐藏�?
- SectionItemView 的右侧纵向操作区必须脱离正常布局流，不允许撑�?SectionItemView�?
- 多个 SectionItemView 连续出现时默认竖直贴合，不在外层额外添加 gap / margin�?
- 鼠标 hover �?SectionItemView 的正文区域时，不显示边框，也不显示操作图标�?
- 只有鼠标进入右侧纵向操作热区，或键盘 focus 进入右侧操作区时，右侧操作图标和 SectionItemView 容器边框才一起显现�?

语义�?

- 修改它只修改小节结构引用�?
- 不直接修改源 ContentBlock �?AtomicSection�?
- SectionItemView 是上层概念，具体内容�?ContentBlockDisplay / AtomicSectionBlock / CompositeBlock 承载�?- 实现必须先在 ComponentLabPage 中用 Mock Data 验收默认、选中、禁用、横向填满、内容自适应高度、子级结构和 hover 操作区显隐�?- 组件只通过 emits 暴露选择、前插、后插、上移、下移、缩进、反缩进、移除和 Word 编辑入口�?- 组件不调�?API，不读取 Pinia，不持有 SectionPage 页面状态�?
### SectionTree

表示 SectionStructurePanel 中的当前 Section 结构树�?
职责�?
- 展示当前 Section 内部�?SectionItem 结构�?- 节点可以表达 Section、AtomicSection、CompositeBlock、ContentBlock �?Section 内部结构对象�?- 展示层级、展开 / 折叠、选中态、禁用态和节点类型摘要�?- 点击节点只通过事件把节�?id 交给父级，由父级决定是否滚动工作区、更�?Inspector 或触发其他页面状态�?- 复用 BasicTree 的通用展开 / 折叠和树语义能力�?
边界�?
- 不调�?API�?- 不读�?Pinia�?- 不持�?SectionPage 页面�?selectedNodeId�?- 不直接滚�?SectionWorkspace�?- 不直接修�?SectionItem 顺序、层级或引用关系�?- 不混�?TeachingTopic、Handout、GeneratedFile �?ContentBlockVersion�?- 不把 BasicTree 机制写成 Section 专用规则�?
ComponentLabPage 验收�?
- 默认层级树�?- 折叠 / 展开按钮�?- 选中态�?- 禁用节点�?- 长标题�?- 空状态�?
### SectionTreeNode

表示 SectionTree 中的一行节点内容�?
职责�?
- 展示节点标题；题组和题目类节点的主显示名使用类型名，而不是独立标题�?- 展示难度，使用紧贴节点标题左侧的短竖线表示，竖线使用统一主题色，不在组件中写死具体颜色值�?- 展示业务类型，例如知识点、例题、例题组、变式题组�?- 当节点是题组类对象时，展示题目数量�?- 作为 SectionTree 的节点内容插槽使用，方便后续扩展更多字段�?
边界�?
- 不负责展开 / 折叠�?- 不负责节点选中状态管理�?- 不调�?API�?- 不读�?Pinia�?- 不直接滚�?SectionWorkspace�?
### SectionPage Skeleton Components

当前最小骨架包含：

- `SectionTopToolbar`
- `SectionStructurePanel`
- `SectionWorkspace`
- `SectionInspector`

职责�?

- `SectionTopToolbar` 只作为右侧顶部的紧凑工具控件区，不显示页面标题�?
- `SectionStructurePanel` 只保留左侧结构树区域和空状态�?
- `SectionWorkspace` 保留低高�?Section 信息条、SectionItemView 文档流主列、未�?TeachingNoteColumn 分栏预留和空状态，并在竖直方向占满页面主工作区高度�?
- `SectionWorkspace` 文档流主滚动区使�?`WeakScrollArea`，避免默认粗滚动条抢占内容注意力�?
- `SectionInspector` 只保留右侧选中对象检查区域和空状态�?

边界�?

- 不接 API�?
- 不写入数据�?
- 不实�?SectionTree、BasicTree 联动、真�?SectionItemView 列表、ContentBlockDisplay、AtomicSectionBlock 或真�?InsertPoint 交互�?
- 本轮 ComponentLabPage 只展示这些骨架组件�?

### WeakScrollArea

表示弱视觉滚动容器�?

职责�?

- 统一承载页面中需要竖向滚动的局部区域�?
- 使用轻量轨道和弱视觉滑块，降低默认滚动条对内容区的视觉干扰�?
- 优先用于 SectionWorkspace 文档流、TeachingNoteColumn、SectionStructurePanel、SectionInspector，以及后�?HandoutPage 的类似滚动区域�?

边界�?

- 不理�?CMS 业务语义�?
- 不调�?API�?
- 不读�?Pinia�?
- 不管理滚动区域内部内容状态�?
- 不替代页面布局容器，只负责滚动外壳�?

### SectionInspector

表示 SectionPage 右侧当前选中节点检查面板�?

必须展示�?

- 当前选中标题�?
- 目标类型�?
- 状态�?
- 排序和层级�?
- 引用模式�?
- 锁定版本�?
- 摘要�?
- 备注�?

语义�?

- 只显示当前选中�?SectionItem / AtomicSection / ContentBlock 引用信息�?
- 不直接修�?Section 结构�?
- 不直接修改源 ContentBlock �?AtomicSection�?
- 第一轮只提供预览�?Word 编辑入口事件，不调用 API�?
- 必须�?ComponentLabPage 中同时展示空状态和选中状态�?

### SectionVariantCard

表示同一 Section 下的一个教学用途变体�?

必须展示�?

- 标题�?
- 类型�?
- 难度�?
- 状态�?
- 已�?SectionItem 数量�?

### HandoutItemCard

表示讲义版本中的一个输出编排项�?

必须展示�?

- 目标类型�?
- 目标标题�?
- 排序�?
- 标题覆盖�?
- 备注�?

语义�?

- 引用 SectionVariant 时，展示为展开预览�?
- 调整讲义项不能反向修改源 Section 结构�?

## 6. shadcn-vue 使用规则

优先使用 shadcn-vue 提供的基础组件�?

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

约束�?

- 主导航优先基�?Sidebar 模式，不手写一套无语义 sidebar�?
- 表单优先复用统一表单组件与校验模式�?
- 不为每个页面临时创造相似按钮、Badge 和面板样式�?
- 卡片只用于重复业务对象，不把整页 section 做成卡片套卡片�?

## 7. 图标规则

- 图标优先使用 `lucide-vue-next`�?
- 不使�?emoji 作为 UI 图标�?
- 图标按钮必须�?Tooltip �?`aria-label`�?
- 图标尺寸保持稳定，常�?16px�?8px�?0px�?4px 四档�?

## 8. 文本�?i18n

所有可见文本必须通过 i18n key�?

```vue
<Button>{{ t("common.save") }}</Button>
```

禁止�?

```vue
<Button>Save</Button>
```

组件 props 中如果传入显示文案，调用方也应从 i18n 获取�?


## 当前补充约定：InsertPoint 与 BlockSearchPicker

### InsertPoint 当前使用规则

在 SectionWorkspace 中，InsertPoint 直接呈现当前位置允许的具体操作按钮。

当前按钮为：

1. 新建 ContentBlock
2. 新建 AtomicSection
3. 插入已有块

规则：

- 不再使用单个“插入”按钮作为主入口。
- 不再通过点击“插入”后展开二级面板。
- 点击“新建 ContentBlock”进入新建 ContentBlock 流程。
- 点击“新建 AtomicSection”进入新建 AtomicSection 流程。
- 点击“插入已有块”后续打开 BlockSearchPicker。
- InsertPoint 不直接修改 Section 数据，不调用 API。

### BlockSearchPicker

BlockSearchPicker 表示从已有块中搜索并选择插入目标的业务容器组件。

职责：

- 同时搜索 ContentBlock 和 AtomicSection。
- 在同一个结果列表中展示对象类型、标题和必要摘要。
- 允许用户选择一个已有块，并通过事件把选中对象交给父级。
- 支持空结果、长标题、禁用项和加载状态的 Mock Data 验收。

边界：

- 不直接修改 Section 数据。
- 不直接创建 ContentBlock 或 AtomicSection。
- 不把 ContentBlock 和 AtomicSection 拆成两个互斥搜索入口。
- 不在当前 InsertPoint 小闭环中实现，后续单独开发。
- 后续必须先进入 ComponentLab 使用 Mock Data 验收，再接入 SectionPage。
## 当前补充约定：InsertCreateOverlay

InsertCreateOverlay 表示从 InsertPoint 新建块时弹出的插入面板。

触发入口：

1. 新建 ContentBlock
2. 新建 AtomicSection

职责：

- 作为 SectionPage 上方的最上层 overlay 显示。
- 打开时让背后的整个 SectionPage 模糊。
- 根据 targetType 显示新建 ContentBlock 或新建 AtomicSection 的 Mock 表单。
- 显示当前插入位置上下文。
- 通过事件把用户填写的 Mock 数据交给父级。
- 提供取消和确认新建入口。

字段：

当 targetType = ContentBlock：

- 名称
- 类型：知识点 / 例题 / 变式题 / 练习题 / 变式题组 / 练习题组
- 难度：基础 / 中档 / 提高 / 压轴

当 targetType = AtomicSection：

- 名称
- 难度：基础 / 中档 / 提高 / 压轴
- 备注，可选

边界：

- 字段只表示 Mock UI 字段，不代表后端 DTO 已经固定。
- 不调用 API。
- 不真实创建 ContentBlock。
- 不真实创建 AtomicSection。
- 不修改 Section 数据。
- 不打开 Word。
- 不搜索已有块。
- 不处理 BlockSearchPicker。

ComponentLabPage 验收：

- ContentBlock 新建面板。
- AtomicSection 新建面板。
- 空名称状态。
- 长名称状态。
- 禁用状态。
- 提交后 Mock 反馈。
- 取消关闭状态。
- 背后 SectionPage 模糊效果。
## ��ǰ����Լ����SectionTreeContextMenu

SectionTreeContextMenu ��ʾ SectionTree �ڵ��ϵ��Ҽ������Ĳ˵���

ְ��

- ���������Ĭ���Ҽ��˵���
- ��ʾ��ǰ�Ҽ�Ŀ��ڵ�Ĳ˵�������
- ͨ���¼��Ѳ˵�������������������
- ʹ�� SectionTree ����ʱ context target ���������޸� selectedNodeId��
- ֧�� Escape �͵���ⲿ�رա�

�˵��

1. �½� ContentBlock
2. �½� AtomicSection
3. �������п�
4. �Ƴ�

�߽磺

- �Ҽ��ڵ�ʱֻ�����ýڵ㣬��Ĭ��ѡ�иýڵ㡣
- �Ҽ���Ӧͬ���Ҳ� Inspector��
- �Ҽ���Ӧͬ�� Workspace ѡ��̬��
- ���ơ����ơ��������������������ڸò˵��С�
- ��������� API��
- ������޸� Section ���ݡ�
- ��������� SectionPage ҳ��״̬��

ComponentLabPage ���գ�

- ����һ�� SectionTree �����������ԡ�
- ����ڵ�ʱ���� selectedNodeId��
- �Ҽ��ڵ�ʱֻ���� context target��
- �Ҽ��˵�����ʱ�����ԭ���˵������֡�
- ѡ��˵����ֻ��ʾ Mock �������������ݡ�

## ��ǰ����Լ����Server-confirmed Update

CMS V2 ǰ���漰�־û��Ľ���ͳһ���� server-confirmed update ģʽ��

ְ��߽磺

- չʾ�����Ȼ������ API��
- ҵ�������Ȼֻͨ�� emits ��¶�û���ͼ��
- ҳ�桢ҵ�������� composable ������� `/api/cms-v2`��
- ǰ��ҳ��ֻ���ں�˳ɹ�����ȷ�����ݺ󣬲��������¶�Ӧҵ��������ͼ��

��ֹ��

- ������ optimistic update��
- �������ȱ����޸� Section �ṹ����ʧ�ܻع���
- �������Ѷ���ṹ�޸��ȶ���ǰ�ˣ����ͨ��������ṹ��ͳһ�ύ��
- ����������ڲ�˽��ά��һ�ݻ�����״̬�ֲ��ҵ�����ݸ�����

������

- ά���� UI ״̬������ selectedNodeId��expandedNodeIds��context target��overlay open��loading��error��
- �����ύǰά����ʱ���롣
- API �����ڼ���ʾ loading ״̬��
- API ʧ��ʱ��ʾ������ʾ��������ԭ�к��ȷ�Ϲ������ݡ�
- API �ɹ���ʹ�÷��������滻����ҵ�����ݣ������Ӧ�������¾ۺ����ݣ���ɹ������¶�ȡ��
���䣺SectionTree �Ҽ�Ŀ���������ʹ�þ߱�ҵ����� theme token��

��ǰ token ������

- section-tree-context-target
- section-tree-context-target-foreground
- section-tree-context-target-ring

����

- ��ʹ�� primary��accent �����������ֱ�ӱ���ҵ��״̬��
- ��д��������ɫֵ��
- ��������Ҫ�����Ӿ���ɫ��ֻ�޸� token ӳ�䣬��������и�һ������ɫ��

## ��ǰ����Լ����BasicTreeNodeView �� TeachingTopicTree

### BasicTreeNodeView

BasicTreeNodeView ��ʾ���ڵ��һ��ͨ���Ӿ��ṹ��

ְ��

- ��ʾ�ڵ������⡣
- ��ʾ��ѡ���������߱�ǡ�
- ��ʾ�Ҳ����� meta ��Ϣ��
- ����������ضϺͻ��������ȶ��ԡ�

�߽磺

- ������ Section��TeachingTopic��ContentBlock ��ҵ�����塣
- ������չ�� / �۵���
- ������ѡ��̬���Ҽ�Ŀ��̬�� hover ������
- ������ API��
- ����ȡ Pinia��

ʹ�ù���

- SectionTreeNode ����ͨ�� BasicTreeNodeView ��Ⱦ�ڵ����ݡ�
- TeachingTopicTreeNode ����ͨ�� BasicTreeNodeView ��Ⱦ�ڵ����ݡ�
- ������Ϊ SectionTree �� TeachingTopicTree �ֱ������׽ڵ�����ʽ��

### TeachingTopicTree

TeachingTopicTree ��ʾ��ѧ���⵼������

ְ��

- չʾ TeachingTopic �㼶��
- ����չ�� / �۵���ѧ�����֧��
- ����ѡ��һ�� TeachingTopic����ͨ�� selectTopic �¼���������������
- ��ʾ�����ֶΣ����� Section ������Handout �������鵵״̬��
- ���� BasicTree ������Ϊ�� BasicTreeNodeView �Ľڵ��Ӿ��ṹ��

�߽磺

- ��չʾ Section �ڲ��ṹ��
- ��չʾ SectionItem��ContentBlock���汾�����ɼ�¼��
- ����תҳ�档
- ������ API��
- ������ʵ��ҳ�棬�������� ComponentLab ���� Mock Data ���ա�

ComponentLabPage ���գ�

- ����ֻ�� TeachingTopicTree ����������ݡ�
- ����ڵ���Ҳ���ʾ��ǰѡ�е� TeachingTopic Mock ��Ϣ��
- չ�� / �۵���ѡ��̬������̬��������������� BasicTree ��Ϊ��

### TeachingTopicTreeContextMenu

TeachingTopicTreeContextMenu ��ʾ TeachingTopicTree �ڵ��ϵ��Ҽ������Ĳ˵���

ְ��

- ���������Ĭ���Ҽ��˵���
- ��ʾ��ǰ�Ҽ�Ŀ�� TeachingTopic �Ĳ˵�������
- ͨ���¼��Ѳ˵�������������������
- ʹ�� BasicTree �� context target ����������
- ֧�� Escape �͵���ⲿ�رա�

�˵��

1. �����ӽڵ�
2. ���������ڵ�
3. ɾ��

�߽磺

- �Ҽ��ڵ�ʱֻ�����ýڵ㣬��Ĭ��ѡ�иýڵ㡣
- �Ҽ����ı䵱ǰѡ�е� TeachingTopic��
- ����ֻ�� ComponentLabPage ��ʹ�� Mock Data ���ա�
- ���ֲ���ʵ���� TeachingTopic��
- ���ֲ���ʵɾ�� TeachingTopic��
- ���ֲ����� API��
- ���ֲ�����ʵ��ҳ�档

ComponentLabPage ���գ�

- ����һ�� TeachingTopicTree �����������ԡ�
- ����ڵ�ʱ���� selectedTopicId��
- �Ҽ��ڵ�ʱֻ���� context target��
- �Ҽ��˵�����ʱ�����ԭ���˵������֡�
- ѡ��˵����ֻ��ʾ Mock ���������������ݡ�
