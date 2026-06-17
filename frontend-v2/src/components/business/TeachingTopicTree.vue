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
}>()

const emit = defineEmits<{
  selectTopic: [id: string]
  nodeContextMenu: [payload: TeachingTopicTreeContextMenuPayload]
}>()

const { t } = useI18n()

const basicNodes = computed(() => props.nodes.map(toBasicTreeNode))

function toBasicTreeNode(node: TeachingTopicTreeNodeModel): BasicTreeNode {
  return {
    id: node.id,
    label: node.title,
    payload: node,
    disabled: node.disabled,
    expanded: node.expanded,
    children: node.children?.map(toBasicTreeNode),
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
</script>

<template>
  <section class="space-y-2" :aria-label="t('components.teachingTopicTree.title')">
    <div class="flex items-center justify-between gap-2">
      <div class="min-w-0">
        <h2 class="truncate text-sm font-medium">{{ t('components.teachingTopicTree.title') }}</h2>
        <p class="truncate text-xs text-muted-foreground">
          {{ t('components.teachingTopicTree.description') }}
        </p>
      </div>
      <span class="shrink-0 text-xs text-muted-foreground">
        {{ t('components.teachingTopicTree.nodeCount', { count: nodes.length }) }}
      </span>
    </div>

    <BasicTree
      v-if="basicNodes.length"
      :nodes="basicNodes"
      :selected-node-id="selectedTopicId"
      :context-target-node-id="contextTargetTopicId"
      :expand-label="t('components.basicTree.expand')"
      :collapse-label="t('components.basicTree.collapse')"
      @select="emit('selectTopic', $event)"
      @node-context-menu="handleNodeContextMenu"
    >
      <template #default="{ node }">
        <TeachingTopicTreeNode :node="getTeachingTopicNode(node)" />
      </template>
    </BasicTree>

    <EmptyState
      v-else
      :title="t('components.teachingTopicTree.emptyTitle')"
      :description="t('components.teachingTopicTree.emptyDescription')"
    />
  </section>
</template>
