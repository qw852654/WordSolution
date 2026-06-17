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

export type SectionNodeTargetType = 'ContentBlock' | 'AtomicSection'

export type SectionReferenceMode = 'FollowLatest' | 'LockedVersion'

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

export interface StructuredBlockModel {
  id: string
  title: string
  blockKind: StructuredBlockKind
  status: string
  difficulty: string
  summary: string
  children: ContentBlockDisplayModel[]
  selected?: boolean
  disabled?: boolean
}

export interface InsertPointModel {
  id: string
  label: string
  disabled?: boolean
}

export type InsertActionType = 'CreateContentBlock' | 'CreateAtomicSection' | 'SearchExistingBlock'

export interface InsertRequestModel {
  insertPointId: string
  actionType: InsertActionType
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
  disabled?: boolean
}

export interface InsertCreateSubmitPayload {
  insertPointId: string
  targetType: InsertCreateTargetType
  title: string
  contentBlockType?: InsertCreateContentBlockType
  difficulty: InsertCreateDifficulty
  note?: string
}
