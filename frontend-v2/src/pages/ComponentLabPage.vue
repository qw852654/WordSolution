<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import TeachingNoteBadge from '@/components/business/TeachingNoteBadge.vue'
import TeachingNoteBindingSummary from '@/components/business/TeachingNoteBindingSummary.vue'
import TeachingNoteDeleteConfirm from '@/components/business/TeachingNoteDeleteConfirm.vue'
import TeachingNoteEditor from '@/components/business/TeachingNoteEditor.vue'
import TeachingNoteList from '@/components/business/TeachingNoteList.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { usePageTitle } from '@/composables/usePageTitle'
import {
  mockTeachingNoteEditorValue,
  mockTeachingNotes,
} from '@/mocks'
import type { TeachingNoteEditorValue, TeachingNoteListState, TeachingNoteModel } from '@/types'

const { t } = useI18n()
usePageTitle('ComponentLab')

const notes = ref<TeachingNoteModel[]>([...mockTeachingNotes])
const editorValue = ref<TeachingNoteEditorValue>({ ...mockTeachingNoteEditorValue })
const selectedNote = ref<TeachingNoteModel | null>(null)
const deletingNote = ref<TeachingNoteModel | null>(mockTeachingNotes[1])
const listState = ref<TeachingNoteListState>('idle')
const disabled = ref(false)
const saving = ref(false)
const deleting = ref(false)
const showError = ref(false)
const feedback = ref('')

const displayedNotes = computed(() => (listState.value === 'empty' ? [] : notes.value))
const listError = computed(() => (showError.value ? t('lab.sections.teachingNotes.mockError') : ''))
const editorError = computed(() =>
  showError.value ? t('lab.sections.teachingNotes.mockEditorError') : '',
)
const deleteError = computed(() =>
  showError.value ? t('lab.sections.teachingNotes.mockDeleteError') : '',
)

function recordFeedback(message: string, payload?: unknown) {
  feedback.value = payload ? `${message}\n${JSON.stringify(payload, null, 2)}` : message
}

function startCreate() {
  selectedNote.value = null
  editorValue.value = { ...mockTeachingNoteEditorValue }
  recordFeedback(t('teachingNote.events.create'))
}

function startEdit(note: TeachingNoteModel) {
  selectedNote.value = note
  editorValue.value = {
    noteType: note.noteType,
    content: note.content,
    effectLevel: note.effectLevel,
    occurredAt: note.occurredAt ?? null,
    bindings: [...note.bindings],
  }
  recordFeedback(t('teachingNote.events.edit', { id: note.id }), note)
}

function requestDelete(note: TeachingNoteModel) {
  deletingNote.value = note
  recordFeedback(t('teachingNote.events.deleteRequested', { id: note.id }), note.bindings)
}

function submitEditor(value: TeachingNoteEditorValue) {
  saving.value = true
  recordFeedback(t('teachingNote.events.submit'), value)
  window.setTimeout(() => {
    saving.value = false
  }, 300)
}

function confirmDelete(note: TeachingNoteModel) {
  deleting.value = true
  recordFeedback(t('teachingNote.events.deleteConfirmed', { id: note.id }), note.bindings)
  window.setTimeout(() => {
    deleting.value = false
  }, 300)
}

function resetMockData() {
  notes.value = [...mockTeachingNotes]
  editorValue.value = { ...mockTeachingNoteEditorValue }
  selectedNote.value = null
  deletingNote.value = mockTeachingNotes[1]
  listState.value = 'idle'
  disabled.value = false
  saving.value = false
  deleting.value = false
  showError.value = false
  feedback.value = ''
}
</script>

