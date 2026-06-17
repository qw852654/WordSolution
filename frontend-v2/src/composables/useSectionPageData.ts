import { cmsV2Api } from '@/apis/cmsV2Client'
import type {
  CmsV2AtomicSectionDto,
  CmsV2AtomicSectionItemDto,
  CmsV2ContentBlockDto,
  CmsV2ContentBlockRelationDto,
  CmsV2ContentBlockVersionDto,
  CmsV2SectionDto,
  CmsV2SectionItemDto,
  CmsV2TeachingTopicDto,
} from '@/apis/cmsV2Client'
import type {
  ContentBlockDisplayModel,
  SectionPageShellModel,
  SectionReferenceMode,
  SectionTreeNodeModel,
  SectionWorkspaceFlowItemModel,
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

const groupContentBlockTypes = new Set(['ExampleGroup', 'ExerciseGroup', 'VariantGroup'])

export async function loadSectionPageData(routeSectionId?: string): Promise<SectionPageDataModel> {
  const [sections, topics] = await Promise.all([
    cmsV2Api.listSections(),
    cmsV2Api.listTeachingTopics(),
  ])

  const section = await resolveSection(routeSectionId, sections)
  const topicById = new Map(topics.map((topic) => [topic.id, topic]))
  const sectionItems = await cmsV2Api.listSectionItems(section.id)
  const sortedSectionItems = sortByOrder(sectionItems).filter((item) => !item.parentItemId)

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
  const flowItems = await Promise.all(
    sortedSectionItems.map((item) => buildSectionFlowItem(item, context)),
  )

  const rootNodeId = createSectionNodeId(section.id)
  const treeNodes: SectionTreeNodeModel[] = [
    {
      id: rootNodeId,
      title: section.title,
      kind: 'Section',
      typeLabel: 'Section',
      difficulty: mapDifficulty(section.difficulty),
      status: mapStatus(section.status),
      itemCount: sectionChildren.length,
      expanded: true,
      disabled: section.status === 'Archived',
      children: sectionChildren,
    },
  ]

  return {
    section: {
      sectionId: String(section.id),
      title: section.title,
      teachingTopicTitle: topicById.get(section.teachingTopicId)?.name ?? 'TeachingTopic',
      status: mapStatus(section.status),
    },
    treeNodes,
    flowItems,
    workspaceNodeMap,
    teachingTopicNodes: buildTeachingTopicTree(topics, sections),
    selectedTeachingTopicId: createTeachingTopicNodeId(section.teachingTopicId),
    defaultSelectedNodeId: sectionChildren[0]?.id ?? rootNodeId,
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
      itemCount: childNodes.length,
      expanded: true,
      disabled: item.status === 'Archived' || atomicSection.status === 'Archived',
      children: childNodes,
    }
  }

  const resolvedBlock = await resolveContentBlock(item.targetId, context)
  const isComposite = groupContentBlockTypes.has(resolvedBlock.block.blockType)
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
    workspaceNodeMap: Record<string, string>
  },
): Promise<SectionTreeNodeModel> {
  const resolvedBlock = await resolveContentBlock(item.contentBlockId, context)
  const nodeId = createAtomicSectionItemNodeId(item.id)
  context.workspaceNodeMap[nodeId] = nodeId

  return {
    id: nodeId,
    title: item.titleOverride || resolvedBlock.block.title,
    kind: 'ContentBlock',
    typeLabel: mapContentBlockType(resolvedBlock.block.blockType),
    difficulty: mapDifficulty(resolvedBlock.block.difficulty),
    status: item.referenceMode,
    disabled: resolvedBlock.block.status === 'Archived',
  }
}

