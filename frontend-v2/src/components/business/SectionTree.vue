<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import BasicTree from '@/components/business/BasicTree.vue'
import SectionTreeNode from '@/components/business/SectionTreeNode.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import type { BasicTreeNode, SectionTreeNodeModel } from '@/types'

const props = defineProps<{
  nodes: SectionTreeNodeModel[]
  selectedNodeId?: string
}>()

defineEmits<{
  selectNode: [id: string]
}>()

const { t } = useI18n()

const basicNodes = computed(() => props.nodes.map(toBasicTreeNode))

function toBasicTreeNode(node: SectionTreeNodeModel): BasicTreeNode {
  return {
    id: node.id,
    label: node.title,
    meta: buildMeta(node),
    payload: node,
    disabled: node.disabled,
    expanded: node.expanded,
    children: node.children?.map(toBasicTreeNode),
  }
}

function buildMeta(node: SectionTreeNodeModel) {
  const metaParts = [
    t(`components.sectionTree.kind.${node.kind}`),
    node.status,
    typeof node.itemCount === 'number'
      ? t('components.sectionTree.itemCount', { count: node.itemCount })
      : undefined,
  ]

  return metaParts.filter(Boolean).join(' · ')
}

function getSectionNode(node: BasicTreeNode) {
  return node.payload as SectionTreeNodeModel
}
</script>

<template>
  <section class="space-y-2" :aria-label="t('components.sectionTree.title')">
    <div class="flex items-center justify-between gap-2">
      <div class="min-w-0">
        <h2 class="truncate text-sm font-medium">{{ t('components.sectionTree.title') }}</h2>
        <p class="truncate text-xs text-muted-foreground">
          {{ t('components.sectionTree.description') }}
        </p>
      </div>
      <span class="shrink-0 text-xs text-muted-foreground">
        {{ t('components.sectionTree.nodeCount', { count: nodes.length }) }}
      </span>
    </div>

    <BasicTree
      v-if="basicNodes.length"
      :nodes="basicNodes"
      :selected-node-id="selectedNodeId"
      :expand-label="t('components.basicTree.expand')"
      :collapse-label="t('components.basicTree.collapse')"
      @select="$emit('selectNode', $event)"
    >
      <template #default="{ node, selected }">
        <SectionTreeNode :node="getSectionNode(node)" :selected="selected" />
      </template>
    </BasicTree>

    <EmptyState
      v-else
      :title="t('components.sectionTree.emptyTitle')"
      :description="t('components.sectionTree.emptyDescription')"
    />
  </section>
</template>
