<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { FileText, Upload, X } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import type { CmsV2QuestionImportCandidateDto, CmsV2QuestionImportSessionDto } from '@/apis/cmsV2Client'
import type { QuestionImportConfirmPayload, QuestionImportContext } from '@/types'
import { Button } from '@/components/ui/button'

const props = defineProps<{
  open: boolean
  importContext: QuestionImportContext
  session?: CmsV2QuestionImportSessionDto | null
  busy?: boolean
  errorMessage?: string
  feedbackMessage?: string
}>()

const emit = defineEmits<{
  close: []
  upload: [file: File]
  confirmCandidate: [payload: QuestionImportConfirmPayload]
  cancelSession: []
}>()

const { t } = useI18n()
const selectedFile = ref<File | null>(null)
const selectedCandidateId = ref<string>()
const metadata = reactive({
  title: '',
  summary: '',
  difficulty: 'Medium' as QuestionImportConfirmPayload['difficulty'],
  questionType: 'Calculation' as QuestionImportConfirmPayload['questionType'],
})

const candidates = computed(() => props.session?.candidates ?? [])
const selectedCandidate = computed(() =>
  candidates.value.find((candidate) => candidate.candidateId === selectedCandidateId.value),
)
const importTargetTitle = computed(() => {
  if (props.importContext.target === 'AtomicSectionPanel') {
    return props.importContext.atomicSectionPanelTitle
  }

  return props.importContext.sectionTitle
})
const importTargetDescription = computed(() => {
  if (props.importContext.target === 'AtomicSectionPanel') {
    return t('sectionPage.questionImport.target.atomicSectionPanel', {
      sectionTitle: props.importContext.sectionTitle,
      atomicSectionTitle: props.importContext.atomicSectionTitle,
      panelTitle: props.importContext.atomicSectionPanelTitle,
    })
  }

  return t('sectionPage.questionImport.target.sectionTopLevel', {
    sectionTitle: props.importContext.sectionTitle,
  })
})
const hasSession = computed(() => Boolean(props.session))
const uploadDisabled = computed(() => props.busy || !selectedFile.value)
const confirmDisabled = computed(() => props.busy || !selectedCandidate.value)
const parseToneByStatus = {
  Parsed: 'text-muted-foreground',
  ParsedWithWarnings: 'text-muted-foreground',
  Failed: 'text-destructive',
  NotApplicable: 'text-muted-foreground',
} satisfies Record<NonNullable<CmsV2QuestionImportCandidateDto['parseStatus']>, string>

function handleFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  selectedFile.value = input.files?.[0] ?? null
}

function submitUpload() {
  if (selectedFile.value) {
    emit('upload', selectedFile.value)
  }
}

function submitConfirm() {
  const candidate = selectedCandidate.value
  if (!candidate) {
    return
  }

  emit('confirmCandidate', {
    candidateId: candidate.candidateId,
    title: metadata.title.trim(),
    summary: metadata.summary.trim() || undefined,
    difficulty: metadata.difficulty,
    questionType: metadata.questionType,
  })
}

function resetMetadataForCandidate(candidate?: CmsV2QuestionImportCandidateDto) {
  metadata.title = ''
  metadata.summary = candidate?.parts.find((part) => part.partType === 'Stem')?.plainText.slice(0, 80) ?? ''
  metadata.difficulty = 'Medium'
  metadata.questionType = 'Calculation'
}

watch(
  candidates,
  (value) => {
    if (!value.length) {
      selectedCandidateId.value = undefined
      resetMetadataForCandidate()
      return
    }

    if (!selectedCandidateId.value || !value.some((candidate) => candidate.candidateId === selectedCandidateId.value)) {
      selectedCandidateId.value = value[0].candidateId
      resetMetadataForCandidate(value[0])
    }
  },
  { immediate: true },
)

