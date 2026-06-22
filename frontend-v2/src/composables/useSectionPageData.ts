import { cmsV2Api } from '@/apis/cmsV2Client'
import type {
  CmsV2AtomicSectionDto,
  CmsV2AtomicSectionItemDto,
  CmsV2ContentBlockDto,
  CmsV2ContentBlockRelationDto,
  CmsV2ContentBlockVersionDto,
  CmsV2SectionDto,
  CmsV2SectionItemDto,
  CmsV2SectionVariantDto,
} from '@/apis/cmsV2Client'
import {
  createTeachingTopicNodeId,
  findTeachingStructureTopicTitle,
  mapTeachingStructureNodesToTreeNodes,
} from '@/utils/teachingStructureTree'
import type {
  ContentBlockDisplayModel,
  HtmlPreviewState,
  SectionPageShellModel,
  SectionReferenceMode,
  SectionTreeNodeModel,
  SectionWorkspaceFlowItemModel,
  StructuredBlockChildModel,
  TeachingTopicTreeNodeModel,
} from '@/types'

export interface SectionPageDataModel {
  section: SectionPageShellModel
  treeNodes: SectionTreeNodeModel[]
  flowItems: SectionWorkspaceFlowItemModel[]
  workspaceNodeMap: Record<string, string>
  teachingTopicNodes: TeachingTopicTreeNodeModel[]
  selectedTeachingTopicId?: string
  defaultSelectedNodeId?: string
}

interface ResolvedContentBlock {
  block: CmsV2ContentBlockDto
  versions: CmsV2ContentBlockVersionDto[]
}

interface StructuredBlockChildContext {
  sortOrder?: number
  atomicSectionId?: number
  atomicSectionItemId?: number
  parentBlockId?: number
  relationId?: number
  contentBlockId?: number
}

const compositeContentBlockTypes = new Set(['ExampleGroup', 'ExerciseGroup', 'VariantGroup'])

function isCompositeContentBlockType(blockType?: string | null) {
  return blockType ? compositeContentBlockTypes.has(blockType) : false
}

export async function loadSectionPageData(routeSectionId?: string): Promise<SectionPageDataModel> {
  const [sections, teachingStructure] = await Promise.all([
    cmsV2Api.listSections(),
    cmsV2Api.getTeachingStructure(),
  ])

  const section = await resolveSection(routeSectionId, sections)
  const [sectionItems, sectionVariants] = await Promise.all([
    cmsV2Api.listSectionItems(section.id),
    cmsV2Api.listSectionVariants(section.id),
  ])
  const sortedSectionItems = sortByOrder(sectionItems).filter((item) => !item.parentItemId)
  const sortedSectionVariants = sortByOrder(sectionVariants)

  const blockCache = new Map<number, Promise<ResolvedContentBlock>>()
  const atomicSectionCache = new Map<number, Promise<CmsV2AtomicSectionDto>>()
  const atomicSectionItemsCache = new Map<number, Promise<CmsV2AtomicSectionItemDto[]>>()
  const relationCache = new Map<number, Promise<CmsV2ContentBlockRelationDto[]>>()
  const workspaceNodeMap: Record<string, string> = {}

  const context = {
    blockCache,
    atomicSectionCache,
    atomicSectionItemsCache,
    relationCache,
    workspaceNodeMap,
  }

  const sectionChildren = await Promise.all(
    sortedSectionItems.map((item) => buildSectionItemNode(item, context)),
  )
  const variantChildren = sortedSectionVariants.map(buildSectionVariantNode)
  const flowItems = await Promise.all(
    sortedSectionItems.map((item) => buildSectionFlowItem(item, context)),
  )

  const rootNodeId = createSectionNodeId(section.id)
  const teachingTopicTitle =
    findTeachingStructureTopicTitle(teachingStructure, section.teachingTopicId) ??
    'TeachingTopic'
  const treeNodes: SectionTreeNodeModel[] = [
    {
      id: rootNodeId,
      title: section.title,
      kind: 'Section',
      typeLabel: 'Section',
      sectionId: section.id,
      teachingTopicTitle,
      difficulty: mapDifficulty(section.difficulty),
      status: mapStatus(section.status),
      itemCount: sectionChildren.length,
      expanded: true,
      disabled: section.status === 'Archived',
      children: sectionChildren,
    },
    ...variantChildren,
  ]

  return {
    section: {
      sectionId: String(section.id),
      title: section.title,
      teachingTopicTitle,
      status: mapStatus(section.status),
    },
    treeNodes,
    flowItems,
    workspaceNodeMap,
    teachingTopicNodes: mapTeachingStructureNodesToTreeNodes(teachingStructure),
    selectedTeachingTopicId: createTeachingTopicNodeId(section.teachingTopicId),
    defaultSelectedNodeId: sectionChildren[0]?.id ?? rootNodeId,
  }
}

