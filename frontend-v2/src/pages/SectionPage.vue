<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import SectionInspector from '@/components/business/SectionInspector.vue'
import SectionTreeContextMenu from '@/components/business/SectionTreeContextMenu.vue'
import TeachingTopicTree from '@/components/business/TeachingTopicTree.vue'
import TeachingTopicTreeContextMenu from '@/components/business/TeachingTopicTreeContextMenu.vue'
import InsertCreateOverlay from '@/components/containers/InsertCreateOverlay.vue'
import SectionStructurePanel from '@/components/containers/SectionStructurePanel.vue'
import SectionTopToolbar from '@/components/containers/SectionTopToolbar.vue'
import SectionWorkspace from '@/components/containers/SectionWorkspace.vue'
import { cmsV2Api } from '@/apis/cmsV2Client'
import { loadSectionPageData, type SectionPageDataModel } from '@/composables/useSectionPageData'
import type {
  InsertCreateContentBlockType,
  InsertCreateDifficulty,
  InsertCreatePanelModel,
  InsertCreateSubmitPayload,
  InsertRequestModel,
  SectionPageShellModel,
  SectionTreeContextMenuActionPayload,
  SectionTreeContextMenuModel,
  SectionTreeContextMenuPayload,
  SectionTreeNodeModel,
  TeachingTopicTreeContextMenuActionPayload,
  TeachingTopicTreeContextMenuModel,
  TeachingTopicTreeContextMenuPayload,
} from '@/types'

const route = useRoute()
const { t } = useI18n()
const sectionPageData = ref<SectionPageDataModel | null>(null)
const selectedStructureNodeId = ref<string>()
const activeInsertPointId = ref<string>()
const activeCreatePanel = ref<InsertCreatePanelModel | null>(null)
const sectionTreeContextMenu = ref<SectionTreeContextMenuModel | null>(null)
const insertFeedback = ref('')
const workspaceScrollTargetNodeId = ref<string>()
const workspaceScrollRequestKey = ref(0)
const teachingTopicDrawerOpen = ref(false)
const selectedTeachingTopicId = ref<string>()
const teachingTopicTreeContextMenu = ref<TeachingTopicTreeContextMenuModel | null>(null)
const isLoadingSectionPage = ref(false)
const sectionPageError = ref('')
const isSubmittingInsertCreate = ref(false)
let teachingTopicDrawerTimer: number | undefined
let sectionPageLoadSequence = 0

const sectionId = computed(() => {
  const value = route.params.sectionId
  return Array.isArray(value) ? value.join('/') : value
})

const sectionShell = computed<SectionPageShellModel>(() => {
  if (sectionPageData.value) {
    return sectionPageData.value.section
  }

  return {
    sectionId: sectionId.value || 'pending',
    title: isLoadingSectionPage.value
      ? t('sectionPage.api.loadingTitle')
      : t('sectionPage.api.emptyTitle'),
    teachingTopicTitle: 'TeachingTopic',
    status: isLoadingSectionPage.value
      ? t('sectionPage.api.loadingStatus')
      : t('sectionPage.api.emptyStatus'),
  }
})

const sectionTreeNodes = computed(() => sectionPageData.value?.treeNodes ?? [])
const sectionWorkspaceFlowItems = computed(() => sectionPageData.value?.flowItems ?? [])
const workspaceNodeMap = computed(() => sectionPageData.value?.workspaceNodeMap ?? {})
const teachingTopicTreeNodes = computed(() => sectionPageData.value?.teachingTopicNodes ?? [])
const activeCreatePanelModel = computed(() =>
  activeCreatePanel.value
    ? {
        ...activeCreatePanel.value,
        disabled: activeCreatePanel.value.disabled || isSubmittingInsertCreate.value,
      }
    : null,
)

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
  selectedStructureNodeId.value
    ? findSectionTreeNode(sectionTreeNodes.value, selectedStructureNodeId.value)
    : undefined,
)

const contextTargetNodeId = computed(() => sectionTreeContextMenu.value?.node.id)
const teachingTopicContextTargetId = computed(() => teachingTopicTreeContextMenu.value?.node.id)

