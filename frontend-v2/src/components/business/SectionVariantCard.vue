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
import type { SectionVariantCardModel } from '@/types'

defineProps<{
  variant: SectionVariantCardModel
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
      variant.disabled ? 'opacity-60' : '',
    ]"
  >
    <CardHeader class="gap-3">
      <div class="flex min-w-0 flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <CardTitle class="truncate text-base">{{ variant.title }}</CardTitle>
        <StatusPill
          :label="variant.status"
          :tone="variant.disabled ? 'muted' : selected ? 'active' : 'neutral'"
        />
      </div>
    </CardHeader>
    <CardContent>
      <dl class="grid gap-2 text-sm">
        <div class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.sectionVariantCard.purpose') }}</dt>
          <dd class="truncate font-medium">{{ variant.purpose }}</dd>
        </div>
        <div class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.sectionVariantCard.difficulty') }}</dt>
          <dd class="truncate font-medium">{{ variant.difficulty }}</dd>
        </div>
        <div class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2">
          <dt class="text-xs text-muted-foreground">{{ t('components.sectionVariantCard.itemCount') }}</dt>
          <dd class="truncate font-medium">{{ variant.itemCount }}</dd>
        </div>
      </dl>
    </CardContent>
    <CardFooter class="justify-end border-t pt-4">
      <Button
        type="button"
        size="sm"
        variant="outline"
        :disabled="variant.disabled"
        @click="$emit('select', variant.id)"
      >
        {{ t('components.sectionVariantCard.open') }}
      </Button>
    </CardFooter>
  </Card>
</template>