function buildSectionVariantNode(variant: CmsV2SectionVariantDto): SectionTreeNodeModel {
  return {
    id: createSectionVariantNodeId(variant.id),
    title: variant.title,
    kind: 'SectionVariant',
    typeLabel: 'SectionVariant',
    sectionVariantId: variant.id,
    difficulty: mapDifficulty(variant.difficulty),
    status: mapStatus(variant.status),
    disabled: variant.status === 'Archived',
  }
}

async function resolveSection(routeSectionId: string | undefined, sections: CmsV2SectionDto[]) {
  const parsedSectionId = Number(routeSectionId)

  if (Number.isInteger(parsedSectionId) && parsedSectionId > 0) {
    return await cmsV2Api.getSection(parsedSectionId)
  }

  const firstSection = sortByOrder(sections)[0]

  if (!firstSection) {
    throw new Error('当前 CMS V2 题库中还没有 Section 数据。')
  }

  return firstSection
}

async function resolveContentBlock(
  contentBlockId: number,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
  },
) {
  const cached = context.blockCache.get(contentBlockId)
  if (cached) {
    return await cached
  }

  const request = Promise.all([
    cmsV2Api.getContentBlock(contentBlockId),
    cmsV2Api.listContentBlockVersions(contentBlockId),
  ]).then(([block, versions]) => ({ block, versions }))

  context.blockCache.set(contentBlockId, request)
  return await request
}

async function resolveAtomicSection(
  atomicSectionId: number,
  context: {
    atomicSectionCache: Map<number, Promise<CmsV2AtomicSectionDto>>
  },
) {
  const cached = context.atomicSectionCache.get(atomicSectionId)
  if (cached) {
    return await cached
  }

  const request = cmsV2Api.getAtomicSection(atomicSectionId)
  context.atomicSectionCache.set(atomicSectionId, request)
  return await request
}

async function resolveAtomicSectionItems(
  atomicSectionId: number,
  context: {
    atomicSectionItemsCache: Map<number, Promise<CmsV2AtomicSectionItemDto[]>>
  },
) {
  const cached = context.atomicSectionItemsCache.get(atomicSectionId)
  if (cached) {
    return await cached
  }

  const request = cmsV2Api.listAtomicSectionItems(atomicSectionId).then(sortByOrder)
  context.atomicSectionItemsCache.set(atomicSectionId, request)
  return await request
}

async function resolveContentBlockRelations(
  contentBlockId: number,
  context: {
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
  },
) {
  const cached = context.relationCache.get(contentBlockId)
  if (cached) {
    return await cached
  }

  const request = cmsV2Api.listContentBlockChildren(contentBlockId).then(sortByOrder)
  context.relationCache.set(contentBlockId, request)
  return await request
}

function hasContentBlockWordDocument(resolvedBlock: ResolvedContentBlock) {
  return Boolean(getDisplayVersion(resolvedBlock.versions, 'FollowLatest'))
}

function resolveContentBlockPreviewState(
  resolvedBlock: ResolvedContentBlock,
  referenceMode: SectionReferenceMode,
  lockedVersionId?: number | null,
): HtmlPreviewState {
  const version = getDisplayVersion(resolvedBlock.versions, referenceMode, lockedVersionId)

  if (!version) {
    return 'empty'
  }

  return version.htmlPreviewPath ? 'ready' : 'empty'
}

