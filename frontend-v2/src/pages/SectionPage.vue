<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import SectionInspector from '@/components/business/SectionInspector.vue'
import SectionTreeContextMenu from '@/components/business/SectionTreeContextMenu.vue'
import TeachingTopicTree from '@/components/business/TeachingTopicTree.vue'
import TeachingTopicTreeContextMenu from '@/components/business/TeachingTopicTreeContextMenu.vue'
import InsertCreateOverlay from '@/components/containers/InsertCreateOverlay.vue'
import SectionStructurePanel from '@/components/containers/SectionStructurePanel.vue'
import SectionTopToolbar from '@/components/containers/SectionTopToolbar.vue'
import SectionWorkspace from '@/components/containers/SectionWorkspace.vue'
import { cmsV2Api } from '@/apis/cmsV2Client'
import { Button } from '@/components/ui/button'
import { useAtomicSectionActions } from '@/composables/useAtomicSectionActions'
import { useContentBlockActions } from '@/composables/useContentBlockActions'
import { useContentBlockRelationActions } from '@/composables/useContentBlockRelationActions'
import { useSectionItemActions } from '@/composables/useSectionItemActions'
import { loadSectionPageData, type SectionPageDataModel } from '@/composables/useSectionPageData'
import { resolveAtomicSectionChildContentBlockTitle } from '@/utils/sectionInsertDefaults'
import { createTeachingTopicNodeId, findTeachingTopicTreeNodePath } from '@/utils/teachingStructureTree'
import type {
  InsertCreateContentBlockType,
  InsertCreateDifficulty,
  InsertCreatePanelModel,
  InsertCreateSubmitPayload,
  InsertRequestModel,
  AtomicSectionItemActionPayload,
  AtomicSectionItemMovePayload,
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  SectionPageShellModel,
  SectionTreeContextMenuActionPayload,
  SectionTreeContextMenuModel,
  SectionTreeContextMenuPayload,
  SectionTreeNodeModel,
  SectionWorkspaceFlowItemModel,
  TeachingTopicTreeContextMenuActionPayload,
  TeachingTopicTreeContextMenuModel,
  TeachingTopicTreeContextMenuPayload,
  TeachingTopicTreeNodeModel,
} from '@/types'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const sectionPageData = ref<SectionPageDataModel | null>(null)
const selectedStructureNodeId = ref<string>()
const activeInsertPointId = ref<string>()
const activeCreatePanel = ref<InsertCreatePanelModel | null>(null)
const sectionTreeContextMenu = ref<SectionTreeContextMenuModel | null>(null)
const teachingTopicTreeContextMenu = ref<TeachingTopicTreeContextMenuModel | null>(null)
const insertFeedback = ref('')
const insertCreateError = ref('')
const wrapSelectionMode = ref(false)
const wrapSelectedNodeIds = ref<string[]>([])
const wrapSelectionFeedback = ref('')
const wrappingAsAtomicSection = ref(false)
const workspaceScrollTargetNodeId = ref<string>()
const workspaceScrollRequestKey = ref(0)
const collapsedWorkspaceNodeIds = ref(new Set<string>())
const teachingTopicDrawerOpen = ref(false)
const selectedTeachingTopicId = ref<string>()
const teachingTopicDisplayRootNodeId = ref<string | null>(null)
const isLoadingSectionPage = ref(false)
const sectionPageError = ref('')
const isSubmittingInsertCreate = ref(false)
let teachingTopicDrawerTimer: number | undefined
let sectionPageLoadSequence = 0

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
const collapsedWorkspaceNodeIdList = computed(() => Array.from(collapsedWorkspaceNodeIds.value))
const teachingTopicTreeNodes = computed(() => sectionPageData.value?.teachingTopicNodes ?? [])
const teachingTopicDisplayRootPath = computed(() =>
  teachingTopicDisplayRootNodeId.value
    ? findTeachingTopicTreeNodePath(teachingTopicTreeNodes.value, teachingTopicDisplayRootNodeId.value)
    : [],
)
const teachingTopicDisplayRootNode = computed(
  () => teachingTopicDisplayRootPath.value.at(-1),
)
const visibleTeachingTopicTreeNodes = computed(() =>
  teachingTopicDisplayRootNode.value ? [teachingTopicDisplayRootNode.value] : teachingTopicTreeNodes.value,
)
const selectedTeachingTopicNodePath = computed(() =>
  selectedTeachingTopicId.value
    ? findTeachingTopicTreeNodePath(teachingTopicTreeNodes.value, selectedTeachingTopicId.value)
    : [],
)
const selectedTeachingTopicNode = computed(() => selectedTeachingTopicNodePath.value.at(-1))
const canSetSelectedTeachingTopicAsDisplayRoot = computed(() => {
  const node = selectedTeachingTopicNode.value

  return Boolean(
    node &&
      node.kind !== 'SectionVariant' &&
      node.canSetDisplayRoot &&
      teachingTopicDisplayRootNodeId.value !== node.id,
  )
})
const activeCreatePanelModel = computed(() =>
  activeCreatePanel.value
    ? {
        ...activeCreatePanel.value,
        disabled:
          activeCreatePanel.value.disabled ||
          isSubmittingInsertCreate.value ||
          wrappingAsAtomicSection.value,
      }
    : null,
)
const wrapSelectedFlowItems = computed(() =>
  sectionWorkspaceFlowItems.value.filter((item) => wrapSelectedNodeIds.value.includes(item.nodeId)),
)
const wrapSelectedSectionItemIds = computed(() =>
  wrapSelectedFlowItems.value
    .map((item) => item.sectionItemId)
    .filter((id): id is number => typeof id === 'number'),
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
const teachingTopicContextTargetNodeId = computed(() => teachingTopicTreeContextMenu.value?.node.id)

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
      teachingTopicDisplayRootNodeId.value &&
      !findTeachingTopicTreeNodePath(data.teachingTopicNodes, teachingTopicDisplayRootNodeId.value)
        .length
    ) {
      teachingTopicDisplayRootNodeId.value = null
    }

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

