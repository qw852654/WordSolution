<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import BasicTree from '@/components/business/BasicTree.vue'
import TeachingTopicTreeNode from '@/components/business/TeachingTopicTreeNode.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import type {
  BasicTreeContextMenuPayload,
  BasicTreeNode,
  TeachingTopicTreeContextMenuPayload,
  TeachingTopicTreeNodeModel,
} from '@/types'

const props = defineProps<{
  nodes: TeachingTopicTreeNodeModel[]
  selectedTopicId?: string
  contextTargetTopicId?: string
  fullWidthContent?: boolean
  defaultExpandedDepth?: number
}>()

const emit = defineEmits<{
  selectTopic: [id: string]
  openSection: [sectionId: number]
  nodeContextMenu: [payload: TeachingTopicTreeContextMenuPayload]
}>()

const { t } = useI18n()

const defaultExpandedDepth = computed(() => props.defaultExpandedDepth ?? 1)

const basicNodes = computed(() => props.nodes.map((node) => toBasicTreeNode(node, 1)))

const treeResetKey = computed(() =>
  [
    defaultExpandedDepth.value,
    ...props.nodes.map((node) => node.id),
  ].join(':'),
)

function toBasicTreeNode(node: TeachingTopicTreeNodeModel, level: number): BasicTreeNode {
  const children = node.children?.map((child) => toBasicTreeNode(child, level + 1))

  return {
    id: node.id,
    label: node.title,
    payload: node,
    disabled: node.disabled,
    expanded: Boolean(children?.length) && level <= defaultExpandedDepth.value,
    children,
  }
}

function getTeachingTopicNode(node: BasicTreeNode) {
  return node.payload as TeachingTopicTreeNodeModel
}

function handleNodeContextMenu(payload: BasicTreeContextMenuPayload) {
  const topicNode = payload.node.payload as TeachingTopicTreeNodeModel | undefined

  if (!topicNode) {
    return
  }

  emit('nodeContextMenu', {
    node: topicNode,
    x: payload.x,
    y: payload.y,
  })
}

function handleNodeDoubleClick(node: BasicTreeNode) {
  const topicNode = getTeachingTopicNode(node)

  if (topicNode.kind !== 'TeachingTopic' || !topicNode.sectionId) {
    return
  }

  emit('openSection', topicNode.sectionId)
}
</script>

<template>
  <section
    class="space-y-2"
    :class="fullWidthContent ? 'w-max min-w-80' : ''"
    :aria-label="t('components.teachingTopicTree.title')"
  >
    <div class="flex items-center justify-between gap-2">
      <div class="min-w-0">
        <h2
          class="text-sm font-medium"
          :class="fullWidthContent ? 'whitespace-nowrap' : 'truncate'"
        >
          {{ t('components.teachingTopicTree.title') }}
        </h2>
        <p
          class="text-xs text-muted-foreground"
          :class="fullWidthContent ? 'whitespace-nowrap' : 'truncate'"
        >
          {{ t('components.teachingTopicTree.description') }}
        </p>
      </div>
      <span class="shrink-0 text-xs text-muted-foreground">
        {{ t('components.teachingTopicTree.nodeCount', { count: nodes.length }) }}
      </span>
    </div>

    <BasicTree
      v-if="basicNodes.length"
      :key="treeResetKey"
      :nodes="basicNodes"
      :selected-node-id="selectedTopicId"
      :context-target-node-id="contextTargetTopicId"
      :expand-label="t('components.basicTree.expand')"
      :collapse-label="t('components.basicTree.collapse')"
      @select="emit('selectTopic', $event)"
      @node-double-click="handleNodeDoubleClick"
      @node-context-menu="handleNodeContextMenu"
    >
      <template #default="{ node }">
        <TeachingTopicTreeNode
          :node="getTeachingTopicNode(node)"
          :truncate-title="!fullWidthContent"
        />
      </template>
    </BasicTree>

    <EmptyState
      v-else
      :title="t('components.teachingTopicTree.emptyTitle')"
      :description="t('components.teachingTopicTree.emptyDescription')"
    />
  </section>
</template>
