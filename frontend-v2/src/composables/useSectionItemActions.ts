import { cmsV2Api, type CmsV2MoveSectionItemRequest } from '@/apis/cmsV2Client'

export interface SectionItemActionsOptions {
  refreshSection: () => Promise<void>
}

export function useSectionItemActions(options: SectionItemActionsOptions) {
  async function moveSectionItem(
    sectionId: number,
    sectionItemId: number,
    direction: CmsV2MoveSectionItemRequest['direction'],
  ) {
    await cmsV2Api.moveSectionItem(sectionId, sectionItemId, { direction })
    await options.refreshSection()
  }

  async function moveSectionItemUp(sectionId: number, sectionItemId: number) {
    await moveSectionItem(sectionId, sectionItemId, 'Up')
  }

  async function moveSectionItemDown(sectionId: number, sectionItemId: number) {
    await moveSectionItem(sectionId, sectionItemId, 'Down')
  }

  async function removeSectionItemReference(sectionId: number, sectionItemId: number) {
    await cmsV2Api.removeSectionItem(sectionId, sectionItemId)
    await options.refreshSection()
  }

  return {
    moveSectionItem,
    moveSectionItemUp,
    moveSectionItemDown,
    removeSectionItemReference,
  }
}