const sectionItemActions = useSectionItemActions({
  refreshSection: loadCurrentSectionPage,
})

const atomicSectionActions = useAtomicSectionActions({
  refreshSection: loadCurrentSectionPage,
})

const contentBlockActions = useContentBlockActions({
  setFeedback: (message) => {
    insertFeedback.value = message
  },
  refreshSection: loadCurrentSectionPage,
  wordEditStartedMessage: t('sectionPage.workspace.contentBlockActions.wordEditStarted'),
  wordEditSyncingMessage: t('sectionPage.workspace.contentBlockActions.wordEditSyncing'),
  wordEditSyncedMessage: t('sectionPage.workspace.contentBlockActions.wordEditSynced'),
  wordEditNoChangesMessage: t('sectionPage.workspace.contentBlockActions.wordEditNoChanges'),
  wordEditCancelledMessage: t('sectionPage.workspace.contentBlockActions.wordEditCancelled'),
  wordEditFailedMessage: t('sectionPage.workspace.contentBlockActions.wordEditFailed'),
})

const contentBlockRelationActions = useContentBlockRelationActions({
  refreshSection: loadCurrentSectionPage,
})

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
  insertCreateError.value = ''
}

function clearWrapSelection() {
  wrapSelectedNodeIds.value = []
  wrapSelectionFeedback.value = ''
}

function enterWrapSelectionMode() {
  clearActiveInsertPoint()
  closeSectionTreeContextMenu()
  wrapSelectionMode.value = true
  clearWrapSelection()
}

function cancelWrapSelectionMode() {
  wrapSelectionMode.value = false
  clearWrapSelection()
}

function getCurrentNumericSectionId() {
  const currentSectionId = Number(sectionShell.value.sectionId)

  return Number.isInteger(currentSectionId) && currentSectionId > 0 ? currentSectionId : undefined
}

function selectStructureNode(nodeId: string) {
  selectedStructureNodeId.value = nodeId
  closeSectionTreeContextMenu()
  clearActiveInsertPoint()
  cancelWrapSelectionMode()
}

function isWrappableWorkspaceItem(item: SectionWorkspaceFlowItemModel) {
  return item.kind !== 'AtomicSection' && typeof item.sectionItemId === 'number'
}

function selectWorkspaceNode(nodeId: string, event?: MouseEvent) {
  if (wrapSelectionMode.value) {
    toggleWrapNodeSelection(nodeId)
    return
  }

  selectedStructureNodeId.value = nodeId
  closeSectionTreeContextMenu()
  clearActiveInsertPoint()
  clearWrapSelection()
  void event
}

