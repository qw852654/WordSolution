<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import type {
  SectionVariantCreateDifficulty,
  SectionVariantCreateMetadata,
  SectionVariantCreateType,
} from '@/types'

const variantTypeOptions: SectionVariantCreateType[] = [
  'Lecture',
  'Exercise',
  'Homework',
  'Review',
  'ExamTraining',
  'Custom',
]

const difficultyOptions: SectionVariantCreateDifficulty[] = [
  'Basic',
  'Medium',
  'Advanced',
  'Top',
]

const props = defineProps<{
  modelValue: SectionVariantCreateMetadata
  showValidation?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: SectionVariantCreateMetadata]
}>()

const { t } = useI18n()

const titleIsEmpty = computed(() => props.modelValue.title.trim().length === 0)

function updateMetadata(patch: Partial<SectionVariantCreateMetadata>) {
  emit('update:modelValue', {
    ...props.modelValue,
    ...patch,
  })
}
</script>

<template>
  <div class="grid gap-4">
    <label class="grid gap-1 text-sm font-medium">
      <span>{{ t('components.sectionVariantCreate.fields.title') }}</span>
      <input
        :value="modelValue.title"
        class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30"
        :placeholder="t('components.sectionVariantCreate.placeholders.title')"
        :aria-invalid="showValidation && titleIsEmpty"
        @input="updateMetadata({ title: ($event.target as HTMLInputElement).value })"
      />
      <span v-if="showValidation && titleIsEmpty" class="text-xs text-muted-foreground">
        {{ t('components.sectionVariantCreate.validation.titleRequired') }}
      </span>
    </label>

    <div class="grid gap-3 sm:grid-cols-2">
      <label class="grid gap-1 text-sm font-medium">
        <span>{{ t('components.sectionVariantCreate.fields.type') }}</span>
        <select
          :value="modelValue.type"
          class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30"
          @change="
            updateMetadata({
              type: ($event.target as HTMLSelectElement).value as SectionVariantCreateType,
            })
          "
        >
          <option v-for="variantType in variantTypeOptions" :key="variantType" :value="variantType">
            {{ t(`components.sectionVariantCreate.types.${variantType}`) }}
          </option>
        </select>
      </label>

      <label class="grid gap-1 text-sm font-medium">
        <span>{{ t('components.sectionVariantCreate.fields.difficulty') }}</span>
        <select
          :value="modelValue.difficulty"
          class="h-9 rounded-md border bg-background px-3 text-sm text-foreground outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30"
          @change="
            updateMetadata({
              difficulty: ($event.target as HTMLSelectElement)
                .value as SectionVariantCreateDifficulty,
            })
          "
        >
          <option
            v-for="difficulty in difficultyOptions"
            :key="difficulty"
            :value="difficulty"
          >
            {{ t(`components.sectionVariantCreate.difficulties.${difficulty}`) }}
          </option>
        </select>
      </label>
    </div>

    <label class="grid gap-1 text-sm font-medium">
      <span>{{ t('components.sectionVariantCreate.fields.description') }}</span>
      <textarea
        :value="modelValue.description"
        class="min-h-20 resize-y rounded-md border bg-background px-3 py-2 text-sm text-foreground outline-none transition-colors placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/30"
        :placeholder="t('components.sectionVariantCreate.placeholders.description')"
        @input="updateMetadata({ description: ($event.target as HTMLTextAreaElement).value })"
      />
    </label>
  </div>
</template>
