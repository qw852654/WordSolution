<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import AtomicSectionBlock from '@/components/business/AtomicSectionBlock.vue'
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import SectionItemView from '@/components/business/SectionItemView.vue'
import InsertPoint from '@/components/presentation/InsertPoint.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import type {
  AtomicSectionItemActionPayload,
  AtomicSectionItemMovePayload,
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  ContentBlockDisplayModel,
  InsertActionType,
  InsertPointModel,
  InsertRequestModel,
  SectionPageShellModel,
  SectionItemViewAction,
  SectionVariantSelectionCandidateModel,
  SectionWorkspaceFlowItemModel,
  StructuredBlockChildModel,
  StructuredBlockModel,
  WorkspaceItemSelectionState,
} from '@/types'

const { t } = useI18n()

const props = withDefaults(
  defineProps<{
    section: SectionPageShellModel
    contentBlocks?: ContentBlockDisplayModel[]
    structuredBlocks?: StructuredBlockModel[]
    flowItems?: SectionWorkspaceFlowItemModel[]
    selectedNodeId?: string
    workspaceNodeMap?: Record<string, string>
    scrollTargetNodeId?: string
    scrollRequestKey?: number
    activeInsertPointId?: string
    insertFeedback?: string
    wrapSelectionMode?: boolean
    wrapSelectedNodeIds?: string[]
    wrapSelectionFeedback?: string
    variantSelectionMode?: boolean
    variantSelectionCandidates?: SectionVariantSelectionCandidateModel[]
    variantSelectionFeedback?: string
    collapsedWorkspaceNodeIds?: string[]
    teachingNoteMode?: boolean
  }>(),
  {
    contentBlocks: () => [],
    structuredBlocks: () => [],
    flowItems: () => [],
    workspaceNodeMap: () => ({}),
    wrapSelectionMode: false,
    wrapSelectedNodeIds: () => [],
    variantSelectionMode: false,
    variantSelectionCandidates: () => [],
    collapsedWorkspaceNodeIds: () => [],
    teachingNoteMode: false,
  },
)

const emit = defineEmits<{
  selectNode: [id: string, event?: MouseEvent]
  requestInsert: [request: InsertRequestModel]
  enterWrapSelectionMode: []
  cancelWrapSelectionMode: []
  clearWrapSelection: []
  toggleWrapNodeSelection: [nodeId: string]
  requestWrapAsAtomicSection: []
  toggleVariantSelection: [sectionItemId: number]
  clearVariantSelection: []
  cancelVariantSelection: []
  confirmVariantSelection: []
  requestAtomicChildContentBlock: [request: AtomicSectionWorkspaceActionPayload]
  requestAtomicMove: [request: AtomicSectionWorkspaceMovePayload]
  requestAtomicRename: [request: AtomicSectionWorkspaceActionPayload]
  requestAtomicRemove: [request: AtomicSectionWorkspaceActionPayload]
  requestAtomicSectionItemOpenWord: [request: AtomicSectionItemActionPayload]
  requestAtomicSectionItemMove: [request: AtomicSectionItemMovePayload]
  requestAtomicSectionItemRemove: [request: AtomicSectionItemActionPayload]
  requestContentBlockOpenWord: [request: ContentBlockWorkspaceActionPayload]
  requestContentBlockMove: [request: ContentBlockWorkspaceMovePayload]
  requestContentBlockRemove: [request: ContentBlockWorkspaceActionPayload]
  requestContentBlockRelationOpenWord: [request: ContentBlockRelationActionPayload]
  requestContentBlockRelationMove: [request: ContentBlockRelationMovePayload]
  requestContentBlockRelationRemove: [request: ContentBlockRelationActionPayload]
  toggleWorkspaceNodeCollapse: [id: string]
}>()