function toggleWrapNodeSelection(nodeId: string) {
  if (!wrapSelectionMode.value) {
    return
  }

  const item = sectionWorkspaceFlowItems.value.find((flowItem) => flowItem.nodeId === nodeId)

  if (!item || !isWrappableWorkspaceItem(item)) {
    wrapSelectionFeedback.value = t('sectionPage.workspace.wrap.atomicSectionNotAllowed')
    return
  }

  const next = new Set(wrapSelectedNodeIds.value)

  if (next.has(nodeId)) {
    next.delete(nodeId)
  } else {
    next.add(nodeId)
  }

  wrapSelectedNodeIds.value = Array.from(next)
  wrapSelectionFeedback.value = ''
}

function selectStructureNodeFromTree(nodeId: string) {
  selectStructureNode(nodeId)
  workspaceScrollTargetNodeId.value = nodeId
  workspaceScrollRequestKey.value += 1
}

function toggleWorkspaceNodeCollapse(nodeId: string) {
  const next = new Set(collapsedWorkspaceNodeIds.value)

  if (next.has(nodeId)) {
    next.delete(nodeId)
  } else {
    next.add(nodeId)
  }

  collapsedWorkspaceNodeIds.value = next
}

function getInsertPositionLabel(insertPointId: string) {
  return insertPointId === 'insert-first-section-item'
    ? t('sectionPage.workspace.insertPanel.firstInsertPositionLabel')
    : t('sectionPage.workspace.insertPanel.insertPositionLabel')
}

function requestInsert(request: InsertRequestModel) {
  cancelWrapSelectionMode()
  activeInsertPointId.value = request.insertPointId
  insertCreateError.value = ''

  if (request.actionType === 'CreateContentBlock' || request.actionType === 'CreateAtomicSection') {
    const currentSectionId = getCurrentNumericSectionId()

    if (!currentSectionId) {
      insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
      return
    }

    insertFeedback.value = ''
    activeCreatePanel.value = {
      insertPointId: request.insertPointId,
      targetType: request.actionType === 'CreateContentBlock' ? 'ContentBlock' : 'AtomicSection',
      insertPositionLabel: getInsertPositionLabel(request.insertPointId),
      sectionId: currentSectionId,
      sectionTitle: sectionShell.value.title,
      insertMode: 'SectionItem',
    }
    return
  }

  activeCreatePanel.value = null
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSearchExistingBlock')
}

function requestWrapAsAtomicSection() {
  const currentSectionId = getCurrentNumericSectionId()

  if (!currentSectionId) {
    wrapSelectionFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
    return
  }

  if (wrapSelectedSectionItemIds.value.length < 2) {
    wrapSelectionFeedback.value = t('sectionPage.workspace.wrap.invalidSelection')
    return
  }

  const sectionItemIds = [...wrapSelectedSectionItemIds.value]

  activeInsertPointId.value = undefined
  insertFeedback.value = ''
  insertCreateError.value = ''
  activeCreatePanel.value = {
    insertPointId: 'wrap-as-atomic-section',
    targetType: 'AtomicSection',
    insertPositionLabel: t('sectionPage.workspace.wrap.selectedPositionLabel', {
      count: sectionItemIds.length,
    }),
    sectionId: currentSectionId,
    sectionTitle: sectionShell.value.title,
    insertMode: 'WrapAsAtomicSection',
    wrapSectionItemIds: sectionItemIds,
  }
}

function requestAtomicChildContentBlock(payload: AtomicSectionWorkspaceActionPayload) {
  const currentSectionId = getCurrentNumericSectionId()

  if (!currentSectionId) {
    insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
    return
  }

  activeInsertPointId.value = `atomic-section-child-${payload.atomicSectionId}`
  insertFeedback.value = ''
  insertCreateError.value = ''
  activeCreatePanel.value = {
    insertPointId: activeInsertPointId.value,
    targetType: 'ContentBlock',
    insertPositionLabel: t('sectionPage.workspace.atomicSectionActions.insertChildPosition', {
      title: payload.title,
    }),
    sectionId: currentSectionId,
    sectionTitle: sectionShell.value.title,
    insertMode: 'AtomicSectionChild',
    atomicSectionId: payload.atomicSectionId,
    atomicSectionTitle: payload.title,
  }
}

