<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import HandoutVersionItemView from '@/components/business/HandoutVersionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import type { HandoutWorkspaceItemModel } from '@/types'

defineProps<{
  items: HandoutWorkspaceItemModel[]
  readOnly?: boolean
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
</script>

<template>
  <section class="flex min-h-0 flex-col rounded-lg border bg-background">
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
