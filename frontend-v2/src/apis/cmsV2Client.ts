import type { AtomicSectionStatusValue, WordGenerationValidationResult } from '@/types'

export const CMS_V2_API_BASE = '/api/cms-v2'

export interface CmsV2HealthDto {
  status: string
  bankKey: string
  bankDisplayName: string
  bankKind: 'Test' | 'Production' | string
  bankRootDirectory: string
}

export interface CmsV2TeachingTopicDto {
  id: number
  parentId?: number | null
  name: string
  description?: string | null
  sortOrder: number
  status: string
  updatedTime: string
}

export interface CmsV2SectionDto {
  id: number
  teachingTopicId: number
  title: string
  description?: string | null
  type: string
  difficulty: string
  status: string
  sortOrder: number
  updatedTime: string
}

export interface CmsV2SectionVariantDto {
  id: number
  sectionId: number
  title: string
  description?: string | null
  type: string
  difficulty: string
  status: string
  sortOrder: number
  updatedTime: string
}

export interface CmsV2SectionVariantSelectionTreeSectionDto {
  section: CmsV2SectionDto
  sectionVariants: CmsV2SectionVariantDto[]
}

export interface CmsV2SectionVariantSelectionTreeTopicDto {
  teachingTopic: CmsV2TeachingTopicDto
  sections: CmsV2SectionVariantSelectionTreeSectionDto[]
  children: CmsV2SectionVariantSelectionTreeTopicDto[]
}

export interface CmsV2SectionVariantItemDto {
  id: number
  sectionVariantId: number
  sectionItemId: number
  sortOrder: number
  note?: string | null
  updatedTime: string
}

export interface CmsV2TeachingStructureNodeDto {
  teachingTopic: CmsV2TeachingTopicDto
  section?: CmsV2SectionDto | null
  sectionVariants: CmsV2SectionVariantDto[]
  children: CmsV2TeachingStructureNodeDto[]
  isEmptyTopic: boolean
  canSetDisplayRoot: boolean
  canDelete: boolean
}

export interface CmsV2SectionItemDto {
  id: number
  sectionId: number
  targetType: 'ContentBlock' | 'AtomicSection'
  targetId: number
  referenceMode: 'FollowLatest' | 'LockedVersion'
  lockedContentBlockVersionId?: number | null
  titleOverride?: string | null
  parentItemId?: number | null
  sortOrder: number
  selectionLayer?: string | null
  teachingUseOverride?: string | null
  status: string
  note?: string | null
  updatedTime: string
}

export interface CmsV2AtomicSectionDto {
  id: number
  sectionId: number
  title: string
  description?: string | null
  type: string
  difficulty: string
  status: CmsV2AtomicSectionStatus
  updatedTime: string
}

export type CmsV2AtomicSectionStatus = AtomicSectionStatusValue

export interface CmsV2AtomicSectionItemDto {
  id: number
  atomicSectionId: number
  contentBlockId: number
  atomicSectionPanelId?: number | null
  teachingRole?: CmsV2AtomicSectionTeachingRole
  referenceMode: 'FollowLatest' | 'LockedVersion'
  lockedContentBlockVersionId?: number | null
  titleOverride?: string | null
  sortOrder: number
  note?: string | null
  updatedTime: string
}

export interface CmsV2ContentBlockDto {
  id: number
  sectionId: number
  title: string
  summary?: string | null
  blockType: string
  difficulty: string
  questionType?: string | null
  status: string
  currentVersionId?: number | null
  updatedTime: string
}

export type CmsV2TagStatus = 'Active' | 'Archived'

export type CmsV2TagBindingTargetType = 'ContentBlock' | 'AtomicSection' | 'Section'

export type CmsV2TagColorToken =
  | 'tag-gray'
  | 'tag-orange'
  | 'tag-yellow'
  | 'tag-green'
  | 'tag-blue'
  | 'tag-purple'
  | 'tag-pink'
  | 'tag-red'

export interface CmsV2TagDto {
  id: number
  name: string
  normalizedName?: string
  color: CmsV2TagColorToken
  status: CmsV2TagStatus
  createdTime: string
  updatedTime: string
}

export interface CmsV2TagBindingDto {
  id: number
  tagId: number
  targetType: CmsV2TagBindingTargetType
  targetId: number
  tag: CmsV2TagDto
}

export interface CmsV2CreateTagRequest {
  name: string
  color?: CmsV2TagColorToken
}

export interface CmsV2UpdateTagRequest {
  name?: string
  color?: CmsV2TagColorToken
}

export interface CmsV2SetTargetTagsRequest {
  targetType: CmsV2TagBindingTargetType
  targetId: number
  tagIds: number[]
}

export interface CmsV2ListContentBlocksQuery {
  tagIds?: number[]
}

export type CmsV2TeachingNoteTargetType =
  | 'ContentBlock'
  | 'Section'
  | 'AtomicSection'
  | 'AtomicSectionPanel'
  | 'AtomicSectionItem'
  | 'SectionItem'

export type CmsV2NoteType =
  | 'General'
  | 'ClassroomRecord'
  | 'LearningEffect'
  | 'TeachingReflection'
  | 'RevisionSuggestion'
  | 'QuestionReplacement'
  | 'CommonMistake'

export type CmsV2TeachingNoteEffectLevel = 'Unknown' | 'Good' | 'Normal' | 'Weak' | 'Failed'

export interface CmsV2TeachingNoteBindingDto {
  id: number
  teachingNoteId: number
  targetType: CmsV2TeachingNoteTargetType
  targetId: number
  createdTime: string
}

export interface CmsV2TeachingNoteDto {
  id: number
  noteType: CmsV2NoteType
  content: string
  effectLevel: CmsV2TeachingNoteEffectLevel | null
  occurredAt?: string | null
  bindings: CmsV2TeachingNoteBindingDto[]
  createdTime: string
  updatedTime: string
}

