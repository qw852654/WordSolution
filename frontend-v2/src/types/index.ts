import type { Component } from 'vue'

export interface AppNavItem {
  id: string
  to: string
  label: string
  description?: string
  icon?: Component
}

export interface ContentBlockCardModel {
  id: string
  title: string
  role: string
  blockType: string
  difficulty: string
  status: string
  version: string
  summary: string
  disabled?: boolean
}

export interface SectionVariantCardModel {
  id: string
  title: string
  purpose: string
  difficulty: string
  status: string
  itemCount: number
  disabled?: boolean
}

export interface BasicTreeNode {
  id: string
  label: string
  meta?: string
  payload?: unknown
  disabled?: boolean
  expanded?: boolean
  children?: BasicTreeNode[]
}

export interface BasicTreeContextMenuPayload {
  node: BasicTreeNode
  x: number
  y: number
}

export type SectionTreeNodeKind = 'Section' | 'AtomicSection' | 'CompositeBlock' | 'ContentBlock'

export interface SectionTreeNodeModel {
  id: string
  title: string
  kind: SectionTreeNodeKind
  typeLabel: string
  difficulty?: string
  status?: string
  itemCount?: number
  questionCount?: number
  disabled?: boolean
  expanded?: boolean
  children?: SectionTreeNodeModel[]
}

export interface SectionTreeContextMenuPayload {
  node: SectionTreeNodeModel
  x: number
  y: number
}

export type SectionTreeContextActionType =
  | 'CreateContentBlock'
  | 'CreateAtomicSection'
  | 'SearchExistingBlock'
  | 'Remove'

export interface SectionTreeContextMenuModel {
  node: SectionTreeNodeModel
  position: {
    x: number
    y: number
  }
}

export interface SectionTreeContextMenuActionPayload {
  nodeId: string
  actionType: SectionTreeContextActionType
}

export type TeachingTopicTreeNodeKind = 'TeachingTopic' | 'SectionVariant'

export interface TeachingTopicTreeNodeModel {
  id: string
  kind?: TeachingTopicTreeNodeKind
  title: string
  teachingTopicId?: number
  sectionId?: number
  sectionVariantId?: number
  sectionTitle?: string
  variantCount?: number
  status?: string
  sectionCount?: number
  handoutCount?: number
  archived?: boolean
  readOnly?: boolean
  isEmptyTopic?: boolean
  canSetDisplayRoot?: boolean
  canDelete?: boolean
  disabled?: boolean
  expanded?: boolean
  children?: TeachingTopicTreeNodeModel[]
}

export interface TeachingTopicTreeContextMenuPayload {
  node: TeachingTopicTreeNodeModel
  x: number
  y: number
}

export type TeachingTopicTreeContextActionType =
  | 'AddChild'
  | 'AddAfter'
  | 'CreateSection'
  | 'Rename'
  | 'Delete'

export interface TeachingTopicTreeContextMenuModel {
  node: TeachingTopicTreeNodeModel
  position: {
    x: number
    y: number
  }
}

export interface TeachingTopicTreeContextMenuActionPayload {
  nodeId: string
  actionType: TeachingTopicTreeContextActionType
}

export type SectionNodeTargetType = 'ContentBlock' | 'AtomicSection'

export type SectionReferenceMode = 'FollowLatest' | 'LockedVersion'

export type SectionItemViewAction =
  | 'InsertBefore'
  | 'InsertAfter'
  | 'InsertChildContentBlock'
  | 'OpenWord'
  | 'MoveUp'
  | 'MoveDown'
  | 'Rename'
  | 'Indent'
  | 'Outdent'
  | 'Remove'

export interface SectionNodeModel {
  id: string
  title: string
  targetType: SectionNodeTargetType
  status: string
  referenceMode?: SectionReferenceMode
  lockedVersionLabel?: string
  sortOrder: number
  level: number
  summary: string
  note?: string
  disabled?: boolean
}

export interface SectionItemViewShellModel {
  id: string
  placeholderTitleKey: string
  placeholderDescriptionKey: string
  selected?: boolean
  disabled?: boolean
  children?: SectionItemViewShellModel[]
}

export interface SectionPageShellModel {
  sectionId: string
  title: string
  teachingTopicTitle: string
  status: string
}

export type HtmlPreviewState = 'ready' | 'loading' | 'empty' | 'error'

export interface ContentBlockDisplayModel {
  id: string
  title: string
  role: string
  blockType: string
  difficulty: string
  status: string
  referenceMode: SectionReferenceMode
  versionLabel: string
  htmlPreviewState: HtmlPreviewState
  htmlPreview?: string | null
  selected?: boolean
  disabled?: boolean
}

