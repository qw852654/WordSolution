# SectionPage UI Development Plan

说明：

- 本文档描述的是阶段 2 之后的 SectionPage 开发路径。
- 当前全局阶段 1 只做文档清理，不执行本计划中的任何工程或页面实现。
- Section 与 SectionVariant 的关系固定为：Section 是上帝小节 / 完整知识池 / 完整教学结构，SectionVariant 是从 Section 派生出的教学用途方案。
- SectionWorkspace 中统一使用 `SectionItemView` 表示 SectionItem 的可视化表现，不再使用 `SectionItemCard` 命名。
- ComponentLabPage 是当前开发轮次的验收入口，不是永久组件展览馆；每一轮只保留本轮需要验收的组件。
- ComponentLabPage 必须作为独立验收页面使用，不应包在带主导航或左侧导航的 AppShell 中。
- SectionPage 后续每轮只完成计划中的最小开发目标，不顺手扩展后续组件、后续交互或真实 API 能力。
- 每轮新增组件前，必须先确认组件职责、边界、输入数据、事件和 ComponentLab 验收场景。

## Phase 1: V2 前端基础结构扫描与缺口确认

前提：

- V1 前端已经废弃。
- 新前端必须作为独立 V2 工程重建。
- 新前端只能对接 `/api/cms-v2`。

阶段目标：

- 确认独立 V2 前端工程目录是否存在
- 确认 V2 前端是否已具备 Vue/Vite/Tailwind/Pinia/I18n/Lab 基础
- 把缺口写清楚，避免后续在错误底座上开工

前置依赖：

- `docs/ui/ui-architecture.md`
- `docs/ui/codex-workflow.md`

新增文件：

- 无

修改文件：

- `docs/ui/section-page-architecture-plan.md`
- `docs/ui/section-page-development-plan.md`
- `docs/ui/section-page-open-questions.md`

Mock Data：

- 无

Lab 验收方式：

- 不适用

页面验收方式：

- 不适用

禁止事项：

- 不在本阶段直接创建完整前端工程
- 不开始写 Vue 页面

## Phase 2: ComponentLabPage / mock 基础

阶段目标：

- 创建 V2 前端基础目录
- 建立 `/lab`
- 建立 `src/mocks/`
- 建立最小 `src/locales/en.ts`

前置依赖：

- Phase 1 完成

新增文件：

- `frontend-v2/index.html`
- `frontend-v2/package.json`
- `frontend-v2/vite.config.ts`
- `frontend-v2/tailwind.config.ts`
- `frontend-v2/src/app/router.ts`
- `frontend-v2/src/app/pinia.ts`
- `frontend-v2/src/app/i18n.ts`
- `frontend-v2/src/pages/ComponentLabPage.vue`
- `frontend-v2/src/mocks/*`
- `frontend-v2/src/labs/*`
- `frontend-v2/src/locales/en.ts`

修改文件：

- 无

Mock Data：

- `mockContentBlocks`
- `mockSectionItemViews`
- `mockTreeNodes`

Lab 验收方式：

- 浏览 `/lab`
- 至少能切换多个组件演示场景

页面验收方式：

- 不接 SectionPage

禁止事项：

- 不接真实 API
- 不开始写 SectionPage 完整逻辑

## Phase 3: FocusTree / useFocusTree 设计验证

阶段目标：

- 实现 `useFocusTree`
- 验证 Root Promotion、Breadcrumb、Expand/Collapse、Keyboard Navigation
- 保证 FocusTree 不理解业务对象

前置依赖：

- Phase 2
- `docs/ui/focus-tree.md`

新增文件：

- `frontend-v2/src/composables/useFocusTree.ts`
- `frontend-v2/src/components/business/FocusTree.vue`
- `frontend-v2/src/labs/focus-tree/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- `mockFocusTreeNodes`

Lab 验收方式：

- 默认树
- Root Promotion 后树
- 不允许提升的节点
- 键盘展开/收起

页面验收方式：

- 不接 SectionPage

禁止事项：

- 不把 Section 业务规则写死进 `useFocusTree`

## Phase 4: InsertPoint

阶段目标：

- 实现插入点视觉和事件边界
- 只表达插入位置，不决定插入目标
- InsertPoint 直接展示三个具体按钮：新建 ContentBlock、新建 AtomicSection、插入已有块
- 不再使用单个“插入”按钮再展开二级面板的交互

前置依赖：

- Phase 2

新增文件：

- `frontend-v2/src/components/presentation/InsertPoint.vue`
- `frontend-v2/src/labs/insert-point/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- `mockInsertContexts`
- 允许新建 ContentBlock 的插入点
- 允许新建 AtomicSection 的插入点
- 允许插入已有块的插入点

Lab 验收方式：

- 默认透明
- Hover 显示
- Keyboard focus 显示
- 禁用状态
- 三个具体按钮在同一插入点内稳定展示

页面验收方式：

- 不接真实业务流

禁止事项：

- 不在 InsertPoint 里写业务规则
- 不调 API

## Phase 4.1: InsertCreateOverlay（下一轮开发）

