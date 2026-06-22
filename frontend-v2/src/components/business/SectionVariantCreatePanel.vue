<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import SectionVariantMetadataForm from '@/components/business/SectionVariantMetadataForm.vue'
import SectionVariantSelectionList from '@/components/business/SectionVariantSelectionList.vue'
import { Button } from '@/components/ui/button'
import type {
  SectionVariantCreateMetadata,
  SectionVariantCreateSubmitPayload,
  SectionVariantSelectionCandidateModel,
} from '@/types'

const props = defineProps<{
  initialMetadata: SectionVariantCreateMetadata
  candidates: SectionVariantSelectionCandidateModel[]
  sectionTitle?: string
}>()

const emit = defineEmits<{
  cancel: []
  submit: [payload: SectionVariantCreateSubmitPayload]
}>()

const { t } = useI18n()

const activeStep = ref<'metadata' | 'selection'>('metadata')
const showValidation = ref(false)
const metadata = reactive<SectionVariantCreateMetadata>({ ...props.initialMetadata })
const selectionCandidates = ref<SectionVariantSelectionCandidateModel[]>([])

const titleIsValid = computed(() => metadata.title.trim().length > 0)
const selectedSectionItemIds = computed(() =>
  selectionCandidates.value
    .filter((candidate) => candidate.selectable && candidate.selected)
    .map((candidate) => candidate.sectionItemId),
)
const stepLabel = computed(() =>
  activeStep.value === 'metadata'
    ? t('components.sectionVariantCreate.stepMetadata')
    : t('components.sectionVariantCreate.stepSelection'),
)

function resetState() {
  Object.assign(metadata, props.initialMetadata)
  selectionCandidates.value = props.candidates.map((candidate) => ({ ...candidate }))
  activeStep.value = 'metadata'
  showValidation.value = false
}

function updateMetadata(value: SectionVariantCreateMetadata) {
  Object.assign(metadata, value)
}

function goToSelection() {
  showValidation.value = true

  if (!titleIsValid.value) {
    return
  }

  activeStep.value = 'selection'
}

function toggleCandidate(sectionItemId: number) {
  selectionCandidates.value = selectionCandidates.value.map((candidate) =>
    candidate.sectionItemId === sectionItemId && candidate.selectable
      ? { ...candidate, selected: !candidate.selected }
      : candidate,
  )
}

function handleSubmit() {
  if (!titleIsValid.value) {
    activeStep.value = 'metadata'
    showValidation.value = true
    return
  }

  emit('submit', {
    sectionId: metadata.sectionId,
    title: metadata.title.trim(),
    description: metadata.description?.trim() || undefined,
    type: metadata.type,
    difficulty: metadata.difficulty,
    selectedSectionItemIds: selectedSectionItemIds.value,
  })
}

watch(
  () => [props.initialMetadata, props.candidates] as const,
  resetState,
  { immediate: true, deep: true },
)
</script>

<template>
  <section class="rounded-lg border bg-card text-card-foreground" aria-label="SectionVariantCreatePanel">
    <header class="border-b px-4 py-3">
      <div class="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
        <div class="space-y-1">
          <p class="text-xs font-medium text-muted-foreground">
            {{ stepLabel }}
          </p>
          <h2 class="text-lg font-semibold tracking-normal">
            SectionVariantCreatePanel
          </h2>
          <p class="text-sm text-muted-foreground">
            {{ t('components.sectionVariantCreate.description') }}
          </p>
        </div>
        <div class="rounded-md border bg-background px-3 py-2 text-sm text-muted-foreground">
          <span class="font-medium text-foreground">Section</span>
          <span class="ml-2">{{ sectionTitle ?? initialMetadata.sectionId }}</span>
        </div>
      </div>
    </header>

    <div class="grid gap-4 p-4">
      <SectionVariantMetadataForm
        v-if="activeStep === 'metadata'"
        :model-value="metadata"
        :show-validation="showValidation"
        @update:model-value="updateMetadata"
      />

      <SectionVariantSelectionList
        v-else
        :candidates="selectionCandidates"
        @toggle="toggleCandidate"
      />
    </div>

    <footer class="flex flex-col gap-2 border-t px-4 py-3 sm:flex-row sm:justify-end">
      <Button type="button" variant="outline" @click="emit('cancel')">
        {{ t('components.sectionVariantCreate.actions.cancel') }}
      </Button>
      <Button
        v-if="activeStep === 'selection'"
        type="button"
        variant="outline"
        @click="activeStep = 'metadata'"
      >
        {{ t('components.sectionVariantCreate.actions.previous') }}
      </Button>
      <Button v-if="activeStep === 'metadata'" type="button" @click="goToSelection">
        {{ t('components.sectionVariantCreate.actions.next') }}
      </Button>
      <Button v-else type="button" @click="handleSubmit">
        {{ t('components.sectionVariantCreate.actions.submit') }}
      </Button>
    </footer>
  </section>
</template>
