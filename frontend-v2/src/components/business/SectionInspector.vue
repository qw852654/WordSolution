<script setup lang="ts">
import { FileText, PackageOpen } from 'lucide-vue-next'
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
import type { SectionNodeModel } from '@/types'

defineProps<{
  node?: SectionNodeModel
}>()

defineEmits<{
  preview: [id: string]
  openWord: [id: string]
}>()

const { t } = useI18n()
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
          <CardTitle class="truncate text-sm">{{ node.title }}</CardTitle>
        </div>
        <StatusPill :label="node.targetType" tone="active" />
      </div>
    </CardHeader>

    <WeakScrollArea class="space-y-3 px-4 pb-4">
      <p class="text-sm leading-6 text-muted-foreground">{{ node.summary }}</p>

      <dl class="grid gap-2 text-sm">
        <div class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.sectionInspector.status') }}</dt>
          <dd class="truncate font-medium">{{ node.status }}</dd>
        </div>
        <div class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.sectionInspector.position') }}</dt>
          <dd class="truncate font-medium">
            {{ t('components.sectionItemView.sortOrder') }} {{ node.sortOrder }}
            ·
            {{ t('components.sectionItemView.level') }} {{ node.level }}
          </dd>
        </div>
        <div class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.sectionInspector.referenceMode') }}</dt>
          <dd class="truncate font-medium">
            {{ node.referenceMode ?? t('components.sectionItemView.atomicSectionReference') }}
          </dd>
        </div>
        <div class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.sectionInspector.lockedVersion') }}</dt>
          <dd class="truncate font-medium">
            {{ node.lockedVersionLabel ?? t('components.sectionItemView.noLockedVersion') }}
          </dd>
        </div>
      </dl>

      <section v-if="node.note" class="rounded-md border bg-background px-3 py-2">
        <h3 class="text-xs font-medium text-muted-foreground">
          {{ t('components.sectionInspector.note') }}
        </h3>
        <p class="mt-1 text-sm leading-6">{{ node.note }}</p>
      </section>
    </WeakScrollArea>

    <CardFooter class="flex flex-wrap gap-2 border-t px-4 py-3">
      <Button type="button" size="sm" variant="outline" @click="$emit('preview', node.id)">
        <FileText class="size-4" />
        {{ t('components.sectionInspector.preview') }}
      </Button>
      <Button type="button" size="sm" variant="outline" @click="$emit('openWord', node.id)">
        {{ t('components.sectionInspector.openWord') }}
      </Button>
    </CardFooter>
  </Card>
</template>
