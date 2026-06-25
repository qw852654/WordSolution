<script setup lang="ts">
import { computed } from 'vue'
import { Link2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import type { TeachingNoteBindingModel } from '@/types'

const props = withDefaults(defineProps<{
  binding: TeachingNoteBindingModel
  compact?: boolean
}>(), {
  compact: false,
})

const { t } = useI18n()

const targetLabel = computed(() =>
  t(`teachingNote.targetType.${props.binding.targetType}`),
)
</script>

<template>
  <span
    class="inline-flex max-w-full items-center gap-1 rounded-md border bg-muted/30 px-2 py-1 text-xs text-muted-foreground"
    :title="`${targetLabel} #${binding.targetId}`"
  >
    <Link2 v-if="!compact" class="size-3 shrink-0" aria-hidden="true" />
    <span class="truncate">{{ targetLabel }}</span>
    <span class="shrink-0">#{{ binding.targetId }}</span>
  </span>
</template>
