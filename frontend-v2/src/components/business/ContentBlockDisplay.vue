<script setup lang="ts">
import { computed } from 'vue'
import { FileText, MoreHorizontal, RefreshCw } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
import type { ContentBlockDisplayModel } from '@/types'

const props = defineProps<{
  block: ContentBlockDisplayModel
  readOnly?: boolean
}>()

defineEmits<{
  select: [id: string]
  openWord: [id: string]
  refreshPreview: [id: string]
  openMore: [id: string]
}>()

const { t } = useI18n()

const hasReadyPreview = computed(
  () => props.block.htmlPreviewState === 'ready' && Boolean(props.block.htmlPreview),
)
const usesStructuredPreview = computed(
  () =>
    hasReadyPreview.value &&
    (props.block.partParseStatus === 'Parsed' ||
      props.block.partParseStatus === 'ParsedWithWarnings'),
)
const questionPartHtml = computed(() => {
  const html = props.block.htmlPreview ?? ''
  const sections = new Map<string, string>()
  const pattern =
    /<section\b[^>]*data-question-part=["']([^"']+)["'][^>]*>([\s\S]*?)<\/section>/gi
  let match: RegExpExecArray | null

  while ((match = pattern.exec(html))) {
    sections.set(match[1], match[2].trim())
  }

  return sections
})
const visibleQuestionParts = computed(() =>
  ['Answer', 'Analysis', 'Hint', 'Other']
    .map((partType) => ({
      partType,
      html: questionPartHtml.value.get(partType) ?? '',
      warningMessage: props.block.parts?.find((part) => part.partType === partType)?.warningMessage,
    }))
    .filter((part) => part.html || part.warningMessage),
)
const previewStateLabel = computed(() =>
  t(`components.contentBlockDisplay.previewState.${props.block.htmlPreviewState}`),
)
const difficultyMarkerClass = computed(() => getDifficultyMarkerClass(props.block.difficulty))
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
      <div
        v-if="!readOnly && hasReadyPreview"
        class="absolute right-0 top-0 flex shrink-0 items-center gap-1 opacity-0 transition-opacity group-hover:opacity-100 group-focus-within:opacity-100"
      >
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

      <div v-if="usesStructuredPreview" class="grid gap-2">
        <div
          v-if="questionPartHtml.get('Stem')"
          class="content-block-display-preview text-sm leading-7"
          v-html="questionPartHtml.get('Stem')"
        />
        <details
          v-for="part in visibleQuestionParts"
          :key="part.partType"
          class="content-block-display-part rounded-md border border-border px-3 py-2 text-sm"
          :open="part.partType === 'Other'"
        >
          <summary class="cursor-pointer font-medium">
            {{ t(`components.contentBlockDisplay.part.${part.partType}`) }}
          </summary>
          <p v-if="part.warningMessage" class="mt-2 text-xs text-destructive">
            {{ part.warningMessage }}
          </p>
          <div
            v-if="part.html"
            class="content-block-display-preview mt-2 leading-7"
            v-html="part.html"
          />
        </details>
        <p v-if="block.partParseMessage" class="text-xs text-muted-foreground">
          {{ block.partParseMessage }}
        </p>
      </div>
      <div
        v-else-if="hasReadyPreview"
        class="content-block-display-preview text-sm leading-7"
        v-html="block.htmlPreview"
      />
      <div v-else class="grid gap-2 text-sm">
        <p class="text-muted-foreground">
          <span class="font-medium text-foreground">{{ block.role }}</span>
          <span class="mx-2 text-muted-foreground">·</span>
          <span>{{ previewStateLabel }}</span>
        </p>
        <div class="grid gap-1">
          <p class="font-medium">{{ t('components.contentBlockDisplay.noWordDocument') }}</p>
          <p class="text-muted-foreground">
            {{ t('components.contentBlockDisplay.noWordDocumentDescription') }}
          </p>
        </div>
        <div v-if="!readOnly">
          <Button
            type="button"
            size="sm"
            variant="outline"
            :disabled="block.disabled"
            @click.stop="$emit('openWord', block.id)"
          >
            <FileText class="mr-1 size-4" />
            {{ t('components.contentBlockDisplay.openWord') }}
          </Button>
        </div>
      </div>
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