async function loadCurrentSectionPage() {
  const loadId = ++sectionPageLoadSequence
  isLoadingSectionPage.value = true
  sectionPageError.value = ''

  try {
    const data = await loadSectionPageData(sectionId.value)

    if (loadId !== sectionPageLoadSequence) {
      return
    }

    sectionPageData.value = data
    selectedTeachingTopicId.value = data.selectedTeachingTopicId

    if (
      !selectedStructureNodeId.value ||
      !findSectionTreeNode(data.treeNodes, selectedStructureNodeId.value)
    ) {
      selectedStructureNodeId.value = data.defaultSelectedNodeId
    }
  } catch (error) {
    if (loadId !== sectionPageLoadSequence) {
      return
    }

    sectionPageData.value = null
    selectedStructureNodeId.value = undefined
    selectedTeachingTopicId.value = undefined
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.api.loadError')
  } finally {
    if (loadId === sectionPageLoadSequence) {
      isLoadingSectionPage.value = false
    }
  }
}

function startTeachingTopicDrawerTimer() {
  stopTeachingTopicDrawerTimer()
  teachingTopicDrawerTimer = window.setTimeout(() => {
    teachingTopicDrawerOpen.value = true
  }, 2000)
}

function stopTeachingTopicDrawerTimer() {
  if (teachingTopicDrawerTimer) {
    window.clearTimeout(teachingTopicDrawerTimer)
    teachingTopicDrawerTimer = undefined
  }
}

function openTeachingTopicDrawer() {
  stopTeachingTopicDrawerTimer()
  teachingTopicDrawerOpen.value = true
}

function closeTeachingTopicDrawer() {
  stopTeachingTopicDrawerTimer()
  teachingTopicDrawerOpen.value = false
  closeTeachingTopicTreeContextMenu()
}

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
    const currentSectionId = Number(sectionShell.value.sectionId)

    if (!Number.isInteger(currentSectionId) || currentSectionId <= 0) {
      insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
      return
    }

    insertFeedback.value = ''
    activeCreatePanel.value = {
      insertPointId: request.insertPointId,
      targetType: request.actionType === 'CreateContentBlock' ? 'ContentBlock' : 'AtomicSection',
      insertPositionLabel: t('sectionPage.workspace.insertPanel.insertPositionLabel'),
      sectionId: currentSectionId,
      sectionTitle: sectionShell.value.title,
    }
    return
  }

  activeCreatePanel.value = null
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSearchExistingBlock')
}

function cancelInsertCreateOverlay() {
  activeCreatePanel.value = null
}

async function submitInsertCreateOverlay(payload: InsertCreateSubmitPayload) {
  if (isSubmittingInsertCreate.value) {
    return
  }

  isSubmittingInsertCreate.value = true
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSubmitting')

  try {
    const sortOrder = getSortOrderForInsertPoint(payload.insertPointId)
    const createdTarget =
      payload.targetType === 'ContentBlock'
        ? await createContentBlockForInsert(payload)
        : await createAtomicSectionForInsert(payload)
    const createdSectionItem = await cmsV2Api.addSectionItem(payload.sectionId, {
      targetType: payload.targetType,
      targetId: createdTarget.id,
      referenceMode: 'FollowLatest',
      lockedContentBlockVersionId: null,
      sortOrder,
      titleOverride: null,
      parentItemId: null,
      selectionLayer: null,
      teachingUseOverride: null,
      status: 'Active',
      note: null,
    })

    activeCreatePanel.value = null
    activeInsertPointId.value = undefined
    insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackCreateSubmitted', {
      targetType: payload.targetType,
      title: payload.title,
    })

    await loadCurrentSectionPage()
    selectedStructureNodeId.value = `section-item-${createdSectionItem.id}`
    workspaceScrollTargetNodeId.value = selectedStructureNodeId.value
    workspaceScrollRequestKey.value += 1
  } catch (error) {
    insertFeedback.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.insertPanel.feedbackCreateFailed')
  } finally {
    isSubmittingInsertCreate.value = false
  }
}

