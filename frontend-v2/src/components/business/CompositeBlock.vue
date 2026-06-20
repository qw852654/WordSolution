<script setup lang="ts">
import { computed } from 'vue'
import { Layers3, MoreHorizontal } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
import SectionItemView from '@/components/business/SectionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import InsertPoint from '@/components/presentation/InsertPoint.vue'
import StructuredContainer from '@/components/presentation/StructuredContainer.vue'
import { Button } from '@/components/ui/button'
import type {
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  InsertPointModel,
  InsertRequestModel,
  SectionItemViewAction,
  StructuredBlockChildModel,
  StructuredBlockModel,
} from '@/types'

defineOptions({
  name: 'CompositeBlock',
})

const props = defineProps<{
  block: StructuredBlockModel
  nodeIdMap?: Record<string, string>
  activeInsertPointId?: string
  insertFeedback?: string
}>()

const emit = defineEmits<{
  select: [id: string]
  toggleCollapse: [id: string]
  openMore: [id: string]
  selectContentBlock: [id: string]
  openWord: [id: string]
  refreshPreview: [id: string]
  openContentBlockMore: [id: string]
  openContentBlockRelationWord: [payload: ContentBlockRelationActionPayload]
  moveContentBlockRelation: [payload: ContentBlockRelationMovePayload]
  removeContentBlockRelation: [payload: ContentBlockRelationActionPayload]
  requestInsert: [request: InsertRequestModel]
}>()

const { t } = useI18n()
const isExpanded = computed(() => props.block.expanded !== false)
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
const childInsertActions = ['CreateContentBlock', 'SearchExistingBlock'] as const

function isInsertPointActive(insertPointId: string) {
  return props.activeInsertPointId === insertPointId
}

function getChildInsertPointAt(index: number): InsertPointModel {
  const previous = props.block.children[index - 1]
  const next = props.block.children[index]
  const anchor = next ? `before-${next.nodeId}` : previous ? `after-${previous.nodeId}` : 'first-child'

  return {
    id: `insert-composite-${props.block.contentBlockId ?? props.block.id}-${anchor}-${index}`,
    label: t('components.insertPoint.insert'),
    disabled: !props.block.contentBlockId,
    allowedActions: [...childInsertActions],
    context: {
      parentType: 'CompositeBlock',
      parentId: props.block.contentBlockId,
      parentTitle: props.block.title,
      afterSortOrder: previous?.sortOrder,
      beforeSortOrder: next?.sortOrder,
    },
  }
}

function getFirstChildInsertPoint(): InsertPointModel {
  return getChildInsertPointAt(0)
}

function emitInsertRequest(point: InsertPointModel, actionType: InsertRequestModel['actionType']) {
  emit('requestInsert', {
    insertPointId: point.id,
    actionType,
    context: point.context,
  })
}

function createRelationActionPayload(
  child: StructuredBlockChildModel,
): ContentBlockRelationActionPayload | undefined {
  if (!child.parentBlockId || !child.relationId || !child.contentBlockId) {
    return undefined
  }

  return {
    nodeId: child.nodeId,
    parentBlockId: child.parentBlockId,
    relationId: child.relationId,
    contentBlockId: child.contentBlockId,
    title: child.block.title,
  }
}

function emitRelationWord(child: StructuredBlockChildModel) {
  const payload = createRelationActionPayload(child)

  if (payload) {
    emit('openContentBlockRelationWord', payload)
  }
}

function emitRelationMove(child: StructuredBlockChildModel, direction: 'Up' | 'Down') {
  const payload = createRelationActionPayload(child)

  if (payload) {
    emit('moveContentBlockRelation', { ...payload, direction })
  }
}

function emitRelationRemove(child: StructuredBlockChildModel) {
  const payload = createRelationActionPayload(child)

  if (payload) {
    emit('removeContentBlockRelation', payload)
  }
}
</script>

