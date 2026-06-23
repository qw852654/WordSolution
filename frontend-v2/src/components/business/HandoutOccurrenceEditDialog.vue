<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'

const props = defineProps<{
  open: boolean
  itemTitle: string
  titleOverride?: string | null
  note?: string | null
}>()

const emit = defineEmits<{
  close: []
  submit: [payload: { titleOverride: string | null; note: string | null }]
}>()

const { t } = useI18n()
const localTitleOverride = ref('')
const localNote = ref('')

watch(
  () => props.open,
  (open) => {
    if (open) {
      localTitleOverride.value = props.titleOverride ?? ''
      localNote.value = props.note ?? ''
    }
  },
  { immediate: true },
)

function submit() {
  emit('submit', {
    titleOverride: localTitleOverride.value.trim() || null,
    note: localNote.value.trim() || null,
  })
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[65] flex min-h-screen items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-label="t('handoutOccurrenceEditDialog.dialogLabel')"
    >
      <button
        type="button"
        class="absolute inset-0 bg-background/70 backdrop-blur-sm"
        :aria-label="t('handoutOccurrenceEditDialog.close')"
        @click="emit('close')"
      />

      <section class="relative z-10 w-full max-w-xl rounded-lg border bg-card text-card-foreground">
        <header class="border-b px-4 py-3">
          <h2 class="truncate text-lg font-semibold">
            {{ t('handoutOccurrenceEditDialog.title') }}
          </h2>
          <p class="mt-1 truncate text-sm text-muted-foreground">
            {{ itemTitle }}
          </p>
        </header>

        <div class="space-y-4 p-4">
          <label class="block space-y-2">
            <span class="text-sm font-medium">TitleOverride</span>
            <input
              v-model="localTitleOverride"
              class="w-full rounded-md border bg-background px-3 py-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
              :placeholder="t('handoutOccurrenceEditDialog.titleOverridePlaceholder')"
            />
          </label>

          <label class="block space-y-2">
            <span class="text-sm font-medium">Note</span>
            <textarea
              v-model="localNote"
              rows="4"
              class="w-full resize-y rounded-md border bg-background px-3 py-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
              :placeholder="t('handoutOccurrenceEditDialog.notePlaceholder')"
            />
          </label>

          <p class="rounded-md border bg-muted/20 px-3 py-2 text-xs text-muted-foreground">
            {{ t('handoutOccurrenceEditDialog.occurrenceOnly') }}
          </p>
        </div>

        <footer class="flex justify-end gap-2 border-t px-4 py-3">
          <Button type="button" variant="outline" @click="emit('close')">
            {{ t('handoutOccurrenceEditDialog.close') }}
          </Button>
          <Button type="button" @click="submit">
            {{ t('handoutOccurrenceEditDialog.submit') }}
          </Button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>
