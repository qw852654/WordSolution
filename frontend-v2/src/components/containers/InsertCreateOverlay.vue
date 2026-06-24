<script setup lang="ts">
import { computed, reactive, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import CreateOverlayShell from '@/components/containers/CreateOverlayShell.vue'
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

const defaultContentBlockType: InsertCreateContentBlockType = '知识点'
const defaultDifficulty: InsertCreateDifficulty = '基础'
const difficultyOptions: InsertCreateDifficulty[] = ['未设置', '基础', '中档', '提高', '压轴']

const props = defineProps<{
  model: InsertCreatePanelModel
  open: boolean
  errorMessage?: string
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
  contentBlockType: defaultContentBlockType,
  difficulty: defaultDifficulty,
  note: '',
})

const isContentBlock = computed(() => props.model.targetType === 'ContentBlock')
const isWrapAsAtomicSection = computed(() => props.model.insertMode === 'WrapAsAtomicSection')
const canSubmit = computed(() => !props.model.disabled && (isContentBlock.value || form.title.trim().length > 0))
const showTitleRequired = computed(
  () => !props.model.disabled && !isContentBlock.value && form.title.trim().length === 0,
)
const panelTitle = computed(() =>
  isWrapAsAtomicSection.value
    ? t('components.insertCreateOverlay.wrapAtomicSectionTitle')
    : isContentBlock.value
      ? t('components.insertCreateOverlay.contentBlockTitle')
      : t('components.insertCreateOverlay.atomicSectionTitle'),
)
const panelDescription = computed(() =>
  isWrapAsAtomicSection.value
    ? t('components.insertCreateOverlay.wrapDescription')
    : t('components.insertCreateOverlay.description'),
)
const submitLabel = computed(() =>
  isWrapAsAtomicSection.value
    ? t('components.insertCreateOverlay.submitWrapAtomicSection')
    : isContentBlock.value
      ? t('components.insertCreateOverlay.submitContentBlock')
      : t('components.insertCreateOverlay.submitAtomicSection'),
)

function resetForm() {
  form.title = ''
  form.contentBlockType = props.model.defaultContentBlockType ?? defaultContentBlockType
  form.difficulty = props.model.defaultDifficulty ?? defaultDifficulty
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
    placement: props.model.placement,
    insertMode: props.model.insertMode,
    atomicSectionId: props.model.atomicSectionId,
    atomicSectionTitle: props.model.atomicSectionTitle,
    atomicSectionPanelId: props.model.atomicSectionPanelId,
    atomicSectionTeachingRole: props.model.atomicSectionTeachingRole,
    compositeBlockId: props.model.compositeBlockId,
    compositeBlockTitle: props.model.compositeBlockTitle,
    wrapSectionItemIds: props.model.wrapSectionItemIds,
    title,
    contentBlockType: isContentBlock.value ? form.contentBlockType : undefined,
    difficulty: form.difficulty,
    note: !isContentBlock.value && form.note.trim() ? form.note.trim() : undefined,
  })
}

watch(
  [
    () => props.open,
    () => props.model.insertPointId,
    () => props.model.targetType,
    () => props.model.insertMode,
    () => props.model.atomicSectionId,
    () => props.model.defaultContentBlockType,
    () => props.model.defaultDifficulty,
  ],
  () => {
    if (props.open) {
      resetForm()
    }
  },
  { immediate: true },
)
</script>

<template>
  <CreateOverlayShell
    :open="open"
    :dialog-label="t('components.insertCreateOverlay.dialogLabel')"
    :eyebrow="t('components.insertCreateOverlay.insertPosition')"
    :title="panelTitle"
    :description="panelDescription"
    :position-label="model.insertPositionLabel"
    :cancel-label="t('components.insertCreateOverlay.cancel')"
    :submit-label="submitLabel"
    :submit-disabled="!canSubmit"
    :error-message="errorMessage"
    @cancel="handleCancel"
    @submit="handleSubmit"
  >
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
  </CreateOverlayShell>
</template>