const workspaceRoot = ref<HTMLElement | null>(null)
const atomicSectionActions: SectionItemViewAction[] = [
  'InsertChildContentBlock',
  'MoveUp',
  'MoveDown',
  'Rename',
  'Remove',
]
const contentBlockActions: SectionItemViewAction[] = [
  'OpenWord',
  'MoveUp',
  'MoveDown',
  'Remove',
]
const compositeBlockActions: SectionItemViewAction[] = [
  'OpenWord',
  'MoveUp',
  'MoveDown',
  'Remove',
]
const collapsedWorkspaceNodeIdSet = computed(() => new Set(props.collapsedWorkspaceNodeIds))
const wrapSelectedNodeIdSet = computed(() => new Set(props.wrapSelectedNodeIds))
const wrapSelectedCount = computed(() => props.wrapSelectedNodeIds.length)
const variantCandidateBySectionItemId = computed(
  () =>
    new Map(
      props.variantSelectionCandidates.map((candidate) => [candidate.sectionItemId, candidate]),
    ),
)
const variantSelectedCount = computed(
  () =>
    props.variantSelectionCandidates.filter((candidate) => candidate.selectable && candidate.selected)
      .length,
)
type WorkspaceSelectionMode = 'WrapAsAtomicSectionMode' | 'SectionVariantSelectionMode'

const activeWorkspaceSelectionMode = computed<WorkspaceSelectionMode | undefined>(() => {
  if (props.variantSelectionMode) {
    return 'SectionVariantSelectionMode'
  }

  if (props.wrapSelectionMode) {
    return 'WrapAsAtomicSectionMode'
  }

  return undefined
})
const workspaceSelectionFeedback = computed(() =>
  props.variantSelectionMode ? props.variantSelectionFeedback : props.wrapSelectionFeedback,
)
const firstInsertPoint = computed<InsertPointModel>(() => ({
  id: 'insert-first-section-item',
  label: t('sectionPage.workspace.emptyTitle'),
}))

interface AtomicSectionWorkspaceActionPayload {
  nodeId: string
  sectionItemId: number
  atomicSectionId: number
  title: string
}

interface AtomicSectionWorkspaceMovePayload extends AtomicSectionWorkspaceActionPayload {
  direction: 'Up' | 'Down'
}

interface ContentBlockWorkspaceActionPayload {
  nodeId: string
  sectionItemId: number
  contentBlockId: number
  title: string
}

interface ContentBlockWorkspaceMovePayload extends ContentBlockWorkspaceActionPayload {
  direction: 'Up' | 'Down'
}

function getNodeIdForWorkspaceItem(itemId: string, explicitNodeId?: string) {
  return explicitNodeId ?? props.workspaceNodeMap[itemId] ?? itemId
}

function isWorkspaceItemSelected(itemId: string, fallback?: boolean, explicitNodeId?: string) {
  if (activeWorkspaceSelectionMode.value) {
    return false
  }

  const nodeId = getNodeIdForWorkspaceItem(itemId, explicitNodeId)
  if (wrapSelectedNodeIdSet.value.has(nodeId)) {
    return true
  }

  return props.selectedNodeId ? props.selectedNodeId === nodeId : fallback
}

function withContentSelection(block: ContentBlockDisplayModel): ContentBlockDisplayModel {
  return {
    ...block,
    selected: isWorkspaceItemSelected(block.id, block.selected),
  }
}

function withStructuredChildSelection(child: StructuredBlockChildModel): StructuredBlockChildModel {
  const selected = isWorkspaceItemSelected(child.id, child.selected, child.nodeId)

  if (child.kind === 'ContentBlock') {
    return {
      ...child,
      selected,
      block: withContentSelection({
        ...child.block,
        selected,
      }),
    }
  }

  return {
    ...child,
    selected,
    block: withStructuredSelection({
      ...child.block,
      selected,
    }),
  }
}

function withStructuredSelection(block: StructuredBlockModel): StructuredBlockModel {
  return {
    ...block,
    expanded: isStructuredBlockExpanded(block.id, block.expanded),
    selected: isWorkspaceItemSelected(block.id, block.selected),
    children: block.children.map((child) => withStructuredChildSelection(child)),
  }
}

function isStructuredBlockExpanded(blockId: string, fallback?: boolean) {
  if (collapsedWorkspaceNodeIdSet.value.has(blockId)) {
    return false
  }

  return fallback !== false
}

function emitWorkspaceSelection(itemId: string, event?: MouseEvent) {
  emit('selectNode', getNodeIdForWorkspaceItem(itemId), event)
}

function isWrappableWorkspaceItem(item: SectionWorkspaceFlowItemModel) {
  return item.kind !== 'AtomicSection' && typeof item.sectionItemId === 'number'
}

function getVariantCandidateForItem(item: SectionWorkspaceFlowItemModel) {
  return typeof item.sectionItemId === 'number'
    ? variantCandidateBySectionItemId.value.get(item.sectionItemId)
    : undefined
}

