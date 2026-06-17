<script setup lang="ts">
import { computed, reactive, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type {
  InsertCreateContentBlockType,
  InsertCreateDifficulty,
  InsertCreatePanelModel,
  InsertCreateSubmitPayload,
} from '@/types'

const contentBlockTypeOptions: InsertCreateContentBlockType[] = [
  '知识点',
  '例题',
  '变式题',
  '练习题',
  '变式题组',
  '练习题组',
]

const difficultyOptions: InsertCreateDifficulty[] = ['基础', '中档', '提高', '压轴']

const props = defineProps<{
  model: InsertCreatePanelModel
  open: boolean
}>()

const emit = defineEmits<{
  cancel: [insertPointId: string]
  submit: [payload: InsertCreateSubmitPayload]
}>()

const { t } = useI18n()

const form = reactive<{
  title: string
  contentBlockType: InsertCreateContentBlockType
  difficulty: InsertCreateDifficulty
  note: string
}>({
  title: '',
  contentBlockType: contentBlockTypeOptions[0],
  difficulty: difficultyOptions[0],
  note: '',
})

const isContentBlock = computed(() => props.model.targetType === 'ContentBlock')
const canSubmit = computed(() => !props.model.disabled && (isContentBlock.value || form.title.trim().length > 0))
const showTitleRequired = computed(
  () => !props.model.disabled && !isContentBlock.value && form.title.trim().length === 0,
)
const panelTitle = computed(() =>
  isContentBlock.value
    ? t('components.insertCreateOverlay.contentBlockTitle')
    : t('components.insertCreateOverlay.atomicSectionTitle'),
)
const submitLabel = computed(() =>
  isContentBlock.value
    ? t('components.insertCreateOverlay.submitContentBlock')
    : t('components.insertCreateOverlay.submitAtomicSection'),
)

function resetForm() {
  form.title = ''
  form.contentBlockType = contentBlockTypeOptions[0]
  form.difficulty = difficultyOptions[0]
  form.note = ''
}

function handleCancel() {
  emit('cancel', props.model.insertPointId)
}

function handleSubmit() {
  const title = form.title.trim()

  if (props.model.disabled || (!isContentBlock.value && !title)) {
    return
  }

  emit('submit', {
    insertPointId: props.model.insertPointId,
    targetType: props.model.targetType,
    sectionId: props.model.sectionId,
    insertMode: props.model.insertMode,
    atomicSectionId: props.model.atomicSectionId,
    atomicSectionTitle: props.model.atomicSectionTitle,
    title,
    contentBlockType: isContentBlock.value ? form.contentBlockType : undefined,
    difficulty: form.difficulty,
    note: !isContentBlock.value && form.note.trim() ? form.note.trim() : undefined,
  })
}

watch(
  () => [props.open, props.model.insertPointId, props.model.targetType],
  () => {
    if (props.open) {
      resetForm()
    }
  },
  { immediate: true },
)
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex min-h-screen items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-label="t('components.insertCreateOverlay.dialogLabel')"
    >
      <div class="absolute inset-0 bg-background/70 backdrop-blur-sm" aria-hidden="true" />

      <form
        class="relative z-10 flex w-full max-w-xl flex-col gap-4 rounded-lg border bg-card p-4 text-card-foreground"
        @submit.prevent="handleSubmit"
      >
        <header class="space-y-1">
          <p class="text-xs font-medium text-muted-foreground">
            {{ t('components.insertCreateOverlay.insertPosition') }}
          </p>
          <div class="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between">
            <h2 class="text-lg font-semibold tracking-normal">{{ panelTitle }}</h2>
            <p class="text-sm text-muted-foreground">{{ model.insertPositionLabel }}</p>
          </div>
          <p class="text-sm text-muted-foreground">
            {{ t('components.insertCreateOverlay.description') }}
          </p>
        </header>

        <div class="grid gap-3">
          <label class="grid gap-1 text-sm font-medium">
            <span>{{ t('components.insertCreateOverlay.sectionLabel') }}</span>
            <input
              :value="model.sectionTitle"
              disabled
              class="h-9 cursor-not-allowed rounded-md border bg-muted/30 px-3 text-sm text-muted-foreground outline-none"
            />
          </label>

          <label class="grid gap-1 text-sm font-medium">
            <span>{{ t('components.insertCreateOverlay.titleLabel') }}</span>
            <input
              v-model="form.title"
              :disabled="model.disabled"
              class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50"
              :placeholder="t('components.insertCreateOverlay.titlePlaceholder')"
              :aria-invalid="showTitleRequired"
            />
            <span v-if="showTitleRequired" class="text-xs text-muted-foreground">
              {{ t('components.insertCreateOverlay.titleRequired') }}
            </span>
          </label>

          <label v-if="isContentBlock" class="grid gap-1 text-sm font-medium">
            <span>{{ t('components.insertCreateOverlay.contentBlockTypeLabel') }}</span>
            <select
              v-model="form.contentBlockType"
              :disabled="model.disabled"
              class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option
                v-for="contentBlockType in contentBlockTypeOptions"
                :key="contentBlockType"
                :value="contentBlockType"
              >
                {{ contentBlockType }}
              </option>
            </select>
          </label>

          <label class="grid gap-1 text-sm font-medium">
            <span>{{ t('components.insertCreateOverlay.difficultyLabel') }}</span>
            <select
              v-model="form.difficulty"
              :disabled="model.disabled"
              class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option v-for="difficulty in difficultyOptions" :key="difficulty" :value="difficulty">
                {{ difficulty }}
              </option>
            </select>
          </label>

          <label v-if="!isContentBlock" class="grid gap-1 text-sm font-medium">
            <span>{{ t('components.insertCreateOverlay.noteLabel') }}</span>
            <textarea
              v-model="form.note"
              :disabled="model.disabled"
              class="min-h-20 resize-y rounded-md border bg-background px-3 py-2 text-sm text-foreground outline-none transition-colors placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50"
              :placeholder="t('components.insertCreateOverlay.notePlaceholder')"
            />
          </label>
        </div>

        <footer class="flex justify-end gap-2">
          <Button type="button" variant="outline" @click="handleCancel">
            {{ t('components.insertCreateOverlay.cancel') }}
          </Button>
          <Button type="submit" :disabled="!canSubmit">
            {{ submitLabel }}
          </Button>
        </footer>
      </form>
    </div>
  </Teleport>
</template>
