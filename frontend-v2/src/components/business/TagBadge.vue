<script setup lang="ts">
import { computed } from 'vue'
import { X } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { cn } from '@/lib/utils'
import { tagColorClasses } from '@/components/business/tagColorTone'
import type { TagModel } from '@/types'

const props = withDefaults(defineProps<{
  tag: TagModel
  selected?: boolean
  removable?: boolean
  disabled?: boolean
}>(), {
  selected: false,
  removable: false,
  disabled: false,
})

const emit = defineEmits<{
  remove: [tagId: number]
}>()

const { t } = useI18n()

const isArchived = computed(() => props.tag.status === 'Archived')
const badgeClass = computed(() =>
  cn(
    tagColorClasses[props.tag.color],
    'inline-flex min-h-6 max-w-full items-center gap-1 rounded-md border px-2 text-xs font-medium leading-5',
    props.selected && 'ring-1 ring-ring',
    isArchived.value && 'opacity-60',
    props.disabled && 'pointer-events-none opacity-50',
  ),
)
</script>

<template>
  <span
    :class="badgeClass"
    :title="isArchived ? t('tag.archivedTooltip') : tag.name"
  >
    <span class="min-w-0 truncate">
      {{ tag.name }}
    </span>
    <span v-if="isArchived" class="shrink-0 text-[10px] font-normal">
      {{ t('tag.archivedShort') }}
    </span>
    <button
      v-if="removable && !disabled"
      type="button"
      class="-mr-1 inline-flex size-4 shrink-0 items-center justify-center rounded-sm hover:bg-background/50 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
      :aria-label="t('tag.removeTag', { name: tag.name })"
      @click.stop="emit('remove', tag.id)"
    >
      <X class="size-3" aria-hidden="true" />
    </button>
  </span>
</template>
