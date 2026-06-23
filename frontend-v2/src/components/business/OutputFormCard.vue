<script setup lang="ts">
import { FileText, Play } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import type { OutputFormCardModel } from '@/types'

defineProps<{
  outputForm: OutputFormCardModel
}>()

defineEmits<{
  generateWord: [id: number]
}>()

const { t } = useI18n()
</script>

<template>
  <article class="rounded-md border bg-background p-3">
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0">
        <div class="flex min-w-0 items-center gap-2">
          <FileText class="size-4 shrink-0 text-muted-foreground" />
          <h3 class="truncate text-sm font-semibold">{{ outputForm.title }}</h3>
        </div>
        <p class="mt-1 truncate text-xs text-muted-foreground">
          {{ outputForm.templateTitle }}
        </p>
      </div>
      <StatusPill :label="outputForm.status" tone="neutral" />
    </div>

    <dl class="mt-3 grid gap-2 text-xs text-muted-foreground">
      <div class="flex items-center justify-between gap-3">
        <dt>{{ t('components.outputFormCard.audience') }}</dt>
        <dd class="font-medium text-foreground">{{ outputForm.audience }}</dd>
      </div>
      <div class="flex items-center justify-between gap-3">
        <dt>{{ t('components.outputFormCard.outputFormat') }}</dt>
        <dd class="font-medium text-foreground">{{ outputForm.outputFormat }}</dd>
      </div>
      <div class="flex items-center justify-between gap-3">
        <dt>{{ t('components.outputFormCard.visibilityMode') }}</dt>
        <dd class="font-medium text-foreground">{{ outputForm.visibilityMode }}</dd>
      </div>
    </dl>

    <Button type="button" size="sm" class="mt-3 w-full" @click="$emit('generateWord', outputForm.id)">
      <Play class="size-4" />
      {{ t('components.outputFormCard.generateWord') }}
    </Button>
  </article>
</template>
