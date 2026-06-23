<script setup lang="ts">
import { Box } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import type { HandoutInspectorModel } from '@/types'

defineProps<{
  model?: HandoutInspectorModel | null
}>()

const { t } = useI18n()
</script>

<template>
  <aside class="rounded-lg border bg-background p-4">
    <template v-if="model">
      <div class="flex items-start justify-between gap-3">
        <div class="min-w-0">
          <p class="text-xs text-muted-foreground">
            {{ t('components.handoutInspector.currentSelection') }}
          </p>
          <h2 class="mt-1 truncate text-base font-semibold">{{ model.title }}</h2>
        </div>
        <StatusPill :label="model.kind" tone="neutral" />
      </div>

      <p v-if="model.description" class="mt-3 text-sm leading-6 text-muted-foreground">
        {{ model.description }}
      </p>

      <dl class="mt-4 grid gap-2 text-sm">
        <div
          v-for="field in model.fields"
          :key="field.label"
          class="flex items-center justify-between gap-3 rounded-md border bg-muted/20 px-3 py-2"
        >
          <dt class="shrink-0 text-xs text-muted-foreground">{{ field.label }}</dt>
          <dd class="min-w-0 truncate font-medium">{{ field.value }}</dd>
        </div>
      </dl>

      <p v-if="model.editableOccurrence" class="mt-4 rounded-md border bg-muted/20 px-3 py-2 text-xs text-muted-foreground">
        {{ t('components.handoutInspector.occurrenceOnly') }}
      </p>
    </template>

    <EmptyState
      v-else
      :title="t('components.handoutInspector.emptyTitle')"
      :description="t('components.handoutInspector.emptyDescription')"
    >
      <template #icon>
        <Box class="size-5" />
      </template>
    </EmptyState>
  </aside>
</template>
