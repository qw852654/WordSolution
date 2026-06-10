<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import type { ContentBlockCardModel } from '@/types'

const props = defineProps<{
  block: ContentBlockCardModel
  selected?: boolean
}>()

defineEmits<{
  select: [id: string]
}>()

const { t } = useI18n()
</script>

<template>
  <Card
    :class="[
      'border transition-colors',
      selected ? 'border-primary bg-muted/40' : 'border-border',
      props.block.disabled ? 'opacity-60' : '',
    ]"
  >
    <CardHeader class="gap-3">
      <div class="flex min-w-0 flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div class="min-w-0 space-y-1">
          <CardTitle class="truncate text-base">{{ block.title }}</CardTitle>
          <p class="text-sm text-muted-foreground">{{ block.summary }}</p>
        </div>
        <StatusPill
          :label="block.status"
          :tone="block.disabled ? 'muted' : selected ? 'active' : 'neutral'"
        />
      </div>
    </CardHeader>
    <CardContent>
      <dl class="grid grid-cols-[minmax(0,1fr)] gap-2 text-sm sm:grid-cols-2">
        <div class="rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.contentBlockCard.role') }}</dt>
          <dd class="mt-1 truncate font-medium">{{ block.role }}</dd>
        </div>
        <div class="rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.contentBlockCard.blockType') }}</dt>
          <dd class="mt-1 truncate font-medium">{{ block.blockType }}</dd>
        </div>
        <div class="rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.contentBlockCard.difficulty') }}</dt>
          <dd class="mt-1 truncate font-medium">{{ block.difficulty }}</dd>
        </div>
        <div class="rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.contentBlockCard.version') }}</dt>
          <dd class="mt-1 truncate font-medium">{{ block.version }}</dd>
        </div>
      </dl>
    </CardContent>
    <CardFooter class="justify-end border-t pt-4">
      <Button
        type="button"
        size="sm"
        variant="outline"
        :disabled="block.disabled"
        @click="$emit('select', block.id)"
      >
        {{ t('components.contentBlockCard.open') }}
      </Button>
    </CardFooter>
  </Card>
</template>
