import type {
  ContentBlockCardModel,
  FocusTreeNode,
  SectionVariantCardModel,
} from '@/types'

export const scaffoldChecks = [
  { id: 'router', state: 'ready' },
  { id: 'i18n', state: 'ready' },
  { id: 'mock', state: 'ready' },
  { id: 'api', state: 'ready' },
] as const

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