<template>
  <StructuredContainer
    :title="block.title"
    :meta="t('components.structuredBlock.compositeBlock')"
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
        :aria-expanded="isExpanded"
        :disabled="block.disabled"
        @click.stop="$emit('toggleCollapse', block.id)"
      >
        {{ isExpanded ? t('components.structuredBlock.collapse') : t('components.structuredBlock.expand') }}
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

    <template v-if="isExpanded">
    <ContentBlockDisplay
      v-if="block.selfContent"
      :block="block.selfContent"
      @select="$emit('select', block.id)"
      @open-word="$emit('openWord', block.id)"
      @refresh-preview="$emit('refreshPreview', block.id)"
      @open-more="$emit('openContentBlockMore', block.id)"
    />
    <p class="text-sm leading-6 text-muted-foreground">{{ block.summary }}</p>
    <div v-if="block.children.length">
      <template
        v-for="(child, index) in block.children"
        :key="child.id"
      >
      <div class="space-y-1">
        <InsertPoint
          :point="getChildInsertPointAt(index)"
          :selected="isInsertPointActive(getChildInsertPointAt(index).id)"
          @request-action="emitInsertRequest(getChildInsertPointAt(index), $event.actionType)"
        />
        <p
          v-if="isInsertPointActive(getChildInsertPointAt(index).id) && insertFeedback"
          class="mx-4 rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
        >
          {{ insertFeedback }}
        </p>
      </div>
      <SectionItemView
        :item-id="child.id"
        :selected="child.selected"
        :disabled="child.disabled"
        :actions="childContentBlockActions"
        :data-workspace-node-id="nodeIdMap?.[child.nodeId] ?? child.nodeId"
        @select="emit('selectContentBlock', $event)"
        @open-word="emitRelationWord(child)"
        @move-up="emitRelationMove(child, 'Up')"
        @move-down="emitRelationMove(child, 'Down')"
        @remove="emitRelationRemove(child)"
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
          :active-insert-point-id="activeInsertPointId"
          :insert-feedback="insertFeedback"
          @select="emit('selectContentBlock', $event)"
          @select-content-block="emit('selectContentBlock', $event)"
          @toggle-collapse="emit('toggleCollapse', $event)"
          @open-word="emitRelationWord(child)"
          @refresh-preview="emit('refreshPreview', $event)"
          @open-content-block-more="emit('openContentBlockMore', $event)"
          @request-insert="emit('requestInsert', $event)"
          @open-content-block-relation-word="emit('openContentBlockRelationWord', $event)"
          @move-content-block-relation="emit('moveContentBlockRelation', $event)"
          @remove-content-block-relation="emit('removeContentBlockRelation', $event)"
        />
      </SectionItemView>
      </template>
      <div class="space-y-1">
        <InsertPoint
          :point="getChildInsertPointAt(block.children.length)"
          :selected="isInsertPointActive(getChildInsertPointAt(block.children.length).id)"
          @request-action="emitInsertRequest(getChildInsertPointAt(block.children.length), $event.actionType)"
        />
        <p
          v-if="isInsertPointActive(getChildInsertPointAt(block.children.length).id) && insertFeedback"
          class="mx-4 rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
        >
          {{ insertFeedback }}
        </p>
      </div>
    </div>
    <div v-else class="space-y-2">
    <EmptyState
      :title="t('components.structuredBlock.emptyTitle')"
      :description="t('components.structuredBlock.compositeEmptyDescription')"
    >
      <template #icon>
        <Layers3 class="size-5" aria-hidden="true" />
      </template>
    </EmptyState>
      <InsertPoint
        :point="getFirstChildInsertPoint()"
        :selected="isInsertPointActive(getFirstChildInsertPoint().id)"
        @request-action="emitInsertRequest(getFirstChildInsertPoint(), $event.actionType)"
      />
      <p
        v-if="isInsertPointActive(getFirstChildInsertPoint().id) && insertFeedback"
        class="mx-4 rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
      >
        {{ insertFeedback }}
      </p>
    </div>
    </template>
  </StructuredContainer>
</template>
