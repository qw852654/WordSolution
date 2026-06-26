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
  tags?: TagModel[]
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

export type TagStatus = 'Active' | 'Archived'

export type TagBindingTargetType = 'ContentBlock' | 'AtomicSection' | 'Section'

export type TagColorToken =
  | 'tag-gray'
  | 'tag-orange'
  | 'tag-yellow'
  | 'tag-green'
  | 'tag-blue'
  | 'tag-purple'
  | 'tag-pink'
  | 'tag-red'

export interface TagModel {
  id: number
  name: string
  color: TagColorToken
  status: TagStatus
  createdTime?: string
  updatedTime?: string
}

export interface TagBindingModel {
  id: number
  tagId: number
  targetType: TagBindingTargetType
  targetId: number
  tag: TagModel
}

export type TagPickerState = 'idle' | 'loading' | 'empty' | 'error'

export type TeachingNoteTargetType =
  | 'ContentBlock'
  | 'Section'
  | 'AtomicSection'
  | 'AtomicSectionPanel'
  | 'AtomicSectionItem'
  | 'SectionItem'

export type NoteType =
  | 'General'
  | 'ClassroomRecord'
  | 'LearningEffect'
  | 'TeachingReflection'
  | 'RevisionSuggestion'
  | 'QuestionReplacement'
  | 'CommonMistake'

export type TeachingNoteEffectLevel = 'Unknown' | 'Good' | 'Normal' | 'Weak' | 'Failed'

export interface TeachingNoteBindingModel {
  id?: number
  teachingNoteId?: number
  targetType: TeachingNoteTargetType
  targetId: number
  createdTime?: string
}

export interface TeachingNoteModel {
  id: number
  noteType: NoteType
  content: string
  effectLevel: TeachingNoteEffectLevel | null
  occurredAt?: string | null
  bindings: TeachingNoteBindingModel[]
  createdTime: string
  updatedTime: string
}

export type TeachingNoteListState = 'idle' | 'loading' | 'empty' | 'error'

export interface TeachingNoteEditorValue {
  noteType: NoteType
  content: string
  effectLevel: TeachingNoteEffectLevel | null
  occurredAt?: string | null
  bindings: TeachingNoteBindingModel[]
}

export interface TeachingNoteSearchQuery {
  keyword?: string
  noteType?: NoteType
  effectLevel?: TeachingNoteEffectLevel
  targetType?: TeachingNoteTargetType
  targetId?: number
  occurredFrom?: string
  occurredTo?: string
}

export interface CreateTeachingNoteRequestModel {
  noteType: NoteType
  content: string
  effectLevel?: TeachingNoteEffectLevel | null
  occurredAt?: string | null
  bindings: TeachingNoteBindingModel[]
}