function getWrapSelectionState(item: SectionWorkspaceFlowItemModel): WorkspaceItemSelectionState {
  if (!props.wrapSelectionMode) {
    return 'none'
  }

  if (!isWrappableWorkspaceItem(item)) {
    return 'unavailable'
  }

  return wrapSelectedNodeIdSet.value.has(item.nodeId) ? 'selected' : 'selectable'
}

function getVariantSelectionState(
  item: SectionWorkspaceFlowItemModel,
): WorkspaceItemSelectionState {
  if (!props.variantSelectionMode) {
    return 'none'
  }

  const candidate = getVariantCandidateForItem(item)

  if (!candidate || !candidate.selectable) {
    return 'unavailable'
  }

  return candidate.selected ? 'selected' : 'selectable'
}

function getWorkspaceItemSelectionState(item: SectionWorkspaceFlowItemModel): WorkspaceItemSelectionState {
  if (props.variantSelectionMode) {
    return getVariantSelectionState(item)
  }

  if (props.wrapSelectionMode) {
    return getWrapSelectionState(item)
  }

  return 'none'
}

function getWorkspaceItemSelectionUnavailableReason(item: SectionWorkspaceFlowItemModel) {
  if (props.variantSelectionMode) {
    return getVariantCandidateForItem(item)?.unavailableReason
  }

  if (props.wrapSelectionMode && !isWrappableWorkspaceItem(item)) {
    return t('sectionPage.workspace.wrap.notWrappableLabel')
  }

  return undefined
}

function getWorkspaceItemAriaLabel(item: SectionWorkspaceFlowItemModel) {
  return getWorkspaceItemSelectionUnavailableReason(item)
}

function handleWorkspaceItemSelect(item: SectionWorkspaceFlowItemModel, event?: MouseEvent) {
  if (props.variantSelectionMode) {
    const candidate = getVariantCandidateForItem(item)

    if (candidate?.selectable) {
      emit('toggleVariantSelection', candidate.sectionItemId)
    }

    return
  }

  if (props.wrapSelectionMode) {
    if (isWrappableWorkspaceItem(item)) {
      emit('toggleWrapNodeSelection', item.nodeId)
    }
    return
  }

  emitWorkspaceSelection(item.id, event)
}

function handleNestedWorkspaceSelection(itemId: string, event?: MouseEvent) {
  if (props.wrapSelectionMode || props.variantSelectionMode) {
    return
  }

  emitWorkspaceSelection(itemId, event)
}

function getInsertPointBefore(item: SectionWorkspaceFlowItemModel, index: number): InsertPointModel {
  return {
    id: `insert-before-${item.nodeId}-${index}`,
    label: t('components.insertPoint.insert'),
  }
}

function isInsertPointActive(insertPointId: string) {
  return props.activeInsertPointId === insertPointId
}

function emitInsertRequest(insertPointId: string, actionType: InsertActionType) {
  emit('requestInsert', { insertPointId, actionType })
}

function createAtomicSectionActionPayload(item: SectionWorkspaceFlowItemModel) {
  if (item.kind !== 'AtomicSection' || !item.sectionItemId || !item.targetId) {
    return undefined
  }

  return {
    nodeId: item.nodeId,
    sectionItemId: item.sectionItemId,
    atomicSectionId: item.targetId,
    title: item.block.title,
  }
}

function emitAtomicChildContentBlock(item: SectionWorkspaceFlowItemModel) {
  const payload = createAtomicSectionActionPayload(item)
  if (payload) {
    emit('requestAtomicChildContentBlock', payload)
  }
}

function emitAtomicMove(item: SectionWorkspaceFlowItemModel, direction: 'Up' | 'Down') {
  const payload = createAtomicSectionActionPayload(item)
  if (payload) {
    emit('requestAtomicMove', { ...payload, direction })
  }
}

function emitAtomicRename(item: SectionWorkspaceFlowItemModel) {
  const payload = createAtomicSectionActionPayload(item)
  if (payload) {
    emit('requestAtomicRename', payload)
  }
}

function emitAtomicRemove(item: SectionWorkspaceFlowItemModel) {
  const payload = createAtomicSectionActionPayload(item)
  if (payload) {
    emit('requestAtomicRemove', payload)
  }
}

