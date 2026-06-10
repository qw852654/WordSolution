import type {
  ContentBlockCardModel,
  FocusTreeNode,
  SectionItemViewShellModel,
  SectionNodeModel,
  SectionPageShellModel,
  SectionVariantCardModel,
} from '@/types'

export const scaffoldChecks = [
  { id: 'router', state: 'ready' },
  { id: 'i18n', state: 'ready' },
  { id: 'mock', state: 'ready' },
  { id: 'api', state: 'ready' },
] as const

export const mockSectionPageShells: SectionPageShellModel[] = [
  {
    sectionId: 'demo-section',
    title: '机械能守恒',
    teachingTopicTitle: '功能关系',
    status: '骨架验收',
  },
]

export const mockSectionItemViewShells: SectionItemViewShellModel[] = [
  {
    id: 'section-item-view-content-block',
    placeholderTitleKey: 'sectionPage.workspace.mock.contentBlockPlaceholderTitle',
    placeholderDescriptionKey: 'sectionPage.workspace.mock.contentBlockPlaceholderDescription',
    selected: true,
  },
  {
    id: 'section-item-view-atomic-section',
    placeholderTitleKey: 'sectionPage.workspace.mock.atomicSectionPlaceholderTitle',
    placeholderDescriptionKey: 'sectionPage.workspace.mock.atomicSectionPlaceholderDescription',
    children: [
      {
        id: 'section-item-view-child-knowledge',
        placeholderTitleKey: 'sectionPage.workspace.mock.childKnowledgePlaceholderTitle',
        placeholderDescriptionKey: 'sectionPage.workspace.mock.childKnowledgePlaceholderDescription',
      },
      {
        id: 'section-item-view-child-example',
        placeholderTitleKey: 'sectionPage.workspace.mock.childExamplePlaceholderTitle',
        placeholderDescriptionKey: 'sectionPage.workspace.mock.childExamplePlaceholderDescription',
      },
    ],
  },
  {
    id: 'section-item-view-disabled',
    placeholderTitleKey: 'sectionPage.workspace.mock.disabledPlaceholderTitle',
    placeholderDescriptionKey: 'sectionPage.workspace.mock.disabledPlaceholderDescription',
    disabled: true,
  },
]

export const mockContentBlocks: ContentBlockCardModel[] = [
  {
    id: 'block-energy-law',
    title: 'Mechanical energy conservation law',
    role: 'Knowledge point',
    blockType: 'Atomic block',
    difficulty: 'Foundation',
    status: 'Reusable',
    version: 'v3',
    summary: 'Defines the conserved quantity and the condition for choosing the mechanical system.',
  },
  {
    id: 'block-long-title',
    title:
      'Long-title example for circular-track threshold analysis with multiple constraints and classroom notes',
    role: 'Example',
    blockType: 'Atomic block',
    difficulty: 'Advanced',
    status: 'Reusable',
    version: 'v2',
    summary:
      'Used to verify truncation, wrapping, and stable card dimensions when metadata text is longer than usual.',
  },
  {
    id: 'block-disabled',
    title: 'Deprecated worksheet fragment',
    role: 'Practice',
    blockType: 'Composite block',
    difficulty: 'Intermediate',
    status: 'Disabled',
    version: 'v1',
    summary: 'Kept in mock data to validate disabled state without connecting to the real API.',
    disabled: true,
  },
]

export const mockSectionVariants: SectionVariantCardModel[] = [
  {
    id: 'variant-foundation',
    title: 'Foundation lecture version',
    purpose: 'Classroom explanation',
    difficulty: 'Foundation',
    status: 'Draft',
    itemCount: 8,
  },
  {
    id: 'variant-review',
    title: 'First-round review version with a longer name',
    purpose: 'Review',
    difficulty: 'Intermediate',
    status: 'Ready',
    itemCount: 12,
  },
  {
    id: 'variant-disabled',
    title: 'Archived competition extension',
    purpose: 'Archive',
    difficulty: 'Advanced',
    status: 'Disabled',
    itemCount: 6,
    disabled: true,
  },
]

export const mockFocusTreeNodes: FocusTreeNode[] = [
  {
    id: 'topic-energy',
    label: 'Mechanical energy conservation',
    meta: 'Section',
    expanded: true,
    children: [
      {
        id: 'node-law',
        label: 'Conservation condition',
        meta: 'Knowledge',
      },
      {
        id: 'node-examples',
        label: 'Example group',
        meta: 'Group',
        expanded: true,
        children: [
          {
            id: 'node-example-1',
            label: 'Single body conservation',
            meta: 'Example',
          },
          {
            id: 'node-example-2',
            label: 'Circular track threshold',
            meta: 'Example',
          },
        ],
      },
      {
        id: 'node-practice',
        label: 'Classroom practice set',
        meta: 'Practice',
      },
      {
        id: 'node-archived',
        label: 'Archived extension node',
        meta: 'Disabled',
        disabled: true,
      },
    ],
  },
]

export const emptyFocusTreeNodes: FocusTreeNode[] = []

export const mockSectionNodes: SectionNodeModel[] = [
  {
    id: 'section-node-law',
    title: '机械能守恒定律讲解',
    targetType: 'ContentBlock',
    status: '可用',
    referenceMode: 'FollowLatest',
    sortOrder: 1,
    level: 1,
    summary: '用于说明机械能守恒的系统选择、守恒条件和基本表达式。',
    note: '第一轮复习时建议先讲系统选择，再列方程。',
  },
  {
    id: 'section-node-example-group',
    title: '圆轨道临界问题例题组',
    targetType: 'AtomicSection',
    status: '草稿',
    sortOrder: 2,
    level: 1,
    summary: '由两个 ContentBlock 组成，用于承接从守恒条件到临界速度的分析。',
    note: 'AtomicSection 不使用 ContentBlock 版本锁定。',
  },
  {
    id: 'section-node-locked',
    title: '长标题示例：带弹簧与重力势能变化的综合题讲解路径',
    targetType: 'ContentBlock',
    status: '可用',
    referenceMode: 'LockedVersion',
    lockedVersionLabel: 'v2',
    sortOrder: 3,
    level: 2,
    summary: '验证长标题、锁定版本和层级信息在卡片中是否稳定显示。',
  },
  {
    id: 'section-node-disabled',
    title: '已停用的旧练习组',
    targetType: 'ContentBlock',
    status: '停用',
    referenceMode: 'FollowLatest',
    sortOrder: 4,
    level: 1,
    summary: '用于验证禁用状态、按钮不可用和弱化显示。',
    disabled: true,
  },
]