async function createContentBlockForInsert(payload: InsertCreateSubmitPayload) {
  const created = await cmsV2Api.createContentBlockWithBlankDocument({
    sectionId: payload.sectionId,
    title: payload.title,
    blockType: mapInsertContentBlockType(payload.contentBlockType),
    summary: null,
    difficulty: mapInsertDifficulty(payload.difficulty),
    questionType: null,
    status: 'Draft',
  })

  return { id: created.contentBlockId }
}

async function createAtomicSectionForInsert(payload: InsertCreateSubmitPayload) {
  const created = await cmsV2Api.createAtomicSection({
    sectionId: payload.sectionId,
    title: payload.title,
    description: payload.note ?? null,
    type: 'Custom',
    difficulty: mapInsertDifficulty(payload.difficulty),
    status: 'Draft',
  })

  return { id: created.id }
}

function getSortOrderForInsertPoint(insertPointId: string) {
  const indexText = /-(\d+)$/.exec(insertPointId)?.[1]
  const index = indexText ? Number(indexText) : Number.NaN
  const items = sectionWorkspaceFlowItems.value

  if (Number.isInteger(index) && index > 0 && index < items.length) {
    const previous = items[index - 1]
    const next = items[index]

    if (
      typeof previous.sortOrder === 'number' &&
      typeof next.sortOrder === 'number' &&
      next.sortOrder - previous.sortOrder > 1
    ) {
      return Math.floor((previous.sortOrder + next.sortOrder) / 2)
    }

    if (typeof next.sortOrder === 'number') {
      return next.sortOrder
    }
  }

  const lastSortOrder = items.reduce(
    (max, item) => Math.max(max, typeof item.sortOrder === 'number' ? item.sortOrder : 0),
    0,
  )

  return lastSortOrder + 10
}

function mapInsertContentBlockType(type?: InsertCreateContentBlockType) {
  const map: Record<InsertCreateContentBlockType, string> = {
    知识点: 'KnowledgePoint',
    例题: 'Question',
    变式题: 'Question',
    练习题: 'Question',
    变式题组: 'VariantGroup',
    练习题组: 'ExerciseGroup',
  }

  return type ? map[type] : 'GeneralText'
}

function mapInsertDifficulty(difficulty: InsertCreateDifficulty) {
  const map: Record<InsertCreateDifficulty, string> = {
    基础: 'Basic',
    中档: 'Medium',
    提高: 'Advanced',
    压轴: 'Top',
  }

  return map[difficulty]
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
    const currentSectionId = Number(sectionShell.value.sectionId)

    if (!Number.isInteger(currentSectionId) || currentSectionId <= 0) {
      insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
      return
    }

    activeInsertPointId.value = `section-tree-context-${payload.nodeId}`
    insertFeedback.value = ''
    activeCreatePanel.value = {
      insertPointId: activeInsertPointId.value,
      targetType: payload.actionType === 'CreateContentBlock' ? 'ContentBlock' : 'AtomicSection',
      insertPositionLabel: contextNode.typeLabel,
      sectionId: currentSectionId,
      sectionTitle: sectionShell.value.title,
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

function selectTeachingTopic(topicId: string) {
  selectedTeachingTopicId.value = topicId
  closeTeachingTopicTreeContextMenu()
}

function openTeachingTopicTreeContextMenu(payload: TeachingTopicTreeContextMenuPayload) {
  teachingTopicTreeContextMenu.value = {
    node: payload.node,
    position: {
      x: payload.x,
      y: payload.y,
    },
  }
}

function closeTeachingTopicTreeContextMenu() {
  teachingTopicTreeContextMenu.value = null
}

function handleTeachingTopicTreeContextMenuAction(
  _payload: TeachingTopicTreeContextMenuActionPayload,
) {
  closeTeachingTopicTreeContextMenu()
}

function handleDocumentKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && teachingTopicDrawerOpen.value) {
    closeTeachingTopicDrawer()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleDocumentKeydown)
  void loadCurrentSectionPage()
})

