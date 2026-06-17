<script setup lang="ts">
import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import SectionInspector from '@/components/business/SectionInspector.vue'
import SectionStructurePanel from '@/components/containers/SectionStructurePanel.vue'
import SectionTopToolbar from '@/components/containers/SectionTopToolbar.vue'
import SectionWorkspace from '@/components/containers/SectionWorkspace.vue'
import {
  mockContentBlockDisplays,
  mockSectionPageShells,
  mockSectionTreeNodes,
  mockStructuredBlocks,
} from '@/mocks'
import type { SectionPageShellModel, SectionTreeNodeModel } from '@/types'
import type { InsertRequestModel } from '@/types'

const route = useRoute()
const { t } = useI18n()
const selectedStructureNodeId = ref('section-tree-atomic-basics')
const activeInsertPointId = ref<string>()
const insertFeedback = ref('')

const workspaceNodeMap: Record<string, string> = {
  'display-energy-law': 'section-tree-law',
  'atomic-energy-basics': 'section-tree-atomic-basics',
  'composite-circular-track': 'section-tree-composite',
  'display-locked-example': 'section-tree-example-one',
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

function clearActiveInsertPoint() {
  activeInsertPointId.value = undefined
  insertFeedback.value = ''
}

function selectStructureNode(nodeId: string) {
  selectedStructureNodeId.value = nodeId
  clearActiveInsertPoint()
}

function requestInsert(request: InsertRequestModel) {
  activeInsertPointId.value = request.insertPointId
  const feedbackKeyByAction: Record<InsertRequestModel['actionType'], string> = {
    CreateContentBlock: 'sectionPage.workspace.insertPanel.feedbackCreateContentBlock',
    CreateAtomicSection: 'sectionPage.workspace.insertPanel.feedbackCreateAtomicSection',
    SearchExistingBlock: 'sectionPage.workspace.insertPanel.feedbackSearchExistingBlock',
  }

  insertFeedback.value = t(feedbackKeyByAction[request.actionType])
}
</script>

<template>
  <main class="min-h-screen bg-background text-foreground xl:h-screen xl:overflow-hidden">
    <section class="grid min-h-screen grid-cols-[minmax(0,1fr)] gap-3 p-3 xl:h-full xl:min-h-0 xl:grid-cols-[240px_minmax(0,1fr)_280px]">
      <SectionStructurePanel
        :nodes="mockSectionTreeNodes"
        :selected-node-id="selectedStructureNodeId"
        @select-node="selectStructureNode"
      />
      <SectionWorkspace
        :section="sectionShell"
        :content-blocks="mockContentBlockDisplays"
        :structured-blocks="mockStructuredBlocks"
        :selected-node-id="selectedStructureNodeId"
        :workspace-node-map="workspaceNodeMap"
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
  </main>
</template>
