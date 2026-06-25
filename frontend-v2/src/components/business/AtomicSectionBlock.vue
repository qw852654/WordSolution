<script setup lang="ts">
import { computed } from 'vue'
import { MoreHorizontal } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import AtomicSectionPanelBlock from '@/components/business/AtomicSectionPanelBlock.vue'
import AtomicSectionUnassignedArea from '@/components/business/AtomicSectionUnassignedArea.vue'
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
import SectionItemView from '@/components/business/SectionItemView.vue'
import InsertPoint from '@/components/presentation/InsertPoint.vue'
import StructuredContainer from '@/components/presentation/StructuredContainer.vue'
import { Button } from '@/components/ui/button'
import type {
  AtomicSectionItemActionPayload,
  AtomicSectionItemMovePayload,
  AtomicSectionPanelActionPayload,
  AtomicSectionPanelCreatePayload,
  AtomicSectionPanelModel,
  AtomicSectionPanelMovePayload,
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  InsertPointModel,
  InsertRequestModel,
  SectionItemViewAction,
  StructuredBlockChildModel,
  StructuredBlockModel,
} from '@/types'

const props = defineProps<{
  block: StructuredBlockModel
  nodeIdMap?: Record<string, string>
  readOnly?: boolean
}>()

const emit = defineEmits<{
  select: [id: string]
  toggleCollapse: [id: string]
  openMore: [id: string]
  selectContentBlock: [id: string]
  openWord: [id: string]
  refreshPreview: [id: string]
  openContentBlockMore: [id: string]
  createAtomicSectionPanel: [payload: AtomicSectionPanelCreatePayload]
  selectAtomicSectionPanel: [payload: AtomicSectionPanelActionPayload]
  renameAtomicSectionPanel: [payload: AtomicSectionPanelActionPayload]
  moveAtomicSectionPanel: [payload: AtomicSectionPanelMovePayload]
  removeAtomicSectionPanel: [payload: AtomicSectionPanelActionPayload]
  requestAtomicSectionPanelQuestionImport: [payload: AtomicSectionPanelActionPayload]
  openAtomicSectionItemWord: [payload: AtomicSectionItemActionPayload]
  moveAtomicSectionItem: [payload: AtomicSectionItemMovePayload]
  removeAtomicSectionItem: [payload: AtomicSectionItemActionPayload]
  openContentBlockRelationWord: [payload: ContentBlockRelationActionPayload]
  moveContentBlockRelation: [payload: ContentBlockRelationMovePayload]
  removeContentBlockRelation: [payload: ContentBlockRelationActionPayload]
  requestInsert: [request: InsertRequestModel]
}>()

const { t } = useI18n()
const isExpanded = computed(() => props.block.expanded !== false)
const hasPanelLayout = computed(
  () => Array.isArray(props.block.panels) || Boolean(props.block.unassignedChildren),
)
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

function createPanelCreatePayload(
  beforePanel?: AtomicSectionPanelModel,
  afterPanel?: AtomicSectionPanelModel,
): AtomicSectionPanelCreatePayload | undefined {
  if (!props.block.atomicSectionId) {
    return undefined
  }

  return {
    nodeId: props.block.id,
    atomicSectionId: props.block.atomicSectionId,
    title: props.block.title,
    beforeAtomicSectionPanelId: beforePanel?.panelId ?? null,
    afterAtomicSectionPanelId: afterPanel?.panelId ?? null,
  }
}

function emitCreatePanel(beforePanel?: AtomicSectionPanelModel, afterPanel?: AtomicSectionPanelModel) {
  const payload = createPanelCreatePayload(beforePanel, afterPanel)
  if (payload) {
    emit('createAtomicSectionPanel', payload)
  }
}

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