watch(
  selectedCandidate,
  (candidate) => {
    resetMetadataForCandidate(candidate)
  },
)
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[70] flex min-h-screen items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-label="t('sectionPage.questionImport.dialogLabel')"
    >
      <button
        type="button"
        class="absolute inset-0 bg-background/70 backdrop-blur-sm"
        :aria-label="t('sectionPage.questionImport.closeLabel')"
        @click="emit('close')"
      />

      <section class="relative z-10 grid max-h-[calc(100vh-2rem)] w-full max-w-6xl overflow-hidden rounded-lg border bg-card text-card-foreground">
        <header class="flex items-start justify-between gap-3 border-b px-4 py-3">
          <div class="min-w-0">
            <p class="text-xs text-muted-foreground">
              {{ t('sectionPage.questionImport.eyebrow') }}
            </p>
            <h2 class="mt-1 text-lg font-semibold tracking-normal">
              {{ t('sectionPage.questionImport.title') }}
            </h2>
            <p class="mt-1 text-sm text-muted-foreground">
              {{ t('sectionPage.questionImport.description') }}
              <span class="font-medium text-foreground">{{ importTargetTitle }}</span>
            </p>
            <p class="mt-1 text-xs text-muted-foreground">
              {{ importTargetDescription }}
            </p>
          </div>
          <Button
            type="button"
            size="sm"
            variant="ghost"
            class="h-8 px-2"
            :disabled="busy"
            @click="emit('close')"
          >
            <X class="size-4" />
          </Button>
        </header>

        <div class="grid max-h-[calc(100vh-10rem)] gap-4 overflow-auto p-4">
          <div v-if="!hasSession" class="grid gap-3 rounded-lg border bg-background p-4">
            <label class="grid gap-2 text-sm">
              <span class="font-medium">{{ t('sectionPage.questionImport.fileLabel') }}</span>
              <input
                type="file"
                accept=".docx,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                class="rounded-md border border-input bg-background px-3 py-2 text-sm"
                :disabled="busy"
                @change="handleFileChange"
              >
            </label>
            <div class="flex flex-wrap justify-end gap-2">
              <Button type="button" variant="outline" :disabled="busy" @click="emit('close')">
                {{ t('sectionPage.questionImport.cancelAction') }}
              </Button>
              <Button type="button" :disabled="uploadDisabled" @click="submitUpload">
                <Upload class="size-4" />
                {{ t('sectionPage.questionImport.uploadAction') }}
              </Button>
            </div>
          </div>

          <div v-else class="grid gap-4 lg:grid-cols-[280px_minmax(0,1fr)_320px]">
            <aside class="grid content-start gap-2 rounded-lg border bg-background p-3">
              <div class="flex items-center justify-between gap-2">
                <h3 class="text-sm font-semibold">
                  {{ t('sectionPage.questionImport.candidateListTitle') }}
                </h3>
                <span class="text-xs text-muted-foreground">
                  {{ t('sectionPage.questionImport.candidateCount', { count: candidates.length }) }}
                </span>
              </div>
              <button
                v-for="candidate in candidates"
                :key="candidate.candidateId"
                type="button"
                class="grid gap-1 rounded-md border px-3 py-2 text-left text-sm hover:bg-muted/50"
                :class="candidate.candidateId === selectedCandidateId ? 'bg-muted' : 'bg-card'"
                :disabled="busy"
                @click="selectedCandidateId = candidate.candidateId"
              >
                <span class="font-medium">
                  {{ t('sectionPage.questionImport.candidateTitle', { index: candidate.sortOrder }) }}
                </span>
                <span class="text-xs" :class="parseToneByStatus[candidate.parseStatus]">
                  {{ candidate.parseStatus }}
                </span>
                <span v-if="candidate.parseMessage" class="text-xs text-muted-foreground">
                  {{ candidate.parseMessage }}
                </span>
              </button>
            </aside>

            <section class="grid min-h-[28rem] content-start gap-3 rounded-lg border bg-background p-3">
              <div class="flex items-center gap-2 border-b pb-2">
                <FileText class="size-4 text-muted-foreground" />
                <h3 class="text-sm font-semibold">
                  {{ t('sectionPage.questionImport.previewTitle') }}
                </h3>
              </div>
              <div
                v-if="selectedCandidate?.htmlPreview"
                class="prose max-w-none text-sm"
                v-html="selectedCandidate.htmlPreview"
              />
              <p v-else class="text-sm text-muted-foreground">
                {{ t('sectionPage.questionImport.previewEmpty') }}
              </p>
            </section>

            <aside class="grid content-start gap-3 rounded-lg border bg-background p-3">
              <h3 class="text-sm font-semibold">
                {{ t('sectionPage.questionImport.metadataTitle') }}
              </h3>
              <label class="grid gap-1 text-sm">
                <span>{{ t('sectionPage.questionImport.titleLabel') }}</span>
                <input
                  v-model="metadata.title"
                  class="rounded-md border border-input bg-background px-3 py-2"
                  :placeholder="t('sectionPage.questionImport.titlePlaceholder')"
                  :disabled="busy"
                >
              </label>
              <label class="grid gap-1 text-sm">
                <span>{{ t('sectionPage.questionImport.summaryLabel') }}</span>
                <textarea
                  v-model="metadata.summary"
                  rows="3"
                  class="rounded-md border border-input bg-background px-3 py-2"
                  :disabled="busy"
                />
              </label>
              <label class="grid gap-1 text-sm">
                <span>{{ t('sectionPage.questionImport.difficultyLabel') }}</span>
                <select
                  v-model="metadata.difficulty"
                  class="rounded-md border border-input bg-background px-3 py-2"
                  :disabled="busy"
                >
                  <option value="Basic">{{ t('sectionPage.questionImport.difficulty.basic') }}</option>
                  <option value="Medium">{{ t('sectionPage.questionImport.difficulty.medium') }}</option>
                  <option value="Advanced">{{ t('sectionPage.questionImport.difficulty.advanced') }}</option>
                  <option value="Top">{{ t('sectionPage.questionImport.difficulty.top') }}</option>
                </select>
              </label>
              <label class="grid gap-1 text-sm">
                <span>{{ t('sectionPage.questionImport.questionTypeLabel') }}</span>
                <select
                  v-model="metadata.questionType"
                  class="rounded-md border border-input bg-background px-3 py-2"
                  :disabled="busy"
                >
                  <option value="Unset">{{ t('sectionPage.questionImport.questionType.unset') }}</option>
                  <option value="Choice">{{ t('sectionPage.questionImport.questionType.choice') }}</option>
                  <option value="Blank">{{ t('sectionPage.questionImport.questionType.blank') }}</option>
                  <option value="Calculation">{{ t('sectionPage.questionImport.questionType.calculation') }}</option>
                  <option value="Experiment">{{ t('sectionPage.questionImport.questionType.experiment') }}</option>
                  <option value="Diagram">{{ t('sectionPage.questionImport.questionType.diagram') }}</option>
                  <option value="Composite">{{ t('sectionPage.questionImport.questionType.composite') }}</option>
                </select>
              </label>
              <div class="grid gap-2 border-t pt-3">
                <Button type="button" :disabled="confirmDisabled" @click="submitConfirm">
                  {{ t('sectionPage.questionImport.confirmAction') }}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  :disabled="busy"
                  @click="emit('cancelSession')"
                >
                  {{ t('sectionPage.questionImport.cancelSessionAction') }}
                </Button>
              </div>
            </aside>
          </div>

          <p
            v-if="feedbackMessage"
            class="rounded-md border bg-muted/30 px-3 py-2 text-sm text-muted-foreground"
            role="status"
          >
            {{ feedbackMessage }}
          </p>
          <p
            v-if="errorMessage"
            class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
            role="alert"
          >
            {{ errorMessage }}
          </p>
        </div>
      </section>
    </div>
  </Teleport>
</template>
