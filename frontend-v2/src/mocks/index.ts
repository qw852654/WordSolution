import type { CmsV2TeachingStructureNodeDto } from '@/apis/cmsV2Client'
import { mapTeachingStructureNodesToTreeNodes } from '@/utils/teachingStructureTree'
import type {
  BasicTreeNode,
  ContentBlockCardModel,
  ContentBlockDisplayModel,
  InsertCreateContentBlockType,
  InsertCreateDifficulty,
  InsertCreatePanelModel,
  InsertPointModel,
  SectionItemViewShellModel,
  SectionNodeModel,
  SectionPageShellModel,
  SectionTreeNodeModel,
  SectionVariantCardModel,
  SectionVariantCreateMetadata,
  SectionVariantSelectionCandidateModel,
  StructuredBlockChildModel,
  StructuredBlockModel,
  TeachingTopicTreeNodeModel,
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

export const mockSectionVariantCreateMetadata: SectionVariantCreateMetadata = {
  sectionId: 1,
  title: '基础讲解版',
  description: '用于课堂基础讲解的 SectionVariant。',
  type: 'Lecture',
  difficulty: 'Medium',
}

export const mockSectionVariantSelectionCandidates: SectionVariantSelectionCandidateModel[] = [
  {
    sectionItemId: 101,
    targetType: 'ContentBlock',
    title: '机械能守恒定律',
    displayType: '知识点',
    resolvedDifficulty: 'Basic',
    defaultSelected: true,
    selected: true,
    selectable: true,
  },
  {
    sectionItemId: 102,
    targetType: 'AtomicSection',
    title: '基础例题讲解单元',
    displayType: 'AtomicSection',
    resolvedDifficulty: 'Medium',
    defaultSelected: true,
    selected: true,
    selectable: true,
  },
  {
    sectionItemId: 103,
    targetType: 'ContentBlock',
    title: '圆轨道临界问题例题组',
    displayType: '例题组',
    resolvedDifficulty: 'Advanced',
    defaultSelected: false,
    selected: false,
    selectable: true,
  },
  {
    sectionItemId: 104,
    targetType: 'ContentBlock',
    title: '压轴综合训练：带弹簧、圆轨道和多状态能量方程的长标题候选项',
    displayType: '练习题组',
    resolvedDifficulty: 'Top',
    defaultSelected: false,
    selected: false,
    selectable: true,
  },
  {
    sectionItemId: 105,
    targetType: 'ContentBlock',
    title: '未设置难度的补充说明',
    displayType: '知识点',
    resolvedDifficulty: 'Unset',
    defaultSelected: false,
    selected: false,
    selectable: true,
  },
  {
    sectionItemId: 106,
    targetType: 'ContentBlock',
    title: '已归档旧练习',
    displayType: '练习题',
    resolvedDifficulty: 'Basic',
    defaultSelected: false,
    selected: false,
    selectable: false,
    unavailableReason: 'ContentBlock is archived.',
  },
]

export const mockBasicTreeNodes: BasicTreeNode[] = [
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

export const emptyBasicTreeNodes: BasicTreeNode[] = []

export const mockSectionTreeNodes: SectionTreeNodeModel[] = [
  {
    id: 'section-tree-root',
    title: '机械能守恒',
    kind: 'Section',
    typeLabel: 'Section',
    difficulty: '中档',
    status: '编辑中',
    itemCount: 5,
    expanded: true,
    children: [
      {
        id: 'section-tree-law',
        title: '机械能守恒条件',
        kind: 'ContentBlock',
        typeLabel: '知识点',
        difficulty: '基础',
        status: 'FollowLatest',
      },
      {
        id: 'section-tree-atomic-basics',
        title: '基础讲解 AtomicSection',
        kind: 'AtomicSection',
        typeLabel: '知识点组',
        difficulty: '基础',
        status: '可用',
        itemCount: 2,
        expanded: true,
        children: [
          {
            id: 'section-tree-example-one',
            title: '单物体机械能守恒例题',
            kind: 'ContentBlock',
            typeLabel: '例题',
            difficulty: '基础',
            status: 'LockedVersion',
          },
          {
            id: 'section-tree-example-two',
            title: '圆轨道临界问题例题',
            kind: 'ContentBlock',
            typeLabel: '例题',
            difficulty: '提高',
            status: 'FollowLatest',
          },
        ],
      },
      {
        id: 'section-tree-composite',
        title: '圆轨道模型 CompositeBlock',
        kind: 'CompositeBlock',
        typeLabel: '例题组',
        difficulty: '提高',
        status: '可用',
        itemCount: 3,
        questionCount: 3,
      },
      {
        id: 'section-tree-long-title',
        title: '长标题验收：带弹簧、圆轨道和多状态能量方程的综合讲解节点',
        kind: 'ContentBlock',
        typeLabel: '变式题',
        difficulty: '压轴',
        status: 'FollowLatest',
      },
      {
        id: 'section-tree-disabled',
        title: '已停用旧练习',
        kind: 'ContentBlock',
        typeLabel: '练习',
        difficulty: '基础',
        status: '停用',
        disabled: true,
      },
    ],
  },
]

export const emptySectionTreeNodes: SectionTreeNodeModel[] = []

export const mockTeachingStructureNodes: CmsV2TeachingStructureNodeDto[] = [
  {
    teachingTopic: {
      id: 1,
      parentId: null,
      name: '功能关系',
      description: '用于组织能量与功的章节结构。',
      sortOrder: 10,
      status: 'Active',
      updatedTime: '2026-06-18T08:00:00+08:00',
    },
    section: null,
    sectionVariants: [],
    isEmptyTopic: false,
    canSetDisplayRoot: true,
    canDelete: false,
    children: [
      {
        teachingTopic: {
          id: 2,
          parentId: 1,
          name: '机械能守恒',
          description: '已绑定 Section，并在展开后显示只读 SectionVariant。',
          sortOrder: 10,
          status: 'Active',
          updatedTime: '2026-06-18T08:10:00+08:00',
        },
        section: {
          id: 1,
          teachingTopicId: 2,
          title: '机械能守恒',
          description: '完整知识池 / 上帝小节。',
          type: 'NormalCourse',
          difficulty: 'Medium',
          status: 'Active',
          sortOrder: 10,
          updatedTime: '2026-06-18T08:12:00+08:00',
        },
        sectionVariants: [
          {
            id: 101,
            sectionId: 1,
            title: '基础讲解版',
            description: '面向新课讲解的 SectionVariant。',
            type: 'Lecture',
            difficulty: 'Basic',
            status: 'Draft',
            sortOrder: 10,
            updatedTime: '2026-06-18T08:13:00+08:00',
          },
          {
            id: 102,
            sectionId: 1,
            title: '提高版',
            description: '面向提高训练的 SectionVariant。',
            type: 'Lecture',
            difficulty: 'Advanced',
            status: 'Draft',
            sortOrder: 20,
            updatedTime: '2026-06-18T08:14:00+08:00',
          },
          {
            id: 103,
            sectionId: 1,
            title: '一轮复习版',
            description: '面向复习课的 SectionVariant。',
            type: 'Review',
            difficulty: 'Medium',
            status: 'Draft',
            sortOrder: 30,
            updatedTime: '2026-06-18T08:15:00+08:00',
          },
        ],
        isEmptyTopic: false,
        canSetDisplayRoot: true,
        canDelete: false,
        children: [
          {
            teachingTopic: {
              id: 3,
              parentId: 2,
              name: '竖直圆轨道',
              description: '已绑定 Section，但没有 SectionVariant。',
              sortOrder: 10,
              status: 'Active',
              updatedTime: '2026-06-18T08:20:00+08:00',
            },
            section: {
              id: 2,
              teachingTopicId: 3,
              title: '竖直圆轨道',
              description: null,
              type: 'NormalCourse',
              difficulty: 'Advanced',
              status: 'Draft',
              sortOrder: 10,
              updatedTime: '2026-06-18T08:21:00+08:00',
            },
            sectionVariants: [],
            children: [],
            isEmptyTopic: false,
            canSetDisplayRoot: true,
            canDelete: false,
          },
          {
            teachingTopic: {
              id: 4,
              parentId: 2,
              name: '杆模型',
              description: '空主题：允许删除，不能提升为空根。',
              sortOrder: 20,
              status: 'Active',
              updatedTime: '2026-06-18T08:30:00+08:00',
            },
            section: null,
            sectionVariants: [],
            children: [],
            isEmptyTopic: true,
            canSetDisplayRoot: false,
            canDelete: true,
          },
          {
            teachingTopic: {
              id: 5,
              parentId: 2,
              name: '球模型',
              description: '空主题：允许删除，不能提升为空根。',
              sortOrder: 30,
              status: 'Active',
              updatedTime: '2026-06-18T08:35:00+08:00',
            },
            section: null,
            sectionVariants: [],
            children: [],
            isEmptyTopic: true,
            canSetDisplayRoot: false,
            canDelete: true,
          },
        ],
      },
      {
        teachingTopic: {
          id: 6,
          parentId: 1,
          name: '动能定理与机械能变化的长标题节点验收',
          description: '用于验证完整宽度和长标题显示。',
          sortOrder: 20,
          status: 'Active',
          updatedTime: '2026-06-18T08:40:00+08:00',
        },
        section: null,
        sectionVariants: [],
        children: [],
        isEmptyTopic: true,
        canSetDisplayRoot: false,
        canDelete: true,
      },
      {
        teachingTopic: {
          id: 7,
          parentId: 1,
          name: '旧版能量专题',
          description: '归档节点用于验证弱化显示。',
          sortOrder: 30,
          status: 'Archived',
          updatedTime: '2026-06-18T08:50:00+08:00',
        },
        section: null,
        sectionVariants: [],
        children: [],
        isEmptyTopic: true,
        canSetDisplayRoot: false,
        canDelete: true,
      },
    ],
  },
]

export const mockTeachingTopicTreeNodes: TeachingTopicTreeNodeModel[] =
  mapTeachingStructureNodesToTreeNodes(mockTeachingStructureNodes)

export const emptyTeachingTopicTreeNodes: TeachingTopicTreeNodeModel[] = []

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
    summary: '验收长标题、锁定版本和层级信息在卡片中是否稳定显示。',
  },
  {
    id: 'section-node-disabled',
    title: '已停用的旧练习组',
    targetType: 'ContentBlock',
    status: '停用',
    referenceMode: 'FollowLatest',
    sortOrder: 4,
    level: 1,
    summary: '用于验收禁用状态、按钮不可用和弱化显示。',
    disabled: true,
  },
]

