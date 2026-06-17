<script setup lang="ts">
import { ListTree } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import SectionTree from '@/components/business/SectionTree.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import { Card } from '@/components/ui/card'
import type { SectionTreeContextMenuPayload, SectionTreeNodeModel } from '@/types'

const { t } = useI18n()

withDefaults(
  defineProps<{
    nodes?: SectionTreeNodeModel[]
    selectedNodeId?: string
    contextTargetNodeId?: string
  }>(),
  {
    nodes: () => [],
  },
)

defineEmits<{
  selectNode: [id: string]
  nodeContextMenu: [payload: SectionTreeContextMenuPayload]
}>()
</script>

<template>
  <Card class="flex h-full min-h-0 flex-col overflow-hidden">
    <WeakScrollArea class="p-4">
      <SectionTree
        v-if="nodes.length"
        :nodes="nodes"
        :selected-node-id="selectedNodeId"
        :context-target-node-id="contextTargetNodeId"
        @select-node="$emit('selectNode', $event)"
        @node-context-menu="$emit('nodeContextMenu', $event)"
      />
      <EmptyState
        v-else
        :title="t('sectionPage.structure.emptyTitle')"
        :description="t('sectionPage.structure.emptyDescription')"
      >
        <template #icon>
          <ListTree class="size-5" aria-hidden="true" />
        </template>
      </EmptyState>
    </WeakScrollArea>
  </Card>
</template>
