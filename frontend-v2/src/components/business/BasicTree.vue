<script setup lang="ts">
import { computed, ref } from 'vue'
import { ChevronDown, ChevronRight } from 'lucide-vue-next'
import type { BasicTreeNode } from '@/types'

const props = defineProps<{
  nodes: BasicTreeNode[]
  selectedNodeId?: string
  expandLabel: string
  collapseLabel: string
}>()

defineEmits<{
  select: [id: string]
}>()

defineSlots<{
  default?: (props: {
    node: BasicTreeNode
    level: number
    hasChildren: boolean
    expanded: boolean
    selected: boolean
  }) => unknown
}>()

interface VisibleTreeNode {
  node: BasicTreeNode
  level: number
  hasChildren: boolean
}

const expandedNodeIds = ref(new Set(
  props.nodes
    .filter((node) => node.expanded)
    .map((node) => node.id),
))

const visibleNodes = computed(() => {
  const rows: VisibleTreeNode[] = []

  function append(nodes: BasicTreeNode[], level: number) {
    for (const node of nodes) {
      const hasChildren = Boolean(node.children?.length)
      rows.push({ node, level, hasChildren })

      if (hasChildren && expandedNodeIds.value.has(node.id)) {
        append(node.children ?? [], level + 1)
      }
    }
  }

  append(props.nodes, 1)
  return rows
})

function toggleNode(nodeId: string) {
  const next = new Set(expandedNodeIds.value)

  if (next.has(nodeId)) {
    next.delete(nodeId)
  } else {
    next.add(nodeId)
  }

  expandedNodeIds.value = next
}
</script>

<template>
  <div role="tree" class="rounded-lg border bg-card p-2 text-sm">
    <div
      v-for="{ node, level, hasChildren } in visibleNodes"
      :key="node.id"
      role="treeitem"
      :aria-level="level"
      :aria-selected="selectedNodeId === node.id"
      :aria-expanded="hasChildren ? expandedNodeIds.has(node.id) : undefined"
      class="flex min-h-8 items-center gap-1 rounded-md"
      :style="{ paddingLeft: `${(level - 1) * 16}px` }"
    >
      <button
        v-if="hasChildren"
        type="button"
        class="inline-flex h-7 w-4 shrink-0 items-center justify-center rounded-md text-muted-foreground hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
        :aria-label="expandedNodeIds.has(node.id) ? collapseLabel : expandLabel"
        @click="toggleNode(node.id)"
      >
        <ChevronDown v-if="expandedNodeIds.has(node.id)" class="size-3.5" aria-hidden="true" />
        <ChevronRight v-else class="size-3.5" aria-hidden="true" />
      </button>
      <span v-else class="h-7 w-4 shrink-0" aria-hidden="true" />

      <button
        type="button"
        class="flex min-w-0 flex-1 items-center justify-between gap-2 rounded-md px-2 py-1.5 text-left hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30 disabled:pointer-events-none disabled:opacity-60"
        :class="selectedNodeId === node.id ? 'bg-muted text-foreground' : 'text-muted-foreground'"
        :disabled="node.disabled"
        @click="$emit('select', node.id)"
      >
        <slot
          :node="node"
          :level="level"
          :has-children="hasChildren"
          :expanded="expandedNodeIds.has(node.id)"
          :selected="selectedNodeId === node.id"
        >
          <span class="min-w-0 truncate font-medium text-foreground">{{ node.label }}</span>
          <span v-if="node.meta" class="shrink-0 text-xs text-muted-foreground">{{ node.meta }}</span>
        </slot>
      </button>
    </div>
  </div>
</template>
