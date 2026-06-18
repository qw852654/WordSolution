<script setup lang="ts">
import { computed } from 'vue'
import { MoreHorizontal, Rows3 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
import SectionItemView from '@/components/business/SectionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StructuredContainer from '@/components/presentation/StructuredContainer.vue'
import { Button } from '@/components/ui/button'
import type {
  AtomicSectionItemActionPayload,
  AtomicSectionItemMovePayload,
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  SectionItemViewAction,
  StructuredBlockChildModel,
  StructuredBlockModel,
} from '@/types'

const props = defineProps<{
  block: StructuredBlockModel
  nodeIdMap?: Record<string, string>
}>()

const emit = defineEmits<{
  select: [id: string]
  toggleCollapse: [id: string]
  openMore: [id: string]
  selectContentBlock: [id: string]
  openWord: [id: string]
  refreshPreview: [id: string]
  openContentBlockMore: [id: string]
  openAtomicSectionItemWord: [payload: AtomicSectionItemActionPayload]
  moveAtomicSectionItem: [payload: AtomicSectionItemMovePayload]
  removeAtomicSectionItem: [payload: AtomicSectionItemActionPayload]
  openContentBlockRelationWord: [payload: ContentBlockRelationActionPayload]
  moveContentBlockRelation: [payload: ContentBlockRelationMovePayload]
  removeContentBlockRelation: [payload: ContentBlockRelationActionPayload]
}>()

const { t } = useI18n()
const difficultyMarkerClass = computed(() => getDifficultyMarkerClass(props.block.difficulty))
const difficultyMarkerLabel = computed(
  () => `${t('components.contentBlockDisplay.difficulty')}: ${props.block.difficulty}`,
)
const childContentBlockActions: SectionItemViewAction[] = [
  'OpenWord',
  'MoveUp',
  'MoveDown',
  'Remove',
]

function createAtomicSectionItemActionPayload(
  child: StructuredBlockChildModel,
): AtomicSectionItemActionPayload | undefined {
  if (!child.atomicSectionId || !child.atomicSectionItemId || !child.contentBlockId) {
    return undefined
  }

  return {
    nodeId: child.nodeId,
    atomicSectionId: child.atomicSectionId,
    atomicSectionItemId: child.atomicSectionItemId,
    contentBlockId: child.contentBlockId,
    title: child.block.title,
  }
}

function emitAtomicSectionItemWord(child: StructuredBlockChildModel) {
  const payload = createAtomicSectionItemActionPayload(child)

  if (payload) {
    emit('openAtomicSectionItemWord', payload)
  }
}

function emitAtomicSectionItemMove(child: StructuredBlockChildModel, direction: 'Up' | 'Down') {
  const payload = createAtomicSectionItemActionPayload(child)

  if (payload) {
    emit('moveAtomicSectionItem', { ...payload, direction })
  }
}

function emitAtomicSectionItemRemove(child: StructuredBlockChildModel) {
  const payload = createAtomicSectionItemActionPayload(child)

  if (payload) {
    emit('removeAtomicSectionItem', payload)
  }
}
</script>

<template>
  <StructuredContainer
    :title="block.title"
    :meta="t('components.structuredBlock.atomicSection')"
    :difficulty-marker-class="difficultyMarkerClass"
    :difficulty-marker-label="difficultyMarkerLabel"
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
        :actions="childContentBlockActions"
        :data-workspace-node-id="nodeIdMap?.[child.nodeId] ?? child.nodeId"
        @select="emit('selectContentBlock', $event)"
        @open-word="emitAtomicSectionItemWord(child)"
        @move-up="emitAtomicSectionItemMove(child, 'Up')"
        @move-down="emitAtomicSectionItemMove(child, 'Down')"
        @remove="emitAtomicSectionItemRemove(child)"
      >
        <ContentBlockDisplay
          v-if="child.kind === 'ContentBlock'"
          :block="child.block"
          @open-word="emit('openWord', $event)"
          @refresh-preview="emit('refreshPreview', $event)"
          @open-more="emit('openContentBlockMore', $event)"
        />
        <CompositeBlock
          v-else
          :block="child.block"
          :node-id-map="nodeIdMap"
          @select="emit('selectContentBlock', $event)"
          @select-content-block="emit('selectContentBlock', $event)"
          @open-word="emit('openWord', $event)"
          @refresh-preview="emit('refreshPreview', $event)"
          @open-content-block-more="emit('openContentBlockMore', $event)"
          @open-content-block-relation-word="emit('openContentBlockRelationWord', $event)"
          @move-content-block-relation="emit('moveContentBlockRelation', $event)"
          @remove-content-block-relation="emit('removeContentBlockRelation', $event)"
          @open-atomic-section-item-word="emit('openAtomicSectionItemWord', $event)"
          @move-atomic-section-item="emit('moveAtomicSectionItem', $event)"
          @remove-atomic-section-item="emit('removeAtomicSectionItem', $event)"
        />
      </SectionItemView>
    </div>
    <EmptyState
      v-else
      :title="t('components.structuredBlock.emptyTitle')"
      :description="t('components.structuredBlock.atomicEmptyDescription')"
    >
      <template #icon>
        <Rows3 class="size-5" aria-hidden="true" />
      </template>
    </EmptyState>
  </StructuredContainer>
</template>
