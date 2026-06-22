<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
import type { SectionVariantSelectionCandidateModel } from '@/types'

const props = defineProps<{
  candidate: SectionVariantSelectionCandidateModel
}>()

const emit = defineEmits<{
  toggle: [sectionItemId: number]
}>()

const { t } = useI18n()

const difficultyMarkerClass = computed(() =>
  getDifficultyMarkerClass(props.candidate.resolvedDifficulty),
)

function handleToggle() {
  if (!props.candidate.selectable) {
    return
  }

  emit('toggle', props.candidate.sectionItemId)
}
</script>

<template>
  <button
    type="button"
    class="grid w-full grid-cols-[auto_minmax(0,1fr)] gap-3 rounded-md border bg-background px-3 py-2 text-left transition-colors hover:bg-muted/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-60"
    :disabled="!candidate.selectable"
    :aria-pressed="candidate.selected"
    @click="handleToggle"
  >
    <span
      class="mt-1 flex size-4 items-center justify-center rounded-sm border text-[10px] font-semibold"
      :class="candidate.selected ? 'bg-primary text-primary-foreground' : 'bg-background'"
      aria-hidden="true"
    >
      <span v-if="candidate.selected" class="size-1.5 rounded-full bg-current" />
    </span>

    <span class="min-w-0 space-y-1">
      <span class="flex min-w-0 flex-wrap items-center gap-x-2 gap-y-1">
        <span class="min-w-0 truncate text-sm font-medium text-foreground">
          {{ candidate.title }}
        </span>
        <span class="text-xs text-muted-foreground">
          {{ candidate.targetType }}
        </span>
      </span>

      <span class="flex min-w-0 flex-wrap items-center gap-x-3 gap-y-1 text-xs text-muted-foreground">
        <span>{{ candidate.displayType }}</span>
        <span class="inline-flex items-center gap-1">
          <span class="h-3 w-1 rounded-full" :class="difficultyMarkerClass" aria-hidden="true" />
          {{ t(`components.sectionVariantCreate.difficulties.${candidate.resolvedDifficulty}`) }}
        </span>
        <span v-if="candidate.defaultSelected">
          {{ t('components.sectionVariantCreate.defaultSelected') }}
        </span>
      </span>

      <span v-if="!candidate.selectable && candidate.unavailableReason" class="block text-xs text-muted-foreground">
        {{ t('components.sectionVariantCreate.unavailableReason') }}:
        {{ candidate.unavailableReason }}
      </span>
    </span>
  </button>
</template>
