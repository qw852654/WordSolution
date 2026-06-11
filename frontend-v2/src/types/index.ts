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

export interface FocusTreeNode {
  id: string
  label: string
  meta?: string
  disabled?: boolean
  expanded?: boolean
  children?: FocusTreeNode[]
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
