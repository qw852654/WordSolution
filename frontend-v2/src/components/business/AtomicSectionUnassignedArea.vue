<script setup lang="ts">
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import SectionItemView from '@/components/business/SectionItemView.vue'
import InsertPoint from '@/components/presentation/InsertPoint.vue'
import { useI18n } from 'vue-i18n'
import type {
  AtomicSectionItemActionPayload,
  AtomicSectionItemMovePayload,
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  InsertPointModel,
  InsertRequestModel,
  SectionItemViewAction,
  StructuredBlockChildModel,
} from '@/types'

const props = defineProps<{
  atomicSectionId: number
  children: StructuredBlockChildModel[]
  nodeIdMap?: Record<string, string>
  readOnly?: boolean
}>()

const emit = defineEmits<{
  selectContentBlock: [id: string]
  toggleCollapse: [id: string]
  openAtomicSectionItemWord: [payload: AtomicSectionItemActionPayload]
  moveAtomicSectionItem: [payload: AtomicSectionItemMovePayload]
  removeAtomicSectionItem: [payload: AtomicSectionItemActionPayload]
  openContentBlockRelationWord: [payload: ContentBlockRelationActionPayload]
  moveContentBlockRelation: [payload: ContentBlockRelationMovePayload]
  removeContentBlockRelation: [payload: ContentBlockRelationActionPayload]
  requestInsert: [request: InsertRequestModel]
}>()

const { t } = useI18n()
const childContentBlockActions: SectionItemViewAction[] = ['OpenWord', 'MoveUp', 'MoveDown', 'Remove']

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
    atomicSectionPanelId: child.atomicSectionPanelId,
    teachingRole: child.teachingRole,
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

function createUnassignedInsertPoint(
  beforeChild?: StructuredBlockChildModel,
  afterChild?: StructuredBlockChildModel,
): InsertPointModel {
  const suffix = `${afterChild?.id ?? 'start'}-${beforeChild?.id ?? 'end'}`

  return {
    id: `atomic-section-unassigned-${props.atomicSectionId}-insert-${suffix}`,
    label: t('components.insertPoint.insert'),
    allowedActions: ['CreateContentBlock'],
    placement: {
      parentType: 'AtomicSection',
      parentId: props.atomicSectionId,
      beforeItemId: beforeChild?.atomicSectionItemId,
      afterItemId: afterChild?.atomicSectionItemId,
      beforeSortOrder: beforeChild?.sortOrder,
      afterSortOrder: afterChild?.sortOrder,
      atomicSectionPanelId: null,
      teachingRole: 'Unclassified',
    },
  }
}
</script>

<template>
  <section
    class="space-y-0 border-t pt-1"
    :data-workspace-node-id="`atomic-section-unassigned-${atomicSectionId}`"
  >
    <p class="px-1 text-xs text-muted-foreground">
      {{ t('components.atomicSectionUnassigned.title') }}
    </p>
    <template v-for="(child, index) in children" :key="child.id">
      <InsertPoint
        v-if="!readOnly"
        :point="createUnassignedInsertPoint(child, children[index - 1])"
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
          @open-content-block-relation-word="emit('openContentBlockRelationWord', $event)"
          @move-content-block-relation="emit('moveContentBlockRelation', $event)"
          @remove-content-block-relation="emit('removeContentBlockRelation', $event)"
          @request-insert="emit('requestInsert', $event)"
        />
      </SectionItemView>
    </template>
    <InsertPoint
      v-if="!readOnly"
      :point="createUnassignedInsertPoint(undefined, children[children.length - 1])"
      @request-action="emit('requestInsert', $event)"
    />
    <p v-if="!children.length" class="px-1 text-xs leading-4 text-muted-foreground">
      {{ t('components.atomicSectionUnassigned.empty') }}
    </p>
  </section>
</template>