async function requestAtomicMove(payload: AtomicSectionWorkspaceMovePayload) {
  const currentSectionId = getCurrentNumericSectionId()

  if (!currentSectionId) {
    return
  }

  try {
    if (payload.direction === 'Up') {
      await sectionItemActions.moveSectionItemUp(currentSectionId, payload.sectionItemId)
    } else {
      await sectionItemActions.moveSectionItemDown(currentSectionId, payload.sectionItemId)
    }

    selectedStructureNodeId.value = payload.nodeId
    workspaceScrollTargetNodeId.value = payload.nodeId
    workspaceScrollRequestKey.value += 1
  } catch (error) {
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.atomicSectionActions.operationFailed')
  }
}

async function requestAtomicRename(payload: AtomicSectionWorkspaceActionPayload) {
  const nextTitle = window.prompt(
    t('sectionPage.workspace.atomicSectionActions.renamePrompt'),
    payload.title,
  )

  if (nextTitle === null || nextTitle.trim() === '' || nextTitle.trim() === payload.title) {
    return
  }

  try {
    await atomicSectionActions.renameAtomicSection(payload.atomicSectionId, nextTitle.trim())
    selectedStructureNodeId.value = payload.nodeId
    workspaceScrollTargetNodeId.value = payload.nodeId
    workspaceScrollRequestKey.value += 1
  } catch (error) {
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.atomicSectionActions.operationFailed')
  }
}

async function requestAtomicRemove(payload: AtomicSectionWorkspaceActionPayload) {
  const currentSectionId = getCurrentNumericSectionId()

  if (!currentSectionId) {
    return
  }

  const confirmed = window.confirm(
    t('sectionPage.workspace.atomicSectionActions.removeConfirm', {
      title: payload.title,
    }),
  )

  if (!confirmed) {
    return
  }

  try {
    await sectionItemActions.removeSectionItemReference(currentSectionId, payload.sectionItemId)
    selectedStructureNodeId.value = undefined
    workspaceScrollTargetNodeId.value = selectedStructureNodeId.value
    workspaceScrollRequestKey.value += 1
  } catch (error) {
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.atomicSectionActions.operationFailed')
  }
}

async function requestAtomicSectionItemOpenWord(payload: AtomicSectionItemActionPayload) {
  try {
    await contentBlockActions.startContentBlockWordEdit(payload.contentBlockId)
    selectedStructureNodeId.value = payload.nodeId
  } catch (error) {
    sectionPageError.value =
      error instanceof Error
        ? error.message
        : t('sectionPage.workspace.atomicSectionItemActions.operationFailed')
  }
}

async function requestAtomicSectionItemMove(payload: AtomicSectionItemMovePayload) {
  try {
    if (payload.direction === 'Up') {
      await atomicSectionActions.moveAtomicSectionItemUp(
        payload.atomicSectionId,
        payload.atomicSectionItemId,
      )
    } else {
      await atomicSectionActions.moveAtomicSectionItemDown(
        payload.atomicSectionId,
        payload.atomicSectionItemId,
      )
    }

    selectedStructureNodeId.value = payload.nodeId
  } catch (error) {
    sectionPageError.value =
      error instanceof Error
        ? error.message
        : t('sectionPage.workspace.atomicSectionItemActions.operationFailed')
  }
}

async function requestAtomicSectionItemRemove(payload: AtomicSectionItemActionPayload) {
  const confirmed = window.confirm(
    t('sectionPage.workspace.atomicSectionItemActions.removeConfirm', {
      title: payload.title || 'ContentBlock',
    }),
  )

  if (!confirmed) {
    return
  }

  try {
    await atomicSectionActions.removeAtomicSectionItem(
      payload.atomicSectionId,
      payload.atomicSectionItemId,
    )
    selectedStructureNodeId.value = undefined
  } catch (error) {
    sectionPageError.value =
      error instanceof Error
        ? error.message
        : t('sectionPage.workspace.atomicSectionItemActions.operationFailed')
  }
}

async function requestContentBlockOpenWord(payload: ContentBlockWorkspaceActionPayload) {
  try {
    await contentBlockActions.startContentBlockWordEdit(payload.contentBlockId)
    selectedStructureNodeId.value = payload.nodeId
  } catch (error) {
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.contentBlockActions.operationFailed')
  }
}

