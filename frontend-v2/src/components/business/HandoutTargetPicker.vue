<script setup lang="ts">
import { computed, ref } from 'vue'
import { Search } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import { Button } from '@/components/ui/button'
import type { HandoutTargetPickerCandidateModel } from '@/types'

const props = defineProps<{
  open: boolean
  title: string
  description: string
  candidates: HandoutTargetPickerCandidateModel[]
  loading?: boolean
  error?: string
}>()

const emit = defineEmits<{
  close: []
  select: [id: number]
}>()

const { t } = useI18n()
const searchText = ref('')

const filteredCandidates = computed(() => {
  const query = searchText.value.trim().toLocaleLowerCase()

  if (!query) {
    return props.candidates
  }

  return props.candidates.filter((candidate) => {
    const haystack = [candidate.title, ...(candidate.metaItems ?? [])].join(' ').toLocaleLowerCase()
    return haystack.includes(query)
  })
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[65] flex min-h-screen items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-label="title"
    >
      <button
        type="button"
        class="absolute inset-0 bg-background/70 backdrop-blur-sm"
        :aria-label="t('handoutTargetPicker.close')"
        @click="emit('close')"
      />

      <section class="relative z-10 flex max-h-[calc(100vh-2rem)] w-full max-w-2xl flex-col rounded-lg border bg-card text-card-foreground">
        <header class="border-b px-4 py-3">
          <h2 class="truncate text-lg font-semibold">{{ title }}</h2>
          <p class="mt-1 text-sm text-muted-foreground">{{ description }}</p>
        </header>

        <div class="flex min-h-0 flex-1 flex-col gap-3 p-4">
          <label class="flex items-center gap-2 rounded-md border bg-background px-3 py-2 text-sm">
            <Search class="size-4 text-muted-foreground" aria-hidden="true" />
            <input
              v-model="searchText"
              class="min-w-0 flex-1 bg-transparent outline-none placeholder:text-muted-foreground"
              :placeholder="t('handoutTargetPicker.searchPlaceholder')"
            />
          </label>

          <EmptyState
            v-if="loading"
            :title="t('handoutTargetPicker.loadingTitle')"
            :description="t('handoutTargetPicker.loadingDescription')"
          />

          <EmptyState
            v-else-if="error"
            :title="t('handoutTargetPicker.errorTitle')"
            :description="error"
          />

          <div v-else-if="filteredCandidates.length" class="min-h-0 space-y-2 overflow-auto">
            <button
              v-for="candidate in filteredCandidates"
              :key="candidate.id"
              type="button"
              class="flex w-full items-center justify-between gap-3 rounded-md border bg-background px-3 py-2 text-left hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30 disabled:pointer-events-none disabled:opacity-60"
              :disabled="candidate.disabled"
              @click="emit('select', candidate.id)"
            >
              <span class="min-w-0">
                <span class="block truncate text-sm font-medium">{{ candidate.title }}</span>
                <span
                  v-if="candidate.metaItems?.length"
                  class="mt-1 flex min-w-0 items-center gap-1.5 text-xs text-muted-foreground"
                >
                  <template
                    v-for="(metaItem, index) in candidate.metaItems"
                    :key="`${candidate.id}-${metaItem}-${index}`"
                  >
                    <span v-if="index > 0" aria-hidden="true">&middot;</span>
                    <span class="truncate">{{ metaItem }}</span>
                  </template>
                </span>
              </span>
              <span class="shrink-0 text-xs text-muted-foreground">#{{ candidate.id }}</span>
            </button>
          </div>

          <EmptyState
            v-else
            :title="t('handoutTargetPicker.emptyTitle')"
            :description="t('handoutTargetPicker.emptyDescription')"
          />
        </div>

        <footer class="flex justify-end border-t px-4 py-3">
          <Button type="button" variant="outline" @click="emit('close')">
            {{ t('handoutTargetPicker.close') }}
          </Button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>