阶段目标：

- 开发点击“新建 ContentBlock / 新建 AtomicSection”后弹出的插入新块面板。
- 面板打开时位于 SectionPage 最上层。
- 背后的整个 SectionPage 模糊。
- 只做 Mock UI 和事件，不调用 API，不真实创建数据。

前置依赖：

- Phase 4

建议新增文件：

- `frontend-v2/src/components/containers/InsertCreateOverlay.vue`
- `frontend-v2/src/labs/insert-create-overlay/*`

Mock Data：

- ContentBlock 新建：名称、类型、难度
- AtomicSection 新建：名称、难度、备注
- 空名称状态
- 长名称状态
- 禁用状态
- 提交后 Mock 反馈

字段规则：

当 targetType = ContentBlock：

- 名称
- 类型：知识点 / 例题 / 变式题 / 练习题 / 变式题组 / 练习题组
- 难度：基础 / 中档 / 提高 / 压轴

当 targetType = AtomicSection：

- 名称
- 难度：基础 / 中档 / 提高 / 压轴
- 备注，可选

Lab 验收方式：

- ContentBlock 新建面板
- AtomicSection 新建面板
- 背景模糊 overlay
- 空名称状态
- 长名称状态
- 取消关闭状态
- 提交后只显示 Mock 反馈

页面接入方式：

- 从 InsertPoint 的“新建 ContentBlock”按钮打开。
- 从 InsertPoint 的“新建 AtomicSection”按钮打开。
- 父级记录 insertPointId 和 targetType。
- submit 后只向父级 emit Mock 表单数据。

禁止事项：

- 不在 InsertCreateOverlay 中调用 API。
- 不真实创建 ContentBlock。
- 不真实创建 AtomicSection。
- 不修改 Section 数据。
- 不打开 Word。
- 不处理“插入已有块”搜索。

## Phase 4.2: BlockSearchPicker（后续开发）

阶段目标：

- 开发“插入已有块”使用的块搜索组件
- 搜索范围同时包含 ContentBlock 和 AtomicSection
- 用户可以在同一个搜索结果列表中区分 ContentBlock 与 AtomicSection
- 选择已有块后只向父级返回选中对象，不直接修改 Section 数据

前置依赖：

- Phase 4

建议新增文件：

- `frontend-v2/src/components/containers/BlockSearchPicker.vue`
- `frontend-v2/src/labs/block-search-picker/*`

Mock Data：

- 已有 ContentBlock
- 已有 AtomicSection
- 长标题
- 禁用或不可插入项
- 空搜索结果

Lab 验收方式：

- 搜索 ContentBlock
- 搜索 AtomicSection
- 混合结果列表
- 空结果
- 选择已有块后显示选中态

页面接入方式：

- 从 InsertPoint 的“插入已有块”按钮打开
- 后续接入 SectionPage 时只记录插入上下文和选中块
- 不在本阶段接真实 API

禁止事项：

- 不在 BlockSearchPicker 中修改 Section 数据
- 不把搜索范围拆成两个互斥入口
- 不提前实现真实保存

## Phase 5: StructuredContainer / InlineBorderHeader

阶段目标：

- 建立弱边框结构容器
- 建立边框线上标题和 action slot

前置依赖：

- Phase 2

新增文件：

- `frontend-v2/src/components/presentation/StructuredContainer.vue`
- `frontend-v2/src/components/presentation/InlineBorderHeader.vue`
- `frontend-v2/src/labs/structured-container/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- 简单标题
- 长标题
- 无 action
- 多 action

Lab 验收方式：

- AtomicSection 风格
- CompositeBlock 风格

页面验收方式：

- 不接 SectionPage

禁止事项：

- 不给 `ContentBlockDisplay` 套这个容器

## Phase 6: ContentBlockDisplay

阶段目标：

- 实现工作区里的 `ContentBlockDisplay`
- 支持正文 HTML 预览、难度、状态、版本、引用模式和轻量动作

前置依赖：

- Phase 2
- `StructuredContainer` 非必需

新增文件：

- `frontend-v2/src/components/business/ContentBlockDisplay.vue`
- `frontend-v2/src/composables/useContentBlockWordEditor.ts`
- `frontend-v2/src/labs/content-block-display/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- `mockContentBlocks`
- `mockContentBlockHtmlPreviews`
- `mockEditSessionsByContentBlockId`

Lab 验收方式：

- 默认
- 选中
- LockedVersion
- 无预览
- 长正文
- 不同 difficulty

页面验收方式：

- 不接真实 Word 编辑

禁止事项：

- 不在组件内部散落编辑会话 polling

## Phase 7: AtomicSectionBlock / CompositeBlock

阶段目标：

- 用 `StructuredContainer` 组合出 `AtomicSectionBlock` 和 `CompositeBlock`
- 验证内部微缩进和 slot action

前置依赖：

- Phase 5
- Phase 6

新增文件：

