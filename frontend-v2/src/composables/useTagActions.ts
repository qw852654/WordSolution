import {
  cmsV2Api,
  type CmsV2SetTargetTagsRequest,
  type CmsV2TagBindingTargetType,
} from '@/apis/cmsV2Client'

export interface TagActionTarget {
  targetType: CmsV2TagBindingTargetType
  targetId: number
}

export function useTagActions() {
  async function searchTags(keyword: string) {
    const tags = await cmsV2Api.listTags(keyword)
    return tags.filter((tag) => tag.status === 'Active')
  }

  async function createTag(name: string) {
    return await cmsV2Api.createTag({ name })
  }

  async function loadTargetTags(target: TagActionTarget) {
    return await cmsV2Api.listTagBindings(target.targetType, target.targetId)
  }

  async function replaceTargetTags(request: CmsV2SetTargetTagsRequest) {
    return await cmsV2Api.replaceTagBindings(request)
  }

  async function listContentBlocksByTags(tagIds: number[]) {
    return await cmsV2Api.listContentBlocks({ tagIds })
  }

  return {
    searchTags,
    createTag,
    loadTargetTags,
    replaceTargetTags,
    listContentBlocksByTags,
  }
}
