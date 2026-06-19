import { cmsV2Api, type CmsV2MoveAtomicSectionItemRequest } from '@/apis/cmsV2Client'

export interface AtomicSectionActionsOptions {
  refreshSection: () => Promise<void>
}

export interface CreateContentBlockInsideAtomicSectionInput {
  atomicSectionId: number
  sectionId: number
  title: string
  blockType: string
  difficulty: string
  sortOrder: number
}

export function useAtomicSectionActions(options: AtomicSectionActionsOptions) {
  async function renameAtomicSection(atomicSectionId: number, title: string) {
    const renamed = await cmsV2Api.renameAtomicSection(atomicSectionId, title)
    await options.refreshSection()
    return renamed
  }

  async function createContentBlockInsideAtomicSection(
    input: CreateContentBlockInsideAtomicSectionInput,
  ) {
    const createdContentBlock = await cmsV2Api.createContentBlock({
      sectionId: input.sectionId,
      title: input.title,
      blockType: input.blockType,
      summary: null,
      difficulty: input.difficulty,
      questionType: null,
      status: 'Draft',
    })

    const createdAtomicSectionItem = await cmsV2Api.addAtomicSectionItem(input.atomicSectionId, {
      contentBlockId: createdContentBlock.id,
      referenceMode: 'FollowLatest',
      lockedContentBlockVersionId: null,
      sortOrder: input.sortOrder,
      titleOverride: null,
      note: null,
    })

    await options.refreshSection()

    return {
      contentBlock: createdContentBlock,
      atomicSectionItem: createdAtomicSectionItem,
    }
  }

  async function moveAtomicSectionItem(
    atomicSectionId: number,
    atomicSectionItemId: number,
    direction: CmsV2MoveAtomicSectionItemRequest['direction'],
  ) {
    await cmsV2Api.moveAtomicSectionItem(atomicSectionId, atomicSectionItemId, { direction })
    await options.refreshSection()
  }

  async function moveAtomicSectionItemUp(atomicSectionId: number, atomicSectionItemId: number) {
    await moveAtomicSectionItem(atomicSectionId, atomicSectionItemId, 'Up')
  }

  async function moveAtomicSectionItemDown(atomicSectionId: number, atomicSectionItemId: number) {
    await moveAtomicSectionItem(atomicSectionId, atomicSectionItemId, 'Down')
  }

  async function removeAtomicSectionItem(atomicSectionId: number, atomicSectionItemId: number) {
    await cmsV2Api.removeAtomicSectionItem(atomicSectionId, atomicSectionItemId)
    await options.refreshSection()
  }

  return {
    renameAtomicSection,
    createContentBlockInsideAtomicSection,
    moveAtomicSectionItem,
    moveAtomicSectionItemUp,
    moveAtomicSectionItemDown,
    removeAtomicSectionItem,
  }
}
