<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import BasicTree from '@/components/business/BasicTree.vue'
import BasicTreeNodeView from '@/components/presentation/BasicTreeNodeView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import { Button } from '@/components/ui/button'
import type { BasicTreeNode, HandoutOverviewNodeModel } from '@/types'

const props = defineProps<{
  nodes: HandoutOverviewNodeModel[]
  currentHandoutVersionId?: number | null
  loading?: boolean
  error?: string
}>()

const emit = defineEmits<{
  close: []
  openVersion: [handoutVersionId: number]
  openManagement: []
}>()

const { t } = useI18n()

const selectedNodeId = computed(() =>
  props.currentHandoutVersionId ? `handout-version:${props.currentHandoutVersionId}` : undefined,
)
const basicNodes = computed(() => props.nodes.map(toBasicTreeNode))

function toBasicTreeNode(node: HandoutOverviewNodeModel): BasicTreeNode {
  return {
    id: node.id,
    label: node.title,
    meta: node.status,
    payload: node,
    expanded: Boolean(node.children?.length) && node.expanded !== false,
    children: node.children?.map(toBasicTreeNode),
  }
}

function getOverviewNode(node: BasicTreeNode) {
  return node.payload as HandoutOverviewNodeModel
}

function handleSelect(nodeId: string) {
  const node = findOverviewNode(props.nodes, nodeId)
  if (node?.kind === 'HandoutVersion' && node.handoutVersionId) {
    emit('openVersion', node.handoutVersionId)
  }
}

function findOverviewNode(
  nodes: HandoutOverviewNodeModel[],
  nodeId: string,
): HandoutOverviewNodeModel | undefined {
  for (const node of nodes) {
    if (node.id === nodeId) {
      return node
    }

    const child = node.children ? findOverviewNode(node.children, nodeId) : undefined
    if (child) {
      return child
    }
  }

  return undefined
}
</script>

<template>
  <div class="fixed inset-0 z-40" role="dialog" aria-modal="true" :aria-label="t('handoutOverview.dialogLabel')">
    <button
      type="button"
      class="absolute inset-0 bg-background/70 backdrop-blur-sm"
      :aria-label="t('handoutOverview.close')"
      @click="emit('close')"
    />

    <aside
      class="relative z-10 m-3 max-h-[calc(100vh-1.5rem)] w-max max-w-[calc(100vw-1.5rem)] overflow-auto rounded-lg border bg-card p-3 text-card-foreground"
    >
      <header class="mb-3 flex min-w-80 items-start justify-between gap-3 border-b pb-3">
        <div class="min-w-0">
          <h2 class="truncate text-sm font-semibold">{{ t('handoutOverview.title') }}</h2>
          <p class="mt-1 max-w-[44rem] truncate text-xs text-muted-foreground">
            {{ t('handoutOverview.description') }}
          </p>
        </div>
        <div class="flex shrink-0 items-center gap-2">
          <Button type="button" size="sm" variant="outline" @click="emit('openManagement')">
            {{ t('handoutOverview.openManagement') }}
          </Button>
          <Button type="button" size="sm" variant="ghost" @click="emit('close')">
            {{ t('handoutOverview.close') }}
          </Button>
        </div>
      </header>

      <EmptyState
        v-if="loading"
        :title="t('handoutOverview.loadingTitle')"
        :description="t('handoutOverview.loadingDescription')"
      />

      <EmptyState
        v-else-if="error"
        :title="t('handoutOverview.errorTitle')"
        :description="error"
      />

      <BasicTree
        v-else-if="basicNodes.length"
        :nodes="basicNodes"
        :selected-node-id="selectedNodeId"
        :expand-label="t('components.basicTree.expand')"
        :collapse-label="t('components.basicTree.collapse')"
        @select="handleSelect"
      >
        <template #default="{ node }">
          <BasicTreeNodeView
            :title="getOverviewNode(node).title"
            :marker-label="getOverviewNode(node).kind"
            marker-class="bg-muted-foreground"
            :meta-items="[getOverviewNode(node).status || '']"
          />
        </template>
      </BasicTree>

      <EmptyState
        v-else
        :title="t('handoutOverview.emptyTitle')"
        :description="t('handoutOverview.emptyDescription')"
      />
    </aside>
  </div>
</template>