async function buildContentBlockRelationNode(
  relation: CmsV2ContentBlockRelationDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    workspaceNodeMap: Record<string, string>
  },
): Promise<SectionTreeNodeModel> {
  const resolvedBlock = await resolveContentBlock(relation.childBlockId, context)
  const nodeId = createContentRelationNodeId(relation.id)
  context.workspaceNodeMap[nodeId] = nodeId

  return {
    id: nodeId,
    title: relation.titleOverride || resolvedBlock.block.title,
    kind: 'ContentBlock',
    typeLabel: mapContentBlockType(resolvedBlock.block.blockType),
    difficulty: mapDifficulty(resolvedBlock.block.difficulty),
    status: relation.referenceMode,
    disabled: resolvedBlock.block.status === 'Archived',
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
        buildContentBlockDisplayFromAtomicSectionItem(atomicItem, context),
      ),
    )

    return {
      kind: 'AtomicSection',
      id: nodeId,
      nodeId,
      sortOrder: item.sortOrder,
      disabled: item.status === 'Archived' || atomicSection.status === 'Archived',
      block: {
        id: nodeId,
        title: item.titleOverride || atomicSection.title,
        blockKind: 'AtomicSection',
        status: mapStatus(atomicSection.status),
        difficulty: mapDifficulty(atomicSection.difficulty),
        summary: atomicSection.description || '',
        children,
        disabled: item.status === 'Archived' || atomicSection.status === 'Archived',
      },
    }
  }

  const resolvedBlock = await resolveContentBlock(item.targetId, context)

  if (!groupContentBlockTypes.has(resolvedBlock.block.blockType)) {
    return {
      kind: 'ContentBlock',
      id: nodeId,
      nodeId,
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
    relations.map((relation) => buildContentBlockDisplayFromRelation(relation, context)),
  )

  return {
    kind: 'CompositeBlock',
    id: nodeId,
    nodeId,
    sortOrder: item.sortOrder,
    disabled: item.status === 'Archived' || resolvedBlock.block.status === 'Archived',
    block: {
      id: nodeId,
      title: item.titleOverride || resolvedBlock.block.title,
      blockKind: 'CompositeBlock',
      status: mapStatus(resolvedBlock.block.status),
      difficulty: mapDifficulty(resolvedBlock.block.difficulty),
      summary: resolvedBlock.block.summary || '',
      children,
      disabled: item.status === 'Archived' || resolvedBlock.block.status === 'Archived',
    },
  }
}

async function buildContentBlockDisplayFromAtomicSectionItem(
  item: CmsV2AtomicSectionItemDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    workspaceNodeMap: Record<string, string>
  },
) {
  const nodeId = createAtomicSectionItemNodeId(item.id)
  context.workspaceNodeMap[nodeId] = nodeId
  return await buildContentBlockDisplay(
    nodeId,
    await resolveContentBlock(item.contentBlockId, context),
    item.referenceMode,
    item.lockedContentBlockVersionId,
  )
}

async function buildContentBlockDisplayFromRelation(
  relation: CmsV2ContentBlockRelationDto,
  context: {
    blockCache: Map<number, Promise<ResolvedContentBlock>>
    workspaceNodeMap: Record<string, string>
  },
) {
  const nodeId = createContentRelationNodeId(relation.id)
  context.workspaceNodeMap[nodeId] = nodeId
  return await buildContentBlockDisplay(
    nodeId,
    await resolveContentBlock(relation.childBlockId, context),
    relation.referenceMode,
    relation.lockedContentBlockVersionId,
  )
}

async function buildContentBlockDisplay(
  id: string,
  resolvedBlock: ResolvedContentBlock,
  referenceMode: SectionReferenceMode,
  lockedVersionId?: number | null,
): Promise<ContentBlockDisplayModel> {
  const version = getDisplayVersion(resolvedBlock.versions, referenceMode, lockedVersionId)
  const htmlPreview = await readHtmlPreview(resolvedBlock.block.id, referenceMode, version?.id)

  return {
    id,
    title: resolvedBlock.block.title,
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

function buildTeachingTopicTree(
  topics: CmsV2TeachingTopicDto[],
  sections: CmsV2SectionDto[],
): TeachingTopicTreeNodeModel[] {
  const childrenByParent = new Map<number | null, CmsV2TeachingTopicDto[]>()
  const sectionCountByTopic = new Map<number, number>()

  for (const section of sections) {
    sectionCountByTopic.set(section.teachingTopicId, (sectionCountByTopic.get(section.teachingTopicId) ?? 0) + 1)
  }

  for (const topic of topics) {
    const parentId = topic.parentId ?? null
    const siblings = childrenByParent.get(parentId) ?? []
    siblings.push(topic)
    childrenByParent.set(parentId, siblings)
  }

  function build(parentId: number | null): TeachingTopicTreeNodeModel[] {
    return sortByOrder(childrenByParent.get(parentId) ?? []).map((topic) => ({
      id: createTeachingTopicNodeId(topic.id),
      title: topic.name,
      status: mapStatus(topic.status),
      sectionCount: sectionCountByTopic.get(topic.id) ?? 0,
      archived: topic.status === 'Archived',
      disabled: topic.status === 'Archived',
      expanded: true,
      children: build(topic.id),
    }))
  }

  return build(null)
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

function createSectionItemNodeId(itemId: number) {
  return `section-item-${itemId}`
}

function createAtomicSectionItemNodeId(itemId: number) {
  return `atomic-section-item-${itemId}`
}

function createContentRelationNodeId(relationId: number) {
  return `content-block-relation-${relationId}`
}

function createTeachingTopicNodeId(topicId: number) {
  return `topic-${topicId}`
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
