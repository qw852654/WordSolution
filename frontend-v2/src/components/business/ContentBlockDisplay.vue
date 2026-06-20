<script setup lang="ts">
import { computed } from 'vue'
import { FileText, MoreHorizontal, RefreshCw } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
import type { ContentBlockDisplayModel } from '@/types'

const props = defineProps<{
  block: ContentBlockDisplayModel
}>()

defineEmits<{
  select: [id: string]
  openWord: [id: string]
  refreshPreview: [id: string]
  openMore: [id: string]
}>()

const { t } = useI18n()

const previewStateLabel = computed(() => t(`components.contentBlockDisplay.previewState.${props.block.htmlPreviewState}`))
const difficultyMarkerClass = computed(() => getDifficultyMarkerClass(props.block.difficulty))
const hasHtmlPreview = computed(() => props.block.htmlPreviewState === 'ready' && Boolean(props.block.htmlPreview))
const showMissingDocumentPrompt = computed(() => props.block.htmlPreviewState === 'empty')
</script>

<template>
  <article
    class="group relative grid grid-cols-[0.5rem_minmax(0,1fr)] gap-x-2 bg-background"
    :class="block.disabled ? 'opacity-60' : ''"
    :aria-disabled="block.disabled ? 'true' : undefined"
    @click="$emit('select', block.id)"
  >
    <span
      class="content-block-display-difficulty-dot mt-2 size-2 rounded-full"
      :class="difficultyMarkerClass"
      :aria-label="`${t('components.contentBlockDisplay.difficulty')}: ${block.difficulty}`"
      :title="block.difficulty"
    />

    <div class="min-w-0">
      <div class="absolute right-0 top-0 flex shrink-0 items-center gap-1 opacity-0 transition-opacity group-hover:opacity-100 group-focus-within:opacity-100">
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.contentBlockDisplay.openWord')"
          :disabled="block.disabled"
          @click.stop="$emit('openWord', block.id)"
        >
          <FileText class="size-4" />
        </Button>
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.contentBlockDisplay.refreshPreview')"
          :disabled="block.disabled"
          @click.stop="$emit('refreshPreview', block.id)"
        >
          <RefreshCw class="size-4" />
        </Button>
        <Button
          type="button"
          size="icon"
          variant="ghost"
          class="size-8"
          :aria-label="t('components.contentBlockDisplay.more')"
          :disabled="block.disabled"
          @click.stop="$emit('openMore', block.id)"
        >
          <MoreHorizontal class="size-4" />
        </Button>
      </div>

      <div
        v-if="hasHtmlPreview"
        class="content-block-display-preview text-sm leading-7"
        v-html="block.htmlPreview"
      />
      <div v-else-if="showMissingDocumentPrompt" class="flex items-start gap-2 py-1 text-sm">
        <FileText class="mt-0.5 size-4 shrink-0 text-muted-foreground" />
        <div class="min-w-0 space-y-1">
          <p class="font-medium text-foreground">
            {{ t('components.contentBlockDisplay.missingDocumentTitle') }}
          </p>
          <p class="text-muted-foreground">
            {{ t('components.contentBlockDisplay.missingDocumentDescription') }}
          </p>
          <Button
            type="button"
            size="sm"
            variant="outline"
            class="mt-1 h-7 px-2 text-xs"
            :disabled="block.disabled"
            @click.stop="$emit('openWord', block.id)"
          >
            <FileText class="size-3.5" />
            {{ t('components.contentBlockDisplay.openWord') }}
          </Button>
        </div>
      </div>
      <p v-else class="text-sm text-muted-foreground">
        {{ previewStateLabel }}
      </p>
    </div>
  </article>
</template>

<style scoped>
.content-block-display-preview :deep(p) {
  margin-block: 0.5rem;
}

.content-block-display-preview :deep(ol),
.content-block-display-preview :deep(ul) {
  margin-block: 0.5rem;
  padding-inline-start: 1.25rem;
}
</style>
