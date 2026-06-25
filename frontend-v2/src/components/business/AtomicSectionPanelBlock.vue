<script setup lang="ts">
import { computed } from 'vue'
import { FileUp } from 'lucide-vue-next'
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import { getDifficultyMarkerClass } from '@/components/business/difficultyTone'
import SectionItemView from '@/components/business/SectionItemView.vue'
import InsertPoint from '@/components/presentation/InsertPoint.vue'
import StructuredContainer from '@/components/presentation/StructuredContainer.vue'
import { Button } from '@/components/ui/button'
import { useI18n } from 'vue-i18n'
import type {
  AtomicSectionItemActionPayload,
  AtomicSectionItemMovePayload,
  AtomicSectionPanelActionPayload,
  AtomicSectionPanelModel,
  AtomicSectionPanelMovePayload,
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  InsertPointModel,
  InsertRequestModel,
  SectionItemViewAction,
  StructuredBlockChildModel,
} from '@/types'

const props = defineProps<{
  panel: AtomicSectionPanelModel
  nodeIdMap?: Record<string, string>
  readOnly?: boolean
}>()

const emit = defineEmits<{
  selectPanel: [payload: AtomicSectionPanelActionPayload]
  renamePanel: [payload: AtomicSectionPanelActionPayload]
  movePanel: [payload: AtomicSectionPanelMovePayload]
  removePanel: [payload: AtomicSectionPanelActionPayload]
  requestQuestionImport: [payload: AtomicSectionPanelActionPayload]
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
const canImportQuestions = computed(() => props.panel.teachingRole !== 'Knowledge')

function createPanelPayload(): AtomicSectionPanelActionPayload {
  return {
    nodeId: props.panel.id,
    atomicSectionId: props.panel.atomicSectionId,
    atomicSectionPanelId: props.panel.panelId,
    title: props.panel.title,
    teachingRole: props.panel.teachingRole,
    difficulty: props.panel.difficulty,
    difficultyValue: props.panel.difficultyValue,
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

function createPanelInsertPoint(
  beforeChild?: StructuredBlockChildModel,
  afterChild?: StructuredBlockChildModel,
): InsertPointModel {
  const suffix = `${afterChild?.id ?? 'start'}-${beforeChild?.id ?? 'end'}`

  return {
    id: `atomic-section-panel-${props.panel.id}-insert-${suffix}`,
    label: t('components.insertPoint.insert'),
    allowedActions: ['CreateContentBlock'],
    disabled: props.panel.disabled,
    placement: {
      parentType: 'AtomicSection',
      parentId: props.panel.atomicSectionId,
      beforeItemId: beforeChild?.atomicSectionItemId,
      afterItemId: afterChild?.atomicSectionItemId,
      beforeSortOrder: beforeChild?.sortOrder,
      afterSortOrder: afterChild?.sortOrder,
      atomicSectionPanelId: props.panel.panelId,
      teachingRole: props.panel.teachingRole,
      atomicSectionPanelDifficulty: props.panel.difficultyValue ?? props.panel.difficulty,
    },
  }
}
</script>

<template>
  <StructuredContainer
    :title="panel.title"
    :meta="`${panel.teachingRole} · ${panel.difficulty}`"
    :data-workspace-node-id="nodeIdMap?.[panel.id] ?? panel.id"
    :difficulty-marker-class="getDifficultyMarkerClass(panel.difficulty)"
    :difficulty-marker-label="`${t('components.contentBlockDisplay.difficulty')}: ${panel.difficulty}`"
    :selected="panel.selected"
    :disabled="panel.disabled"
    @select-title="emit('selectPanel', createPanelPayload())"
  >
    <template #actions>
      <Button
        v-if="!readOnly && canImportQuestions"
        type="button"
        size="sm"
        variant="ghost"
        :disabled="panel.disabled"
        @click.stop="emit('requestQuestionImport', createPanelPayload())"
      >
        <FileUp class="size-3.5" aria-hidden="true" />
        {{ t('components.atomicSectionPanel.importQuestions') }}
      </Button>
      <Button
        v-if="!readOnly"
        type="button"
        size="sm"
        variant="ghost"
        :disabled="panel.disabled"
        @click.stop="emit('renamePanel', createPanelPayload())"
      >
        {{ t('components.atomicSectionPanel.rename') }}
      </Button>
      <Button
        v-if="!readOnly"
        type="button"
        size="sm"
        variant="ghost"
        :disabled="panel.disabled"
        @click.stop="emit('movePanel', { ...createPanelPayload(), direction: 'Up' })"
      >
        {{ t('components.atomicSectionPanel.moveUp') }}
      </Button>
      <Button
        v-if="!readOnly"
        type="button"
        size="sm"
        variant="ghost"
        :disabled="panel.disabled"
        @click.stop="emit('movePanel', { ...createPanelPayload(), direction: 'Down' })"
      >
        {{ t('components.atomicSectionPanel.moveDown') }}
      </Button>
      <Button
        v-if="!readOnly"
        type="button"
        size="sm"
        variant="ghost"
        :disabled="panel.disabled"
        @click.stop="emit('removePanel', createPanelPayload())"
      >
        {{ t('components.atomicSectionPanel.remove') }}
      </Button>
    </template>

    <div class="space-y-0">
      <template v-for="(child, index) in panel.children" :key="child.id">
        <InsertPoint
          v-if="!readOnly"
          :point="createPanelInsertPoint(child, panel.children[index - 1])"
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
        :point="createPanelInsertPoint(undefined, panel.children[panel.children.length - 1])"
        @request-action="emit('requestInsert', $event)"
      />
      <p v-if="!panel.children.length" class="text-xs leading-4 text-destructive">
        {{ t('components.atomicSectionPanel.empty') }}
      </p>
    </div>
  </StructuredContainer>
</template>
