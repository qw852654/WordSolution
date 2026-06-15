<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import type { SectionTreeNodeModel } from '@/types'

const props = defineProps<{
  node: SectionTreeNodeModel
  selected?: boolean
}>()

const { t } = useI18n()

const displayTitle = computed(() => {
  if (props.node.kind === 'ContentBlock' || props.node.kind === 'CompositeBlock') {
    return props.node.typeLabel
  }

  return props.node.title
})
</script>

<template>
  <span class="grid w-full min-w-0 grid-cols-[minmax(0,1fr)_auto] items-center gap-2">
    <span class="flex min-w-0 items-center gap-1.5">
      <span
        v-if="node.difficulty"
        class="h-3 w-0.5 shrink-0 rounded-full bg-primary"
        :title="node.difficulty"
        aria-hidden="true"
      />
      <span class="min-w-0 truncate font-medium text-foreground">
        {{ displayTitle }}
      </span>
    </span>

    <span class="flex min-w-0 shrink-0 items-center gap-1.5 text-xs text-muted-foreground">
      <span class="whitespace-nowrap">{{ node.typeLabel }}</span>
      <span v-if="typeof node.questionCount === 'number'" aria-hidden="true">·</span>
      <span v-if="typeof node.questionCount === 'number'" class="whitespace-nowrap">
        {{ t('components.sectionTree.questionCount', { count: node.questionCount }) }}
      </span>
    </span>
  </span>
</template>