export interface CmsV2TeachingNoteBindingRequest {
  targetType: CmsV2TeachingNoteTargetType
  targetId: number
}

export interface CmsV2SearchTeachingNotesQuery {
  keyword?: string
  noteType?: CmsV2NoteType
  effectLevel?: CmsV2TeachingNoteEffectLevel
  targetType?: CmsV2TeachingNoteTargetType
  targetId?: number
  occurredFrom?: string
  occurredTo?: string
}

export interface CmsV2CreateTeachingNoteRequest {
  noteType: CmsV2NoteType
  content: string
  effectLevel?: CmsV2TeachingNoteEffectLevel | null
  occurredAt?: string | null
  bindings: CmsV2TeachingNoteBindingRequest[]
}

export interface CmsV2UpdateTeachingNoteRequest {
  noteType?: CmsV2NoteType
  content?: string
  effectLevel?: CmsV2TeachingNoteEffectLevel | null
  occurredAt?: string | null
  bindings?: CmsV2TeachingNoteBindingRequest[]
}

export interface CmsV2ContentBlockVersionDto {
  id: number
  contentBlockId: number
  versionNumber: number
  docxPath: string
  htmlPreviewPath?: string | null
  plainText?: string | null
  partParseStatus?: 'NotApplicable' | 'Parsed' | 'ParsedWithWarnings' | 'Failed'
  partParseMessage?: string | null
  isCurrent: boolean
  updatedTime: string
}

export interface CmsV2ContentBlockVersionPartDto {
  id: number
  contentBlockVersionId: number
  partType: 'Stem' | 'Answer' | 'Analysis' | 'Hint' | 'Other'
  sortOrder: number
  plainText?: string | null
  sourceStyleNamesJson: string
  warningMessage?: string | null
}

export type CmsV2ContentBlockPartParseStatus =
  | 'NotApplicable'
  | 'Parsed'
  | 'ParsedWithWarnings'
  | 'Failed'

export interface CmsV2QuestionImportCandidatePartDto {
  partType: 'Stem' | 'Answer' | 'Analysis' | 'Hint' | 'Other'
  sortOrder: number
  plainText: string
  sourceStyleNames: string[]
  warningMessage?: string | null
}

export interface CmsV2QuestionImportCandidateDto {
  candidateId: string
  sortOrder: number
  parseStatus: CmsV2ContentBlockPartParseStatus
  parseMessage?: string | null
  htmlPreview?: string | null
  parts: CmsV2QuestionImportCandidatePartDto[]
}

export type CmsV2QuestionImportSessionStatus =
  | 'Created'
  | 'Opening'
  | 'Editing'
  | 'Parsing'
  | 'ReadyForReview'
  | 'Importing'
  | 'Imported'
  | 'Failed'
  | 'Cancelled'
  | 'Expired'

export type CmsV2Difficulty = 'Unset' | 'Basic' | 'Medium' | 'Advanced' | 'Top'

export interface CmsV2InsertQuestionContext {
  sectionId: number
  atomicSectionId?: number | null
  atomicSectionPanelId?: number | null
  afterAtomicSectionItemId?: number | null
  afterSectionItemId?: number | null
  defaultTeachingRole: CmsV2AtomicSectionTeachingRole
  defaultDifficulty: CmsV2Difficulty
}

export interface CmsV2CreateQuestionImportSessionRequest {
  context: CmsV2InsertQuestionContext
  openWord: boolean
}

export interface CmsV2QuestionImportSessionDto {
  sessionId: string
  context: CmsV2InsertQuestionContext
  status: CmsV2QuestionImportSessionStatus
  message?: string | null
  createdTime: string
  updatedTime: string
  candidates: CmsV2QuestionImportCandidateDto[]
}

export interface CmsV2ConfirmQuestionImportCandidateSelectionDto {
  candidateId: string
  selected: boolean
  title: string
}

export interface CmsV2ConfirmQuestionImportRequest {
  candidates: CmsV2ConfirmQuestionImportCandidateSelectionDto[]
}

export interface CmsV2QuestionImportConfirmResultDto {
  contentBlockIds: number[]
  contentBlockVersionIds: number[]
  sectionItemIds: number[]
  atomicSectionItemIds: number[]
  firstInsertedNodeType?: 'SectionItem' | 'AtomicSectionItem' | string | null
  firstInsertedNodeId?: number | null
}

export interface CmsV2ContentBlockRelationDto {
  id: number
  parentBlockId: number
  childBlockId: number
  referenceMode: 'FollowLatest' | 'LockedVersion'
  lockedContentBlockVersionId?: number | null
  titleOverride?: string | null
  sortOrder: number
  note?: string | null
  updatedTime: string
}

export interface CmsV2CreatedEntityResultDto {
  id: number
}

export interface CmsV2ContentBlockDocumentVersionResultDto {
  contentBlockId: number
  contentBlockVersionId: number
  versionNumber: number
  docxPath: string
  htmlPreviewPath: string
  plainTextPath: string
}

export interface CmsV2CreateContentBlockEditSessionRequest {
  openWord: boolean
}

export type CmsV2ContentBlockEditSessionStatus =
  | 'Created'
  | 'Opening'
  | 'Editing'
  | 'Synced'
  | 'Cancelled'
  | 'Failed'

export type CmsV2ContentBlockEditLaunchMode = 'LocalShell' | 'ExternalUri' | 'Cloud' | 'None'

export interface CmsV2ContentBlockEditSessionDto {
  sessionId: string
  contentBlockId: number
  sourceContentBlockVersionId: number
  status: CmsV2ContentBlockEditSessionStatus
  launchMode: CmsV2ContentBlockEditLaunchMode
  openedByServer: boolean
  message?: string | null
  createdTime: string
  updatedTime: string
}

export interface CmsV2SyncContentBlockEditSessionResultDto {
  sessionId: string
  contentBlockId: number
  changed: boolean
  newContentBlockVersionId?: number | null
  currentVersionNumber?: number | null
  status: 'Synced'
  message?: string | null
}

