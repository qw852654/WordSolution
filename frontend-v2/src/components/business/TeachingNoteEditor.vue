<script setup lang="ts">
import { computed, reactive, watch } from 'vue'
import { Loader2, Save, X } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import TeachingNoteBindingSummary from '@/components/business/TeachingNoteBindingSummary.vue'
import { Button } from '@/components/ui/button'
import type {
  NoteType,
  TeachingNoteEffectLevel,
  TeachingNoteEditorValue,
} from '@/types'

const props = withDefaults(defineProps<{
  modelValue: TeachingNoteEditorValue
  mode?: 'create' | 'edit'
  saving?: boolean
  disabled?: boolean
  error?: string
}>(), {
  mode: 'create',
  saving: false,
  disabled: false,
  error: '',
})

const emit = defineEmits<{
  submit: [value: TeachingNoteEditorValue]
  cancel: []
}>()

const { t } = useI18n()

const noteTypes: NoteType[] = [
  'General',
  'ClassroomRecord',
  'LearningEffect',
  'TeachingReflection',
  'RevisionSuggestion',
  'QuestionReplacement',
  'CommonMistake',
]

const effectLevels: TeachingNoteEffectLevel[] = ['Good', 'Normal', 'Weak', 'Failed']

const form = reactive<TeachingNoteEditorValue>({
  noteType: props.modelValue.noteType,
  content: props.modelValue.content,
  effectLevel: props.modelValue.effectLevel,
  occurredAt: props.modelValue.occurredAt ?? null,
  bindings: [...props.modelValue.bindings],
})

watch(
  () => props.modelValue,
  (value) => {
    form.noteType = value.noteType
    form.content = value.content
    form.effectLevel = value.effectLevel
    form.occurredAt = value.occurredAt ?? null
    form.bindings = [...value.bindings]
  },
  { deep: true },
)

const isDisabled = computed(() => props.disabled || props.saving)
const trimmedContent = computed(() => form.content.trim())
const canSubmit = computed(() => !isDisabled.value && trimmedContent.value.length > 0)
const titleKey = computed(() =>
  props.mode === 'edit' ? 'teachingNote.editor.editTitle' : 'teachingNote.editor.createTitle',
)

function updateEffectLevel(value: string) {
  form.effectLevel = value ? (value as TeachingNoteEffectLevel) : null
}

function updateOccurredAt(value: string) {
  form.occurredAt = value || null
}

function submit() {
  if (!canSubmit.value) {
    return
  }

  emit('submit', {
    noteType: form.noteType,
    content: trimmedContent.value,
    effectLevel: form.effectLevel,
    occurredAt: form.occurredAt || null,
    bindings: [...form.bindings],
  })
}
</script>

<template>
  <form
    class="grid gap-3 rounded-md border bg-card p-3 text-card-foreground"
    @submit.prevent="submit"
  >
    <div class="grid gap-1">
      <h3 class="text-sm font-semibold">
        {{ t(titleKey) }}
      </h3>
      <p class="text-xs leading-5 text-muted-foreground">
        {{ t('teachingNote.editor.description') }}
      </p>
    </div>

    <div class="grid gap-3 sm:grid-cols-2">
      <label class="grid gap-1">
        <span class="text-xs font-medium text-muted-foreground">
          {{ t('teachingNote.noteTypeLabel') }}
        </span>
        <select
          v-model="form.noteType"
          class="h-9 rounded-md border bg-background px-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
          :disabled="isDisabled"
        >
          <option v-for="noteType in noteTypes" :key="noteType" :value="noteType">
            {{ t(`teachingNote.noteType.${noteType}`) }}
          </option>
        </select>
      </label>

      <label class="grid gap-1">
        <span class="text-xs font-medium text-muted-foreground">
          {{ t('teachingNote.effectLevelLabel') }}
        </span>
        <select
          :value="form.effectLevel ?? ''"
          class="h-9 rounded-md border bg-background px-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
          :disabled="isDisabled"
          @change="updateEffectLevel(($event.target as HTMLSelectElement).value)"
        >
          <option value="">
            {{ t('teachingNote.effectLevel.none') }}
          </option>
          <option v-for="effectLevel in effectLevels" :key="effectLevel" :value="effectLevel">
            {{ t(`teachingNote.effectLevel.${effectLevel}`) }}
          </option>
        </select>
      </label>
    </div>

    <label class="grid gap-1">
      <span class="text-xs font-medium text-muted-foreground">
        {{ t('teachingNote.occurredAt') }}
      </span>
      <input
        :value="form.occurredAt ?? ''"
        type="datetime-local"
        class="h-9 rounded-md border bg-background px-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="isDisabled"
        @input="updateOccurredAt(($event.target as HTMLInputElement).value)"
      />
    </label>

    <label class="grid gap-1">
      <span class="text-xs font-medium text-muted-foreground">
        {{ t('teachingNote.contentLabel') }}
      </span>
      <textarea
        v-model="form.content"
        rows="5"
        class="min-h-28 resize-y rounded-md border bg-background px-3 py-2 text-sm leading-6 outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
        :placeholder="t('teachingNote.editor.contentPlaceholder')"
        :disabled="isDisabled"
      />
    </label>

    <div class="grid gap-1">
      <span class="text-xs font-medium text-muted-foreground">
        {{ t('teachingNote.bindings') }}
      </span>
      <TeachingNoteBindingSummary :bindings="form.bindings" />
    </div>

    <div
      v-if="error"
      class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
    >
      {{ error }}
    </div>

    <div class="flex flex-wrap justify-end gap-2">
      <Button type="button" variant="outline" :disabled="isDisabled" @click="emit('cancel')">
        <X class="size-4" aria-hidden="true" />
        {{ t('teachingNote.cancel') }}
      </Button>
      <Button type="submit" :disabled="!canSubmit">
        <Loader2 v-if="saving" class="size-4 animate-spin" aria-hidden="true" />
        <Save v-else class="size-4" aria-hidden="true" />
        {{ t(props.mode === 'edit' ? 'teachingNote.saveEdit' : 'teachingNote.saveCreate') }}
      </Button>
    </div>
  </form>
</template>
