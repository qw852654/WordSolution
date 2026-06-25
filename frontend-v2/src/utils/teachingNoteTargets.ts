import type {
  SectionTreeNodeModel,
  TeachingNoteTargetType,
} from '@/types'

export type TeachingNoteTargetSource =
  | 'Direct'
  | 'SectionItemOccurrence'
  | 'AtomicSectionItemOccurrence'
  | 'ContentBlockBody'

export interface TeachingNoteTargetModel {
  targetType: TeachingNoteTargetType
  targetId: number
  source: TeachingNoteTargetSource
}

export function resolveTeachingNoteTargetFromSectionNode(
  node?: SectionTreeNodeModel,
): TeachingNoteTargetModel | undefined {
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

  if (node.kind === 'AtomicSectionPanel' && typeof node.atomicSectionPanelId === 'number') {
    return {
      targetType: 'AtomicSectionPanel',
      targetId: node.atomicSectionPanelId,
      source: 'Direct',
    }
  }

  if (
    (node.kind === 'ContentBlock' || node.kind === 'CompositeBlock') &&
    typeof node.atomicSectionItemId === 'number'
  ) {
    return {
      targetType: 'AtomicSectionItem',
      targetId: node.atomicSectionItemId,
      source: 'AtomicSectionItemOccurrence',
    }
  }

  if (
    (node.kind === 'ContentBlock' || node.kind === 'CompositeBlock') &&
    typeof node.sectionItemId === 'number'
  ) {
    return {
      targetType: 'SectionItem',
      targetId: node.sectionItemId,
      source: 'SectionItemOccurrence',
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
      source: 'ContentBlockBody',
    }
  }

  return undefined
}
