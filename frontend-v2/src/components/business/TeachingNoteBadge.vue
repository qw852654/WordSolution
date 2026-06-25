<script setup lang="ts">
import { computed } from 'vue'
import { MessageSquareText } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import type { TeachingNoteEffectLevel, TeachingNoteModel } from '@/types'

const props = defineProps<{
  note?: TeachingNoteModel
  count?: number
}>()

const { t } = useI18n()

const effectToneClass: Record<TeachingNoteEffectLevel, string> = {
  Unknown: 'border-muted bg-muted text-muted-foreground',
  Good: 'border-primary/30 bg-primary/10 text-primary',
  Normal: 'border-border bg-muted/40 text-muted-foreground',
  Weak: 'border-secondary bg-secondary text-secondary-foreground',
  Failed: 'border-destructive/30 bg-destructive/10 text-destructive',
}

const label = computed(() => {
  if (props.note) {
    return t(`teachingNote.noteType.${props.note.noteType}`)
  }

  return t('teachingNote.noteCount', { count: props.count ?? 0 })
})

const badgeClass = computed(() => {
  if (!props.note?.effectLevel) {
    return 'border-border bg-muted/30 text-muted-foreground'
  }

  return effectToneClass[props.note.effectLevel]
})
</script>

<template>
  <span
    :class="[
      'inline-flex max-w-full items-center gap-1 rounded-md border px-2 py-1 text-xs font-medium',
      badgeClass,
    ]"
  >
    <MessageSquareText class="size-3 shrink-0" aria-hidden="true" />
    <span class="truncate">{{ label }}</span>
  </span>
</template>