async function requestContentBlockMove(payload: ContentBlockWorkspaceMovePayload) {
  const currentSectionId = getCurrentNumericSectionId()

  if (!currentSectionId) {
    return
  }

  try {
    if (payload.direction === 'Up') {
      await sectionItemActions.moveSectionItemUp(currentSectionId, payload.sectionItemId)
    } else {
      await sectionItemActions.moveSectionItemDown(currentSectionId, payload.sectionItemId)
    }

    selectedStructureNodeId.value = payload.nodeId
    workspaceScrollTargetNodeId.value = payload.nodeId
    workspaceScrollRequestKey.value += 1
  } catch (error) {
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.contentBlockActions.operationFailed')
  }
}

async function requestContentBlockRemove(payload: ContentBlockWorkspaceActionPayload) {
  const currentSectionId = getCurrentNumericSectionId()

  if (!currentSectionId) {
    return
  }

  const confirmed = window.confirm(
    t('sectionPage.workspace.contentBlockActions.removeConfirm', {
      title: payload.title || 'ContentBlock',
    }),
  )

  if (!confirmed) {
    return
  }

  try {
    await sectionItemActions.removeSectionItemReference(currentSectionId, payload.sectionItemId)
    selectedStructureNodeId.value = undefined
    workspaceScrollTargetNodeId.value = selectedStructureNodeId.value
    workspaceScrollRequestKey.value += 1
  } catch (error) {
    sectionPageError.value =
      error instanceof Error ? error.message : t('sectionPage.workspace.contentBlockActions.operationFailed')
  }
}

async function requestContentBlockRelationOpenWord(payload: ContentBlockRelationActionPayload) {
  try {
    await contentBlockActions.startContentBlockWordEdit(payload.contentBlockId)
    selectedStructureNodeId.value = payload.nodeId
  } catch (error) {
    sectionPageError.value =
      error instanceof Error
        ? error.message
        : t('sectionPage.workspace.contentBlockRelationActions.operationFailed')
  }
}

async function requestContentBlockRelationMove(payload: ContentBlockRelationMovePayload) {
  try {
    if (payload.direction === 'Up') {
      await contentBlockRelationActions.moveContentBlockRelationUp(
        payload.parentBlockId,
        payload.relationId,
      )
    } else {
      await contentBlockRelationActions.moveContentBlockRelationDown(
        payload.parentBlockId,
        payload.relationId,
      )
    }

    selectedStructureNodeId.value = payload.nodeId
  } catch (error) {
    sectionPageError.value =
      error instanceof Error
        ? error.message
        : t('sectionPage.workspace.contentBlockRelationActions.operationFailed')
  }
}

async function requestContentBlockRelationRemove(payload: ContentBlockRelationActionPayload) {
  const confirmed = window.confirm(
    t('sectionPage.workspace.contentBlockRelationActions.removeConfirm', {
      title: payload.title || 'ContentBlock',
    }),
  )

  if (!confirmed) {
    return
  }

  try {
    await contentBlockRelationActions.removeContentBlockRelation(
      payload.parentBlockId,
      payload.relationId,
    )
    selectedStructureNodeId.value = undefined
  } catch (error) {
    sectionPageError.value =
      error instanceof Error
        ? error.message
        : t('sectionPage.workspace.contentBlockRelationActions.operationFailed')
  }
}

function cancelInsertCreateOverlay() {
  activeCreatePanel.value = null
  insertCreateError.value = ''
}

async function submitInsertCreateOverlay(payload: InsertCreateSubmitPayload) {
  if (isSubmittingInsertCreate.value) {
    return
  }

  isSubmittingInsertCreate.value = true
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSubmitting')
  insertCreateError.value = ''

  try {
    if (payload.insertMode === 'WrapAsAtomicSection') {
      await submitWrapAsAtomicSection(payload)
      return
    }

    if (
      payload.targetType === 'ContentBlock' &&
      payload.insertMode === 'AtomicSectionChild' &&
      payload.atomicSectionId
    ) {
      await submitAtomicSectionChildContentBlock(payload)
      return
    }

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
    const message =
      error instanceof Error ? error.message : t('sectionPage.workspace.insertPanel.feedbackCreateFailed')
    insertFeedback.value = message
    insertCreateError.value = message
  } finally {
    isSubmittingInsertCreate.value = false
  }
}