function createContentBlockActionPayload(item: SectionWorkspaceFlowItemModel) {
  if (item.kind === 'AtomicSection' || !item.sectionItemId || !item.targetId) {
    return undefined
  }

  return {
    nodeId: item.nodeId,
    sectionItemId: item.sectionItemId,
    contentBlockId: item.targetId,
    title: item.block.title,
  }
}

function emitContentBlockOpenWord(item: SectionWorkspaceFlowItemModel) {
  const payload = createContentBlockActionPayload(item)
  if (payload) {
    emit('requestContentBlockOpenWord', payload)
  }
}

function emitContentBlockMove(item: SectionWorkspaceFlowItemModel, direction: 'Up' | 'Down') {
  const payload = createContentBlockActionPayload(item)
  if (payload) {
    emit('requestContentBlockMove', { ...payload, direction })
  }
}

function emitContentBlockRemove(item: SectionWorkspaceFlowItemModel) {
  const payload = createContentBlockActionPayload(item)
  if (payload) {
    emit('requestContentBlockRemove', payload)
  }
}

function getWorkspaceItemActions(item: SectionWorkspaceFlowItemModel) {
  if (activeWorkspaceSelectionMode.value) {
    return []
  }

  if (item.kind === 'AtomicSection') {
    return atomicSectionActions
  }

  if (item.kind === 'ContentBlock') {
    return contentBlockActions
  }

  return compositeBlockActions
}

function findWorkspaceNodeElement(nodeId: string) {
  const candidates = workspaceRoot.value?.querySelectorAll<HTMLElement>('[data-workspace-node-id]')

  return Array.from(candidates ?? []).find(
    (candidate) => candidate.dataset.workspaceNodeId === nodeId,
  )
}

async function scrollToWorkspaceNode(nodeId?: string) {
  if (!nodeId) {
    return
  }

  await nextTick()

  const target = findWorkspaceNodeElement(nodeId)

  if (target) {
    target.scrollIntoView({ block: 'start', behavior: 'smooth' })
    return
  }

  workspaceRoot.value
    ?.querySelector<HTMLElement>('.weak-scroll-area')
    ?.scrollTo({ top: 0, behavior: 'smooth' })
}

const flowItems = computed<SectionWorkspaceFlowItemModel[]>(() => {
  if (props.flowItems.length) {
    return props.flowItems.map((item) => {
      const nodeId = getNodeIdForWorkspaceItem(item.id, item.nodeId)

      if (item.kind === 'ContentBlock') {
        return {
          ...item,
          nodeId,
          selected: isWorkspaceItemSelected(item.id, item.selected, nodeId),
          block: withContentSelection(item.block),
        }
      }

      return {
        ...item,
        nodeId,
        selected: isWorkspaceItemSelected(item.id, item.selected, nodeId),
        block: withStructuredSelection(item.block),
      }
    })
  }

  const [firstContentBlock, ...remainingContentBlocks] = props.contentBlocks
  const items: SectionWorkspaceFlowItemModel[] = []

  if (firstContentBlock) {
    items.push({
      kind: 'ContentBlock',
      id: firstContentBlock.id,
      nodeId: getNodeIdForWorkspaceItem(firstContentBlock.id),
      selected: isWorkspaceItemSelected(firstContentBlock.id, firstContentBlock.selected),
      disabled: firstContentBlock.disabled,
      block: withContentSelection(firstContentBlock),
    })
  }

  for (const block of props.structuredBlocks) {
    items.push({
      kind: block.blockKind,
      id: block.id,
      nodeId: getNodeIdForWorkspaceItem(block.id),
      selected: isWorkspaceItemSelected(block.id, block.selected),
      disabled: block.disabled,
      block: withStructuredSelection(block),
    })
  }

  for (const block of remainingContentBlocks) {
    items.push({
      kind: 'ContentBlock',
      id: block.id,
      nodeId: getNodeIdForWorkspaceItem(block.id),
      selected: isWorkspaceItemSelected(block.id, block.selected),
      disabled: block.disabled,
      block: withContentSelection(block),
    })
  }

  return items
})

watch(
  () => [props.scrollRequestKey, props.scrollTargetNodeId] as const,
  () => {
    void scrollToWorkspaceNode(props.scrollTargetNodeId)
  },
)
</script>

