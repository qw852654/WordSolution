<script setup lang="ts">
import { nextTick, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import HandoutVersionItemView from '@/components/business/HandoutVersionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import type { HandoutWorkspaceItemModel } from '@/types'

const props = defineProps<{
  items: HandoutWorkspaceItemModel[]
  readOnly?: boolean
  selectedNodeId?: string
  scrollTargetNodeId?: string
  scrollRequestKey?: number
}>()

defineEmits<{
  selectItem: [id: string]
  moveUp: [id: string]
  moveDown: [id: string]
  editOccurrence: [id: string]
  remove: [id: string]
  addInitialContent: []
}>()

const { t } = useI18n()
const workspaceRoot = ref<HTMLElement | null>(null)

watch(
  () => [props.scrollRequestKey, props.scrollTargetNodeId] as const,
  async ([, nodeId]) => {
    if (!nodeId) {
      return
    }

    await nextTick()
    scrollNodeToTop(nodeId)
  },
)

function scrollNodeToTop(nodeId: string) {
  const root = workspaceRoot.value
  if (!root) {
    return
  }

  const scroller = root.querySelector<HTMLElement>('.weak-scroll-area')
  const target = Array.from(root.querySelectorAll<HTMLElement>('[data-handout-node-id]')).find(
    (element) => element.dataset.handoutNodeId === nodeId,
  )

  if (!scroller || !target) {
    return
  }

  const scrollerRect = scroller.getBoundingClientRect()
  const targetRect = target.getBoundingClientRect()
  scroller.scrollTo({
    top: scroller.scrollTop + targetRect.top - scrollerRect.top,
    behavior: 'smooth',
  })
}
</script>

<template>
  <section ref="workspaceRoot" class="flex min-h-0 flex-col rounded-lg border bg-background">
    <header class="flex items-center justify-between gap-3 border-b px-4 py-3">
      <div class="min-w-0">
        <h2 class="truncate text-sm font-medium">
          {{ t('components.handoutWorkspace.title') }}
        </h2>
        <p class="truncate text-xs text-muted-foreground">
          {{ t('components.handoutWorkspace.description') }}
        </p>
      </div>
      <span class="shrink-0 text-xs text-muted-foreground">
        {{ t('components.handoutWorkspace.itemCount', { count: items.length }) }}
      </span>
    </header>

    <WeakScrollArea class="min-h-0 flex-1">
      <div v-if="items.length" class="space-y-3 p-4">
        <HandoutVersionItemView
          v-for="item in items"
          :key="item.id"
          :item="item"
          :read-only="readOnly"
          :selected-node-id="selectedNodeId"
          @select="$emit('selectItem', $event)"
          @move-up="$emit('moveUp', $event)"
          @move-down="$emit('moveDown', $event)"
          @edit-occurrence="$emit('editOccurrence', $event)"
          @remove="$emit('remove', $event)"
        />
      </div>

      <div v-else class="p-4">
        <EmptyState
          :title="t('components.handoutWorkspace.emptyTitle')"
          :description="t('components.handoutWorkspace.emptyDescription')"
          :action-label="readOnly ? undefined : t('components.handoutWorkspace.addInitialContent')"
          @action="$emit('addInitialContent')"
        />
      </div>
    </WeakScrollArea>
  </section>
</template>