async function submitWrapAsAtomicSection(payload: InsertCreateSubmitPayload) {
  const sectionItemIds = payload.wrapSectionItemIds ?? wrapSelectedSectionItemIds.value

  if (sectionItemIds.length < 2) {
    throw new Error(t('sectionPage.workspace.wrap.invalidSelection'))
  }

  try {
    wrappingAsAtomicSection.value = true
    const result = await sectionItemActions.wrapSectionItemsAsAtomicSection(payload.sectionId, {
      sectionItemIds,
      title: payload.title,
      description: payload.note ?? null,
      type: 'Custom',
      difficulty: mapInsertDifficulty(payload.difficulty),
      status: 'Draft',
    })

    activeCreatePanel.value = null
    activeInsertPointId.value = undefined
    cancelWrapSelectionMode()
    selectedStructureNodeId.value = `section-item-${result.sectionItemId}`
    insertFeedback.value = t('sectionPage.workspace.wrap.feedbackSuccess', {
      title: payload.title,
    })
  } finally {
    wrappingAsAtomicSection.value = false
  }
}

async function submitAtomicSectionChildContentBlock(payload: InsertCreateSubmitPayload) {
  const contentBlockTitle = resolveAtomicSectionChildContentBlockTitle({
    inputTitle: payload.title,
    atomicSectionTitle: payload.atomicSectionTitle ?? '',
  })
  await atomicSectionActions.createContentBlockInsideAtomicSection({
    atomicSectionId: payload.atomicSectionId!,
    sectionId: payload.sectionId,
    title: contentBlockTitle,
    blockType: mapInsertContentBlockType(payload.contentBlockType),
    difficulty: mapInsertDifficulty(payload.difficulty),
    sortOrder: getAtomicSectionChildSortOrder(payload.atomicSectionId!),
  })

  activeCreatePanel.value = null
  activeInsertPointId.value = undefined
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackCreateAtomicChildSubmitted', {
    title: contentBlockTitle || t('sectionPage.workspace.atomicSectionActions.untitledContentBlock'),
  })
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

