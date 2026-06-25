<script setup lang="ts">
import { computed } from 'vue'
import { Loader2, Trash2, X } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import TeachingNoteBindingSummary from '@/components/business/TeachingNoteBindingSummary.vue'
import { Button } from '@/components/ui/button'
import type { TeachingNoteModel } from '@/types'

const props = withDefaults(defineProps<{
  note?: TeachingNoteModel | null
  deleting?: boolean
  disabled?: boolean
  error?: string
}>(), {
  note: null,
  deleting: false,
  disabled: false,
  error: '',
})

const emit = defineEmits<{
  confirm: [note: TeachingNoteModel]
  cancel: []
}>()

const { t } = useI18n()

const bindingCount = computed(() => props.note?.bindings.length ?? 0)
const hasMultipleBindings = computed(() => bindingCount.value > 1)
const canConfirm = computed(() => !!props.note && !props.disabled && !props.deleting)

function confirm() {
  if (!props.note || !canConfirm.value) {
    return
  }

  emit('confirm', props.note)
}
</script>

<template>
  <section
    class="grid gap-3 rounded-md border border-destructive/30 bg-card p-3 text-card-foreground"
    :aria-label="t('teachingNote.deleteConfirm.title')"
  >
    <div class="flex items-start gap-3">
      <span class="mt-0.5 inline-flex size-8 shrink-0 items-center justify-center rounded-md border border-destructive/30 bg-destructive/10 text-destructive">
        <Trash2 class="size-4" aria-hidden="true" />
      </span>
      <div class="grid min-w-0 gap-1">
        <h3 class="text-sm font-semibold">
          {{ t('teachingNote.deleteConfirm.title') }}
        </h3>
        <p class="text-sm leading-6 text-muted-foreground">
          {{ t('teachingNote.deleteConfirm.description') }}
        </p>
        <p v-if="hasMultipleBindings" class="text-sm leading-6 text-destructive">
          {{ t('teachingNote.deleteConfirm.multiBindingWarning', { count: bindingCount }) }}
        </p>
      </div>
    </div>

    <TeachingNoteBindingSummary v-if="note" :bindings="note.bindings" />

    <div
      v-if="error"
      class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
    >
      {{ error }}
    </div>

    <div class="flex flex-wrap justify-end gap-2">
      <Button type="button" variant="outline" :disabled="deleting" @click="emit('cancel')">
        <X class="size-4" aria-hidden="true" />
        {{ t('teachingNote.cancel') }}
      </Button>
      <Button type="button" variant="destructive" :disabled="!canConfirm" @click="confirm">
        <Loader2 v-if="deleting" class="size-4 animate-spin" aria-hidden="true" />
        <Trash2 v-else class="size-4" aria-hidden="true" />
        {{ t('teachingNote.confirmDelete') }}
      </Button>
    </div>
  </section>
</template>