- `frontend-v2/src/components/business/AtomicSectionBlock.vue`
- `frontend-v2/src/components/business/CompositeBlock.vue`
- `frontend-v2/src/labs/structured-blocks/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- `mockAtomicSections`
- `mockAtomicSectionItems`
- `mockCompositeBlockFlow`

Lab 验收方式：

- 空容器
- 多子块
- 长标题
- 选中状态

页面验收方式：

- 不接真实 Section

禁止事项：

- 不允许 AtomicSection inside AtomicSection

## Phase 8: SectionTree / SectionStructurePanel

阶段目标：

- 在 FocusTree 之上实现 SectionTree
- 验证树与业务元数据、Root Promotion 限制和工作区定位联动

前置依赖：

- Phase 3
- Phase 6
- Phase 7

新增文件：

- `frontend-v2/src/components/business/SectionTree.vue`
- `frontend-v2/src/components/containers/SectionStructurePanel.vue`
- `frontend-v2/src/labs/section-tree/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- `mockSectionTreeNodes`

Lab 验收方式：

- Hidden / Docked / Focused
- Promote to root
- Disallowed root promotion
- Breadcrumb

页面验收方式：

- 仅 mock 联动

禁止事项：

- 不把 TeachingTopic、Handout、GeneratedFile 混进一棵树

## Phase 9: SectionWorkspace 文档流

阶段目标：

- 建立统一顺序流
- 不分裂成多个 list
- 插入点、SectionItemView、ContentBlockDisplay、AtomicSectionBlock、CompositeBlock 在同一流里工作

前置依赖：

- Phase 4
- Phase 6
- Phase 7

新增文件：

- `frontend-v2/src/components/business/SectionItemView.vue`
- `frontend-v2/src/components/containers/SectionWorkspace.vue`
- `frontend-v2/src/labs/section-workspace/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- `mockSectionItemViews`

Lab 验收方式：

- ComponentLabPage 只展示本轮 SectionWorkspace / SectionItemView 相关验收内容
- 单块
- 多块
- AtomicSection 中嵌块
- CompositeBlock 中嵌块
- InsertPoint between items

页面验收方式：

- 只做 mock 文档流

禁止事项：

- 不做卡片墙
- 不做分类列表

## Phase 10: SectionInspector

阶段目标：

- 实现选中对象上下文详情面板
- 分离高频属性和低频属性

前置依赖：

- Phase 6
- Phase 7
- Phase 9

新增文件：

- `frontend-v2/src/components/business/SectionInspectorPanel.vue`
- `frontend-v2/src/components/containers/SectionInspector.vue`
- `frontend-v2/src/labs/section-inspector/*`

修改文件：

- `frontend-v2/src/pages/ComponentLabPage.vue`

Mock Data：

- `mockInspectorStates`
- `mockTeachingNotes`

Lab 验收方式：

- 无选中
- ContentBlock 选中
- AtomicSection 选中
- LockedVersion
- TeachingNote 多条

页面验收方式：

- mock only

禁止事项：

- 不把正文编辑逻辑塞进 Inspector

## Phase 11: SectionPage 外壳与联动

阶段目标：

- 组装 `SectionPage`
- 建立树、工作区、Inspector、Toolbar 联动
- 先跑 mock 数据

前置依赖：

- Phase 3-10

新增文件：

- `frontend-v2/src/pages/SectionPage.vue`
- `frontend-v2/src/composables/useSectionPage.ts`
- `frontend-v2/src/types/section-page.ts`

修改文件：

- `frontend-v2/src/app/router.ts`

Mock Data：

- 汇总前述全部 mock

Lab 验收方式：

- 不适用

页面验收方式：

- `/sections/:sectionId` 使用 mock 数据可完整联动

禁止事项：

- 不接真实写入 API

## Phase 12: API mock 替换为真实读取接口

阶段目标：

- 把读取链路改为真实 `/api/cms-v2`
- 继续保留写入为 mock 或占位，除非用户单独确认

前置依赖：

- Phase 11
- 后端读取接口存在

新增文件：

- `frontend-v2/src/apis/*.ts`

修改文件：

- `frontend-v2/src/composables/useSectionPage.ts`
- `frontend-v2/src/composables/useContentBlockWordEditor.ts`

Mock Data：

- 保留作为 fallback 和 Lab 数据

Lab 验收方式：

- 组件仍能独立吃 mock

页面验收方式：

- 真实读取：
  - sections
  - section items
  - section variants
  - atomic sections
  - content blocks
  - html preview

禁止事项：

- 不直接接真实写入 API，除非后续任务明确要求

## 阶段顺序说明

采用以上顺序的理由：

- 当前仓库缺的不是某个单一组件，而是 V2 前端骨架本身。
- `FocusTree`、`InsertPoint`、`StructuredContainer` 是 SectionPage 的公共底层，必须先在 Lab 验稳。
- 文档流和 Inspector 必须在 mock 下先跑通，否则一接 API 就会把“组件边界问题”和“数据问题”混在一起。
- 当前后端写入接口明显不完整，因此第一轮应先以 mock 和真实只读接口为界。
- 以上顺序不把 ComponentLabPage 当作永久组件展览馆；每个阶段只在 ComponentLabPage 中保留当前阶段需要验收的组件。
