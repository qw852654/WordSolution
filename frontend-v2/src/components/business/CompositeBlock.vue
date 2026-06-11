<script setup lang="ts">
import { Layers3, MoreHorizontal } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import SectionItemView from '@/components/business/SectionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StructuredContainer from '@/components/presentation/StructuredContainer.vue'
import { Button } from '@/components/ui/button'
import type { StructuredBlockModel } from '@/types'

const props = defineProps<{
  block: StructuredBlockModel
}>()

const emit = defineEmits<{
  select: [id: string]
  toggleCollapse: [id: string]
  openMore: [id: string]
  selectContentBlock: [id: string]
  openWord: [id: string]
  refreshPreview: [id: string]
  openContentBlockMore: [id: string]
}>()

const { t } = useI18n()
</script>

<template>
  <StructuredContainer
    :title="block.title"
    :meta="t('components.structuredBlock.compositeBlock')"
    :selected="block.selected"
    :disabled="block.disabled"
    @click="$emit('select', props.block.id)"
  >
    <template #actions>
      <Button
        type="button"
        size="sm"
        variant="ghost"
        :disabled="block.disabled"
        @click.stop="$emit('toggleCollapse', block.id)"
      >
        {{ t('components.structuredBlock.collapse') }}
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.structuredBlock.more')"
        :disabled="block.disabled"
        @click.stop="$emit('openMore', block.id)"
      >
        <MoreHorizontal class="size-4" />
      </Button>
    </template>

    <p class="text-sm leading-6 text-muted-foreground">{{ block.summary }}</p>
    <div v-if="block.children.length">
      <SectionItemView
        v-for="child in block.children"
        :key="child.id"
        :item-id="child.id"
        :selected="child.selected"
        :disabled="child.disabled"
        @select="emit('selectContentBlock', $event)"
        @open-word="emit('openWord', $event)"
      >
        <ContentBlockDisplay
          :block="child"
          @open-word="emit('openWord', $event)"
          @refresh-preview="emit('refreshPreview', $event)"
          @open-more="emit('openContentBlockMore', $event)"
        />
      </SectionItemView>
    </div>
    <EmptyState
      v-else
      :title="t('components.structuredBlock.emptyTitle')"
      :description="t('components.structuredBlock.compositeEmptyDescription')"
    >
      <template #icon>
        <Layers3 class="size-5" aria-hidden="true" />
      </template>
    </EmptyState>
  </StructuredContainer>
</template>
