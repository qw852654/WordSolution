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

export type SectionVariantCreateType =
  | 'Lecture'
  | 'Exercise'
  | 'Homework'
  | 'Review'
  | 'ExamTraining'
  | 'Custom'

export type SectionVariantCreateDifficulty = 'Basic' | 'Medium' | 'Advanced' | 'Top'

export type SectionVariantPreviewState = 'idle' | 'loading' | 'ready' | 'error'

export interface SectionVariantCreateMetadata {
  sectionId: number
  title: string
  description?: string
  type: SectionVariantCreateType
  difficulty: SectionVariantCreateDifficulty
}

export interface SectionVariantSelectionCandidateModel {
  sectionItemId: number
  targetType: 'ContentBlock' | 'AtomicSection'
  title: string
  displayType: string
  resolvedDifficulty: 'Unset' | 'Basic' | 'Medium' | 'Advanced' | 'Top'
  defaultSelected: boolean
  selected: boolean
  selectable: boolean
  unavailableReason?: string
}

export type WorkspaceItemSelectionState = 'none' | 'selectable' | 'selected' | 'unavailable'

export type SectionVariantCreateSubmitPayload = SectionVariantCreateMetadata & {
  selectedSectionItemIds: number[]
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

export type SectionTreeNodeKind =
  | 'Section'
  | 'SectionVariant'
  | 'AtomicSection'
  | 'CompositeBlock'
  | 'ContentBlock'

export interface SectionTreeNodeModel {
  id: string
  title: string
  kind: SectionTreeNodeKind
  typeLabel: string
  sectionId?: number
  teachingTopicTitle?: string
  sectionVariantId?: number
  difficulty?: string
  status?: string
  targetStatus?: string
  hasWordDocument?: boolean
  previewState?: HtmlPreviewState
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
  | 'CreateSectionVariant'
  | 'CreateContentBlock'
  | 'CreateAtomicSection'
  | 'SearchExistingBlock'
  | 'DeleteSectionVariant'
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
      sortOrder?: number
      atomicSectionId?: number
      atomicSectionItemId?: number
      parentBlockId?: number
      relationId?: number
      contentBlockId?: number
      selected?: boolean
      disabled?: boolean
      block: ContentBlockDisplayModel
    }
  | {
      kind: 'CompositeBlock'
      id: string
      nodeId: string
      sortOrder?: number
      atomicSectionId?: number
      atomicSectionItemId?: number
      parentBlockId?: number
      relationId?: number
      contentBlockId?: number
      selected?: boolean
      disabled?: boolean
      block: StructuredBlockModel
    }

export interface StructuredBlockModel {
  id: string
  title: string
  blockKind: StructuredBlockKind
  typeLabel?: string
  atomicSectionId?: number
  contentBlockId?: number
  selfContent?: ContentBlockDisplayModel
  status: string
  difficulty: string
  summary: string
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
  placement?: InsertPointPlacementModel
  allowedActions?: InsertActionType[]
  disabled?: boolean
}

export type InsertActionType = 'CreateContentBlock' | 'CreateAtomicSection' | 'SearchExistingBlock'

export type InsertParentType = 'Section' | 'AtomicSection' | 'CompositeBlock'

export interface InsertPointPlacementModel {
  parentType: InsertParentType
  parentId: number
  beforeItemId?: number
  afterItemId?: number
  beforeSortOrder?: number
  afterSortOrder?: number
}

export interface InsertRequestModel {
  insertPointId: string
  actionType: InsertActionType
  placement?: InsertPointPlacementModel
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
  placement?: InsertPointPlacementModel
  insertMode?: 'SectionItem' | 'AtomicSectionChild' | 'CompositeBlockChild' | 'WrapAsAtomicSection'
  atomicSectionId?: number
  atomicSectionTitle?: string
  compositeBlockId?: number
  compositeBlockTitle?: string
  wrapSectionItemIds?: number[]
  disabled?: boolean
}

export interface InsertCreateSubmitPayload {
  insertPointId: string
  targetType: InsertCreateTargetType
  sectionId: number
  placement?: InsertPointPlacementModel
  insertMode?: 'SectionItem' | 'AtomicSectionChild' | 'CompositeBlockChild' | 'WrapAsAtomicSection'
  atomicSectionId?: number
  atomicSectionTitle?: string
  compositeBlockId?: number
  compositeBlockTitle?: string
  wrapSectionItemIds?: number[]
  title: string
  contentBlockType?: InsertCreateContentBlockType
  difficulty: InsertCreateDifficulty
  note?: string
}

export type HandoutTreeNodeKind =
  | 'HandoutVersion'
  | 'HandoutVersionItem'
  | 'SectionVariant'
  | 'AtomicSection'
  | 'ContentBlock'
  | 'Derived'

export interface HandoutTreeNodeModel {
  id: string
  title: string
  kind: HandoutTreeNodeKind
  metaItems?: string[]
  handoutVersionId?: number
  handoutVersionItemId?: number
  targetType?: 'SectionVariant' | 'AtomicSection' | 'ContentBlock'
  targetId?: number
  status?: string
  readOnly?: boolean
  expanded?: boolean
  disabled?: boolean
  children?: HandoutTreeNodeModel[]
}

export interface HandoutTreeContextMenuPayload {
  node: HandoutTreeNodeModel
  x: number
  y: number
}

export type HandoutTreeContextActionType =
  | 'AddSectionVariantsToEnd'
  | 'AddAtomicSectionToEnd'
  | 'AddContentBlockToEnd'
  | 'AddSectionVariantsAfter'
  | 'AddAtomicSectionAfter'
  | 'AddContentBlockAfter'

export interface HandoutTreeContextMenuModel {
  node: HandoutTreeNodeModel
  position: {
    x: number
    y: number
  }
}

export interface HandoutTreeContextMenuActionPayload {
  nodeId: string
  actionType: HandoutTreeContextActionType
}

export interface HandoutTargetPickerCandidateModel {
  id: number
  title: string
  metaItems?: string[]
  disabled?: boolean
}

export type HandoutOverviewNodeKind = 'Handout' | 'HandoutVersion'

export interface HandoutOverviewNodeModel {
  id: string
  title: string
  kind: HandoutOverviewNodeKind
  handoutId?: number
  handoutVersionId?: number
  status?: string
  expanded?: boolean
  children?: HandoutOverviewNodeModel[]
}

export type HandoutWorkspaceItemKind = 'SectionVariant' | 'AtomicSection' | 'ContentBlock'

export interface HandoutWorkspaceChildModel {
  id: string
  title: string
  kind: 'AtomicSection' | 'ContentBlock'
  typeLabel: string
  sourceLabel?: string
  readOnly?: boolean
  selected?: boolean
  children?: HandoutWorkspaceChildModel[]
}

export interface HandoutWorkspaceItemModel {
  id: string
  nodeId: string
  handoutVersionItemId: number
  kind: HandoutWorkspaceItemKind
  title: string
  titleOverride?: string | null
  note?: string | null
  targetType: 'SectionVariant' | 'AtomicSection' | 'ContentBlock'
  targetId: number
  sourceLabel: string
  status: string
  sortOrder: number
  selected?: boolean
  children?: HandoutWorkspaceChildModel[]
}

export interface HandoutInspectorFieldModel {
  label: string
  value: string
}

export interface HandoutInspectorModel {
  nodeId: string
  title: string
  kind: HandoutTreeNodeKind | HandoutWorkspaceItemKind
  description?: string
  fields: HandoutInspectorFieldModel[]
  editableOccurrence?: boolean
}

export interface OutputFormCardModel {
  id: number
  title: string
  audience: string
  outputFormat: string
  visibilityMode: string
  templateTitle: string
  status: string
}

export interface GeneratedFileRowModel {
  id: number
  fileName: string
  generatedTime: string
  outputFormTitle: string
  manifestSummary: string
}
