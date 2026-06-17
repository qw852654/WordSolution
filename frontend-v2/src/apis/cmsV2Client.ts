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
  title: string
  description?: string | null
  type: string
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
  getAtomicSection: (atomicSectionId: number) =>
    cmsV2FetchJson<CmsV2AtomicSectionDto>(`/atomic-sections/${atomicSectionId}`),
  listAtomicSectionItems: (atomicSectionId: number) =>
    cmsV2FetchJson<CmsV2AtomicSectionItemDto[]>(`/atomic-sections/${atomicSectionId}/items`),
  getContentBlock: (contentBlockId: number) =>
    cmsV2FetchJson<CmsV2ContentBlockDto>(`/content-blocks/${contentBlockId}`),
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
}
