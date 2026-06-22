<script setup lang="ts">
import { computed } from 'vue'
import { PackageOpen, Trash2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import type { SectionPageShellModel, SectionTreeNodeModel } from '@/types'

const props = defineProps<{
  node?: SectionTreeNodeModel
  section?: SectionPageShellModel
  variantItemCount?: number
  deletingContentBlockCascade?: boolean
}>()

const emit = defineEmits<{
  deleteContentBlockCascade: []
}>()

const { t } = useI18n()

const displayTitle = computed(() => {
  if (!props.node) {
    return ''
  }

  if (props.node.kind === 'ContentBlock') {
    return props.node.typeLabel
  }

  return props.node.title || props.node.typeLabel
})

const kindLabel = computed(() => {
  if (!props.node) {
    return ''
  }

  return t(`components.sectionTree.kind.${props.node.kind}`)
})

const showContentBlockCascadeDelete = computed(() =>
  props.node?.kind === 'ContentBlock' || props.node?.kind === 'CompositeBlock',
)

const detailRows = computed(() => {
  const node = props.node
  if (!node) {
    return []
  }

  const notSet = t('components.sectionInspector.notSet')
  const row = (id: string, label: string, value?: string | number | null) => ({
    id,
    label,
    value: value === undefined || value === null || value === '' ? notSet : String(value),
  })
  const previewState = node.previewState
    ? t(`components.contentBlockDisplay.previewState.${node.previewState}`)
    : notSet
  const wordDocumentStatus =
    node.hasWordDocument === undefined
      ? notSet
      : node.hasWordDocument
        ? t('components.sectionInspector.yes')
        : t('components.sectionInspector.no')

  if (node.kind === 'Section') {
    return [
      row('title', t('components.sectionInspector.title'), node.title),
      row('status', t('components.sectionInspector.status'), node.status),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row(
        'teachingTopic',
        t('components.sectionInspector.teachingTopic'),
        node.teachingTopicTitle ?? props.section?.teachingTopicTitle,
      ),
      row('sectionId', t('components.sectionInspector.sectionId'), node.sectionId ?? props.section?.sectionId),
    ]
  }

  if (node.kind === 'ContentBlock') {
    return [
      row('type', t('components.sectionInspector.type'), node.typeLabel),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row('status', t('components.sectionInspector.status'), node.targetStatus ?? node.status),
      row('hasWordDocument', t('components.sectionInspector.hasWordDocument'), wordDocumentStatus),
      row('previewState', t('components.sectionInspector.previewState'), previewState),
    ]
  }

  if (node.kind === 'AtomicSection') {
    return [
      row('name', t('components.sectionInspector.name'), node.title),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row('status', t('components.sectionInspector.status'), node.targetStatus ?? node.status),
      row('childCount', t('components.sectionInspector.childCount'), node.itemCount ?? 0),
    ]
  }

  if (node.kind === 'CompositeBlock') {
    return [
      row('title', t('components.sectionInspector.title'), node.title),
      row('groupType', t('components.sectionInspector.groupType'), node.typeLabel),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row('status', t('components.sectionInspector.status'), node.targetStatus ?? node.status),
      row('hasWordDocument', t('components.sectionInspector.hasWordDocument'), wordDocumentStatus),
      row('childCount', t('components.sectionInspector.childCount'), node.itemCount ?? 0),
    ]
  }

  return [
    row('name', t('components.sectionInspector.name'), node.title),
    row('type', t('components.sectionInspector.type'), node.typeLabel),
    row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
    row('status', t('components.sectionInspector.status'), node.status),
    row(
      'selectedItemCount',
      t('components.sectionInspector.selectedItemCount'),
      t('components.sectionInspector.selectedItemCountValue', {
        count: props.variantItemCount ?? 0,
      }),
    ),
  ]
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

    <div v-if="showContentBlockCascadeDelete" class="border-t px-4 py-3">
      <div class="grid gap-2">
        <p class="text-xs font-medium text-destructive">
          {{ t('components.sectionInspector.dangerZone') }}
        </p>
        <p class="text-xs text-muted-foreground">
          {{ t('components.sectionInspector.deleteContentBlockCascadeDescription') }}
        </p>
        <Button
          type="button"
          variant="destructive"
          size="sm"
          class="w-full"
          :disabled="deletingContentBlockCascade"
          @click="emit('deleteContentBlockCascade')"
        >
          <Trash2 class="size-4" aria-hidden="true" />
          {{
            deletingContentBlockCascade
              ? t('components.sectionInspector.deleteContentBlockCascadeBusy')
              : t('components.sectionInspector.deleteContentBlockCascade')
          }}
        </Button>
      </div>
    </div>
  </Card>
</template>