export const mockContentBlockDisplays: ContentBlockDisplayModel[] = [
  {
    id: 'display-energy-law',
    title: '机械能守恒的条件与表达',
    role: '知识点',
    blockType: 'ContentBlock',
    difficulty: '基础',
    status: '可用',
    referenceMode: 'FollowLatest',
    versionLabel: 'v3',
    htmlPreviewState: 'ready',
    htmlPreview:
      '<p>当系统内只有重力或弹力做功，其他力不做功或做功代数和为零时，系统机械能保持不变。</p><p><strong>E_k1 + E_p1 = E_k2 + E_p2</strong></p>',
    selected: true,
  },
  {
    id: 'display-locked-example',
    title: '圆轨道临界问题：从能量关系判断最高点速度条件的长标题示例',
    role: '例题',
    blockType: 'ContentBlock',
    difficulty: '提高',
    status: '可用',
    referenceMode: 'LockedVersion',
    versionLabel: 'v2',
    htmlPreviewState: 'ready',
    htmlPreview:
      '<p>先确定研究对象和零势能面，再使用机械能守恒得到速度关系。最高点恰好通过时，支持力为零。</p><ol><li>列出最低点到最高点的能量方程。</li><li>结合向心力条件判断临界速度。</li></ol>',
  },
  {
    id: 'display-empty-preview',
    title: '无 HTML 预览的课堂练习',
    role: '练习',
    blockType: 'ContentBlock',
    difficulty: '中档',
    status: '待补预览',
    referenceMode: 'FollowLatest',
    versionLabel: 'v1',
    htmlPreviewState: 'empty',
    htmlPreview: null,
  },
  {
    id: 'display-long-preview',
    title: '长正文预览：机械能守恒综合应用',
    role: '方法总结',
    blockType: 'ContentBlock',
    difficulty: '压轴',
    status: '可用',
    referenceMode: 'FollowLatest',
    versionLabel: 'v4',
    htmlPreviewState: 'ready',
    htmlPreview:
      '<p>处理综合题时，先判断是否能使用机械能守恒，再决定是否需要引入动能定理或牛顿第二定律。若过程中存在非保守力做功，应把非保守力做功单独列入能量变化。</p><p>常见检查顺序：系统选择、外力做功、势能零点、初末状态、隐含约束。每一步都要避免把局部对象的能量方程误写成系统能量方程。</p><p>对于含弹簧、圆轨道、绳杆模型的题目，应优先画出关键状态图，再决定能量方程的起点和终点。</p>',
  },
  {
    id: 'display-disabled',
    title: '已停用旧题讲解',
    role: '例题',
    blockType: 'ContentBlock',
    difficulty: '基础',
    status: '停用',
    referenceMode: 'LockedVersion',
    versionLabel: 'v1',
    htmlPreviewState: 'ready',
    htmlPreview: '<p>该内容仅用于验收禁用状态，不应进入真实工作流。</p>',
    disabled: true,
  },
]