export interface CmsV2DeleteContentBlockCascadeResultDto {
  contentBlockId: number
  removedSectionItemCount: number
  removedSectionVariantItemCount: number
  removedAtomicSectionItemCount: number
  removedContentBlockRelationCount: number
  removedHandoutVersionItemCount: number
  removedVersionCount: number
  deletedAssetCount: number
}

export interface CmsV2ContentAssetRetainReasonDto {
  contentBlockId: number
  reasonCode: string
  message: string
}

export interface CmsV2ContentAssetDeleteResultDto {
  rootContentBlockId: number
  removedCurrentReference: boolean
  deletedRootAsset: boolean
  removedSectionItemCount: number
  removedSectionVariantItemCount: number
  removedAtomicSectionItemCount: number
  removedContentBlockRelationCount: number
  deletedContentBlockCount: number
  deletedContentBlockVersionCount: number
  deletedFileCount: number
  retainReasons: CmsV2ContentAssetRetainReasonDto[]
}

export interface CmsV2HandoutDto {
  id: number
  title: string
  description?: string | null
  status: string
  updatedTime: string
}

export type CmsV2AtomicSectionTeachingRole =
  | 'Unclassified'
  | 'Knowledge'
  | 'Example'
  | 'Variant'
  | 'Practice'
  | 'Homework'
  | 'PreClassQuiz'

export interface CmsV2AtomicSectionPanelDto {
  id: number
  atomicSectionId: number
  title: string
  teachingRole: CmsV2AtomicSectionTeachingRole
  difficulty: string
  sortOrder: number
  updatedTime: string
}

export interface CmsV2CreateHandoutRequest {
  title: string
  description?: string | null
  status?: string
}

export interface CmsV2UpdateHandoutRequest {
  title: string
  description?: string | null
  status: string
}

export interface CmsV2HandoutVersionDto {
  id: number
  handoutId: number
  title: string
  description?: string | null
  type: string
  status: string
  sortOrder: number
  updatedTime: string
}

export interface CmsV2CreateHandoutVersionRequest {
  title: string
  description?: string | null
  type?: string
  status?: string
  sortOrder?: number
}

export interface CmsV2UpdateHandoutVersionRequest {
  title: string
  description?: string | null
  type: string
  status: string
  sortOrder?: number
}

export interface CmsV2HandoutWorkspaceNodeDto {
  nodeId: string
  nodeKind: string
  sourceId: number
  title: string
  children: CmsV2HandoutWorkspaceNodeDto[]
}

export interface CmsV2HandoutWorkspaceItemDto {
  nodeId: string
  handoutVersionItemId: number
  targetType: 'SectionVariant' | 'AtomicSection' | 'ContentBlock'
  targetId: number
  title: string
  titleOverride?: string | null
  note?: string | null
  sortOrder: number
  children: CmsV2HandoutWorkspaceNodeDto[]
}

export interface CmsV2OutputFormDto {
  id: number
  handoutVersionId: number
  outputTemplateId: number
  title: string
  audience: string
  outputFormat: string
  visibilityMode: string
  status: string
  sortOrder: number
  updatedTime: string
}

export interface CmsV2GeneratedFileDto {
  id: number
  outputFormId: number
  filePath: string
  versionManifestJson: string
  generatedTime: string
}

export interface CmsV2HandoutVersionWorkspaceDto {
  handout: CmsV2HandoutDto
  version: CmsV2HandoutVersionDto
  items: CmsV2HandoutWorkspaceItemDto[]
  outputForms: CmsV2OutputFormDto[]
  generatedFiles: CmsV2GeneratedFileDto[]
}

export interface CmsV2AddHandoutVersionItemRequest {
  targetType: 'SectionVariant' | 'AtomicSection' | 'ContentBlock'
  targetId: number
  sortOrder?: number
  titleOverride?: string | null
  note?: string | null
  afterHandoutVersionItemId?: number | null
}

export interface CmsV2BatchAddSectionVariantsToHandoutVersionRequest {
  sectionVariantIds: number[]
  insertAfterHandoutVersionItemId?: number | null
}

export interface CmsV2BatchAddSectionVariantsToHandoutVersionResultDto {
  createdItemIds: number[]
  skippedExistingVariantIds: number[]
}

export interface CmsV2MoveHandoutVersionItemRequest {
  direction: 'Up' | 'Down'
}

export interface CmsV2UpdateHandoutVersionItemRequest {
  titleOverride?: string | null
  note?: string | null
}

export interface CmsV2GenerateHandoutWordRequest {
  generatedTime?: string | null
}

export interface CmsV2GeneratedHandoutFileResultDto {
  generatedFileId: number
  outputFormId: number
  handoutVersionId: number
  filePath: string
  versionManifestJson: string
}

export interface CmsV2CreateContentBlockWithBlankDocumentRequest {
  sectionId: number
  title: string
  blockType: string
  summary?: string | null
  difficulty: string
  questionType?: string | null
  status: string
}

export interface CmsV2CreateContentBlockRequest {
  sectionId: number
  title: string
  blockType: string
  summary?: string | null
  difficulty: string
  questionType?: string | null
  status: string
}

export interface CmsV2CreateAtomicSectionRequest {
  sectionId: number
  title: string
  description?: string | null
  type: string
  difficulty: string
  status: string
}

export interface CmsV2AddSectionItemRequest {
  targetType: 'ContentBlock' | 'AtomicSection'
  targetId: number
  referenceMode: 'FollowLatest' | 'LockedVersion'
  lockedContentBlockVersionId?: number | null
  sortOrder: number
  titleOverride?: string | null
  parentItemId?: number | null
  selectionLayer?: string | null
  teachingUseOverride?: string | null
  status: string
  note?: string | null
}

export interface CmsV2CreateTeachingTopicChildRequest {
  name: string
  description?: string | null
  status?: string
}

