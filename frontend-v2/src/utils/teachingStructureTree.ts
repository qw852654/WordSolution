import type { CmsV2TeachingStructureNodeDto } from '@/apis/cmsV2Client'
import type { TeachingTopicTreeNodeModel } from '@/types'

export function mapTeachingStructureNodesToTreeNodes(
  nodes: CmsV2TeachingStructureNodeDto[],
): TeachingTopicTreeNodeModel[] {
  return nodes.map(mapTeachingStructureNodeToTreeNode)
}

export function mapTeachingStructureNodeToTreeNode(
  node: CmsV2TeachingStructureNodeDto,
): TeachingTopicTreeNodeModel {
  const topic = node.teachingTopic
  const topicChildren = node.children.map(mapTeachingStructureNodeToTreeNode)
  const variantChildren = node.sectionVariants.map((variant) => ({
    id: `section-variant-${variant.id}`,
    kind: 'SectionVariant' as const,
    title: variant.title,
    sectionId: variant.sectionId,
    sectionVariantId: variant.id,
    status: variant.status,
    readOnly: true,
    disabled: variant.status === 'Archived',
  }))

  return {
    id: createTeachingTopicNodeId(topic.id),
    kind: 'TeachingTopic',
    title: topic.name,
    teachingTopicId: topic.id,
    sectionId: node.section?.id,
    sectionTitle: node.section?.title,
    variantCount: node.sectionVariants.length,
    status: node.section?.status ?? topic.status,
    sectionCount: node.section ? 1 : undefined,
    archived: topic.status === 'Archived',
    disabled: topic.status === 'Archived',
    expanded: topic.parentId === null || Boolean(node.section),
    isEmptyTopic: node.isEmptyTopic,
    canSetDisplayRoot: node.canSetDisplayRoot,
    canDelete: node.canDelete,
    children: [...topicChildren, ...variantChildren],
  }
}

export function createTeachingTopicNodeId(topicId: number) {
  return `topic-${topicId}`
}

export function findTeachingTopicTreeNodePath(
  nodes: TeachingTopicTreeNodeModel[],
  nodeId: string,
): TeachingTopicTreeNodeModel[] {
  for (const node of nodes) {
    if (node.id === nodeId) {
      return [node]
    }

    const childPath = node.children ? findTeachingTopicTreeNodePath(node.children, nodeId) : []
    if (childPath.length) {
      return [node, ...childPath]
    }
  }

  return []
}

export function findTeachingStructureTopicTitle(
  nodes: CmsV2TeachingStructureNodeDto[],
  teachingTopicId: number,
): string | undefined {
  for (const node of nodes) {
    if (node.teachingTopic.id === teachingTopicId) {
      return node.teachingTopic.name
    }

    const childMatch = findTeachingStructureTopicTitle(node.children, teachingTopicId)
    if (childMatch) {
      return childMatch
    }
  }

  return undefined
}
