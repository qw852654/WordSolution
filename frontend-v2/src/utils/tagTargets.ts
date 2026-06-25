import type {
  SectionTreeNodeModel,
  TagBindingModel,
  TagBindingTargetType,
  TagModel,
} from '@/types'

export interface TagBindingTargetModel {
  targetType: TagBindingTargetType
  targetId: number
  source: 'Direct' | 'OccurrenceContentBlock'
}

export function resolveTagBindingTargetFromSectionNode(
  node?: SectionTreeNodeModel,
): TagBindingTargetModel | undefined {
  if (!node) {
    return undefined
  }

  if (node.kind === 'Section' && typeof node.sectionId === 'number') {
    return {
      targetType: 'Section',
      targetId: node.sectionId,
      source: 'Direct',
    }
  }

  if (node.kind === 'AtomicSection' && typeof node.atomicSectionId === 'number') {
    return {
      targetType: 'AtomicSection',
      targetId: node.atomicSectionId,
      source: 'Direct',
    }
  }

  if (
    (node.kind === 'ContentBlock' || node.kind === 'CompositeBlock') &&
    typeof node.contentBlockId === 'number'
  ) {
    return {
      targetType: 'ContentBlock',
      targetId: node.contentBlockId,
      source:
        typeof node.atomicSectionItemId === 'number' || node.id.startsWith('section-item-')
          ? 'OccurrenceContentBlock'
          : 'Direct',
    }
  }

  return undefined
}

export function mapTagBindingsToTags(bindings: TagBindingModel[]): TagModel[] {
  return bindings.map((binding) => binding.tag)
}
