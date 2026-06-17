<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { PanelsTopLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import AtomicSectionBlock from '@/components/business/AtomicSectionBlock.vue'
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import SectionItemView from '@/components/business/SectionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import InsertPoint from '@/components/presentation/InsertPoint.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import { Card } from '@/components/ui/card'
import type {
  ContentBlockDisplayModel,
  InsertActionType,
  InsertPointModel,
  InsertRequestModel,
  SectionPageShellModel,
  SectionWorkspaceFlowItemModel,
  StructuredBlockModel,
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
    teachingNoteMode?: boolean
  }>(),
  {
    contentBlocks: () => [],
    structuredBlocks: () => [],
    flowItems: () => [],
    workspaceNodeMap: () => ({}),
    teachingNoteMode: false,
  },
)

const emit = defineEmits<{
  selectNode: [id: string]
  requestInsert: [request: InsertRequestModel]
}>()

const workspaceRoot = ref<HTMLElement | null>(null)

function getNodeIdForWorkspaceItem(itemId: string, explicitNodeId?: string) {
  return explicitNodeId ?? props.workspaceNodeMap[itemId] ?? itemId
}

function isWorkspaceItemSelected(itemId: string, fallback?: boolean, explicitNodeId?: string) {
  const nodeId = getNodeIdForWorkspaceItem(itemId, explicitNodeId)
  return props.selectedNodeId ? props.selectedNodeId === nodeId : fallback
}

function withContentSelection(block: ContentBlockDisplayModel): ContentBlockDisplayModel {
  return {
    ...block,
    selected: isWorkspaceItemSelected(block.id, block.selected),
  }
}

function withStructuredSelection(block: StructuredBlockModel): StructuredBlockModel {
  return {
    ...block,
    selected: isWorkspaceItemSelected(block.id, block.selected),
    children: block.children.map((child) =>
      block.blockKind === 'AtomicSection' ? withContentSelection(child) : { ...child },
    ),
  }
}

function emitWorkspaceSelection(itemId: string) {
  emit('selectNode', getNodeIdForWorkspaceItem(itemId))
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
      </div>

      <div
        class="grid min-h-0 flex-1 gap-3 p-3"
        :class="teachingNoteMode ? 'lg:grid-cols-[minmax(0,1fr)_260px]' : 'grid-cols-[minmax(0,1fr)]'"
      >
      <WeakScrollArea class="rounded-md border bg-background p-3" :aria-label="t('sectionPage.workspace.mainColumnLabel')">
        <div v-if="flowItems.length" class="space-y-0">
          <template
            v-for="(item, index) in flowItems"
            :key="item.id"
          >
            <div v-if="index > 0" class="space-y-1">
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
              :disabled="item.disabled"
              :data-workspace-node-id="item.nodeId"
              @select="emitWorkspaceSelection"
            >
              <ContentBlockDisplay
                v-if="item.kind === 'ContentBlock'"
                :block="item.block"
              />
              <AtomicSectionBlock
                v-else-if="item.kind === 'AtomicSection'"
                :block="item.block"
                :node-id-map="workspaceNodeMap"
                @select="emitWorkspaceSelection"
                @select-content-block="emitWorkspaceSelection"
              />
              <CompositeBlock
                v-else
                :block="item.block"
                @select="emitWorkspaceSelection"
                @select-content-block="emitWorkspaceSelection(item.id)"
              />
            </SectionItemView>
          </template>
        </div>

        <EmptyState
          v-else
          :title="t('sectionPage.workspace.emptyTitle')"
          :description="t('sectionPage.workspace.emptyDescription')"
        >
          <template #icon>
            <PanelsTopLeft class="size-5" aria-hidden="true" />
          </template>
        </EmptyState>
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
