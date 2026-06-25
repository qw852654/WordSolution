<script setup lang="ts">
import { computed } from 'vue'
import { Check } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { cn } from '@/lib/utils'
import { tagColorSwatchClasses, tagColorTokens } from '@/components/business/tagColorTone'
import type { TagColorToken } from '@/types'

const props = withDefaults(defineProps<{
  modelValue: TagColorToken
  disabled?: boolean
}>(), {
  disabled: false,
})

const emit = defineEmits<{
  'update:modelValue': [color: TagColorToken]
}>()

const { t } = useI18n()

const options = computed(() =>
  tagColorTokens.map((color) => ({
    color,
    label: t(`tag.colors.${color}`),
  })),
)
</script>

<template>
  <div class="grid gap-2">
    <span class="text-sm font-medium">
      {{ t('tag.color') }}
    </span>
    <div class="flex flex-wrap gap-2" role="radiogroup" :aria-label="t('tag.color')">
      <button
        v-for="option in options"
        :key="option.color"
        type="button"
        role="radio"
        class="inline-flex h-8 items-center gap-2 rounded-md border px-2 text-xs transition-colors hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
        :class="modelValue === option.color ? 'border-primary bg-accent text-foreground' : 'border-border bg-background text-muted-foreground'"
        :aria-checked="modelValue === option.color"
        :disabled="disabled"
        @click="emit('update:modelValue', option.color)"
      >
        <span
          :class="cn('size-3 rounded-full border border-border', tagColorSwatchClasses[option.color])"
          aria-hidden="true"
        />
        <span>{{ option.label }}</span>
        <Check v-if="modelValue === option.color" class="size-3" aria-hidden="true" />
      </button>
    </div>
  </div>
</template>
