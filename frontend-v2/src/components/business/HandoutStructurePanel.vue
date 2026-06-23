<script setup lang="ts">
import { computed } from 'vue'
import { Plus } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import BasicTree from '@/components/business/BasicTree.vue'
import BasicTreeNodeView from '@/components/presentation/BasicTreeNodeView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import { Button } from '@/components/ui/button'
import type {
  BasicTreeContextMenuPayload,
  BasicTreeNode,
  HandoutTreeContextMenuPayload,
  HandoutTreeNodeModel,
} from '@/types'

const props = defineProps<{
  nodes: HandoutTreeNodeModel[]
  selectedNodeId?: string
  contextTargetNodeId?: string
  readOnly?: boolean
}>()

const emit = defineEmits<{
  selectNode: [id: string]
  addToEnd: []
  nodeContextMenu: [payload: HandoutTreeContextMenuPayload]
}>()

const { t } = useI18n()

const basicNodes = computed(() => props.nodes.map(toBasicTreeNode))

function toBasicTreeNode(node: HandoutTreeNodeModel): BasicTreeNode {
  return {
    id: node.id,
    label: node.title,
    meta: node.metaItems?.join(' · ') ?? t(`components.handoutStructure.kind.${node.kind}`),
    payload: node,
    disabled: node.disabled,
    expanded: node.expanded,
    children: node.children?.map(toBasicTreeNode),
  }
}

function getHandoutNode(node: BasicTreeNode) {
  return node.payload as HandoutTreeNodeModel
}

function handleContextMenu(payload: BasicTreeContextMenuPayload) {
  const node = payload.node.payload as HandoutTreeNodeModel | undefined
  if (!node) {
    return
  }

  emit('nodeContextMenu', {
    node,
    x: payload.x,
    y: payload.y,
  })
}
</script>

<template>
  <section class="space-y-3" :aria-label="t('components.handoutStructure.title')">
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0">
        <h2 class="truncate text-sm font-medium">
          {{ t('components.handoutStructure.title') }}
        </h2>
        <p class="truncate text-xs text-muted-foreground">
          {{ t('components.handoutStructure.description') }}
        </p>
      </div>
      <Button
        v-if="!readOnly"
        type="button"
        size="sm"
        variant="outline"
        @click="$emit('addToEnd')"
      >
        <Plus class="size-4" />
        {{ t('components.handoutStructure.addToEnd') }}
      </Button>
    </div>

    <BasicTree
      v-if="basicNodes.length"
      :nodes="basicNodes"
      :selected-node-id="selectedNodeId"
      :context-target-node-id="contextTargetNodeId"
      :expand-label="t('components.basicTree.expand')"
      :collapse-label="t('components.basicTree.collapse')"
      @select="emit('selectNode', $event)"
      @node-context-menu="handleContextMenu"
    >
      <template #default="{ node }">
        <BasicTreeNodeView
          :title="getHandoutNode(node).title"
          :marker-label="t(`components.handoutStructure.kind.${getHandoutNode(node).kind}`)"
          marker-class="bg-muted-foreground"
          :meta-items="getHandoutNode(node).metaItems"
        />
      </template>
    </BasicTree>

    <EmptyState
      v-else
      :title="t('components.handoutStructure.emptyTitle')"
      :description="t('components.handoutStructure.emptyDescription')"
    />
  </section>
</template>
