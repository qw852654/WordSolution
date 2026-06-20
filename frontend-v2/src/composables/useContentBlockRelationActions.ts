import { cmsV2Api, type CmsV2MoveContentBlockRelationRequest } from '@/apis/cmsV2Client'

export interface ContentBlockRelationActionsOptions {
  refreshSection: () => Promise<void>
}

export interface CreateContentBlockInsideCompositeBlockInput {
  parentBlockId: number
  sectionId: number
  title: string
  blockType: string
  difficulty: string
  sortOrder: number
}

export function useContentBlockRelationActions(options: ContentBlockRelationActionsOptions) {
  async function createContentBlockInsideCompositeBlock(
    input: CreateContentBlockInsideCompositeBlockInput,
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

    const createdRelation = await cmsV2Api.addContentBlockRelation(input.parentBlockId, {
      childBlockId: createdContentBlock.id,
      referenceMode: 'FollowLatest',
      lockedContentBlockVersionId: null,
      sortOrder: input.sortOrder,
      titleOverride: null,
      note: null,
    })

    await options.refreshSection()

    return {
      contentBlock: createdContentBlock,
      relation: createdRelation,
    }
  }

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
    createContentBlockInsideCompositeBlock,
    moveContentBlockRelation,
    moveContentBlockRelationUp,
    moveContentBlockRelationDown,
    removeContentBlockRelation,
  }
}
