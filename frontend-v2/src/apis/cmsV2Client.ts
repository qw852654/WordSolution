export const CMS_V2_API_BASE = '/api/cms-v2'

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
  status: string
  updatedTime: string
}

export interface CmsV2AtomicSectionItemDto {
  id: number
  atomicSectionId: number
  contentBlockId: number
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

export interface CmsV2ContentBlockVersionDto {
  id: number
  contentBlockId: number
  versionNumber: number
  docxPath: string
  htmlPreviewPath?: string | null
  plainText?: string | null
  isCurrent: boolean
  updatedTime: string
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

export interface CmsV2CreateContentBlockWithBlankDocumentRequest {
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

export interface CmsV2MoveSectionItemRequest {
  direction: 'Up' | 'Down'
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
  sortOrder: number
  titleOverride?: string | null
  note?: string | null
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

export async function cmsV2Delete(path: string): Promise<void> {
  const response = await cmsV2Fetch(path, {
    method: 'DELETE',
  })

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }
}

export async function cmsV2FetchText(path: string, init?: RequestInit): Promise<string> {
  const response = await cmsV2Fetch(path, init)

  if (!response.ok) {
    throw new Error(await readErrorMessage(response))
  }

  return await response.text()
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

export const cmsV2Api = {
  listTeachingTopics: () => cmsV2FetchJson<CmsV2TeachingTopicDto[]>('/teaching-topics'),
  listSections: (teachingTopicId?: number) =>
    cmsV2FetchJson<CmsV2SectionDto[]>(withQuery('/sections', { teachingTopicId })),
  getSection: (sectionId: number) => cmsV2FetchJson<CmsV2SectionDto>(`/sections/${sectionId}`),
  listSectionItems: (sectionId: number) =>
    cmsV2FetchJson<CmsV2SectionItemDto[]>(`/sections/${sectionId}/items`),
  addSectionItem: (sectionId: number, request: CmsV2AddSectionItemRequest) =>
    cmsV2PostJson<CmsV2CreatedEntityResultDto>(`/sections/${sectionId}/items`, request),
  moveSectionItem: (
    sectionId: number,
    sectionItemId: number,
    request: CmsV2MoveSectionItemRequest,
  ) => cmsV2PostJson(`/sections/${sectionId}/items/${sectionItemId}/move`, request),
  removeSectionItem: (sectionId: number, sectionItemId: number) =>
    cmsV2Delete(`/sections/${sectionId}/items/${sectionItemId}`),
  getAtomicSection: (atomicSectionId: number) =>
    cmsV2FetchJson<CmsV2AtomicSectionDto>(`/atomic-sections/${atomicSectionId}`),
  createAtomicSection: (request: CmsV2CreateAtomicSectionRequest) =>
    cmsV2PostJson<CmsV2AtomicSectionDto>('/atomic-sections', request),
  renameAtomicSection: (atomicSectionId: number, title: string) =>
    cmsV2PostJson<CmsV2AtomicSectionDto>(`/atomic-sections/${atomicSectionId}/title`, {
      title,
    }),
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
  getContentBlock: (contentBlockId: number) =>
    cmsV2FetchJson<CmsV2ContentBlockDto>(`/content-blocks/${contentBlockId}`),
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
  listContentBlockChildren: (contentBlockId: number) =>
    cmsV2FetchJson<CmsV2ContentBlockRelationDto[]>(
      `/content-blocks/${contentBlockId}/relations/children`,
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
}