async function buildSectionItemNode(
  item: CmsV2SectionItemDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    atomicSectionCache: Map<number, Promise<CmsV2AtomicSectionDto>>
    atomicSectionItemsCache: Map<number, Promise<CmsV2AtomicSectionItemDto[]>>
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
    workspaceNodeMap: Record<string, string>
  },
): Promise<SectionTreeNodeModel> {
  const nodeId = createSectionItemNodeId(item.id)
  context.workspaceNodeMap[nodeId] = nodeId

  if (item.targetType === 'AtomicSection') {
    const atomicSection = await resolveAtomicSection(item.targetId, context)
    const atomicItems = await resolveAtomicSectionItems(item.targetId, context)
    const childNodes = await Promise.all(
      atomicItems.map((atomicItem) => buildAtomicSectionItemNode(atomicItem, context)),
    )

    return {
      id: nodeId,
      title: item.titleOverride || atomicSection.title,
      kind: 'AtomicSection',
      typeLabel: mapAtomicSectionType(atomicSection.type),
      difficulty: mapDifficulty(atomicSection.difficulty),
      status: mapStatus(item.status),
      targetStatus: mapStatus(atomicSection.status),
      itemCount: childNodes.length,
      expanded: true,
      disabled: item.status === 'Archived' || atomicSection.status === 'Archived',
      children: childNodes,
    }
  }

  const resolvedBlock = await resolveContentBlock(item.targetId, context)
  const isComposite = isCompositeContentBlockType(resolvedBlock.block.blockType)
  const relationNodes = isComposite
    ? await Promise.all(
        (await resolveContentBlockRelations(item.targetId, context)).map((relation) =>
          buildContentBlockRelationNode(relation, context),
        ),
      )
    : []

  return {
    id: nodeId,
    title: item.titleOverride || resolvedBlock.block.title,
    kind: isComposite ? 'CompositeBlock' : 'ContentBlock',
    typeLabel: mapContentBlockType(resolvedBlock.block.blockType),
    difficulty: mapDifficulty(resolvedBlock.block.difficulty),
    status: item.referenceMode,
    targetStatus: mapStatus(resolvedBlock.block.status),
    hasWordDocument: hasContentBlockWordDocument(resolvedBlock),
    previewState: resolveContentBlockPreviewState(
      resolvedBlock,
      item.referenceMode,
      item.lockedContentBlockVersionId,
    ),
    itemCount: relationNodes.length || undefined,
    questionCount: isComposite ? countQuestionNodes(relationNodes) : undefined,
    expanded: true,
    disabled: item.status === 'Archived' || resolvedBlock.block.status === 'Archived',
    children: relationNodes.length ? relationNodes : undefined,
  }
}

async function buildAtomicSectionItemNode(
  item: CmsV2AtomicSectionItemDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
    workspaceNodeMap: Record<string, string>
  },
): Promise<SectionTreeNodeModel> {
  const resolvedBlock = await resolveContentBlock(item.contentBlockId, context)
  const nodeId = createAtomicSectionItemNodeId(item.id)
  context.workspaceNodeMap[nodeId] = nodeId
  const isComposite = isCompositeContentBlockType(resolvedBlock.block.blockType)
  const relationNodes = isComposite
    ? await Promise.all(
        (await resolveContentBlockRelations(item.contentBlockId, context)).map((relation) =>
          buildContentBlockRelationNode(relation, context),
        ),
      )
    : []

  return {
    id: nodeId,
    title: item.titleOverride || resolvedBlock.block.title,
    kind: isComposite ? 'CompositeBlock' : 'ContentBlock',
    typeLabel: mapContentBlockType(resolvedBlock.block.blockType),
    difficulty: mapDifficulty(resolvedBlock.block.difficulty),
    status: item.referenceMode,
    targetStatus: mapStatus(resolvedBlock.block.status),
    hasWordDocument: hasContentBlockWordDocument(resolvedBlock),
    previewState: resolveContentBlockPreviewState(
      resolvedBlock,
      item.referenceMode,
      item.lockedContentBlockVersionId,
    ),
    itemCount: relationNodes.length || undefined,
    questionCount: isComposite ? countQuestionNodes(relationNodes) : undefined,
    expanded: isComposite,
    disabled: resolvedBlock.block.status === 'Archived',
    children: relationNodes.length ? relationNodes : undefined,
  }
}

