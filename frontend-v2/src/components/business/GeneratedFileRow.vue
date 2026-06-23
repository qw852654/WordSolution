<script setup lang="ts">
import { Download, FileArchive, ScrollText, Trash2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type { GeneratedFileRowModel } from '@/types'

defineProps<{
  file: GeneratedFileRowModel
}>()

defineEmits<{
  download: [id: number]
  viewManifest: [id: number]
  delete: [id: number]
}>()

const { t } = useI18n()
</script>

<template>
  <article class="rounded-md border bg-background px-3 py-2">
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0">
        <div class="flex min-w-0 items-center gap-2">
          <FileArchive class="size-4 shrink-0 text-muted-foreground" />
          <h3 class="truncate text-sm font-medium">{{ file.fileName }}</h3>
        </div>
        <p class="mt-1 truncate text-xs text-muted-foreground">
          {{ file.generatedTime }} · {{ file.outputFormTitle }}
        </p>
        <p class="mt-1 truncate text-xs text-muted-foreground">
          {{ file.manifestSummary }}
        </p>
      </div>
      <div class="flex shrink-0 items-center gap-1">
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.generatedFileRow.viewManifest')"
          @click="$emit('viewManifest', file.id)"
        >
          <ScrollText class="size-4" />
        </Button>
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.generatedFileRow.download')"
          @click="$emit('download', file.id)"
        >
          <Download class="size-4" />
        </Button>
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.generatedFileRow.delete')"
          @click="$emit('delete', file.id)"
        >
          <Trash2 class="size-4" />
        </Button>
      </div>
    </div>
  </article>
</template>
