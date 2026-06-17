<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import SectionInspector from '@/components/business/SectionInspector.vue'
import SectionTreeContextMenu from '@/components/business/SectionTreeContextMenu.vue'
import InsertCreateOverlay from '@/components/containers/InsertCreateOverlay.vue'
import SectionStructurePanel from '@/components/containers/SectionStructurePanel.vue'
import SectionTopToolbar from '@/components/containers/SectionTopToolbar.vue'
import SectionWorkspace from '@/components/containers/SectionWorkspace.vue'
import {
  mockContentBlockDisplays,
  mockSectionPageShells,
  mockSectionTreeNodes,
  mockStructuredBlocks,
} from '@/mocks'
import type {
  InsertCreatePanelModel,
  InsertCreateSubmitPayload,
  InsertRequestModel,
  SectionPageShellModel,
  SectionTreeContextMenuActionPayload,
  SectionTreeContextMenuModel,
  SectionTreeContextMenuPayload,
  SectionTreeNodeModel,
} from '@/types'

const route = useRoute()
const { t } = useI18n()
const selectedStructureNodeId = ref('section-tree-atomic-basics')
const activeInsertPointId = ref<string>()
const activeCreatePanel = ref<InsertCreatePanelModel | null>(null)
const sectionTreeContextMenu = ref<SectionTreeContextMenuModel | null>(null)
const insertFeedback = ref('')
const workspaceScrollTargetNodeId = ref<string>()
const workspaceScrollRequestKey = ref(0)

const workspaceNodeMap: Record<string, string> = {
  'display-energy-law': 'section-tree-law',
  'atomic-energy-basics': 'section-tree-atomic-basics',
  'composite-circular-track': 'section-tree-composite',
  'atomic-example-one': 'section-tree-example-one',
  'atomic-example-two': 'section-tree-example-two',
  'display-empty-preview': 'section-tree-disabled',
  'display-long-preview': 'section-tree-long-title',
  'display-disabled': 'section-tree-disabled',
  'atomic-empty': 'section-tree-atomic-basics',
}

const sectionId = computed(() => {
  const value = route.params.sectionId
  return Array.isArray(value) ? value.join('/') : value
})

const sectionShell = computed<SectionPageShellModel>(() => {
  const id = sectionId.value || 'demo-section'
  const matched = mockSectionPageShells.find((section) => section.sectionId === id)

  return matched ?? {
    sectionId: id,
    title: `Section ${id}`,
    teachingTopicTitle: 'Mock Data',
    status: '骨架验收',
  }
})

function findSectionTreeNode(
  nodes: SectionTreeNodeModel[],
  nodeId: string,
): SectionTreeNodeModel | undefined {
  for (const node of nodes) {
    if (node.id === nodeId) {
      return node
    }

    const childMatch = node.children ? findSectionTreeNode(node.children, nodeId) : undefined
    if (childMatch) {
      return childMatch
    }
  }

  return undefined
}

const selectedStructureNode = computed(() =>
  findSectionTreeNode(mockSectionTreeNodes, selectedStructureNodeId.value),
)

const contextTargetNodeId = computed(() => sectionTreeContextMenu.value?.node.id)

function clearActiveInsertPoint() {
  activeInsertPointId.value = undefined
  activeCreatePanel.value = null
  insertFeedback.value = ''
}

function selectStructureNode(nodeId: string) {
  selectedStructureNodeId.value = nodeId
  closeSectionTreeContextMenu()
  clearActiveInsertPoint()
}

function selectStructureNodeFromTree(nodeId: string) {
  selectStructureNode(nodeId)
  workspaceScrollTargetNodeId.value = nodeId
  workspaceScrollRequestKey.value += 1
}