async function buildContentBlockRelationNode(
  relation: CmsV2ContentBlockRelationDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
    workspaceNodeMap: Record<string, string>
  },
): Promise<SectionTreeNodeModel> {
  const resolvedBlock = await resolveContentBlock(relation.childBlockId, context)
  const nodeId = createContentRelationNodeId(relation.id)
  context.workspaceNodeMap[nodeId] = nodeId
  const isComposite = isCompositeContentBlockType(resolvedBlock.block.blockType)
  const relationNodes = isComposite
    ? await Promise.all(
        (await resolveContentBlockRelations(relation.childBlockId, context)).map((childRelation) =>
          buildContentBlockRelationNode(childRelation, context),
        ),
      )
    : []

  return {
    id: nodeId,
    title: relation.titleOverride || resolvedBlock.block.title,
    kind: isComposite ? 'CompositeBlock' : 'ContentBlock',
    typeLabel: mapContentBlockType(resolvedBlock.block.blockType),
    difficulty: mapDifficulty(resolvedBlock.block.difficulty),
    status: relation.referenceMode,
    targetStatus: mapStatus(resolvedBlock.block.status),
    hasWordDocument: hasContentBlockWordDocument(resolvedBlock),
    previewState: resolveContentBlockPreviewState(
      resolvedBlock,
      relation.referenceMode,
      relation.lockedContentBlockVersionId,
    ),
    itemCount: relationNodes.length || undefined,
    questionCount: isComposite ? countQuestionNodes(relationNodes) : undefined,
    expanded: isComposite,
    disabled: resolvedBlock.block.status === 'Archived',
    children: relationNodes.length ? relationNodes : undefined,
  }
}

async function buildSectionFlowItem(
  item: CmsV2SectionItemDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    atomicSectionCache: Map<number, Promise<CmsV2AtomicSectionDto>>
    atomicSectionItemsCache: Map<number, Promise<CmsV2AtomicSectionItemDto[]>>
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
    workspaceNodeMap: Record<string, string>
  },
): Promise<SectionWorkspaceFlowItemModel> {
  const nodeId = createSectionItemNodeId(item.id)

  if (item.targetType === 'AtomicSection') {
    const atomicSection = await resolveAtomicSection(item.targetId, context)
    const atomicItems = await resolveAtomicSectionItems(item.targetId, context)
    const children = await Promise.all(
      atomicItems.map((atomicItem) =>
        buildStructuredBlockChildFromAtomicSectionItem(atomicItem, context),
      ),
    )

    return {
      kind: 'AtomicSection',
      id: nodeId,
      nodeId,
      sectionItemId: item.id,
      targetId: item.targetId,
      sortOrder: item.sortOrder,
      disabled: item.status === 'Archived' || atomicSection.status === 'Archived',
      block: {
        id: nodeId,
        title: item.titleOverride || atomicSection.title,
        blockKind: 'AtomicSection',
        atomicSectionId: item.targetId,
        status: mapStatus(atomicSection.status),
        difficulty: mapDifficulty(atomicSection.difficulty),
        summary: atomicSection.description || '',
        children,
        disabled: item.status === 'Archived' || atomicSection.status === 'Archived',
      },
    }
  }

  const resolvedBlock = await resolveContentBlock(item.targetId, context)

  if (!isCompositeContentBlockType(resolvedBlock.block.blockType)) {
    return {
      kind: 'ContentBlock',
      id: nodeId,
      nodeId,
      sectionItemId: item.id,
      targetId: item.targetId,
      sortOrder: item.sortOrder,
      disabled: item.status === 'Archived' || resolvedBlock.block.status === 'Archived',
      block: await buildContentBlockDisplay(
        nodeId,
        resolvedBlock,
        item.referenceMode,
        item.lockedContentBlockVersionId,
      ),
    }
  }

  const relations = await resolveContentBlockRelations(item.targetId, context)
  const children = await Promise.all(
    relations.map((relation) => buildStructuredBlockChildFromRelation(relation, context)),
  )

  return {
    kind: 'CompositeBlock',
    id: nodeId,
    nodeId,
    sectionItemId: item.id,
    targetId: item.targetId,
    sortOrder: item.sortOrder,
    disabled: item.status === 'Archived' || resolvedBlock.block.status === 'Archived',
      block: {
        id: nodeId,
        title: item.titleOverride || resolvedBlock.block.title,
        blockKind: 'CompositeBlock',
        typeLabel: mapContentBlockType(resolvedBlock.block.blockType),
        contentBlockId: item.targetId,
        selfContent: await buildContentBlockDisplay(
          nodeId,
          resolvedBlock,
          item.referenceMode,
          item.lockedContentBlockVersionId,
          item.titleOverride,
        ),
        status: mapStatus(resolvedBlock.block.status),
        difficulty: mapDifficulty(resolvedBlock.block.difficulty),
        summary: resolvedBlock.block.summary || '',
      children,
      disabled: item.status === 'Archived' || resolvedBlock.block.status === 'Archived',
    },
  }
}

