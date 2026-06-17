<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import BasicTreeNodeView from '@/components/presentation/BasicTreeNodeView.vue'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
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

const metaItems = computed(() => {
  const items = [props.node.typeLabel]

  if (typeof props.node.questionCount === 'number') {
    items.push(t('components.sectionTree.questionCount', { count: props.node.questionCount }))
  }

  return items
})

const difficultyMarkerClass = computed(() => getDifficultyMarkerClass(props.node.difficulty))
</script>

<template>
  <BasicTreeNodeView
    :title="displayTitle"
    :marker-label="node.difficulty"
    :marker-class="difficultyMarkerClass"
    :meta-items="metaItems"
  />
</template>
