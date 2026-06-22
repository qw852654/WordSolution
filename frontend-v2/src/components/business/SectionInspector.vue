<script setup lang="ts">
import { computed } from 'vue'
import { PackageOpen } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import type { SectionTreeNodeModel } from '@/types'

const props = defineProps<{
  node?: SectionTreeNodeModel
  variantItemCount?: number
}>()

const { t } = useI18n()

const displayTitle = computed(() => {
  if (!props.node) {
    return ''
  }

  if (props.node.kind === 'ContentBlock' || props.node.kind === 'CompositeBlock') {
    return props.node.typeLabel
  }

  return props.node.title
})

const kindLabel = computed(() => {
  if (!props.node) {
    return ''
  }

  return t(`components.sectionTree.kind.${props.node.kind}`)
})

const detailRows = computed(() => {
  const node = props.node
  if (!node) {
    return []
  }

  const rows = [
    {
      id: 'kind',
      label: t('components.sectionInspector.kind'),
      value: kindLabel.value,
    },
    {
      id: 'type',
      label: t('components.sectionInspector.type'),
      value: node.typeLabel || t('components.sectionInspector.notSet'),
    },
    {
      id: 'difficulty',
      label: t('components.sectionInspector.difficulty'),
      value: node.difficulty || t('components.sectionInspector.notSet'),
    },
    {
      id: 'status',
      label: t('components.sectionInspector.status'),
      value: node.status || t('components.sectionInspector.notSet'),
    },
  ]

  if (typeof node.itemCount === 'number') {
    rows.push({
      id: 'itemCount',
      label: t('components.sectionInspector.itemCount'),
      value: t('components.sectionTree.itemCount', { count: node.itemCount }),
    })
  }

  if (typeof node.questionCount === 'number') {
    rows.push({
      id: 'questionCount',
      label: t('components.sectionInspector.questionCount'),
      value: t('components.sectionTree.questionCount', { count: node.questionCount }),
    })
  }

  if (node.kind === 'SectionVariant' && typeof props.variantItemCount === 'number') {
    rows.push({
      id: 'variantItemCount',
      label: t('components.sectionInspector.variantItemCount'),
      value: t('components.sectionInspector.variantItemCountValue', {
        count: props.variantItemCount,
      }),
    })
  }

  rows.push({
    id: 'disabled',
    label: t('components.sectionInspector.disabled'),
    value: node.disabled ? t('components.sectionInspector.yes') : t('components.sectionInspector.no'),
  })

  return rows
})
</script>

<template>
  <EmptyState
    v-if="!node"
    class="h-full"
    :title="t('components.sectionInspector.emptyTitle')"
    :description="t('components.sectionInspector.emptyDescription')"
  >
    <template #icon>
      <PackageOpen class="size-5" aria-hidden="true" />
    </template>
  </EmptyState>

  <Card v-else class="flex h-full min-h-0 flex-col overflow-hidden border">
    <CardHeader class="gap-2 px-4 py-3">
      <div class="flex min-w-0 items-start justify-between gap-3">
        <div class="min-w-0 space-y-1">
          <p class="text-xs text-muted-foreground">{{ t('components.sectionInspector.currentSelection') }}</p>
          <CardTitle class="truncate text-sm">{{ displayTitle }}</CardTitle>
        </div>
        <StatusPill :label="kindLabel" :tone="node.disabled ? 'muted' : 'active'" />
      </div>
    </CardHeader>

    <WeakScrollArea class="space-y-2 px-4 pb-4">
      <dl class="grid gap-2 text-sm">
        <div
          v-for="row in detailRows"
          :key="row.id"
          class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2"
        >
          <dt class="text-xs text-muted-foreground">{{ row.label }}</dt>
          <dd class="truncate font-medium">{{ row.value }}</dd>
        </div>
      </dl>
    </WeakScrollArea>

    <CardFooter class="flex flex-wrap gap-2 border-t px-4 py-3">
      <Button type="button" size="sm" variant="outline" disabled>
        {{ t('components.sectionInspector.preview') }}
      </Button>
      <Button type="button" size="sm" variant="outline" disabled>
        {{ t('components.sectionInspector.openWord') }}
      </Button>
    </CardFooter>
  </Card>
</template>
