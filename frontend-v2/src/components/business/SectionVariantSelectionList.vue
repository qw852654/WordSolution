<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import SectionVariantSelectionItem from '@/components/business/SectionVariantSelectionItem.vue'
import type { SectionVariantSelectionCandidateModel } from '@/types'

const props = defineProps<{
  candidates: SectionVariantSelectionCandidateModel[]
}>()

const emit = defineEmits<{
  toggle: [sectionItemId: number]
}>()

const { t } = useI18n()

const selectedCount = computed(
  () => props.candidates.filter((candidate) => candidate.selectable && candidate.selected).length,
)
const selectableCount = computed(
  () => props.candidates.filter((candidate) => candidate.selectable).length,
)
</script>

<template>
  <section class="grid gap-3" :aria-label="t('components.sectionVariantCreate.selectionTitle')">
    <header class="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <h3 class="text-sm font-medium">
          {{ t('components.sectionVariantCreate.selectionTitle') }}
        </h3>
        <p class="text-sm text-muted-foreground">
          {{ t('components.sectionVariantCreate.selectionDescription') }}
        </p>
      </div>
      <p class="text-sm text-muted-foreground">
        {{
          t('components.sectionVariantCreate.selectedSummary', {
            selected: selectedCount,
            total: selectableCount,
          })
        }}
      </p>
    </header>

    <div class="grid gap-2">
      <SectionVariantSelectionItem
        v-for="candidate in candidates"
        :key="candidate.sectionItemId"
        :candidate="candidate"
        @toggle="emit('toggle', $event)"
      />
    </div>

    <p class="text-xs text-muted-foreground">
      {{ t('components.sectionVariantCreate.emptyVariantAllowed') }}
    </p>
  </section>
</template>
