<script setup lang="ts">
import { CalendarClock, Edit3, Trash2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import TeachingNoteBadge from '@/components/business/TeachingNoteBadge.vue'
import TeachingNoteBindingSummary from '@/components/business/TeachingNoteBindingSummary.vue'
import { Button } from '@/components/ui/button'
import type { TeachingNoteModel } from '@/types'

withDefaults(defineProps<{
  note: TeachingNoteModel
  disabled?: boolean
  deleting?: boolean
}>(), {
  disabled: false,
  deleting: false,
})

const emit = defineEmits<{
  edit: [note: TeachingNoteModel]
  delete: [note: TeachingNoteModel]
}>()

const { t } = useI18n()
</script>

<template>
  <article class="grid gap-3 rounded-md border bg-card p-3 text-card-foreground">
    <div class="flex min-w-0 items-start justify-between gap-3">
      <div class="grid min-w-0 gap-2">
        <div class="flex flex-wrap items-center gap-2">
          <TeachingNoteBadge :note="note" />
          <span
            v-if="note.effectLevel"
            class="rounded-md border bg-muted/30 px-2 py-1 text-xs text-muted-foreground"
          >
            {{ t(`teachingNote.effectLevel.${note.effectLevel}`) }}
          </span>
          <span v-else class="rounded-md border bg-muted/30 px-2 py-1 text-xs text-muted-foreground">
            {{ t('teachingNote.effectLevel.none') }}
          </span>
        </div>
        <p v-if="note.occurredAt" class="inline-flex items-center gap-1 text-xs text-muted-foreground">
          <CalendarClock class="size-3" aria-hidden="true" />
          {{ t('teachingNote.occurredAtValue', { value: note.occurredAt }) }}
        </p>
      </div>

      <div class="flex shrink-0 items-center gap-1">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          :disabled="disabled || deleting"
          :title="t('teachingNote.edit')"
          @click="emit('edit', note)"
        >
          <Edit3 class="size-4" aria-hidden="true" />
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          :disabled="disabled || deleting"
          :title="t('teachingNote.delete')"
          @click="emit('delete', note)"
        >
          <Trash2 class="size-4" aria-hidden="true" />
        </Button>
      </div>
    </div>

    <p class="whitespace-pre-wrap text-sm leading-6">
      {{ note.content }}
    </p>

    <div class="grid gap-1">
      <span class="text-xs font-medium text-muted-foreground">
        {{ t('teachingNote.bindings') }}
      </span>
      <TeachingNoteBindingSummary :bindings="note.bindings" />
    </div>

    <p class="text-xs text-muted-foreground">
      {{ t('teachingNote.updatedAtValue', { value: note.updatedTime }) }}
    </p>
  </article>
</template>