export interface CmsV2CreateTeachingTopicNextSiblingRequest {
  name: string
  description?: string | null
  status?: string
}

export interface CmsV2RenameTeachingTopicRequest {
  name: string
  description?: string | null
}

export interface CmsV2RenameSectionRequest {
  title: string
}

export interface CmsV2CreateSectionForTeachingTopicRequest {
  title?: string | null
  description?: string | null
  type?: string
  difficulty?: string
  status?: string
}

export interface CmsV2PreviewSectionVariantSelectionRequest {
  sectionId: number
  difficulty: 'Basic' | 'Medium' | 'Advanced' | 'Top'
}

export interface CmsV2CreateSectionVariantRequest {
  sectionId: number
  title: string
  description?: string | null
  type: 'Lecture' | 'Exercise' | 'Homework' | 'Review' | 'ExamTraining' | 'Custom'
  difficulty: 'Basic' | 'Medium' | 'Advanced' | 'Top'
  selectedSectionItemIds: number[]
}

export interface CmsV2SectionVariantSelectionCandidateDto {
  sectionItemId: number
  parentItemId?: number | null
  sourceSortOrder: number
  targetType: 'ContentBlock' | 'AtomicSection'
  targetId: number
  resolvedDifficulty: 'Unset' | 'Basic' | 'Medium' | 'Advanced' | 'Top'
  defaultSelected: boolean
  selectable: boolean
  unavailableReason?: string | null
}

export interface CmsV2MoveSectionItemRequest {
  direction: 'Up' | 'Down'
}

export interface CmsV2WrapSectionItemsAsAtomicSectionRequest {
  sectionItemIds: number[]
  title: string
  description?: string | null
  type: string
  difficulty: string
  status: string
}

export interface CmsV2WrapSectionItemsAsAtomicSectionResultDto {
  sectionId: number
  atomicSectionId: number
  sectionItemId: number
  wrappedSectionItemIds: number[]
  atomicSectionItemIds: number[]
}

export interface CmsV2MoveContentBlockRelationRequest {
  direction: 'Up' | 'Down'
}

export interface CmsV2MoveAtomicSectionItemRequest {
  direction: 'Up' | 'Down'
}

export interface CmsV2AddAtomicSectionItemRequest {
  contentBlockId: number
  referenceMode: 'FollowLatest' | 'LockedVersion'
  lockedContentBlockVersionId?: number | null
  sortOrder?: number | null
  titleOverride?: string | null
  note?: string | null
  atomicSectionPanelId?: number | null
  teachingRole?: CmsV2AtomicSectionTeachingRole
  beforeAtomicSectionItemId?: number | null
  afterAtomicSectionItemId?: number | null
}

export interface CmsV2CreateAtomicSectionPanelRequest {
  title: string
  teachingRole: CmsV2AtomicSectionTeachingRole
  difficulty: string
  beforeAtomicSectionPanelId?: number | null
  afterAtomicSectionPanelId?: number | null
}

export interface CmsV2UpdateAtomicSectionPanelRequest {
  title: string
  teachingRole: CmsV2AtomicSectionTeachingRole
  difficulty: string
}

export interface CmsV2MoveAtomicSectionPanelRequest {
  direction: 'Up' | 'Down'
}

export interface CmsV2DeleteAtomicSectionPanelResultDto {
  atomicSectionId: number
  atomicSectionPanelId: number
  removedAtomicSectionItemCount: number
}

export interface CmsV2ChangeAtomicSectionItemClassificationRequest {
  teachingRole: CmsV2AtomicSectionTeachingRole
  difficulty: string
}

export interface CmsV2ChangeDifficultyRequest {
  difficulty: CmsV2Difficulty
}

export interface CmsV2ChangeAtomicSectionStatusRequest {
  status: CmsV2AtomicSectionStatus
}

export interface CmsV2AddContentBlockRelationRequest {
  childBlockId: number
  referenceMode: 'FollowLatest' | 'LockedVersion'
  lockedContentBlockVersionId?: number | null
  sortOrder: number
  titleOverride?: string | null
  note?: string | null
}

export interface CmsV2DownloadedFileResult {
  filename: string
  contentType: string
  size: number
}

export function createCmsV2Url(path = ''): string {
  if (!path) {
    return CMS_V2_API_BASE
  }

  return path.startsWith('/')
    ? `${CMS_V2_API_BASE}${path}`
    : `${CMS_V2_API_BASE}/${path}`
}

export async function cmsV2Fetch(path: string, init?: RequestInit): Promise<Response> {
  return fetch(createCmsV2Url(path), init)
}

async function readErrorMessage(response: Response): Promise<string> {
  const contentType = response.headers.get('content-type') ?? ''

  if (contentType.includes('application/problem+json') || contentType.includes('application/json')) {
    try {
      const payload = (await response.json()) as { title?: string; detail?: string; message?: string }
      return payload.detail || payload.message || payload.title || response.statusText
    } catch {
      return response.statusText
    }
  }

  try {
    return (await response.text()) || response.statusText
  } catch {
    return response.statusText
  }
}

export async function cmsV2FetchJson<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await cmsV2Fetch(path, init)

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }

  return (await response.json()) as T
}

export async function cmsV2PostJson<T>(path: string, value: unknown): Promise<T> {
  return await cmsV2FetchJson<T>(path, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(value),
  })
}

export async function cmsV2PostNoContent(path: string, value: unknown): Promise<void> {
  const response = await cmsV2Fetch(path, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(value),
  })

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }
}

export async function cmsV2PatchNoContent(path: string, value: unknown): Promise<void> {
  const response = await cmsV2Fetch(path, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(value),
  })

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }
}

export async function cmsV2PatchJson<T>(path: string, value: unknown): Promise<T> {
  return await cmsV2FetchJson<T>(path, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(value),
  })
}

export async function cmsV2PutJson<T>(path: string, value: unknown): Promise<T> {
  return await cmsV2FetchJson<T>(path, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(value),
  })
}