async function buildStructuredBlockChildFromAtomicSectionItem(
  item: CmsV2AtomicSectionItemDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
    workspaceNodeMap: Record<string, string>
  },
): Promise<StructuredBlockChildModel> {
  const nodeId = createAtomicSectionItemNodeId(item.id)
  context.workspaceNodeMap[nodeId] = nodeId

  return await buildStructuredBlockChild(
    nodeId,
    await resolveContentBlock(item.contentBlockId, context),
    item.referenceMode,
    item.lockedContentBlockVersionId,
    item.titleOverride,
    context,
    {
      atomicSectionId: item.atomicSectionId,
      atomicSectionItemId: item.id,
      contentBlockId: item.contentBlockId,
      sortOrder: item.sortOrder,
    },
  )
}

async function buildStructuredBlockChildFromRelation(
  relation: CmsV2ContentBlockRelationDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
    workspaceNodeMap: Record<string, string>
  },
): Promise<StructuredBlockChildModel> {
  const nodeId = createContentRelationNodeId(relation.id)
  context.workspaceNodeMap[nodeId] = nodeId

  return await buildStructuredBlockChild(
    nodeId,
    await resolveContentBlock(relation.childBlockId, context),
    relation.referenceMode,
    relation.lockedContentBlockVersionId,
    relation.titleOverride,
    context,
    {
      parentBlockId: relation.parentBlockId,
      relationId: relation.id,
      contentBlockId: relation.childBlockId,
      sortOrder: relation.sortOrder,
    },
  )
}

async function buildStructuredBlockChild(
  nodeId: string,
  resolvedBlock: ResolvedContentBlock,
  referenceMode: SectionReferenceMode,
  lockedVersionId: number | null | undefined,
  titleOverride: string | null | undefined,
  context: {
    relationCache: Map<number, Promise<CmsV2ContentBlockRelationDto[]>>
    workspaceNodeMap: Record<string, string>
    blockCache: Map<number, Promise<ResolvedContentBlock>>
  },
  childContext: StructuredBlockChildContext = {},
): Promise<StructuredBlockChildModel> {
  const disabled = resolvedBlock.block.status === 'Archived'

  if (!isCompositeContentBlockType(resolvedBlock.block.blockType)) {
    return {
      kind: 'ContentBlock',
      id: nodeId,
      nodeId,
      sortOrder: childContext.sortOrder,
      atomicSectionId: childContext.atomicSectionId,
      atomicSectionItemId: childContext.atomicSectionItemId,
      parentBlockId: childContext.parentBlockId,
      relationId: childContext.relationId,
      contentBlockId: childContext.contentBlockId ?? resolvedBlock.block.id,
      disabled,
      block: await buildContentBlockDisplay(
        nodeId,
        resolvedBlock,
        referenceMode,
        lockedVersionId,
        titleOverride,
      ),
    }
  }

  const relations = await resolveContentBlockRelations(resolvedBlock.block.id, context)
  const children = await Promise.all(
    relations.map((relation) => buildStructuredBlockChildFromRelation(relation, context)),
  )

  return {
    kind: 'CompositeBlock',
    id: nodeId,
    nodeId,
    sortOrder: childContext.sortOrder,
    atomicSectionId: childContext.atomicSectionId,
    atomicSectionItemId: childContext.atomicSectionItemId,
    parentBlockId: childContext.parentBlockId,
    relationId: childContext.relationId,
    contentBlockId: childContext.contentBlockId ?? resolvedBlock.block.id,
    disabled,
    block: {
      id: nodeId,
      title: titleOverride || resolvedBlock.block.title,
      blockKind: 'CompositeBlock',
      typeLabel: mapContentBlockType(resolvedBlock.block.blockType),
      contentBlockId: resolvedBlock.block.id,
      selfContent: await buildContentBlockDisplay(
        nodeId,
        resolvedBlock,
        referenceMode,
        lockedVersionId,
        titleOverride,
      ),
      status: mapStatus(resolvedBlock.block.status),
      difficulty: mapDifficulty(resolvedBlock.block.difficulty),
      summary: resolvedBlock.block.summary || '',
      children,
      disabled,
    },
  }
}

