<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import BasicTreeNodeView from '@/components/presentation/BasicTreeNodeView.vue'
import type { TeachingTopicTreeNodeModel } from '@/types'

const props = defineProps<{
  node: TeachingTopicTreeNodeModel
  truncateTitle?: boolean
}>()

const { t } = useI18n()

const metaItems = computed(() => {
  const items: string[] = []

  if (props.node.kind === 'SectionVariant') {
    items.push('SectionVariant')
  }

  if (props.node.kind !== 'SectionVariant' && props.node.sectionId) {
    items.push('Section')
  }

  if (props.node.status) {
    items.push(props.node.status)
  }

  if (props.node.readOnly) {
    items.push('只读')
  }

  if (props.node.isEmptyTopic) {
    items.push('空主题')
  }

  if (typeof props.node.variantCount === 'number' && props.node.variantCount > 0) {
    items.push(`SectionVariant ${props.node.variantCount}`)
  }

  if (typeof props.node.sectionCount === 'number') {
    items.push(t('components.teachingTopicTree.sectionCount', { count: props.node.sectionCount }))
  }

  if (typeof props.node.handoutCount === 'number') {
    items.push(t('components.teachingTopicTree.handoutCount', { count: props.node.handoutCount }))
  }

  if (props.node.archived) {
    items.push(t('components.teachingTopicTree.archived'))
  }

  return items
})

const markerLabel = computed(() => {
  if (props.node.kind === 'SectionVariant') {
    return 'SectionVariant'
  }

  if (props.node.sectionId) {
    return 'Section'
  }

  return undefined
})

const markerClass = computed(() => {
  if (props.node.kind === 'SectionVariant') {
    return 'bg-muted-foreground'
  }

  if (props.node.sectionId) {
    return 'bg-primary'
  }

  return undefined
})
</script>

<template>
  <BasicTreeNodeView
    :title="node.title"
    :marker-label="markerLabel"
    :marker-class="markerClass"
    :meta-items="metaItems"
    :truncate-title="truncateTitle"
  />
</template>