onBeforeUnmount(() => {
  stopTeachingTopicDrawerTimer()
  document.removeEventListener('keydown', handleDocumentKeydown)
})

watch(sectionId, () => {
  void loadCurrentSectionPage()
})
</script>

<template>
  <main class="min-h-screen bg-background text-foreground xl:h-screen xl:overflow-hidden">
    <button
      type="button"
      class="fixed inset-y-0 left-0 z-30 w-3 cursor-default bg-transparent focus-visible:bg-muted/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
      :aria-label="t('sectionPage.teachingTopicDrawer.triggerLabel')"
      @mouseenter="startTeachingTopicDrawerTimer"
      @mouseleave="stopTeachingTopicDrawerTimer"
      @focus="openTeachingTopicDrawer"
    />

    <section class="grid min-h-screen grid-cols-[minmax(0,1fr)] gap-3 p-3 xl:h-full xl:min-h-0 xl:grid-cols-[240px_minmax(0,1fr)_280px]">
      <SectionStructurePanel
        :nodes="sectionTreeNodes"
        :selected-node-id="selectedStructureNodeId"
        :context-target-node-id="contextTargetNodeId"
        @select-node="selectStructureNodeFromTree"
        @node-context-menu="openSectionTreeContextMenu"
      />
      <SectionWorkspace
        :section="sectionShell"
        :flow-items="sectionWorkspaceFlowItems"
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
      v-if="activeCreatePanelModel"
      :model="activeCreatePanelModel"
      :open="activeCreatePanelModel !== null"
      @cancel="cancelInsertCreateOverlay"
      @submit="submitInsertCreateOverlay"
    />

    <SectionTreeContextMenu
      :model="sectionTreeContextMenu"
      :open="sectionTreeContextMenu !== null"
      @close="closeSectionTreeContextMenu"
      @request-action="handleSectionTreeContextMenuAction"
    />

    <Teleport to="body">
      <div
        v-if="teachingTopicDrawerOpen"
        class="fixed inset-0 z-40"
        role="dialog"
        aria-modal="true"
        :aria-label="t('sectionPage.teachingTopicDrawer.dialogLabel')"
      >
        <button
          type="button"
          class="absolute inset-0 bg-background/70 backdrop-blur-sm"
          :aria-label="t('sectionPage.teachingTopicDrawer.closeLabel')"
          @click="closeTeachingTopicDrawer"
        />

        <aside
          class="relative z-10 m-3 max-h-[calc(100vh-1.5rem)] w-max max-w-[calc(100vw-1.5rem)] overflow-auto rounded-lg border bg-card p-3 text-card-foreground"
        >
          <TeachingTopicTree
            :nodes="teachingTopicTreeNodes"
            :selected-topic-id="selectedTeachingTopicId"
            :context-target-topic-id="teachingTopicContextTargetId"
            full-width-content
            @select-topic="selectTeachingTopic"
            @node-context-menu="openTeachingTopicTreeContextMenu"
          />
        </aside>
      </div>
    </Teleport>

    <TeachingTopicTreeContextMenu
      :model="teachingTopicTreeContextMenu"
      :open="teachingTopicTreeContextMenu !== null"
      @close="closeTeachingTopicTreeContextMenu"
      @request-action="handleTeachingTopicTreeContextMenuAction"
    />

    <div
      v-if="isLoadingSectionPage || sectionPageError"
      class="fixed bottom-3 left-3 z-50 max-w-md rounded-md border bg-card px-3 py-2 text-xs text-card-foreground"
      role="status"
    >
      <p v-if="isLoadingSectionPage" class="font-medium">
        {{ t('sectionPage.api.loadingMessage') }}
      </p>
      <p v-else class="font-medium">
        {{ t('sectionPage.api.errorTitle') }}
      </p>
      <p v-if="sectionPageError" class="mt-1 text-muted-foreground">
        {{ sectionPageError }}
      </p>
    </div>
  </main>
</template>