function createStructuredContentBlockChild(
  block: ContentBlockDisplayModel,
): StructuredBlockChildModel {
  return {
    kind: 'ContentBlock',
    id: block.id,
    nodeId: block.id,
    selected: block.selected,
    disabled: block.disabled,
    block,
  }
}

export const mockStructuredBlocks: StructuredBlockModel[] = [
  {
    id: 'atomic-energy-basics',
    title: '机械能守恒基础讲解片段',
    blockKind: 'AtomicSection',
    status: '草稿',
    difficulty: '基础',
    summary: '用于组织概念条件和基础例题，不直接承载正文。',
    children: [
      createStructuredContentBlockChild({
        ...mockContentBlockDisplays[1],
        id: 'atomic-example-one',
      }),
      createStructuredContentBlockChild({
        ...mockContentBlockDisplays[3],
        id: 'atomic-example-two',
        title: '圆轨道临界问题例题',
        role: '例题',
        difficulty: '提高',
      }),
    ],
    selected: true,
  },
  {
    id: 'composite-circular-track',
    title: '圆轨道临界例题组：速度、支持力与能量关系的连续模型训练',
    blockKind: 'CompositeBlock',
    status: '可用',
    difficulty: '提高',
    summary: '组合两个 ContentBlock，用于表达同一模型下的连续例题。',
    children: mockContentBlockDisplays.slice(1, 4).map(createStructuredContentBlockChild),
  },
  {
    id: 'atomic-empty',
    title: '空 AtomicSection 验收样例',
    blockKind: 'AtomicSection',
    status: '空状态',
    difficulty: '未设置',
    summary: '用于确认结构容器没有子块时的占位效果。',
    children: [],
  },
]