function requestInsert(request: InsertRequestModel) {
  activeInsertPointId.value = request.insertPointId

  if (request.actionType === 'CreateContentBlock' || request.actionType === 'CreateAtomicSection') {
    insertFeedback.value = ''
    activeCreatePanel.value = {
      insertPointId: request.insertPointId,
      targetType: request.actionType === 'CreateContentBlock' ? 'ContentBlock' : 'AtomicSection',
      insertPositionLabel: t('sectionPage.workspace.insertPanel.insertPositionLabel'),
    }
    return
  }

  activeCreatePanel.value = null
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSearchExistingBlock')
}

function cancelInsertCreateOverlay() {
  activeCreatePanel.value = null
}

function submitInsertCreateOverlay(payload: InsertCreateSubmitPayload) {
  activeCreatePanel.value = null
  activeInsertPointId.value = payload.insertPointId
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackCreateSubmitted', {
    targetType: payload.targetType,
    title: payload.title,
  })
}

function openSectionTreeContextMenu(payload: SectionTreeContextMenuPayload) {
  sectionTreeContextMenu.value = {
    node: payload.node,
    position: {
      x: payload.x,
      y: payload.y,
    },
  }
}

function closeSectionTreeContextMenu() {
  sectionTreeContextMenu.value = null
}

function handleSectionTreeContextMenuAction(payload: SectionTreeContextMenuActionPayload) {
  const contextNode = sectionTreeContextMenu.value?.node
  closeSectionTreeContextMenu()

  if (!contextNode) {
    return
  }

  if (payload.actionType === 'CreateContentBlock' || payload.actionType === 'CreateAtomicSection') {
    activeInsertPointId.value = `section-tree-context-${payload.nodeId}`
    insertFeedback.value = ''
    activeCreatePanel.value = {
      insertPointId: activeInsertPointId.value,
      targetType: payload.actionType === 'CreateContentBlock' ? 'ContentBlock' : 'AtomicSection',
      insertPositionLabel: contextNode.typeLabel,
    }
    return
  }

  activeCreatePanel.value = null

  if (payload.actionType === 'SearchExistingBlock') {
    insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSearchExistingBlock')
    return
  }

  insertFeedback.value = ''
}
</script>

<template>
  <main class="min-h-screen bg-background text-foreground xl:h-screen xl:overflow-hidden">
    <section class="grid min-h-screen grid-cols-[minmax(0,1fr)] gap-3 p-3 xl:h-full xl:min-h-0 xl:grid-cols-[240px_minmax(0,1fr)_280px]">
      <SectionStructurePanel
        :nodes="mockSectionTreeNodes"
        :selected-node-id="selectedStructureNodeId"
        :context-target-node-id="contextTargetNodeId"
        @select-node="selectStructureNodeFromTree"
        @node-context-menu="openSectionTreeContextMenu"
      />
      <SectionWorkspace
        :section="sectionShell"
        :content-blocks="mockContentBlockDisplays"
        :structured-blocks="mockStructuredBlocks"
        :selected-node-id="selectedStructureNodeId"
        :workspace-node-map="workspaceNodeMap"
        :scroll-target-node-id="workspaceScrollTargetNodeId"
        :scroll-request-key="workspaceScrollRequestKey"
        :active-insert-point-id="activeInsertPointId"
        :insert-feedback="insertFeedback"
        @select-node="selectStructureNode"
        @request-insert="requestInsert"
      />

      <aside class="flex min-h-0 flex-col gap-3">
        <SectionTopToolbar />
        <SectionInspector class="min-h-0 flex-1" :node="selectedStructureNode" />
      </aside>
    </section>

    <InsertCreateOverlay
      v-if="activeCreatePanel"
      :model="activeCreatePanel"
      :open="activeCreatePanel !== null"
      @cancel="cancelInsertCreateOverlay"
      @submit="submitInsertCreateOverlay"
    />

    <SectionTreeContextMenu
      :model="sectionTreeContextMenu"
      :open="sectionTreeContextMenu !== null"
      @close="closeSectionTreeContextMenu"
      @request-action="handleSectionTreeContextMenuAction"
    />
  </main>
</template>