async function buildContentBlockDisplay(
  id: string,
  resolvedBlock: ResolvedContentBlock,
  referenceMode: SectionReferenceMode,
  lockedVersionId?: number | null,
  titleOverride?: string | null,
): Promise<ContentBlockDisplayModel> {
  const version = getDisplayVersion(resolvedBlock.versions, referenceMode, lockedVersionId)
  const htmlPreview = await readHtmlPreview(resolvedBlock.block.id, referenceMode, version?.id)

  return {
    id,
    title: titleOverride || resolvedBlock.block.title,
    role: mapContentBlockType(resolvedBlock.block.blockType),
    blockType: 'ContentBlock',
    difficulty: mapDifficulty(resolvedBlock.block.difficulty),
    status: mapStatus(resolvedBlock.block.status),
    referenceMode,
    versionLabel: version ? `v${version.versionNumber}` : '未设置',
    htmlPreviewState: htmlPreview ? 'ready' : 'empty',
    htmlPreview,
    disabled: resolvedBlock.block.status === 'Archived',
  }
}

function getDisplayVersion(
  versions: CmsV2ContentBlockVersionDto[],
  referenceMode: SectionReferenceMode,
  lockedVersionId?: number | null,
) {
  if (referenceMode === 'LockedVersion' && lockedVersionId) {
    return versions.find((version) => version.id === lockedVersionId)
  }

  return versions.find((version) => version.isCurrent) ?? versions[0]
}

async function readHtmlPreview(
  contentBlockId: number,
  referenceMode: SectionReferenceMode,
  versionId?: number,
) {
  try {
    const html = await cmsV2Api.getContentBlockHtmlPreview(
      contentBlockId,
      referenceMode === 'LockedVersion' ? versionId : undefined,
    )

    return extractBodyHtml(html)
  } catch {
    return null
  }
}

function extractBodyHtml(html: string) {
  const bodyMatch = /<body[^>]*>([\s\S]*?)<\/body>/i.exec(html)
  return (bodyMatch?.[1] ?? html).trim()
}

function countQuestionNodes(nodes: SectionTreeNodeModel[]) {
  return nodes.filter((node) => node.typeLabel.includes('题')).length
}

function sortByOrder<T extends { sortOrder: number; id: number }>(items: T[]) {
  return [...items].sort((left, right) => left.sortOrder - right.sortOrder || left.id - right.id)
}

function createSectionNodeId(sectionId: number) {
  return `section-${sectionId}`
}

function createSectionVariantNodeId(sectionVariantId: number) {
  return `section-variant-${sectionVariantId}`
}

function createSectionItemNodeId(itemId: number) {
  return `section-item-${itemId}`
}

function createAtomicSectionItemNodeId(itemId: number) {
  return `atomic-section-item-${itemId}`
}

function createContentRelationNodeId(relationId: number) {
  return `content-block-relation-${relationId}`
}

function mapDifficulty(value?: string | null) {
  const labels: Record<string, string> = {
    Unset: '未设置',
    Basic: '基础',
    Medium: '中档',
    Advanced: '提高',
    Top: '压轴',
  }

  return value ? labels[value] ?? value : '未设置'
}

function mapStatus(value?: string | null) {
  const labels: Record<string, string> = {
    Active: '可用',
    Archived: '归档',
    Draft: '草稿',
    Resolved: '已解决',
  }

  return value ? labels[value] ?? value : '未设置'
}

function mapAtomicSectionType(value?: string | null) {
  const labels: Record<string, string> = {
    ConceptBuild: '知识点组',
    MethodExplain: '方法讲解',
    ExampleExplain: '例题组',
    MistakeAnalysis: '错因分析',
    ExerciseArrange: '练习组',
    Custom: 'AtomicSection',
  }

  return value ? labels[value] ?? value : 'AtomicSection'
}

function mapContentBlockType(value?: string | null) {
  const labels: Record<string, string> = {
    KnowledgePoint: '知识点',
    Explanation: '说明',
    Question: '题目',
    Answer: '答案',
    Analysis: '解析',
    MethodSummary: '方法总结',
    CommonMistake: '易错点',
    Analogy: '类比说明',
    DiagramNote: '图示说明',
    ExampleGroup: '例题组',
    ExerciseGroup: '练习组',
    VariantGroup: '变式题组',
    GeneralText: '普通文本',
  }

  return value ? labels[value] ?? value : 'ContentBlock'
}
