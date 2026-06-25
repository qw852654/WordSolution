import {
  mapTagBindingsToTags,
  resolveTagBindingTargetFromSectionNode,
} from '@/utils/tagTargets'
import type { SectionTreeNodeModel, TagBindingModel } from '@/types'

const sectionNode: SectionTreeNodeModel = {
  id: 'section-7',
  title: '能量守恒',
  kind: 'Section',
  typeLabel: 'Section',
  sectionId: 7,
}

const sectionItemContentBlockNode: SectionTreeNodeModel = {
  id: 'section-item-31',
  title: '例题 1',
  kind: 'ContentBlock',
  typeLabel: '题目',
  contentBlockId: 42,
}

const atomicSectionItemContentBlockNode: SectionTreeNodeModel = {
  id: 'atomic-section-item-91',
  title: '变式题',
  kind: 'ContentBlock',
  typeLabel: '题目',
  atomicSectionId: 8,
  atomicSectionItemId: 91,
  contentBlockId: 42,
}

const bindings: TagBindingModel[] = [
  {
    id: 1,
    tagId: 2,
    targetType: 'ContentBlock',
    targetId: 42,
    tag: {
      id: 2,
      name: '机械能守恒',
      color: 'tag-blue',
      status: 'Active',
    },
  },
]

export const tagPhase4Contract = {
  sectionTarget: resolveTagBindingTargetFromSectionNode(sectionNode),
  sectionItemContentBlockTarget: resolveTagBindingTargetFromSectionNode(sectionItemContentBlockNode),
  atomicSectionItemContentBlockTarget:
    resolveTagBindingTargetFromSectionNode(atomicSectionItemContentBlockNode),
  bindingTags: mapTagBindingsToTags(bindings),
}
