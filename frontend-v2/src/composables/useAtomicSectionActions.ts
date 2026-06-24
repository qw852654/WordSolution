import {
  cmsV2Api,
  type CmsV2AtomicSectionTeachingRole,
  type CmsV2MoveAtomicSectionItemRequest,
  type CmsV2MoveAtomicSectionPanelRequest,
} from '@/apis/cmsV2Client'

export interface AtomicSectionActionsOptions {
  refreshSection: () => Promise<void>
}

export interface CreateContentBlockInsideAtomicSectionInput {
  atomicSectionId: number
  sectionId: number
  title: string
  blockType: string
  difficulty: string
  sortOrder?: number
  atomicSectionPanelId?: number | null
  teachingRole?: CmsV2AtomicSectionTeachingRole
  beforeAtomicSectionItemId?: number | null
  afterAtomicSectionItemId?: number | null
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
      atomicSectionPanelId: input.atomicSectionPanelId,
      teachingRole: input.teachingRole,
      beforeAtomicSectionItemId: input.beforeAtomicSectionItemId,
      afterAtomicSectionItemId: input.afterAtomicSectionItemId,
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

  async function createAtomicSectionPanel(
    atomicSectionId: number,
    title: string,
    teachingRole: CmsV2AtomicSectionTeachingRole,
    difficulty: string,
    beforeAtomicSectionPanelId?: number | null,
    afterAtomicSectionPanelId?: number | null,
  ) {
    const created = await cmsV2Api.createAtomicSectionPanel(atomicSectionId, {
      title,
      teachingRole,
      difficulty,
      beforeAtomicSectionPanelId,
      afterAtomicSectionPanelId,
    })
    await options.refreshSection()
    return created
  }

  async function renameAtomicSectionPanel(
    atomicSectionId: number,
    atomicSectionPanelId: number,
    title: string,
    teachingRole: CmsV2AtomicSectionTeachingRole,
    difficulty: string,
  ) {
    const updated = await cmsV2Api.updateAtomicSectionPanel(atomicSectionId, atomicSectionPanelId, {
      title,
      teachingRole,
      difficulty,
    })
    await options.refreshSection()
    return updated
  }

  async function moveAtomicSectionPanel(
    atomicSectionId: number,
    atomicSectionPanelId: number,
    direction: CmsV2MoveAtomicSectionPanelRequest['direction'],
  ) {
    await cmsV2Api.moveAtomicSectionPanel(atomicSectionId, atomicSectionPanelId, { direction })
    await options.refreshSection()
  }

  async function removeAtomicSectionPanel(
    atomicSectionId: number,
    atomicSectionPanelId: number,
  ) {
    await cmsV2Api.deleteAtomicSectionPanel(atomicSectionId, atomicSectionPanelId)
    await options.refreshSection()
  }

  async function changeAtomicSectionItemClassification(
    atomicSectionId: number,
    atomicSectionItemId: number,
    teachingRole: CmsV2AtomicSectionTeachingRole,
    difficulty: string,
  ) {
    await cmsV2Api.changeAtomicSectionItemClassification(
      atomicSectionId,
      atomicSectionItemId,
      {
        teachingRole,
        difficulty,
      },
    )
    await options.refreshSection()
  }

  return {
    renameAtomicSection,
    createContentBlockInsideAtomicSection,
    createAtomicSectionPanel,
    moveAtomicSectionItem,
    moveAtomicSectionItemUp,
    moveAtomicSectionItemDown,
    removeAtomicSectionItem,
    renameAtomicSectionPanel,
    moveAtomicSectionPanel,
    removeAtomicSectionPanel,
    changeAtomicSectionItemClassification,
  }
}
