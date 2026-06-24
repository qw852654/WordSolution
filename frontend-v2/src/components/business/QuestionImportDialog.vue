<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { CheckCircle2, CircleAlert, FileText, Loader2, RefreshCw, RotateCcw, X } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import type {
  CmsV2QuestionImportCandidateDto,
  CmsV2QuestionImportSessionDto,
  CmsV2QuestionImportSessionStatus,
} from '@/apis/cmsV2Client'
import type { QuestionImportCandidateSelectionPayload, QuestionImportContext } from '@/types'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'

const props = withDefaults(
  defineProps<{
    open: boolean
    importContext: QuestionImportContext
    session?: CmsV2QuestionImportSessionDto | null
    candidates?: CmsV2QuestionImportCandidateDto[]
    busy?: boolean
    candidatesLoading?: boolean
    errorMessage?: string
    feedbackMessage?: string
  }>(),
  {
    candidates: () => [],
    busy: false,
    candidatesLoading: false,
    errorMessage: '',
    feedbackMessage: '',
  },
)

const emit = defineEmits<{
  close: []
  startSession: []
  reopenSession: []
  confirmCandidates: [payload: QuestionImportCandidateSelectionPayload[]]
  cancelSession: []
}>()

const { t } = useI18n()
const selectedCandidateId = ref<string>()
const candidateDrafts = reactive<Record<string, QuestionImportCandidateSelectionPayload>>({})