export async function cmsV2Delete(path: string): Promise<void> {
  const response = await cmsV2Fetch(path, {
    method: 'DELETE',
  })

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }
}

export async function cmsV2DeleteJson<T>(path: string): Promise<T> {
  return await cmsV2FetchJson<T>(path, {
    method: 'DELETE',
  })
}

export async function cmsV2FetchText(path: string, init?: RequestInit): Promise<string> {
  const response = await cmsV2Fetch(path, init)

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }

  return await response.text()
}

export async function cmsV2PostForm<T>(path: string, formData: FormData): Promise<T> {
  return await cmsV2FetchJson<T>(path, {
    method: 'POST',
    body: formData,
  })
}

function decodeContentDispositionValue(value: string): string {
  const trimmed = value.trim().replace(/^"(.*)"$/, '$1')
  const encodedValue = trimmed.includes("''")
    ? trimmed.slice(trimmed.indexOf("''") + 2)
    : trimmed

  try {
    return decodeURIComponent(encodedValue)
  } catch {
    return encodedValue
  }
}

function getFileNameFromContentDisposition(contentDisposition: string | null): string | undefined {
  if (!contentDisposition) {
    return undefined
  }

  const encodedFileName = contentDisposition.match(/filename\*\s*=\s*([^;]+)/i)?.[1]
  if (encodedFileName) {
    return decodeContentDispositionValue(encodedFileName)
  }

  const fileName = contentDisposition.match(/filename\s*=\s*("[^"]+"|[^;]+)/i)?.[1]
  return fileName ? decodeContentDispositionValue(fileName) : undefined
}

function triggerBrowserDownload(blob: Blob, filename: string) {
  const objectUrl = window.URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = objectUrl
  anchor.download = filename
  anchor.style.display = 'none'

  document.body.append(anchor)
  anchor.click()
  anchor.remove()
  window.setTimeout(() => window.URL.revokeObjectURL(objectUrl), 0)
}

export async function cmsV2PostDownload(
  path: string,
  fallbackFilename: string,
): Promise<CmsV2DownloadedFileResult> {
  const response = await cmsV2Fetch(path, {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }

  const blob = await response.blob()
  const filename =
    getFileNameFromContentDisposition(response.headers.get('content-disposition')) ||
    fallbackFilename
  const contentType = blob.type || response.headers.get('content-type') || ''

  triggerBrowserDownload(blob, filename)

  return {
    filename,
    contentType,
    size: blob.size,
  }
}

function withQuery(path: string, query: Record<string, string | number | undefined>) {
  const params = new URLSearchParams()

  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined) {
      params.set(key, String(value))
    }
  }

  const queryString = params.toString()
  return queryString ? `${path}?${queryString}` : path
}

function withRepeatedQuery(
  path: string,
  query: Record<string, string | number | readonly (string | number)[] | undefined>,
) {
  const params = new URLSearchParams()

  for (const [key, value] of Object.entries(query)) {
    if (Array.isArray(value)) {
      for (const item of value) {
        params.append(key, String(item))
      }
    } else if (value !== undefined) {
      params.set(key, String(value))
    }
  }

  const queryString = params.toString()
  return queryString ? `${path}?${queryString}` : path
}