export type StructuredBlockKind = 'AtomicSection' | 'CompositeBlock'

export type StructuredBlockChildModel =
  | {
      kind: 'ContentBlock'
      id: string
      nodeId: string
      atomicSectionId?: number
      atomicSectionItemId?: number
      parentBlockId?: number
      relationId?: number
      contentBlockId?: number
      sortOrder?: number
      selected?: boolean
      disabled?: boolean
      block: ContentBlockDisplayModel
    }
  | {
      kind: 'CompositeBlock'
      id: string
      nodeId: string
      atomicSectionId?: number
      atomicSectionItemId?: number
      parentBlockId?: number
      relationId?: number
      contentBlockId?: number
      sortOrder?: number
      selected?: boolean
      disabled?: boolean
      block: StructuredBlockModel
    }

export interface StructuredBlockModel {
  id: string
  title: string
  blockKind: StructuredBlockKind
  atomicSectionId?: number
  contentBlockId?: number
  status: string
  difficulty: string
  summary: string
  selfContent?: ContentBlockDisplayModel
  children: StructuredBlockChildModel[]
  expanded?: boolean
  selected?: boolean
  disabled?: boolean
}

export type SectionWorkspaceFlowItemModel =
  | {
      kind: 'ContentBlock'
      id: string
      nodeId: string
      sectionItemId?: number
      targetId?: number
      sortOrder?: number
      selected?: boolean
      disabled?: boolean
      block: ContentBlockDisplayModel
    }
  | {
      kind: 'AtomicSection' | 'CompositeBlock'
      id: string
      nodeId: string
      sectionItemId?: number
      targetId?: number
      sortOrder?: number
      selected?: boolean
      disabled?: boolean
      block: StructuredBlockModel
    }

export interface ContentBlockRelationActionPayload {
  nodeId: string
  parentBlockId: number
  relationId: number
  contentBlockId: number
  title: string
}

export interface ContentBlockRelationMovePayload extends ContentBlockRelationActionPayload {
  direction: 'Up' | 'Down'
}

export interface AtomicSectionItemActionPayload {
  nodeId: string
  atomicSectionId: number
  atomicSectionItemId: number
  contentBlockId: number
  title: string
}

export interface AtomicSectionItemMovePayload extends AtomicSectionItemActionPayload {
  direction: 'Up' | 'Down'
}

export interface InsertPointModel {
  id: string
  label: string
  disabled?: boolean
  allowedActions?: InsertActionType[]
  context?: InsertPointContext
}

export type InsertActionType = 'CreateContentBlock' | 'CreateAtomicSection' | 'SearchExistingBlock'

export type InsertParentType = 'Section' | 'AtomicSection' | 'CompositeBlock'

export interface InsertPointContext {
  parentType: InsertParentType
  parentId?: number
  parentTitle?: string
  beforeSortOrder?: number
  afterSortOrder?: number
}

export interface InsertRequestModel {
  insertPointId: string
  actionType: InsertActionType
  context?: InsertPointContext
}

export type InsertCreateTargetType = 'ContentBlock' | 'AtomicSection'

export type InsertCreateContentBlockType =
  | '知识点'
  | '例题'
  | '变式题'
  | '练习题'
  | '变式题组'
  | '练习题组'

export type InsertCreateDifficulty = '基础' | '中档' | '提高' | '压轴'

export interface InsertCreatePanelModel {
  insertPointId: string
  targetType: InsertCreateTargetType
  insertPositionLabel: string
  sectionId: number
  sectionTitle: string
  insertMode?: 'SectionItem' | 'AtomicSectionChild' | 'CompositeBlockChild' | 'WrapAsAtomicSection'
  atomicSectionId?: number
  atomicSectionTitle?: string
  compositeBlockId?: number
  compositeBlockTitle?: string
  insertSortOrder?: number
  wrapSectionItemIds?: number[]
  disabled?: boolean
}

export interface InsertCreateSubmitPayload {
  insertPointId: string
  targetType: InsertCreateTargetType
  sectionId: number
  insertMode?: 'SectionItem' | 'AtomicSectionChild' | 'CompositeBlockChild' | 'WrapAsAtomicSection'
  atomicSectionId?: number
  atomicSectionTitle?: string
  compositeBlockId?: number
  compositeBlockTitle?: string
  insertSortOrder?: number
  wrapSectionItemIds?: number[]
  title: string
  contentBlockType?: InsertCreateContentBlockType
  difficulty: InsertCreateDifficulty
  note?: string
}
