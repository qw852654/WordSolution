<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import TeachingNoteBindingTargetView from '@/components/business/TeachingNoteBindingTargetView.vue'
import type { TeachingNoteBindingModel } from '@/types'

const props = withDefaults(defineProps<{
  bindings: TeachingNoteBindingModel[]
  maxVisible?: number
}>(), {
  maxVisible: 3,
})

const { t } = useI18n()

const visibleBindings = computed(() => props.bindings.slice(0, props.maxVisible))
const hiddenCount = computed(() => Math.max(props.bindings.length - visibleBindings.value.length, 0))
</script>

<template>
  <div class="flex min-w-0 flex-wrap items-center gap-1">
    <TeachingNoteBindingTargetView
      v-for="binding in visibleBindings"
      :key="`${binding.targetType}:${binding.targetId}`"
      :binding="binding"
      compact
    />
    <span v-if="hiddenCount" class="text-xs text-muted-foreground">
      {{ t('teachingNote.moreBindings', { count: hiddenCount }) }}
    </span>
  </div>
</template>