export const cmsV2Api = {
  getHealth: () => cmsV2FetchJson<CmsV2HealthDto>('/health'),
  getTeachingStructure: () =>
    cmsV2FetchJson<CmsV2TeachingStructureNodeDto[]>('/teaching-structure'),
  listTeachingTopics: () => cmsV2FetchJson<CmsV2TeachingTopicDto[]>('/teaching-topics'),
  createTeachingTopicChild: (
    topicId: number,
    request: CmsV2CreateTeachingTopicChildRequest,
  ) => cmsV2PostJson<CmsV2TeachingTopicDto>(`/teaching-topics/${topicId}/children`, request),
  createTeachingTopicNextSibling: (
    topicId: number,
    request: CmsV2CreateTeachingTopicNextSiblingRequest,
  ) =>
    cmsV2PostJson<CmsV2TeachingTopicDto>(`/teaching-topics/${topicId}/next-sibling`, request),
  renameTeachingTopic: (topicId: number, request: CmsV2RenameTeachingTopicRequest) =>
    cmsV2PostJson<CmsV2TeachingTopicDto>(`/teaching-topics/${topicId}/rename`, request),
  deleteTeachingTopic: (topicId: number) => cmsV2Delete(`/teaching-topics/${topicId}`),
  createSectionForTeachingTopic: (
    topicId: number,
    request: CmsV2CreateSectionForTeachingTopicRequest,
  ) => cmsV2PostJson<CmsV2SectionDto>(`/teaching-topics/${topicId}/section`, request),
  previewSectionVariantSelection: (request: CmsV2PreviewSectionVariantSelectionRequest) =>
    cmsV2PostJson<CmsV2SectionVariantSelectionCandidateDto[]>(
      '/section-variants/selection-preview',
      request,
    ),
  createSectionVariant: (request: CmsV2CreateSectionVariantRequest) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>('/section-variants', request),
  deleteSectionVariant: (sectionVariantId: number) =>
    cmsV2Delete(`/section-variants/${sectionVariantId}`),
  getSectionVariantTree: () =>
    cmsV2FetchJson<CmsV2SectionVariantSelectionTreeTopicDto[]>('/section-variants/tree'),
  listSectionVariants: (sectionId?: number) =>
    cmsV2FetchJson<CmsV2SectionVariantDto[]>(withQuery('/section-variants', { sectionId })),
  listSectionVariantItems: (sectionVariantId: number) =>
    cmsV2FetchJson<CmsV2SectionVariantItemDto[]>(`/section-variants/${sectionVariantId}/items`),
  listSections: (teachingTopicId?: number) =>
    cmsV2FetchJson<CmsV2SectionDto[]>(withQuery('/sections', { teachingTopicId })),
  getSection: (sectionId: number) => cmsV2FetchJson<CmsV2SectionDto>(`/sections/${sectionId}`),
  renameSection: (sectionId: number, request: CmsV2RenameSectionRequest) =>
    cmsV2PostJson<CmsV2SectionDto>(`/sections/${sectionId}/title`, request),
  validateSectionWordGeneration: (sectionId: number) =>
    cmsV2FetchJson<WordGenerationValidationResult>(
      `/sections/${sectionId}/validate-word-generation`,
      { method: 'POST' },
    ),
  downloadSectionWord: (sectionId: number) =>
    cmsV2PostDownload(`/sections/${sectionId}/generate-word`, `section-${sectionId}.docx`),
  listSectionItems: (sectionId: number) =>
    cmsV2FetchJson<CmsV2SectionItemDto[]>(`/sections/${sectionId}/items`),
  addSectionItem: (sectionId: number, request: CmsV2AddSectionItemRequest) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>(`/sections/${sectionId}/items`, request),
  moveSectionItem: (
    sectionId: number,
    sectionItemId: number,
    request: CmsV2MoveSectionItemRequest,
  ) => cmsV2PostJson(`/sections/${sectionId}/items/${sectionItemId}/move`, request),
  wrapSectionItemsAsAtomicSection: (
    sectionId: number,
    request: CmsV2WrapSectionItemsAsAtomicSectionRequest,
  ) =>
    cmsV2PostJson<CmsV2WrapSectionItemsAsAtomicSectionResultDto>(
      `/sections/${sectionId}/items/wrap-as-atomic-section`,
      request,
    ),
  removeSectionItem: (sectionId: number, sectionItemId: number) =>
    cmsV2Delete(`/sections/${sectionId}/items/${sectionItemId}`),
  deleteSectionItemContentAsset: (sectionId: number, sectionItemId: number) =>
    cmsV2DeleteJson<CmsV2ContentAssetDeleteResultDto>(
      `/sections/${sectionId}/items/${sectionItemId}/content-asset`,
    ),
  getAtomicSection: (atomicSectionId: number) =>
    cmsV2FetchJson<CmsV2AtomicSectionDto>(`/atomic-sections/${atomicSectionId}`),
  listAtomicSections: () => cmsV2FetchJson<CmsV2AtomicSectionDto[]>('/atomic-sections'),
  createAtomicSection: (request: CmsV2CreateAtomicSectionRequest) =>
    cmsV2PostJson<CmsV2AtomicSectionDto>('/atomic-sections', request),
  listAtomicSectionPanels: (atomicSectionId: number) =>
    cmsV2FetchJson<CmsV2AtomicSectionPanelDto[]>(`/atomic-sections/${atomicSectionId}/panels`),
  createAtomicSectionPanel: (
    atomicSectionId: number,
    request: CmsV2CreateAtomicSectionPanelRequest,
  ) =>
    cmsV2PostJson<CmsV2AtomicSectionPanelDto>(
      `/atomic-sections/${atomicSectionId}/panels`,
      request,
    ),
  updateAtomicSectionPanel: (
    atomicSectionId: number,
    atomicSectionPanelId: number,
    request: CmsV2UpdateAtomicSectionPanelRequest,
  ) =>
    cmsV2PutJson<CmsV2AtomicSectionPanelDto>(
      `/atomic-sections/${atomicSectionId}/panels/${atomicSectionPanelId}`,
      request,
    ),
  moveAtomicSectionPanel: (
    atomicSectionId: number,
    atomicSectionPanelId: number,
    request: CmsV2MoveAtomicSectionPanelRequest,
  ) =>
    cmsV2PostNoContent(
      `/atomic-sections/${atomicSectionId}/panels/${atomicSectionPanelId}/move`,
      request,
    ),
  deleteAtomicSectionPanel: (atomicSectionId: number, atomicSectionPanelId: number) =>
    cmsV2Delete(`/atomic-sections/${atomicSectionId}/panels/${atomicSectionPanelId}`),
  renameAtomicSection: (atomicSectionId: number, title: string) =>
    cmsV2PostJson<CmsV2AtomicSectionDto>(`/atomic-sections/${atomicSectionId}/title`, {
      title,
    }),
  changeAtomicSectionDifficulty: (
    atomicSectionId: number,
    request: CmsV2ChangeDifficultyRequest,
  ) =>
    cmsV2PostJson<CmsV2AtomicSectionDto>(
      `/atomic-sections/${atomicSectionId}/difficulty`,
      request,
    ),
  changeAtomicSectionStatus: (
    atomicSectionId: number,
    request: CmsV2ChangeAtomicSectionStatusRequest,
  ) =>
    cmsV2PostJson<CmsV2AtomicSectionDto>(
      `/atomic-sections/${atomicSectionId}/status`,
      request,
    ),
  listAtomicSectionItems: (atomicSectionId: number) =>
    cmsV2FetchJson<CmsV2AtomicSectionItemDto[]>(`/atomic-sections/${atomicSectionId}/items`),
  addAtomicSectionItem: (
    atomicSectionId: number,
    request: CmsV2AddAtomicSectionItemRequest,
  ) => cmsV2PostJson<CmsV2CreatedEntityResultDto>(`/atomic-sections/${atomicSectionId}/items`, request),
  moveAtomicSectionItem: (
    atomicSectionId: number,
    atomicSectionItemId: number,
    request: CmsV2MoveAtomicSectionItemRequest,
  ) =>
    cmsV2PostJson(
      `/atomic-sections/${atomicSectionId}/items/${atomicSectionItemId}/move`,
      request,
    ),
  removeAtomicSectionItem: (atomicSectionId: number, atomicSectionItemId: number) =>
    cmsV2Delete(`/atomic-sections/${atomicSectionId}/items/${atomicSectionItemId}`),
  deleteAtomicSectionItemContentAsset: (
    atomicSectionId: number,
    atomicSectionItemId: number,
  ) =>
    cmsV2DeleteJson<CmsV2ContentAssetDeleteResultDto>(
      `/atomic-sections/${atomicSectionId}/items/${atomicSectionItemId}/content-asset`,
    ),
  changeAtomicSectionItemClassification: (
    atomicSectionId: number,
    atomicSectionItemId: number,
    request: CmsV2ChangeAtomicSectionItemClassificationRequest,
  ) =>
    cmsV2PostNoContent(
      `/atomic-sections/${atomicSectionId}/items/${atomicSectionItemId}/classification`,
      request,
    ),
  listTags: (keyword?: string) =>
    cmsV2FetchJson<CmsV2TagDto[]>(withQuery('/tags', { keyword })),
  createTag: (request: CmsV2CreateTagRequest) =>
    cmsV2PostJson<CmsV2TagDto>('/tags', request),
  updateTag: (tagId: number, request: CmsV2UpdateTagRequest) =>
    cmsV2PatchJson<CmsV2TagDto>(`/tags/${tagId}`, request),
  archiveTag: (tagId: number) =>
    cmsV2PostJson<CmsV2TagDto>(`/tags/${tagId}/archive`, {}),
  restoreTag: (tagId: number) =>
    cmsV2PostJson<CmsV2TagDto>(`/tags/${tagId}/restore`, {}),
  listTagBindings: (targetType: CmsV2TagBindingTargetType, targetId: number) =>
    cmsV2FetchJson<CmsV2TagBindingDto[]>(
      withQuery('/tag-bindings', { targetType, targetId }),
    ),
  replaceTagBindings: (request: CmsV2SetTargetTagsRequest) =>
    cmsV2PutJson<CmsV2TagBindingDto[]>('/tag-bindings', request),
  getTeachingNote: (teachingNoteId: number) =>
    cmsV2FetchJson<CmsV2TeachingNoteDto>(`/teaching-notes/${teachingNoteId}`),
  listTeachingNotes: (query: CmsV2SearchTeachingNotesQuery = {}) =>
    cmsV2FetchJson<CmsV2TeachingNoteDto[]>(
      withQuery('/teaching-notes', {
        keyword: query.keyword,
        noteType: query.noteType,
        effectLevel: query.effectLevel,
        targetType: query.targetType,
        targetId: query.targetId,
        occurredFrom: query.occurredFrom,
        occurredTo: query.occurredTo,
      }),
    ),
  listTeachingNotesByBinding: (
    targetType: CmsV2TeachingNoteTargetType,
    targetId: number,
  ) =>
    cmsV2FetchJson<CmsV2TeachingNoteDto[]>(
      withQuery('/teaching-note-bindings', { targetType, targetId }),
    ),
  createTeachingNote: (request: CmsV2CreateTeachingNoteRequest) =>
    cmsV2PostJson<CmsV2TeachingNoteDto>('/teaching-notes', request),
  updateTeachingNote: (teachingNoteId: number, request: CmsV2UpdateTeachingNoteRequest) =>
    cmsV2PatchJson<CmsV2TeachingNoteDto>(`/teaching-notes/${teachingNoteId}`, request),
  deleteTeachingNote: (teachingNoteId: number) =>
    cmsV2Delete(`/teaching-notes/${teachingNoteId}`),
  getContentBlock: (contentBlockId: number) =>
    cmsV2FetchJson<CmsV2ContentBlockDto>(`/content-blocks/${contentBlockId}`),
  listContentBlocks: (query: CmsV2ListContentBlocksQuery = {}) =>
    cmsV2FetchJson<CmsV2ContentBlockDto[]>(
      withRepeatedQuery('/content-blocks', { tagIds: query.tagIds }),
    ),
  createContentBlock: (request: CmsV2CreateContentBlockRequest) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>('/content-blocks', request),
  changeContentBlockDifficulty: (
    contentBlockId: number,
    request: CmsV2ChangeDifficultyRequest,
  ) =>
    cmsV2PostJson<CmsV2ContentBlockDto>(
      `/content-blocks/${contentBlockId}/difficulty`,
      request,
    ),
  createContentBlockWithBlankDocument: (request: CmsV2CreateContentBlockWithBlankDocumentRequest) =>
    cmsV2PostJson<CmsV2ContentBlockDocumentVersionResultDto>(
      '/content-blocks/blank-document',
      request,
    ),
  listContentBlockVersions: (contentBlockId: number) =>
    cmsV2FetchJson<CmsV2ContentBlockVersionDto[]>(`/content-blocks/${contentBlockId}/versions`),
  getContentBlockHtmlPreview: (contentBlockId: number, versionId?: number | null) =>
    versionId
      ? cmsV2FetchText(`/content-blocks/${contentBlockId}/versions/${versionId}/html-preview`)
      : cmsV2FetchText(`/content-blocks/${contentBlockId}/html-preview`),
  listContentBlockVersionParts: (contentBlockId: number, versionId: number) =>
    cmsV2FetchJson<CmsV2ContentBlockVersionPartDto[]>(
      `/content-blocks/${contentBlockId}/versions/${versionId}/parts`,
    ),
  createQuestionImportSession: (request: CmsV2CreateQuestionImportSessionRequest) =>
    cmsV2PostJson<CmsV2QuestionImportSessionDto>('/question-import-sessions', request),
  getQuestionImportSession: (sessionId: string) =>
    cmsV2FetchJson<CmsV2QuestionImportSessionDto>(`/question-import-sessions/${sessionId}`),
  listQuestionImportCandidates: (sessionId: string) =>
    cmsV2FetchJson<CmsV2QuestionImportCandidateDto[]>(
      `/question-import-sessions/${sessionId}/candidates`,
    ),
  confirmQuestionImportCandidates: (
    sessionId: string,
    request: CmsV2ConfirmQuestionImportRequest,
  ) =>
    cmsV2PostJson<CmsV2QuestionImportConfirmResultDto>(
      `/question-import-sessions/${sessionId}/confirm`,
      request,
    ),
  cancelQuestionImportSession: (sessionId: string) =>
    cmsV2PostJson<CmsV2QuestionImportSessionDto>(
      `/question-import-sessions/${sessionId}/cancel`,
      {},
    ),
  reopenQuestionImportSession: (sessionId: string) =>
    cmsV2PostJson<CmsV2QuestionImportSessionDto>(
      `/question-import-sessions/${sessionId}/reopen`,
      {},
    ),
  createContentBlockEditSession: (
    contentBlockId: number,
    request: CmsV2CreateContentBlockEditSessionRequest,
  ) =>
    cmsV2PostJson<CmsV2ContentBlockEditSessionDto>(
      `/content-blocks/${contentBlockId}/edit-session`,
      request,
    ),
  deleteContentBlockCascade: (contentBlockId: number) =>
    cmsV2PostJson<CmsV2DeleteContentBlockCascadeResultDto>(
      `/content-blocks/${contentBlockId}/delete-cascade`,
      {},
    ),
  getContentBlockEditSession: (sessionId: string) =>
    cmsV2FetchJson<CmsV2ContentBlockEditSessionDto>(
      `/content-block-edit-sessions/${sessionId}`,
    ),
  syncContentBlockEditSession: (sessionId: string) =>
    cmsV2PostJson<CmsV2SyncContentBlockEditSessionResultDto>(
      `/content-block-edit-sessions/${sessionId}/sync`,
      {},
    ),
  cancelContentBlockEditSession: (sessionId: string) =>
    cmsV2PostJson<CmsV2ContentBlockEditSessionDto>(
      `/content-block-edit-sessions/${sessionId}/cancel`,
      {},
    ),
  listContentBlockChildren: (contentBlockId: number) =>
    cmsV2FetchJson<CmsV2ContentBlockRelationDto[]>(
      `/content-blocks/${contentBlockId}/relations/children`,
    ),
  addContentBlockRelation: (
    parentBlockId: number,
    request: CmsV2AddContentBlockRelationRequest,
  ) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>(
      `/content-blocks/${parentBlockId}/relations/children`,
      request,
    ),
  moveContentBlockRelation: (
    parentBlockId: number,
    relationId: number,
    request: CmsV2MoveContentBlockRelationRequest,
  ) =>
    cmsV2PostJson(
      `/content-blocks/${parentBlockId}/relations/children/${relationId}/move`,
      request,
    ),
  removeContentBlockRelation: (parentBlockId: number, relationId: number) =>
    cmsV2Delete(`/content-blocks/${parentBlockId}/relations/children/${relationId}`),
  listHandouts: () => cmsV2FetchJson<CmsV2HandoutDto[]>('/handouts'),
  createHandout: (request: CmsV2CreateHandoutRequest) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>('/handouts', request),
  updateHandout: (handoutId: number, request: CmsV2UpdateHandoutRequest) =>
    cmsV2PatchNoContent(`/handouts/${handoutId}`, request),
  listHandoutVersions: (handoutId: number) =>
    cmsV2FetchJson<CmsV2HandoutVersionDto[]>(`/handouts/${handoutId}/versions`),
  createHandoutVersion: (handoutId: number, request: CmsV2CreateHandoutVersionRequest) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>(`/handouts/${handoutId}/versions`, request),
  updateHandoutVersion: (
    handoutVersionId: number,
    request: CmsV2UpdateHandoutVersionRequest,
  ) => cmsV2PatchNoContent(`/handout-versions/${handoutVersionId}`, request),
  getHandoutVersionWorkspace: (handoutVersionId: number) =>
    cmsV2FetchJson<CmsV2HandoutVersionWorkspaceDto>(
      `/handout-versions/${handoutVersionId}/workspace`,
    ),
  addHandoutVersionItem: (
    handoutVersionId: number,
    request: CmsV2AddHandoutVersionItemRequest,
  ) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>(
      `/handout-versions/${handoutVersionId}/items`,
      request,
    ),
  batchAddSectionVariantsToHandoutVersion: (
    handoutVersionId: number,
    request: CmsV2BatchAddSectionVariantsToHandoutVersionRequest,
  ) =>
    cmsV2PostJson<CmsV2BatchAddSectionVariantsToHandoutVersionResultDto>(
      `/handout-versions/${handoutVersionId}/items/batch-add-section-variants`,
      request,
    ),
  updateHandoutVersionItem: (
    handoutVersionId: number,
    handoutVersionItemId: number,
    request: CmsV2UpdateHandoutVersionItemRequest,
  ) =>
    cmsV2PatchNoContent(
      `/handout-versions/${handoutVersionId}/items/${handoutVersionItemId}`,
      request,
    ),
  moveHandoutVersionItem: (
    handoutVersionId: number,
    handoutVersionItemId: number,
    request: CmsV2MoveHandoutVersionItemRequest,
  ) =>
    cmsV2PostNoContent(
      `/handout-versions/${handoutVersionId}/items/${handoutVersionItemId}/move`,
      request,
    ),
  removeHandoutVersionItem: (handoutVersionId: number, handoutVersionItemId: number) =>
    cmsV2Delete(`/handout-versions/${handoutVersionId}/items/${handoutVersionItemId}`),
  generateHandoutWord: (outputFormId: number, request: CmsV2GenerateHandoutWordRequest) =>
    cmsV2PostJson<CmsV2GeneratedHandoutFileResultDto>(
      `/output-forms/${outputFormId}/generate-word`,
      request,
    ),
  getGeneratedFileManifest: (generatedFileId: number) =>
    cmsV2FetchText(`/generated-files/${generatedFileId}/manifest`),
  getGeneratedFileDownloadUrl: (generatedFileId: number) =>
    createCmsV2Url(`/generated-files/${generatedFileId}/download`),
  deleteGeneratedFile: (generatedFileId: number) =>
    cmsV2Delete(`/generated-files/${generatedFileId}`),
}
