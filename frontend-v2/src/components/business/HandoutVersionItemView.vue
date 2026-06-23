<script setup lang="ts">
import { ChevronDown, ChevronUp, MoreHorizontal, Pencil, Trash2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import type { HandoutWorkspaceChildModel, HandoutWorkspaceItemModel } from '@/types'

defineProps<{
  item: HandoutWorkspaceItemModel
  readOnly?: boolean
}>()

defineEmits<{
  select: [id: string]
  moveUp: [id: string]
  moveDown: [id: string]
  editOccurrence: [id: string]
  remove: [id: string]
}>()

const { t } = useI18n()

function childMeta(child: HandoutWorkspaceChildModel) {
  const parts = [child.typeLabel, child.sourceLabel]
  return parts.filter(Boolean).join(' · ')
}
</script>

<template>
  <article
    class="rounded-md border bg-background transition-colors"
    :class="item.selected ? 'border-primary/40 bg-primary/5' : 'border-border'"
    @click="$emit('select', item.id)"
  >
    <header class="flex items-start justify-between gap-3 border-b px-3 py-2">
      <div class="min-w-0">
        <div class="flex min-w-0 items-center gap-2">
          <h3 class="truncate text-sm font-semibold">{{ item.titleOverride || item.title }}</h3>
          <StatusPill :label="item.targetType" tone="neutral" />
        </div>
        <p class="mt-1 truncate text-xs text-muted-foreground">
          {{ item.sourceLabel }}
        </p>
      </div>

      <div v-if="!readOnly" class="flex shrink-0 items-center gap-1">
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.handoutWorkspace.moveUp')"
          @click.stop="$emit('moveUp', item.id)"
        >
          <ChevronUp class="size-4" />
        </Button>
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.handoutWorkspace.moveDown')"
          @click.stop="$emit('moveDown', item.id)"
        >
          <ChevronDown class="size-4" />
        </Button>
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.handoutWorkspace.editOccurrence')"
          @click.stop="$emit('editOccurrence', item.id)"
        >
          <Pencil class="size-4" />
        </Button>
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.handoutWorkspace.removeReference')"
          @click.stop="$emit('remove', item.id)"
        >
          <Trash2 class="size-4" />
        </Button>
      </div>
      <MoreHorizontal v-else class="mt-1 size-4 shrink-0 text-muted-foreground" aria-hidden="true" />
    </header>

    <div class="space-y-2 px-3 py-3">
      <p v-if="item.note" class="rounded-md border bg-muted/30 px-3 py-2 text-xs text-muted-foreground">
        {{ item.note }}
      </p>

      <div v-if="item.children?.length" class="space-y-1">
        <div
          v-for="child in item.children"
          :key="child.id"
          class="rounded-md border bg-muted/20 px-3 py-2"
        >
          <div class="flex items-center justify-between gap-3">
            <p class="min-w-0 truncate text-sm font-medium">{{ child.title }}</p>
            <span class="shrink-0 text-xs text-muted-foreground">{{ childMeta(child) }}</span>
          </div>
          <div v-if="child.children?.length" class="mt-2 space-y-1 border-l pl-3">
            <div
              v-for="grandChild in child.children"
              :key="grandChild.id"
              class="flex items-center justify-between gap-3 text-sm"
            >
              <span class="min-w-0 truncate">{{ grandChild.title }}</span>
              <span class="shrink-0 text-xs text-muted-foreground">{{ childMeta(grandChild) }}</span>
            </div>
          </div>
        </div>
      </div>
      <p v-else class="text-sm text-muted-foreground">
        {{ t('components.handoutWorkspace.noDerivedChildren') }}
      </p>
    </div>
  </article>
</template>
