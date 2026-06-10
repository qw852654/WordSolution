export const CMS_V2_API_BASE = '/api/cms-v2'

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