<template>
  <main class="min-h-screen bg-background px-4 py-6 text-foreground sm:px-6 lg:px-8">
    <PageHeader
      :eyebrow="t('lab.eyebrow')"
      :title="t('lab.sections.teachingNotes.title')"
      :description="t('lab.sections.teachingNotes.description')"
    >
      <template #actions>
        <Button variant="outline" as-child>
          <RouterLink to="/">
            <ArrowLeft class="size-4" aria-hidden="true" />
            {{ t('lab.backHome') }}
          </RouterLink>
        </Button>
      </template>
    </PageHeader>

    <section class="mt-6 grid gap-4 rounded-lg border bg-card p-4 text-card-foreground">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="grid gap-1">
          <h2 class="text-base font-semibold">
            {{ t('lab.sections.teachingNotes.summaryTitle') }}
          </h2>
          <p class="text-sm leading-6 text-muted-foreground">
            {{ t('lab.sections.teachingNotes.summaryDescription') }}
          </p>
        </div>
        <Button type="button" variant="outline" size="sm" @click="resetMockData">
          {{ t('lab.sections.teachingNotes.reset') }}
        </Button>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <TeachingNoteBadge :count="notes.length" />
        <TeachingNoteBadge v-for="note in notes" :key="note.id" :note="note" />
      </div>

      <div class="grid gap-2">
        <span class="text-xs font-medium text-muted-foreground">
          {{ t('teachingNote.bindings') }}
        </span>
        <TeachingNoteBindingSummary :bindings="mockTeachingNotes[1].bindings" />
      </div>
    </section>

    <section class="mt-6 grid gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(24rem,0.8fr)]">
      <TeachingNoteList
        :notes="displayedNotes"
        :state="listState"
        :disabled="disabled"
        :deleting-note-id="deletingNote?.id ?? null"
        :error="listError"
        @create="startCreate"
        @edit="startEdit"
        @delete="requestDelete"
      />

      <aside class="grid content-start gap-4">
        <TeachingNoteEditor
          :model-value="editorValue"
          :mode="selectedNote ? 'edit' : 'create'"
          :saving="saving"
          :disabled="disabled"
          :error="editorError"
          @submit="submitEditor"
          @cancel="recordFeedback(t('teachingNote.events.cancel'))"
        />

        <TeachingNoteDeleteConfirm
          :note="deletingNote"
          :deleting="deleting"
          :disabled="disabled"
          :error="deleteError"
          @confirm="confirmDelete"
          @cancel="recordFeedback(t('teachingNote.events.cancelDelete'))"
        />
      </aside>
    </section>

    <section class="mt-6 grid gap-4 rounded-lg border bg-card p-4 text-card-foreground">
      <h2 class="text-base font-semibold">
        {{ t('lab.sections.teachingNotes.stateTitle') }}
      </h2>
      <div class="flex flex-wrap gap-4">
        <label class="flex items-center gap-2 text-sm">
          <input v-model="listState" type="radio" value="idle" class="size-4 accent-primary" />
          {{ t('teachingNote.state.idle') }}
        </label>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="listState" type="radio" value="loading" class="size-4 accent-primary" />
          {{ t('teachingNote.state.loading') }}
        </label>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="listState" type="radio" value="empty" class="size-4 accent-primary" />
          {{ t('teachingNote.state.empty') }}
        </label>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="listState" type="radio" value="error" class="size-4 accent-primary" />
          {{ t('teachingNote.state.error') }}
        </label>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="disabled" type="checkbox" class="size-4 accent-primary" />
          {{ t('lab.sections.teachingNotes.disabled') }}
        </label>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="saving" type="checkbox" class="size-4 accent-primary" />
          {{ t('lab.sections.teachingNotes.saving') }}
        </label>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="deleting" type="checkbox" class="size-4 accent-primary" />
          {{ t('lab.sections.teachingNotes.deleting') }}
        </label>
        <label class="flex items-center gap-2 text-sm">
          <input v-model="showError" type="checkbox" class="size-4 accent-primary" />
          {{ t('lab.sections.teachingNotes.showError') }}
        </label>
      </div>
    </section>

    <aside
      class="mt-4 whitespace-pre-wrap rounded-lg border bg-muted/20 px-4 py-3 text-sm text-muted-foreground"
      aria-live="polite"
    >
      <span class="font-medium text-foreground">
        {{ t('lab.sections.teachingNotes.feedbackTitle') }}
      </span>
      <span class="ml-2">
        {{ feedback || t('lab.sections.teachingNotes.emptyFeedback') }}
      </span>
    </aside>
  </main>
</template>