<template>
  <div ref="workspaceRoot" class="h-full min-h-0">
    <Card class="flex h-full min-h-0 flex-col overflow-hidden">
      <div class="flex min-h-10 flex-wrap items-center gap-x-3 gap-y-1 border-b px-3 py-2 text-xs">
      <span class="font-medium">{{ section.title }}</span>
      <StatusPill :label="section.status" tone="active" />
      <span class="text-muted-foreground">{{ t('sectionPage.meta.sectionId') }}: {{ section.sectionId }}</span>
      <span class="text-muted-foreground">{{ t('sectionPage.meta.teachingTopic') }}: {{ section.teachingTopicTitle }}</span>
      <div class="ml-auto flex flex-wrap items-center gap-2">
        <span
          v-if="variantSelectionMode"
          class="text-muted-foreground"
        >
          {{ t('sectionPage.workspace.variantSelection.selectedCount', { count: variantSelectedCount }) }}
        </span>
        <span
          v-else-if="wrapSelectionMode"
          class="text-muted-foreground"
        >
          {{ t('sectionPage.workspace.wrap.selectedCount', { count: wrapSelectedCount }) }}
        </span>
        <template v-if="variantSelectionMode">
          <Button
            type="button"
            size="sm"
            variant="outline"
            class="h-7 px-2 text-xs"
            @click="emit('confirmVariantSelection')"
          >
            {{ t('sectionPage.workspace.variantSelection.confirmAction') }}
          </Button>
          <Button
            type="button"
            size="sm"
            variant="ghost"
            class="h-7 px-2 text-xs"
            :disabled="variantSelectedCount === 0"
            @click="emit('clearVariantSelection')"
          >
            {{ t('sectionPage.workspace.variantSelection.clearAction') }}
          </Button>
          <Button
            type="button"
            size="sm"
            variant="ghost"
            class="h-7 px-2 text-xs"
            @click="emit('cancelVariantSelection')"
          >
            {{ t('sectionPage.workspace.variantSelection.exitAction') }}
          </Button>
        </template>
        <Button
          v-else-if="!wrapSelectionMode"
          type="button"
          size="sm"
          variant="outline"
          class="h-7 px-2 text-xs"
          @click="emit('enterWrapSelectionMode')"
        >
          {{ t('sectionPage.workspace.wrap.enterAction') }}
        </Button>
        <template v-else>
          <Button
            type="button"
            size="sm"
            variant="outline"
            class="h-7 px-2 text-xs"
            :disabled="wrapSelectedCount < 2"
            @click="emit('requestWrapAsAtomicSection')"
          >
            {{ t('sectionPage.workspace.wrap.confirmAction') }}
          </Button>
          <Button
            type="button"
            size="sm"
            variant="ghost"
            class="h-7 px-2 text-xs"
            :disabled="wrapSelectedCount === 0"
            @click="emit('clearWrapSelection')"
          >
            {{ t('sectionPage.workspace.wrap.clearAction') }}
          </Button>
          <Button
            type="button"
            size="sm"
            variant="ghost"
            class="h-7 px-2 text-xs"
            @click="emit('cancelWrapSelectionMode')"
          >
            {{ t('sectionPage.workspace.wrap.exitAction') }}
          </Button>
        </template>
      </div>
      </div>

      <div
        class="grid min-h-0 flex-1 gap-3 p-3"
        :class="teachingNoteMode ? 'lg:grid-cols-[minmax(0,1fr)_260px]' : 'grid-cols-[minmax(0,1fr)]'"
      >
      <WeakScrollArea class="rounded-md border bg-background p-3" :aria-label="t('sectionPage.workspace.mainColumnLabel')">
        <div v-if="flowItems.length" class="space-y-0">
          <p
            v-if="workspaceSelectionFeedback"
            class="mb-2 rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
          >
            {{ workspaceSelectionFeedback }}
          </p>
          <template
            v-for="(item, index) in flowItems"
            :key="item.id"
          >
            <div v-if="index > 0 && !activeWorkspaceSelectionMode" class="space-y-1">
              <InsertPoint
                :point="getInsertPointBefore(item, index)"
                :selected="isInsertPointActive(getInsertPointBefore(item, index).id)"
                @request-action="emitInsertRequest($event.insertPointId, $event.actionType)"
              />
              <p
                v-if="isInsertPointActive(getInsertPointBefore(item, index).id) && insertFeedback"
                class="mx-4 rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
              >
                {{ insertFeedback }}
              </p>
            </div>
            <SectionItemView
              :item-id="item.id"
              :selected="item.selected"
              :selection-state="getWorkspaceItemSelectionState(item)"
              :selection-unavailable-reason="getWorkspaceItemSelectionUnavailableReason(item)"
              :disabled="item.disabled"
              :actions="getWorkspaceItemActions(item)"
              :data-workspace-node-id="item.nodeId"
              :aria-label="getWorkspaceItemAriaLabel(item)"
              @select="(_, event) => handleWorkspaceItemSelect(item, event)"
              @insert-child-content-block="emitAtomicChildContentBlock(item)"
              @move-up="item.kind === 'AtomicSection' ? emitAtomicMove(item, 'Up') : emitContentBlockMove(item, 'Up')"
              @move-down="item.kind === 'AtomicSection' ? emitAtomicMove(item, 'Down') : emitContentBlockMove(item, 'Down')"
              @rename="emitAtomicRename(item)"
              @remove="item.kind === 'AtomicSection' ? emitAtomicRemove(item) : emitContentBlockRemove(item)"
              @open-word="emitContentBlockOpenWord(item)"
            >
              <ContentBlockDisplay
                v-if="item.kind === 'ContentBlock'"
                :block="item.block"
              />
              <AtomicSectionBlock
                v-else-if="item.kind === 'AtomicSection'"
                :block="item.block"
                :node-id-map="workspaceNodeMap"
                @select="handleNestedWorkspaceSelection"
                @select-content-block="handleNestedWorkspaceSelection"
                @toggle-collapse="emit('toggleWorkspaceNodeCollapse', $event)"
                @open-content-block-relation-word="emit('requestContentBlockRelationOpenWord', $event)"
                @move-content-block-relation="emit('requestContentBlockRelationMove', $event)"
                @remove-content-block-relation="emit('requestContentBlockRelationRemove', $event)"
                @open-atomic-section-item-word="emit('requestAtomicSectionItemOpenWord', $event)"
                @move-atomic-section-item="emit('requestAtomicSectionItemMove', $event)"
                @remove-atomic-section-item="emit('requestAtomicSectionItemRemove', $event)"
              />
              <CompositeBlock
                v-else
                :block="item.block"
                :node-id-map="workspaceNodeMap"
                @select="handleNestedWorkspaceSelection"
                @select-content-block="handleNestedWorkspaceSelection"
                @toggle-collapse="emit('toggleWorkspaceNodeCollapse', $event)"
                @open-content-block-relation-word="emit('requestContentBlockRelationOpenWord', $event)"
                @move-content-block-relation="emit('requestContentBlockRelationMove', $event)"
                @remove-content-block-relation="emit('requestContentBlockRelationRemove', $event)"
              />
            </SectionItemView>
          </template>
        </div>

        <div v-else class="flex min-h-full items-center justify-center p-4">
          <div class="w-full max-w-2xl rounded-md border border-dashed bg-muted/10 p-4">
            <p class="text-sm font-medium">{{ t('sectionPage.workspace.emptyTitle') }}</p>
            <p class="mt-1 text-sm leading-6 text-muted-foreground">
              {{ t('sectionPage.workspace.emptyDescription') }}
            </p>
            <div v-if="!activeWorkspaceSelectionMode" class="mt-3">
              <InsertPoint
                :point="firstInsertPoint"
                selected
                @request-action="emitInsertRequest($event.insertPointId, $event.actionType)"
              />
              <p
                v-if="isInsertPointActive(firstInsertPoint.id) && insertFeedback"
                class="mt-2 rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
              >
                {{ insertFeedback }}
              </p>
            </div>
          </div>
        </div>
      </WeakScrollArea>

      <WeakScrollArea
        v-if="teachingNoteMode"
        class="rounded-md border bg-muted/20 p-3"
        :aria-label="t('sectionPage.workspace.teachingNoteColumnLabel')"
      >
        <p class="text-sm font-medium">{{ t('sectionPage.workspace.teachingNoteColumnLabel') }}</p>
        <p class="mt-1 text-sm leading-6 text-muted-foreground">
          {{ t('sectionPage.workspace.teachingNoteColumnDescription') }}
        </p>
      </WeakScrollArea>
      </div>
    </Card>
  </div>
</template>
