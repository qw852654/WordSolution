<script setup lang="ts">
import { Plus } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import TeachingNoteCard from '@/components/business/TeachingNoteCard.vue'
import { Button } from '@/components/ui/button'
import type { TeachingNoteListState, TeachingNoteModel } from '@/types'

withDefaults(defineProps<{
  notes: TeachingNoteModel[]
  state?: TeachingNoteListState
  disabled?: boolean
  deletingNoteId?: number | null
  error?: string
}>(), {
  state: 'idle',
  disabled: false,
  deletingNoteId: null,
  error: '',
})

const emit = defineEmits<{
  create: []
  edit: [note: TeachingNoteModel]
  delete: [note: TeachingNoteModel]
}>()

const { t } = useI18n()
</script>

<template>
  <section class="grid gap-3 rounded-md border bg-card p-3 text-card-foreground">
    <div class="flex items-center justify-between gap-3">
      <div class="min-w-0">
        <h3 class="text-sm font-semibold">{{ t('teachingNote.title') }}</h3>
        <p class="text-xs text-muted-foreground">
          {{ t('teachingNote.listDescription') }}
        </p>
      </div>
      <Button type="button" size="sm" :disabled="disabled" @click="emit('create')">
        <Plus class="size-4" aria-hidden="true" />
        {{ t('teachingNote.create') }}
      </Button>
    </div>

    <div v-if="state === 'error'" class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
      {{ error || t('teachingNote.error') }}
    </div>

    <div v-else-if="state === 'loading'" class="rounded-md border bg-muted/30 px-3 py-3 text-sm text-muted-foreground">
      {{ t('teachingNote.loading') }}
    </div>

    <EmptyState
      v-else-if="state === 'empty' || !notes.length"
      :title="t('teachingNote.emptyTitle')"
      :description="t('teachingNote.emptyDescription')"
      :action-label="disabled ? undefined : t('teachingNote.create')"
      @action="emit('create')"
    />

    <div v-else class="grid gap-3">
      <TeachingNoteCard
        v-for="note in notes"
        :key="note.id"
        :note="note"
        :disabled="disabled"
        :deleting="deletingNoteId === note.id"
        @edit="emit('edit', $event)"
        @delete="emit('delete', $event)"
      />
    </div>
  </section>
</template>