function getAtomicSectionChildSortOrder(atomicSectionId: number) {
  const atomicItem = sectionWorkspaceFlowItems.value.find(
    (item) => item.kind === 'AtomicSection' && item.targetId === atomicSectionId,
  )

  if (!atomicItem || atomicItem.kind !== 'AtomicSection') {
    return 10
  }

  return (atomicItem.block.children.length + 1) * 10
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
    const currentSectionId = getCurrentNumericSectionId()

    if (!currentSectionId) {
      insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
      return
    }

    activeInsertPointId.value = `section-tree-context-${payload.nodeId}`
    insertFeedback.value = ''
    insertCreateError.value = ''
    activeCreatePanel.value = {
      insertPointId: activeInsertPointId.value,
      targetType: payload.actionType === 'CreateContentBlock' ? 'ContentBlock' : 'AtomicSection',
      insertPositionLabel: contextNode.typeLabel,
      sectionId: currentSectionId,
      sectionTitle: sectionShell.value.title,
      insertMode: 'SectionItem',
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

function getTeachingTopicContextNode() {
  const node = teachingTopicTreeContextMenu.value?.node

  if (
    !node ||
    node.kind === 'SectionVariant' ||
    !node.teachingTopicId ||
    node.disabled ||
    node.readOnly
  ) {
    return undefined
  }

  return node
}

function promptTeachingTopicName(messageKey: string, defaultValue: string) {
  const value = window.prompt(t(messageKey), defaultValue)

  return value?.trim()
}

async function handleTeachingTopicTreeContextMenuAction(
  payload: TeachingTopicTreeContextMenuActionPayload,
) {
  const contextNode = getTeachingTopicContextNode()
  closeTeachingTopicTreeContextMenu()

  if (!contextNode?.teachingTopicId) {
    return
  }

  try {
    if (payload.actionType === 'AddChild') {
      const name = promptTeachingTopicName(
        'sectionPage.teachingTopicDrawer.promptAddChild',
        t('sectionPage.teachingTopicDrawer.defaultChildName'),
      )

      if (!name) {
        return
      }

      const createdTopic = await cmsV2Api.createTeachingTopicChild(contextNode.teachingTopicId, {
        name,
        status: 'Active',
      })
      await loadCurrentSectionPage()
      selectedTeachingTopicId.value = createTeachingTopicNodeId(createdTopic.id)
      return
    }

    if (payload.actionType === 'AddAfter') {
      const name = promptTeachingTopicName(
        'sectionPage.teachingTopicDrawer.promptAddAfter',
        t('sectionPage.teachingTopicDrawer.defaultSiblingName'),
      )

      if (!name) {
        return
      }

      const createdTopic = await cmsV2Api.createTeachingTopicNextSibling(contextNode.teachingTopicId, {
        name,
        status: 'Active',
      })
      await loadCurrentSectionPage()
      selectedTeachingTopicId.value = createTeachingTopicNodeId(createdTopic.id)
      return
    }

    if (payload.actionType === 'CreateSection') {
      if (contextNode.sectionId) {
        return
      }

      await cmsV2Api.createSectionForTeachingTopic(contextNode.teachingTopicId, {
        title: contextNode.title,
        status: 'Draft',
      })
      await loadCurrentSectionPage()
      selectedTeachingTopicId.value = contextNode.id
      return
    }

    if (payload.actionType === 'Rename') {
      const name = promptTeachingTopicName(
        'sectionPage.teachingTopicDrawer.promptRename',
        contextNode.title,
      )

      if (!name) {
        return
      }

      await cmsV2Api.renameTeachingTopic(contextNode.teachingTopicId, {
        name,
      })
      await loadCurrentSectionPage()
      selectedTeachingTopicId.value = contextNode.id
      return
    }

    if (payload.actionType === 'Delete') {
      if (!contextNode.canDelete) {
        window.alert(t('sectionPage.teachingTopicDrawer.deleteDisabledMessage'))
        return
      }

      const confirmed = window.confirm(
        t('sectionPage.teachingTopicDrawer.confirmDelete', { title: contextNode.title }),
      )

      if (!confirmed) {
        return
      }

      await cmsV2Api.deleteTeachingTopic(contextNode.teachingTopicId)
      await loadCurrentSectionPage()
    }
  } catch (error) {
    window.alert(
      error instanceof Error ? error.message : t('sectionPage.teachingTopicDrawer.actionFailed'),
    )
  }
}

function openTeachingTopicSection(sectionId: number) {
  closeTeachingTopicDrawer()
  void router.push(`/sections/${sectionId}`)
}

function setSelectedTeachingTopicAsDisplayRoot() {
  if (!canSetSelectedTeachingTopicAsDisplayRoot.value || !selectedTeachingTopicNode.value) {
    return
  }

  teachingTopicDisplayRootNodeId.value = selectedTeachingTopicNode.value.id
}

function showParentTeachingTopicDisplayRoot() {
  if (teachingTopicDisplayRootPath.value.length <= 1) {
    teachingTopicDisplayRootNodeId.value = null
    return
  }

  teachingTopicDisplayRootNodeId.value =
    teachingTopicDisplayRootPath.value[teachingTopicDisplayRootPath.value.length - 2].id
}

function showAllTeachingTopicRoots() {
  teachingTopicDisplayRootNodeId.value = null
}

function formatTeachingTopicDisplayRootPath(path: TeachingTopicTreeNodeModel[]) {
  return path.length
    ? path.map((node) => node.title).join(' / ')
    : t('sectionPage.teachingTopicDrawer.allRoots')
}

function handleDocumentKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && wrapSelectionMode.value && !activeCreatePanel.value) {
    cancelWrapSelectionMode()
    return
  }

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
        :wrap-selection-mode="wrapSelectionMode"
        :wrap-selected-node-ids="wrapSelectedNodeIds"
        :wrap-selection-feedback="wrapSelectionFeedback"
        :collapsed-workspace-node-ids="collapsedWorkspaceNodeIdList"
        @select-node="selectWorkspaceNode"
        @toggle-workspace-node-collapse="toggleWorkspaceNodeCollapse"
        @request-insert="requestInsert"
        @enter-wrap-selection-mode="enterWrapSelectionMode"
        @cancel-wrap-selection-mode="cancelWrapSelectionMode"
        @clear-wrap-selection="clearWrapSelection"
        @toggle-wrap-node-selection="toggleWrapNodeSelection"
        @request-wrap-as-atomic-section="requestWrapAsAtomicSection"
        @request-atomic-child-content-block="requestAtomicChildContentBlock"
        @request-atomic-move="requestAtomicMove"
        @request-atomic-rename="requestAtomicRename"
        @request-atomic-remove="requestAtomicRemove"
        @request-atomic-section-item-open-word="requestAtomicSectionItemOpenWord"
        @request-atomic-section-item-move="requestAtomicSectionItemMove"
        @request-atomic-section-item-remove="requestAtomicSectionItemRemove"
        @request-content-block-open-word="requestContentBlockOpenWord"
        @request-content-block-move="requestContentBlockMove"
        @request-content-block-remove="requestContentBlockRemove"
        @request-content-block-relation-open-word="requestContentBlockRelationOpenWord"
        @request-content-block-relation-move="requestContentBlockRelationMove"
        @request-content-block-relation-remove="requestContentBlockRelationRemove"
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
      :error-message="insertCreateError"
      @cancel="cancelInsertCreateOverlay"
      @submit="submitInsertCreateOverlay"
    />

    <SectionTreeContextMenu
      :model="sectionTreeContextMenu"
      :open="sectionTreeContextMenu !== null"
      @close="closeSectionTreeContextMenu"
      @request-action="handleSectionTreeContextMenuAction"
    />

    <TeachingTopicTreeContextMenu
      :model="teachingTopicTreeContextMenu"
      :open="teachingTopicTreeContextMenu !== null"
      @close="closeTeachingTopicTreeContextMenu"
      @request-action="handleTeachingTopicTreeContextMenuAction"
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
          <div class="mb-3 flex min-w-80 items-start justify-between gap-3 border-b pb-3">
            <div class="min-w-0">
              <p class="text-xs text-muted-foreground">
                {{ t('sectionPage.teachingTopicDrawer.displayRootLabel') }}
              </p>
              <p class="mt-1 max-w-[56rem] truncate text-sm font-medium">
                {{ formatTeachingTopicDisplayRootPath(teachingTopicDisplayRootPath) }}
              </p>
            </div>
            <div class="flex shrink-0 flex-wrap justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                :disabled="teachingTopicDisplayRootPath.length === 0"
                @click="showParentTeachingTopicDisplayRoot"
              >
                {{ t('sectionPage.teachingTopicDrawer.backToParentRoot') }}
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                :disabled="teachingTopicDisplayRootPath.length === 0"
                @click="showAllTeachingTopicRoots"
              >
                {{ t('sectionPage.teachingTopicDrawer.backToAllRoots') }}
              </Button>
              <Button
                type="button"
                size="sm"
                :disabled="!canSetSelectedTeachingTopicAsDisplayRoot"
                @click="setSelectedTeachingTopicAsDisplayRoot"
              >
                {{ t('sectionPage.teachingTopicDrawer.setDisplayRoot') }}
              </Button>
            </div>
          </div>

          <TeachingTopicTree
            :nodes="visibleTeachingTopicTreeNodes"
            :selected-topic-id="selectedTeachingTopicId"
            :context-target-topic-id="teachingTopicContextTargetNodeId"
            :default-expanded-depth="teachingTopicDisplayRootNode ? 2 : 1"
            full-width-content
            @select-topic="selectTeachingTopic"
            @open-section="openTeachingTopicSection"
            @node-context-menu="openTeachingTopicTreeContextMenu"
          />
        </aside>
      </div>
    </Teleport>

    <Teleport to="body">
      <div
        v-if="wrappingAsAtomicSection"
        class="fixed inset-0 z-[70] flex min-h-screen items-center justify-center bg-background/70 p-4 backdrop-blur-sm"
        role="status"
        aria-live="assertive"
      >
        <div class="w-full max-w-sm rounded-lg border bg-card p-4 text-card-foreground">
          <p class="text-sm font-semibold">
            {{ t('sectionPage.workspace.wrap.blockingTitle') }}
          </p>
          <p class="mt-1 text-sm text-muted-foreground">
            {{ t('sectionPage.workspace.wrap.blockingDescription') }}
          </p>
        </div>
      </div>
    </Teleport>

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