function createAtomicSectionInsertPoint(
  beforeChild?: StructuredBlockChildModel,
  afterChild?: StructuredBlockChildModel,
): InsertPointModel {
  const parentId = props.block.atomicSectionId
  const suffix = `${afterChild?.id ?? 'start'}-${beforeChild?.id ?? 'end'}`

  return {
    id: `atomic-section-${props.block.id}-insert-${suffix}`,
    label: t('components.insertPoint.insert'),
    allowedActions: ['CreateContentBlock'],
    disabled: props.block.disabled || !parentId,
    placement: parentId
      ? {
          parentType: 'AtomicSection',
          parentId,
          beforeItemId: beforeChild?.atomicSectionItemId,
          afterItemId: afterChild?.atomicSectionItemId,
          beforeSortOrder: beforeChild?.sortOrder,
          afterSortOrder: afterChild?.sortOrder,
        }
      : undefined,
  }
}

function createAtomicSectionPanelInsertPoint(
  beforePanel?: AtomicSectionPanelModel,
  afterPanel?: AtomicSectionPanelModel,
): InsertPointModel {
  const parentId = props.block.atomicSectionId
  const suffix = `${afterPanel?.panelId ?? 'start'}-${beforePanel?.panelId ?? 'end'}`

  return {
    id: `atomic-section-${props.block.id}-panel-insert-${suffix}`,
    label: t('components.insertPoint.createAtomicSectionPanel'),
    allowedActions: ['CreateAtomicSectionPanel'],
    disabled: props.block.disabled || !parentId,
    placement: parentId
      ? {
          parentType: 'AtomicSectionPanelList',
          parentId,
          beforeItemId: beforePanel?.panelId,
          afterItemId: afterPanel?.panelId,
          beforeSortOrder: beforePanel?.sortOrder,
          afterSortOrder: afterPanel?.sortOrder,
        }
      : undefined,
  }
}

