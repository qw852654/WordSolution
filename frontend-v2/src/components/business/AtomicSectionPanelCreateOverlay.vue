<script setup lang="ts">
import { computed, reactive, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import CreateOverlayShell from '@/components/containers/CreateOverlayShell.vue'
import type {
  AtomicSectionPanelCreateOverlayModel,
  AtomicSectionPanelCreateSubmitPayload,
  AtomicSectionTeachingRole,
} from '@/types'

const teachingRoleOptions: AtomicSectionTeachingRole[] = [
  'Knowledge',
  'Example',
  'Variant',
  'Practice',
  'Homework',
  'PreClassQuiz',
]

const difficultyOptions = ['Unset', 'Basic', 'Medium', 'Advanced', 'Top']

const props = defineProps<{
  model: AtomicSectionPanelCreateOverlayModel
  open: boolean
  busy?: boolean
  errorMessage?: string
}>()

const emit = defineEmits<{
  cancel: []
  submit: [payload: AtomicSectionPanelCreateSubmitPayload]
}>()

const { t } = useI18n()

const form = reactive({
  title: '',
  teachingRole: 'Knowledge' as AtomicSectionTeachingRole,
  difficulty: 'Basic',
})

const canSubmit = computed(() => !props.model.disabled && form.title.trim().length > 0)
const showTitleRequired = computed(() => !props.model.disabled && form.title.trim().length === 0)

function resetForm() {
  form.title = props.model.defaultTitle
  form.teachingRole = 'Knowledge'
  form.difficulty = 'Basic'
}

function submit() {
  const title = form.title.trim()

  if (!canSubmit.value || !title) {
    return
  }

  emit('submit', {
    nodeId: props.model.nodeId,
    atomicSectionId: props.model.atomicSectionId,
    title,
    teachingRole: form.teachingRole,
    difficulty: form.difficulty,
    beforeAtomicSectionPanelId: props.model.beforeAtomicSectionPanelId,
    afterAtomicSectionPanelId: props.model.afterAtomicSectionPanelId,
  })
}

watch(
  [
    () => props.open,
    () => props.model.nodeId,
    () => props.model.atomicSectionId,
    () => props.model.defaultTitle,
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
    :dialog-label="t('components.atomicSectionPanelCreateOverlay.dialogLabel')"
    :eyebrow="t('components.atomicSectionPanelCreateOverlay.eyebrow')"
    :title="t('components.atomicSectionPanelCreateOverlay.title')"
    :description="t('components.atomicSectionPanelCreateOverlay.description')"
    :position-label="model.atomicSectionTitle"
    :cancel-label="t('components.atomicSectionPanelCreateOverlay.cancel')"
    :submit-label="t('components.atomicSectionPanelCreateOverlay.submit')"
    :submit-disabled="!canSubmit"
    :busy="busy"
    :error-message="errorMessage"
    @cancel="emit('cancel')"
    @submit="submit"
  >
    <div class="grid gap-3">
      <label class="grid gap-1 text-sm font-medium">
        <span>{{ t('components.atomicSectionPanelCreateOverlay.titleLabel') }}</span>
        <input
          v-model="form.title"
          :disabled="model.disabled || busy"
          class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50"
          :placeholder="t('components.atomicSectionPanelCreateOverlay.titlePlaceholder')"
          :aria-invalid="showTitleRequired"
        />
        <span v-if="showTitleRequired" class="text-xs text-muted-foreground">
          {{ t('components.atomicSectionPanelCreateOverlay.titleRequired') }}
        </span>
      </label>

      <label class="grid gap-1 text-sm font-medium">
        <span>{{ t('components.atomicSectionPanelCreateOverlay.teachingRoleLabel') }}</span>
        <select
          v-model="form.teachingRole"
          :disabled="model.disabled || busy"
          class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option v-for="role in teachingRoleOptions" :key="role" :value="role">
            {{ t(`components.atomicSectionTeachingRole.${role}`) }}
          </option>
        </select>
      </label>

      <label class="grid gap-1 text-sm font-medium">
        <span>{{ t('components.atomicSectionPanelCreateOverlay.difficultyLabel') }}</span>
        <select
          v-model="form.difficulty"
          :disabled="model.disabled || busy"
          class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30 disabled:cursor-not-allowed disabled:opacity-50"
        >
          <option v-for="difficulty in difficultyOptions" :key="difficulty" :value="difficulty">
            {{ t(`common.difficulty.${difficulty}`) }}
          </option>
        </select>
      </label>
    </div>
  </CreateOverlayShell>
</template>