export interface UpdateTeachingNoteRequestModel {
  noteType?: NoteType
  content?: string
  effectLevel?: TeachingNoteEffectLevel | null
  occurredAt?: string | null
  bindings?: TeachingNoteBindingModel[]
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

export type QuestionImportTarget = 'SectionTopLevel' | 'AtomicSectionPanel'

export type QuestionImportContext =
  | {
      target: 'SectionTopLevel'
      sectionId: number
      sectionTitle: string
      afterSectionItemId?: number | null
      defaultDifficulty?: string
    }
  | {
      target: 'AtomicSectionPanel'
      sectionId: number
      sectionTitle: string
      atomicSectionId: number
      atomicSectionTitle: string
      atomicSectionPanelId: number
      atomicSectionPanelTitle: string
      teachingRole: AtomicSectionTeachingRole
      difficulty: string
    }

export interface QuestionImportCandidateSelectionPayload {
  candidateId: string
  selected: boolean
  title: string
}

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
  | 'AtomicSectionPanel'
  | 'AtomicSectionUnassigned'
  | 'CompositeBlock'
  | 'ContentBlock'

export type AtomicSectionStatusValue = 'Draft' | 'Active' | 'Archived'

export interface SectionTreeNodeModel {
  id: string
  title: string
  kind: SectionTreeNodeKind
  typeLabel: string
  sectionId?: number
  sectionItemId?: number
  contentBlockId?: number
  teachingTopicTitle?: string
  sectionVariantId?: number
  atomicSectionId?: number
  atomicSectionPanelId?: number
  atomicSectionItemId?: number
  teachingRole?: AtomicSectionTeachingRole
  difficulty?: string
  difficultyValue?: string
  status?: string
  targetStatus?: string
  targetStatusValue?: AtomicSectionStatusValue
  hasEmptyPanel?: boolean
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
  | 'RenameSection'
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

export type ContentBlockPartParseStatus =
  | 'NotApplicable'
  | 'Parsed'
  | 'ParsedWithWarnings'
  | 'Failed'

export type ContentBlockPartType = 'Stem' | 'Answer' | 'Analysis' | 'Hint' | 'Other'

export interface ContentBlockDisplayPartModel {
  id: string
  partType: ContentBlockPartType
  sortOrder: number
  plainText?: string | null
  warningMessage?: string | null
}

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
  partParseStatus?: ContentBlockPartParseStatus
  partParseMessage?: string | null
  parts?: ContentBlockDisplayPartModel[]
  tags?: TagModel[]
  selected?: boolean
  disabled?: boolean
}

export type StructuredBlockKind = 'AtomicSection' | 'CompositeBlock'

export type AtomicSectionTeachingRole =
  | 'Unclassified'
  | 'Knowledge'
  | 'Example'
  | 'Variant'
  | 'Practice'
  | 'Homework'
  | 'PreClassQuiz'

export type StructuredBlockChildModel =
  | {
      kind: 'ContentBlock'
      id: string
      nodeId: string
      sortOrder?: number
      atomicSectionId?: number
      atomicSectionItemId?: number
      atomicSectionPanelId?: number | null
      teachingRole?: AtomicSectionTeachingRole
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
      atomicSectionPanelId?: number | null
      teachingRole?: AtomicSectionTeachingRole
      parentBlockId?: number
      relationId?: number
      contentBlockId?: number
      selected?: boolean
      disabled?: boolean
      block: StructuredBlockModel
    }

export interface AtomicSectionPanelModel {
  id: string
  panelId: number
  atomicSectionId: number
  title: string
  teachingRole: AtomicSectionTeachingRole
  difficulty: string
  difficultyValue?: string
  sortOrder: number
  children: StructuredBlockChildModel[]
  expanded?: boolean
  selected?: boolean
  disabled?: boolean
}

export interface AtomicSectionPanelActionPayload {
  nodeId: string
  atomicSectionId: number
  atomicSectionPanelId: number
  title: string
  teachingRole: AtomicSectionTeachingRole
  difficulty: string
  difficultyValue?: string
}

export interface AtomicSectionPanelCreatePayload {
  nodeId: string
  atomicSectionId: number
  title: string
  beforeAtomicSectionPanelId?: number | null
  afterAtomicSectionPanelId?: number | null
}

export interface AtomicSectionPanelCreateOverlayModel {
  nodeId: string
  atomicSectionId: number
  atomicSectionTitle: string
  defaultTitle: string
  beforeAtomicSectionPanelId?: number | null
  afterAtomicSectionPanelId?: number | null
  disabled?: boolean
}

export interface AtomicSectionPanelCreateSubmitPayload {
  nodeId: string
  atomicSectionId: number
  title: string
  teachingRole: AtomicSectionTeachingRole
  difficulty: string
  beforeAtomicSectionPanelId?: number | null
  afterAtomicSectionPanelId?: number | null
}

export interface AtomicSectionPanelMovePayload extends AtomicSectionPanelActionPayload {
  direction: 'Up' | 'Down'
}

export type SectionDifficultyEditableNodeKind =
  | 'ContentBlock'
  | 'CompositeBlock'
  | 'AtomicSection'
  | 'AtomicSectionPanel'

export interface SectionDifficultyChangePayload {
  nodeId: string
  kind: SectionDifficultyEditableNodeKind
  difficulty: string
  atomicSectionId?: number
  atomicSectionPanelId?: number
  title?: string
  teachingRole?: AtomicSectionTeachingRole
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
  panels?: AtomicSectionPanelModel[]
  unassignedChildren?: StructuredBlockChildModel[]
  hasEmptyPanel?: boolean
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
  atomicSectionPanelId?: number | null
  teachingRole?: AtomicSectionTeachingRole
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

export type InsertActionType =
  | 'CreateContentBlock'
  | 'CreateAtomicSection'
  | 'CreateAtomicSectionPanel'
  | 'SearchExistingBlock'

export type InsertParentType = 'Section' | 'AtomicSection' | 'AtomicSectionPanelList' | 'CompositeBlock'

export interface InsertPointPlacementModel {
  parentType: InsertParentType
  parentId: number
  beforeItemId?: number
  afterItemId?: number
  beforeSortOrder?: number
  afterSortOrder?: number
  atomicSectionPanelId?: number | null
  teachingRole?: AtomicSectionTeachingRole
  atomicSectionPanelDifficulty?: string | null
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

export type InsertCreateDifficulty = '未设置' | '基础' | '中档' | '提高' | '压轴'

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
  atomicSectionPanelId?: number | null
  atomicSectionTeachingRole?: AtomicSectionTeachingRole
  defaultContentBlockType?: InsertCreateContentBlockType
  defaultDifficulty?: InsertCreateDifficulty
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
  atomicSectionPanelId?: number | null
  atomicSectionTeachingRole?: AtomicSectionTeachingRole
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

export type WordGenerationIssueSeverity = 'SilentSkip' | 'WarningSkip' | 'Blocking'

export interface WordGenerationIssue {
  code: string
  message: string
  severity: WordGenerationIssueSeverity
  outputFormId?: number | null
  contentBlockId?: number | null
  contentBlockVersionId?: number | null
  outputTemplateId?: number | null
  requiredStyleName?: string | null
  occurrenceRole?: string | null
}

export interface WordGenerationValidationResult {
  isValid: boolean
  issues: WordGenerationIssue[]
}