function handlePanelInsert(request: InsertRequestModel) {
  const placement = request.placement
  if (
    request.actionType !== 'CreateAtomicSectionPanel'
    || placement?.parentType !== 'AtomicSectionPanelList'
  ) {
    emit('requestInsert', request)
    return
  }

  const beforePanel = (props.block.panels ?? []).find(
    (panel) => panel.panelId === placement.beforeItemId,
  )
  const afterPanel = (props.block.panels ?? []).find(
    (panel) => panel.panelId === placement.afterItemId,
  )

  emitCreatePanel(beforePanel, afterPanel)
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
    @select-title="$emit('select', props.block.id)"
  >
    <template #meta-extra>
      <span
        v-if="block.hasEmptyPanel"
        class="rounded-sm border px-1.5 py-0.5 text-[11px] leading-none text-muted-foreground"
      >
        {{ t('components.structuredBlock.incomplete') }}
      </span>
    </template>

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
        v-if="!readOnly"
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
      <p class="text-sm leading-6 text-muted-foreground">{{ block.summary }}</p>
      <div v-if="hasPanelLayout" class="space-y-1">
        <template v-for="(panel, index) in block.panels" :key="panel.id">
          <InsertPoint
            v-if="!readOnly"
            :point="createAtomicSectionPanelInsertPoint(panel, block.panels?.[index - 1])"
            @request-action="handlePanelInsert"
          />
          <AtomicSectionPanelBlock
            :panel="panel"
            :node-id-map="nodeIdMap"
            :read-only="readOnly"
            @select-panel="emit('selectAtomicSectionPanel', $event)"
            @rename-panel="emit('renameAtomicSectionPanel', $event)"
            @move-panel="emit('moveAtomicSectionPanel', $event)"
            @remove-panel="emit('removeAtomicSectionPanel', $event)"
            @request-question-import="emit('requestAtomicSectionPanelQuestionImport', $event)"
            @select-content-block="emit('selectContentBlock', $event)"
            @toggle-collapse="emit('toggleCollapse', $event)"
            @open-atomic-section-item-word="emit('openAtomicSectionItemWord', $event)"
            @move-atomic-section-item="emit('moveAtomicSectionItem', $event)"
            @remove-atomic-section-item="emit('removeAtomicSectionItem', $event)"
            @open-content-block-relation-word="emit('openContentBlockRelationWord', $event)"
            @move-content-block-relation="emit('moveContentBlockRelation', $event)"
            @remove-content-block-relation="emit('removeContentBlockRelation', $event)"
            @request-insert="emit('requestInsert', $event)"
          />
          <div
            v-if="!readOnly && index === (block.panels?.length ?? 0) - 1"
            class="space-y-1"
          >
            <InsertPoint
              :point="createAtomicSectionPanelInsertPoint(undefined, panel)"
              @request-action="handlePanelInsert"
            />
          </div>
        </template>
        <InsertPoint
          v-if="!readOnly && !(block.panels?.length)"
          :point="createAtomicSectionPanelInsertPoint()"
          @request-action="handlePanelInsert"
        />
        <AtomicSectionUnassignedArea
          v-if="block.atomicSectionId"
          :atomic-section-id="block.atomicSectionId"
          :children="block.unassignedChildren ?? []"
          :node-id-map="nodeIdMap"
          :read-only="readOnly"
          @select-content-block="emit('selectContentBlock', $event)"
          @toggle-collapse="emit('toggleCollapse', $event)"
          @open-atomic-section-item-word="emit('openAtomicSectionItemWord', $event)"
          @move-atomic-section-item="emit('moveAtomicSectionItem', $event)"
          @remove-atomic-section-item="emit('removeAtomicSectionItem', $event)"
          @open-content-block-relation-word="emit('openContentBlockRelationWord', $event)"
          @move-content-block-relation="emit('moveContentBlockRelation', $event)"
          @remove-content-block-relation="emit('removeContentBlockRelation', $event)"
          @request-insert="emit('requestInsert', $event)"
        />
      </div>
      <div v-else-if="block.children.length" class="space-y-0">
        <template v-for="(child, index) in block.children" :key="child.id">
          <InsertPoint
            v-if="!readOnly"
            :point="createAtomicSectionInsertPoint(child, block.children[index - 1])"
            @request-action="emit('requestInsert', $event)"
          />
          <SectionItemView
            :item-id="child.id"
            :selected="child.selected"
            :disabled="child.disabled"
            :select-on-container="child.kind === 'ContentBlock'"
            :actions="readOnly ? [] : childContentBlockActions"
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
              :read-only="readOnly"
              @open-word="emitAtomicSectionItemWord(child)"
              @refresh-preview="emit('refreshPreview', $event)"
              @open-more="emit('openContentBlockMore', $event)"
            />
            <CompositeBlock
              v-else
              :block="child.block"
              :node-id-map="nodeIdMap"
              :read-only="readOnly"
              @select="emit('selectContentBlock', $event)"
              @select-content-block="emit('selectContentBlock', $event)"
              @toggle-collapse="emit('toggleCollapse', $event)"
              @open-word="emitAtomicSectionItemWord(child)"
              @refresh-preview="emit('refreshPreview', $event)"
              @open-content-block-more="emit('openContentBlockMore', $event)"
              @open-content-block-relation-word="emit('openContentBlockRelationWord', $event)"
              @move-content-block-relation="emit('moveContentBlockRelation', $event)"
              @remove-content-block-relation="emit('removeContentBlockRelation', $event)"
              @open-atomic-section-item-word="emit('openAtomicSectionItemWord', $event)"
              @move-atomic-section-item="emit('moveAtomicSectionItem', $event)"
              @remove-atomic-section-item="emit('removeAtomicSectionItem', $event)"
              @request-insert="emit('requestInsert', $event)"
            />
          </SectionItemView>
        </template>
        <InsertPoint
          v-if="!readOnly"
          :point="createAtomicSectionInsertPoint(undefined, block.children[block.children.length - 1])"
          @request-action="emit('requestInsert', $event)"
        />
      </div>
      <div v-else class="space-y-0">
        <p class="text-xs leading-4 text-destructive">
          {{ t('components.structuredBlock.atomicEmptyDescription') }}
        </p>
        <InsertPoint
          v-if="!readOnly"
          :point="createAtomicSectionInsertPoint()"
          @request-action="emit('requestInsert', $event)"
        />
      </div>
    </template>
  </StructuredContainer>
</template>