export const mockInsertPoints: InsertPointModel[] = [
  {
    id: 'insert-before-law',
    label: '在此插入 SectionItem',
  },
  {
    id: 'insert-disabled',
    label: '当前位置不可插入',
    disabled: true,
  },
]

export const mockInsertCreateContentBlockTypes: InsertCreateContentBlockType[] = [
  '知识点',
  '例题',
  '变式题',
  '练习题',
  '变式题组',
  '练习题组',
]

export const mockInsertCreateDifficulties: InsertCreateDifficulty[] = [
  '基础',
  '中档',
  '提高',
  '压轴',
]

export const mockInsertCreatePanels = {
  contentBlock: {
    insertPointId: 'insert-before-atomic-basics',
    targetType: 'ContentBlock',
    insertPositionLabel: '机械能守恒基础讲解片段之前',
    sectionId: 1,
    sectionTitle: '机械能守恒',
  },
  atomicSection: {
    insertPointId: 'insert-after-composite-examples',
    targetType: 'AtomicSection',
    insertPositionLabel: '圆轨道例题组之后',
    sectionId: 1,
    sectionTitle: '机械能守恒',
  },
  disabled: {
    insertPointId: 'insert-disabled',
    targetType: 'ContentBlock',
    insertPositionLabel: '当前结构位置不可插入',
    sectionId: 1,
    sectionTitle: '机械能守恒',
    disabled: true,
  },
} satisfies Record<string, InsertCreatePanelModel>
