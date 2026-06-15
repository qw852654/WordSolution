<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import FocusTree from '@/components/business/FocusTree.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import type { FocusTreeNode, SectionTreeNodeModel } from '@/types'

const props = defineProps<{
  nodes: SectionTreeNodeModel[]
  selectedNodeId?: string
}>()

defineEmits<{
  selectNode: [id: string]
}>()

const { t } = useI18n()

const focusNodes = computed(() => props.nodes.map(toFocusTreeNode))

function toFocusTreeNode(node: SectionTreeNodeModel): FocusTreeNode {
  return {
    id: node.id,
    label: node.title,
    meta: buildMeta(node),
    disabled: node.disabled,
    expanded: node.expanded,
    children: node.children?.map(toFocusTreeNode),
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

    <FocusTree
      v-if="focusNodes.length"
      :nodes="focusNodes"
      :selected-node-id="selectedNodeId"
      :expand-label="t('components.focusTree.expand')"
      :collapse-label="t('components.focusTree.collapse')"
      @select="$emit('selectNode', $event)"
    />

    <EmptyState
      v-else
      :title="t('components.sectionTree.emptyTitle')"
      :description="t('components.sectionTree.emptyDescription')"
    />
  </section>
</template>
