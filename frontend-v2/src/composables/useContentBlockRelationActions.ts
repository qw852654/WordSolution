import { cmsV2Api, type CmsV2MoveContentBlockRelationRequest } from '@/apis/cmsV2Client'

export interface ContentBlockRelationActionsOptions {
  refreshSection: () => Promise<void>
}

export function useContentBlockRelationActions(options: ContentBlockRelationActionsOptions) {
  async function moveContentBlockRelation(
    parentBlockId: number,
    relationId: number,
    direction: CmsV2MoveContentBlockRelationRequest['direction'],
  ) {
    await cmsV2Api.moveContentBlockRelation(parentBlockId, relationId, { direction })
    await options.refreshSection()
  }

  async function moveContentBlockRelationUp(parentBlockId: number, relationId: number) {
    await moveContentBlockRelation(parentBlockId, relationId, 'Up')
  }

  async function moveContentBlockRelationDown(parentBlockId: number, relationId: number) {
    await moveContentBlockRelation(parentBlockId, relationId, 'Down')
  }

  async function removeContentBlockRelation(parentBlockId: number, relationId: number) {
    await cmsV2Api.removeContentBlockRelation(parentBlockId, relationId)
    await options.refreshSection()
  }

  return {
    moveContentBlockRelation,
    moveContentBlockRelationUp,
    moveContentBlockRelationDown,
    removeContentBlockRelation,
  }
}
