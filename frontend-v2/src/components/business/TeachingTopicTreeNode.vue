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

  if (props.node.status) {
    items.push(props.node.status)
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
</script>

<template>
  <BasicTreeNodeView
    :title="node.title"
    :meta-items="metaItems"
    :truncate-title="truncateTitle"
  />
</template>