const candidates = computed(() =>
  props.candidates.length ? props.candidates : props.session?.candidates ?? [],
)
const selectedCandidate = computed(() =>
  candidates.value.find((candidate) => candidate.candidateId === selectedCandidateId.value),
)
const selectedCandidateDraft = computed(() =>
  selectedCandidate.value ? getCandidateDraft(selectedCandidate.value) : undefined,
)
const selectedCandidateTitle = computed(() => selectedCandidateDraft.value?.title ?? '')
const selectedCount = computed(
  () => candidates.value.filter((candidate) => getCandidateDraft(candidate).selected).length,
)
const hasSession = computed(() => Boolean(props.session))
const sessionStatus = computed<CmsV2QuestionImportSessionStatus | undefined>(
  () => props.session?.status,
)
const isReviewReady = computed(() => sessionStatus.value === 'ReadyForReview')
const hasTerminalSession = computed(() =>
  ['Imported', 'Failed', 'Cancelled', 'Expired'].includes(sessionStatus.value ?? ''),
)
const canReopenSession = computed(
  () =>
    Boolean(props.session) &&
    !props.busy &&
    !['Importing', 'Imported', 'Cancelled', 'Expired'].includes(sessionStatus.value ?? ''),
)
const canConfirmCandidates = computed(
  () => isReviewReady.value && !props.busy && !props.candidatesLoading && selectedCount.value > 0,
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
const sessionStatusTone = computed<'neutral' | 'active' | 'muted' | 'danger'>(() => {
  switch (sessionStatus.value) {
    case 'ReadyForReview':
    case 'Imported':
      return 'active'
    case 'Failed':
    case 'Cancelled':
    case 'Expired':
      return 'danger'
    case 'Editing':
    case 'Opening':
    case 'Parsing':
    case 'Importing':
      return 'muted'
    default:
      return 'neutral'
  }
})
const waitingIcon = computed(() => {
  if (sessionStatus.value === 'ReadyForReview' || sessionStatus.value === 'Imported') {
    return CheckCircle2
  }

  if (hasTerminalSession.value) {
    return CircleAlert
  }

  return Loader2
})
const selectedCandidateWarnings = computed(() => {
  const candidate = selectedCandidate.value
  if (!candidate) {
    return []
  }

  return candidate.parts
    .filter((part) => Boolean(part.warningMessage))
    .map((part) => `${getPartLabel(part.partType)}: ${part.warningMessage}`)
})
const parseToneByStatus = {
  Parsed: 'text-muted-foreground',
  ParsedWithWarnings: 'text-muted-foreground',
  Failed: 'text-destructive',
  NotApplicable: 'text-muted-foreground',
} satisfies Record<NonNullable<CmsV2QuestionImportCandidateDto['parseStatus']>, string>

function getCandidateDraft(candidate: CmsV2QuestionImportCandidateDto) {
  if (!candidateDrafts[candidate.candidateId]) {
    candidateDrafts[candidate.candidateId] = {
      candidateId: candidate.candidateId,
      selected: true,
      title: '',
    }
  }

  return candidateDrafts[candidate.candidateId]
}

function getSessionStatusLabel(status?: CmsV2QuestionImportSessionStatus) {
  return status ? t(`sectionPage.questionImport.status.${status}`) : t('sectionPage.questionImport.noSession')
}

function getParseStatusLabel(status: CmsV2QuestionImportCandidateDto['parseStatus']) {
  return t(`sectionPage.questionImport.parseStatus.${status}`)
}

function getPartLabel(partType: CmsV2QuestionImportCandidateDto['parts'][number]['partType']) {
  return t(`sectionPage.questionImport.part.${partType}`)
}

function selectCandidate(candidateId: string) {
  selectedCandidateId.value = candidateId
}

function handleCandidateSelectionChange(
  candidate: CmsV2QuestionImportCandidateDto,
  event: Event,
) {
  getCandidateDraft(candidate).selected = (event.target as HTMLInputElement).checked
}

function handleSelectedTitleInput(event: Event) {
  if (!selectedCandidate.value) {
    return
  }

  getCandidateDraft(selectedCandidate.value).title = (event.target as HTMLInputElement).value
}

function submitConfirmCandidates() {
  if (!canConfirmCandidates.value) {
    return
  }

  emit(
    'confirmCandidates',
    candidates.value.map((candidate) => ({
      candidateId: candidate.candidateId,
      selected: getCandidateDraft(candidate).selected,
      title: getCandidateDraft(candidate).title.trim(),
    })),
  )
}

watch(
  candidates,
  (value) => {
    const candidateIds = new Set(value.map((candidate) => candidate.candidateId))

    for (const candidate of value) {
      getCandidateDraft(candidate)
    }

    for (const candidateId of Object.keys(candidateDrafts)) {
      if (!candidateIds.has(candidateId)) {
        delete candidateDrafts[candidateId]
      }
    }

    if (!value.some((candidate) => candidate.candidateId === selectedCandidateId.value)) {
      selectedCandidateId.value = value[0]?.candidateId
    }
  },
  { immediate: true },
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
            :aria-label="t('sectionPage.questionImport.closeLabel')"
            :disabled="busy"
            @click="emit('close')"
          >
            <X class="size-4" />
          </Button>
        </header>

        <div class="grid max-h-[calc(100vh-10rem)] gap-4 overflow-auto p-4">
          <section class="grid gap-3 rounded-lg border bg-background p-4">
            <div class="flex flex-wrap items-center justify-between gap-3">
              <div class="min-w-0">
                <p class="text-sm font-medium">
                  {{ t('sectionPage.questionImport.sessionTitle') }}
                </p>
                <p class="mt-1 text-xs text-muted-foreground">
                  {{ props.session?.sessionId ?? t('sectionPage.questionImport.noSession') }}
                </p>
              </div>
              <StatusPill
                v-if="sessionStatus"
                :label="getSessionStatusLabel(sessionStatus)"
                :tone="sessionStatusTone"
              />
            </div>

            <p
              v-if="props.session?.message"
              class="rounded-md border bg-muted/20 px-3 py-2 text-sm text-muted-foreground"
            >
              {{ props.session.message }}
            </p>

            <div class="flex flex-wrap justify-end gap-2">
              <Button
                v-if="!hasSession"
                type="button"
                :disabled="busy"
                @click="emit('startSession')"
              >
                <FileText class="size-4" />
                {{ t('sectionPage.questionImport.startSessionAction') }}
              </Button>
              <template v-else>
                <Button
                  type="button"
                  variant="outline"
                  :disabled="!canReopenSession"
                  @click="emit('reopenSession')"
                >
                  <RotateCcw class="size-4" />
                  {{ t('sectionPage.questionImport.reopenSessionAction') }}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  :disabled="busy"
                  @click="emit('cancelSession')"
                >
                  <X class="size-4" />
                  {{ t('sectionPage.questionImport.cancelSessionAction') }}
                </Button>
              </template>
            </div>
          </section>

          <section
            v-if="hasSession && !isReviewReady"
            class="grid min-h-[16rem] place-items-center rounded-lg border bg-background p-6 text-center"
          >
            <component
              :is="waitingIcon"
              class="size-7 text-muted-foreground"
              :class="!hasTerminalSession ? 'animate-spin' : ''"
            />
            <div class="mt-3 grid gap-1">
              <p class="text-sm font-medium">
                {{ getSessionStatusLabel(sessionStatus) }}
              </p>
              <p class="text-sm text-muted-foreground">
                {{ t('sectionPage.questionImport.statusHint') }}
              </p>
            </div>
          </section>

          <div v-if="hasSession && isReviewReady" class="grid gap-4 lg:grid-cols-[280px_minmax(0,1fr)_320px]">
            <aside class="grid content-start gap-2 rounded-lg border bg-background p-3">
              <div class="flex items-center justify-between gap-2">
                <h3 class="text-sm font-semibold">
                  {{ t('sectionPage.questionImport.candidateListTitle') }}
                </h3>
                <span class="text-xs text-muted-foreground">
                  {{ t('sectionPage.questionImport.selectedCount', { selected: selectedCount, total: candidates.length }) }}
                </span>
              </div>

              <p
                v-if="candidatesLoading"
                class="rounded-md border bg-muted/20 px-3 py-2 text-sm text-muted-foreground"
              >
                <Loader2 class="mr-1 inline size-4 animate-spin" />
                {{ t('sectionPage.questionImport.candidatesLoading') }}
              </p>

              <p
                v-else-if="!candidates.length"
                class="rounded-md border bg-muted/20 px-3 py-2 text-sm text-muted-foreground"
              >
                {{ t('sectionPage.questionImport.candidatesEmpty') }}
              </p>

              <template v-else>
                <div
                  v-for="candidate in candidates"
                  :key="candidate.candidateId"
                  class="grid gap-2 rounded-md border px-3 py-2"
                  :class="candidate.candidateId === selectedCandidateId ? 'bg-muted' : 'bg-card'"
                >
                  <label class="flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      class="size-4 rounded border-input"
                      :checked="getCandidateDraft(candidate).selected"
                      :disabled="busy"
                      @change="handleCandidateSelectionChange(candidate, $event)"
                    >
                    <span class="font-medium">
                      {{ t('sectionPage.questionImport.candidateTitle', { index: candidate.sortOrder }) }}
                    </span>
                  </label>
                  <button
                    type="button"
                    class="grid gap-1 text-left text-xs text-muted-foreground"
                    :disabled="busy"
                    @click="selectCandidate(candidate.candidateId)"
                  >
                    <span :class="parseToneByStatus[candidate.parseStatus]">
                      {{ getParseStatusLabel(candidate.parseStatus) }}
                    </span>
                    <span v-if="candidate.parseMessage">
                      {{ candidate.parseMessage }}
                    </span>
                  </button>
                </div>
              </template>
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
              <div class="flex items-center justify-between gap-2">
                <h3 class="text-sm font-semibold">
                  {{ t('sectionPage.questionImport.metadataTitle') }}
                </h3>
                <RefreshCw v-if="props.busy" class="size-4 animate-spin text-muted-foreground" />
              </div>

              <label class="grid gap-1 text-sm">
                <span>{{ t('sectionPage.questionImport.titleLabel') }}</span>
                <input
                  :value="selectedCandidateTitle"
                  class="rounded-md border border-input bg-background px-3 py-2"
                  :placeholder="t('sectionPage.questionImport.titlePlaceholder')"
                  :disabled="busy || !selectedCandidate"
                  @input="handleSelectedTitleInput"
                >
              </label>

              <div v-if="selectedCandidate" class="grid gap-2 text-sm">
                <p class="font-medium">
                  {{ t('sectionPage.questionImport.partStatusTitle') }}
                </p>
                <div class="grid gap-1">
                  <p
                    v-for="part in selectedCandidate.parts"
                    :key="`${part.partType}-${part.sortOrder}`"
                    class="rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
                  >
                    <span class="font-medium text-foreground">{{ getPartLabel(part.partType) }}</span>
                    <span v-if="part.warningMessage" class="ml-1 text-destructive">
                      {{ part.warningMessage }}
                    </span>
                  </p>
                </div>
              </div>

              <div v-if="selectedCandidateWarnings.length" class="grid gap-1 text-sm">
                <p class="font-medium text-destructive">
                  {{ t('sectionPage.questionImport.warningTitle') }}
                </p>
                <p
                  v-for="warning in selectedCandidateWarnings"
                  :key="warning"
                  class="rounded-md border border-destructive/30 bg-destructive/10 px-2 py-1 text-xs text-destructive"
                >
                  {{ warning }}
                </p>
              </div>

              <div class="grid gap-2 border-t pt-3">
                <Button type="button" :disabled="!canConfirmCandidates" @click="submitConfirmCandidates">
                  <CheckCircle2 class="size-4" />
                  {{ t('sectionPage.questionImport.confirmAction') }}
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
