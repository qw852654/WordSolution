import { cmsV2Api } from '@/apis/cmsV2Client'

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
    const createdContentBlock = await cmsV2Api.createContentBlockWithBlankDocument({
      sectionId: input.sectionId,
      title: input.title,
      blockType: input.blockType,
      summary: null,
      difficulty: input.difficulty,
      questionType: null,
      status: 'Draft',
    })

    const createdAtomicSectionItem = await cmsV2Api.addAtomicSectionItem(input.atomicSectionId, {
      contentBlockId: createdContentBlock.contentBlockId,
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

  return {
    renameAtomicSection,
    createContentBlockInsideAtomicSection,
  }
}
