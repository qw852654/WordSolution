<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import SectionInspector from '@/components/business/SectionInspector.vue'
import SectionTreeContextMenu from '@/components/business/SectionTreeContextMenu.vue'
import SectionVariantCreatePanel from '@/components/business/SectionVariantCreatePanel.vue'
import TeachingTopicTree from '@/components/business/TeachingTopicTree.vue'
import TeachingTopicTreeContextMenu from '@/components/business/TeachingTopicTreeContextMenu.vue'
import InsertCreateOverlay from '@/components/containers/InsertCreateOverlay.vue'
import SectionStructurePanel from '@/components/containers/SectionStructurePanel.vue'
import SectionTopToolbar from '@/components/containers/SectionTopToolbar.vue'
import SectionWorkspace from '@/components/containers/SectionWorkspace.vue'
import {
  cmsV2Api,
  type CmsV2SectionVariantItemDto,
  type CmsV2SectionVariantSelectionCandidateDto,
} from '@/apis/cmsV2Client'
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
  SectionVariantCreateMetadata,
  SectionVariantPreviewState,
  SectionVariantSelectionCandidateModel,
  SectionTreeContextMenuActionPayload,
  SectionTreeContextMenuModel,
  SectionTreeContextMenuPayload,
  SectionTreeNodeModel,
  SectionWorkspaceFlowItemModel,
  StructuredBlockChildModel,
  StructuredBlockModel,
  TeachingTopicTreeContextMenuActionPayload,
  TeachingTopicTreeContextMenuModel,
  TeachingTopicTreeContextMenuPayload,
  TeachingTopicTreeNodeModel,
} from '@/types'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()
const SECTION_ITEM_REFERENCED_BY_VARIANT_MESSAGE =
  'SectionItem is referenced by SectionVariant and cannot be removed.'
const sectionPageData = ref<SectionPageDataModel | null>(null)
const selectedStructureNodeId = ref<string>()
const activeInsertPointId = ref<string>()
const activeCreatePanel = ref<InsertCreatePanelModel | null>(null)
const sectionVariantCreateMetadata = ref<SectionVariantCreateMetadata | null>(null)
const sectionVariantCreatePanelOpen = ref(false)
const sectionVariantSelectionMode = ref(false)
const sectionVariantSelectionFeedback = ref('')
const sectionVariantCreateError = ref('')
const sectionVariantCandidates = ref<SectionVariantSelectionCandidateModel[]>([])
const sectionVariantPreviewState = ref<SectionVariantPreviewState>('idle')
const sectionVariantPreviewError = ref('')
const isCreatingSectionVariant = ref(false)
const sectionVariantItems = ref<CmsV2SectionVariantItemDto[]>([])
const isLoadingSectionVariantItems = ref(false)
const sectionVariantViewError = ref('')
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

function resolveSectionItemRemoveError(error: unknown, fallback: string) {
  const message = error instanceof Error ? error.message : ''

  if (
    message === SECTION_ITEM_REFERENCED_BY_VARIANT_MESSAGE ||
    message.includes(SECTION_ITEM_REFERENCED_BY_VARIANT_MESSAGE) ||
    (message.includes('SectionVariant') && message.includes('cannot be removed'))
  ) {
    return t('sectionPage.workspace.sectionItemActions.referencedByVariant')
  }

  return message || fallback
}
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
          wrappingAsAtomicSection.value ||
          isCreatingSectionVariant.value,
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
const sectionVariantSelectedCandidateCount = computed(
  () =>
    sectionVariantCandidates.value.filter((candidate) => candidate.selectable && candidate.selected)
      .length,
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
const selectedSectionVariantNode = computed(() =>
  selectedStructureNode.value?.kind === 'SectionVariant' ? selectedStructureNode.value : undefined,
)
const sectionVariantViewMode = computed(() => Boolean(selectedSectionVariantNode.value))
const sectionVariantItemCount = computed(() =>
  sectionVariantViewMode.value ? sectionVariantItems.value.length : undefined,
)
const sectionVariantFlowItems = computed<SectionWorkspaceFlowItemModel[]>(() => {
  if (!sectionVariantViewMode.value) {
    return []
  }

  const flowItemBySectionItemId = new Map<number, SectionWorkspaceFlowItemModel>()

  for (const item of sectionWorkspaceFlowItems.value) {
    if (typeof item.sectionItemId === 'number') {
      flowItemBySectionItemId.set(item.sectionItemId, item)
    }
  }

  return [...sectionVariantItems.value]
    .sort((left, right) => left.sortOrder - right.sortOrder || left.id - right.id)
    .map((item) => flowItemBySectionItemId.get(item.sectionItemId))
    .filter((item): item is SectionWorkspaceFlowItemModel => Boolean(item))
})
const visibleWorkspaceFlowItems = computed(() =>
  sectionVariantViewMode.value ? sectionVariantFlowItems.value : sectionWorkspaceFlowItems.value,
)
const sectionVariantReadOnlyLabel = computed(() =>
  selectedSectionVariantNode.value
    ? t('sectionPage.sectionVariantView.readOnlyLabel', {
        title: selectedSectionVariantNode.value.title,
      })
    : '',
)

const contextTargetNodeId = computed(() => sectionTreeContextMenu.value?.node.id)
const teachingTopicContextTargetNodeId = computed(() => teachingTopicTreeContextMenu.value?.node.id)

function clearSectionVariantView() {
  sectionVariantItems.value = []
  isLoadingSectionVariantItems.value = false
  sectionVariantViewError.value = ''
}

async function loadSectionVariantItemsForNode(node?: SectionTreeNodeModel) {
  if (node?.kind !== 'SectionVariant' || typeof node.sectionVariantId !== 'number') {
    clearSectionVariantView()
    return
  }

  isLoadingSectionVariantItems.value = true
  sectionVariantViewError.value = ''

  try {
    sectionVariantItems.value = await cmsV2Api.listSectionVariantItems(node.sectionVariantId)
  } catch (error) {
    sectionVariantItems.value = []
    sectionVariantViewError.value =
      error instanceof Error ? error.message : t('sectionPage.sectionVariantView.loadFailed')
  } finally {
    isLoadingSectionVariantItems.value = false
  }
}

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

    await loadSectionVariantItemsForNode(
      selectedStructureNodeId.value
        ? findSectionTreeNode(data.treeNodes, selectedStructureNodeId.value)
        : undefined,
    )
  } catch (error) {
    if (loadId !== sectionPageLoadSequence) {
      return
    }

    sectionPageData.value = null
    selectedStructureNodeId.value = undefined
    selectedTeachingTopicId.value = undefined
    clearSectionVariantView()
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
  if (isCreatingSectionVariant.value) {
    return
  }

  selectedStructureNodeId.value = nodeId
  clearSectionVariantView()
  closeSectionTreeContextMenu()
  clearActiveInsertPoint()
  cancelWrapSelectionMode()
  cancelSectionVariantSelectionMode()
}

function isWrappableWorkspaceItem(item: SectionWorkspaceFlowItemModel) {
  return item.kind !== 'AtomicSection' && typeof item.sectionItemId === 'number'
}

function selectWorkspaceNode(nodeId: string, event?: MouseEvent) {
  if (isCreatingSectionVariant.value) {
    return
  }

  if (wrapSelectionMode.value) {
    toggleWrapNodeSelection(nodeId)
    return
  }

  selectedStructureNodeId.value = nodeId
  clearSectionVariantView()
  closeSectionTreeContextMenu()
  clearActiveInsertPoint()
  clearWrapSelection()
  cancelSectionVariantSelectionMode()
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
  if (isCreatingSectionVariant.value) {
    return
  }

  const node = findSectionTreeNode(sectionTreeNodes.value, nodeId)

  if (node?.kind === 'SectionVariant') {
    selectedStructureNodeId.value = nodeId
    closeSectionTreeContextMenu()
    clearActiveInsertPoint()
    cancelWrapSelectionMode()
    cancelSectionVariantSelectionMode()
    void loadSectionVariantItemsForNode(node)
    return
  }

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
  if (isCreatingSectionVariant.value) {
    return
  }

  cancelWrapSelectionMode()
  cancelSectionVariantSelectionMode()
  activeInsertPointId.value = request.insertPointId
  insertCreateError.value = ''

  if (request.actionType === 'CreateContentBlock' || request.actionType === 'CreateAtomicSection') {
    const currentSectionId = getCurrentNumericSectionId()

    if (!currentSectionId) {
      insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
      return
    }

    const parentType = request.placement?.parentType
    if (parentType === 'AtomicSection') {
      if (request.actionType !== 'CreateContentBlock') {
        insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSearchExistingBlock')
        return
      }

      const atomicSectionId = request.placement?.parentId
      const atomicSectionTitle = getAtomicSectionTitleById(atomicSectionId)

      insertFeedback.value = ''
      activeCreatePanel.value = {
        insertPointId: request.insertPointId,
        targetType: 'ContentBlock',
        insertPositionLabel: t('sectionPage.workspace.atomicSectionActions.insertChildPosition', {
          title: atomicSectionTitle || 'AtomicSection',
        }),
        sectionId: currentSectionId,
        sectionTitle: sectionShell.value.title,
        placement: request.placement,
        insertMode: 'AtomicSectionChild',
        atomicSectionId,
        atomicSectionTitle,
      }
      return
    }

    if (parentType === 'CompositeBlock') {
      if (request.actionType !== 'CreateContentBlock') {
        insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSearchExistingBlock')
        return
      }

      const compositeBlockId = request.placement?.parentId
      const compositeBlockTitle = getCompositeBlockTitleById(compositeBlockId)

      insertFeedback.value = ''
      activeCreatePanel.value = {
        insertPointId: request.insertPointId,
        targetType: 'ContentBlock',
        insertPositionLabel: compositeBlockTitle || 'CompositeBlock',
        sectionId: currentSectionId,
        sectionTitle: sectionShell.value.title,
        placement: request.placement,
        insertMode: 'CompositeBlockChild',
        compositeBlockId,
        compositeBlockTitle,
      }
      return
    }

    insertFeedback.value = ''
    activeCreatePanel.value = {
      insertPointId: request.insertPointId,
      targetType: request.actionType === 'CreateContentBlock' ? 'ContentBlock' : 'AtomicSection',
      insertPositionLabel: getInsertPositionLabel(request.insertPointId),
      sectionId: currentSectionId,
      sectionTitle: sectionShell.value.title,
      placement: request.placement,
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
    sectionPageError.value = resolveSectionItemRemoveError(
      error,
      t('sectionPage.workspace.atomicSectionActions.operationFailed'),
    )
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
    sectionPageError.value = resolveSectionItemRemoveError(
      error,
      t('sectionPage.workspace.contentBlockActions.operationFailed'),
    )
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

    if (
      payload.targetType === 'ContentBlock' &&
      payload.insertMode === 'CompositeBlockChild' &&
      payload.compositeBlockId
    ) {
      await submitCompositeBlockChildContentBlock(payload)
      return
    }

    const insertPlan = getSectionItemInsertPlan(payload)
    const createdTarget =
      payload.targetType === 'ContentBlock'
        ? await createContentBlockForInsert(payload)
        : await createAtomicSectionForInsert(payload)
    const createdSectionItem = await cmsV2Api.addSectionItem(payload.sectionId, {
      targetType: payload.targetType,
      targetId: createdTarget.id,
      referenceMode: 'FollowLatest',
      lockedContentBlockVersionId: null,
      sortOrder: insertPlan.sortOrder,
      titleOverride: null,
      parentItemId: null,
      selectionLayer: null,
      teachingUseOverride: null,
      status: 'Active',
      note: null,
    })

    if (insertPlan.moveUpAfterCreate) {
      await cmsV2Api.moveSectionItem(payload.sectionId, createdSectionItem.id, {
        direction: 'Up',
      })
    }

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
  const insertPlan = getNestedInsertPlan(
    payload,
    getAtomicSectionChildLastSortOrder(payload.atomicSectionId!) + 10,
  )
  const created = await atomicSectionActions.createContentBlockInsideAtomicSection({
    atomicSectionId: payload.atomicSectionId!,
    sectionId: payload.sectionId,
    title: contentBlockTitle,
    blockType: mapInsertContentBlockType(payload.contentBlockType),
    difficulty: mapInsertDifficulty(payload.difficulty),
    sortOrder: insertPlan.sortOrder,
  })

  if (insertPlan.moveUpAfterCreate) {
    await cmsV2Api.moveAtomicSectionItem(
      payload.atomicSectionId!,
      created.atomicSectionItem.id,
      { direction: 'Up' },
    )
    await loadCurrentSectionPage()
  }

  activeCreatePanel.value = null
  activeInsertPointId.value = undefined
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackCreateAtomicChildSubmitted', {
    title: contentBlockTitle || t('sectionPage.workspace.atomicSectionActions.untitledContentBlock'),
  })
}

async function createContentBlockForInsert(payload: InsertCreateSubmitPayload) {
  const created = await cmsV2Api.createContentBlock({
    sectionId: payload.sectionId,
    title: payload.title,
    blockType: mapInsertContentBlockType(payload.contentBlockType),
    summary: null,
    difficulty: mapInsertDifficulty(payload.difficulty),
    questionType: null,
    status: 'Draft',
  })

  return { id: created.id }
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

interface SectionItemInsertPlan {
  sortOrder: number
  moveUpAfterCreate: boolean
}

function getSectionItemInsertPlan(payload: InsertCreateSubmitPayload): SectionItemInsertPlan {
  const placement = payload.placement

  if (placement?.parentType === 'Section' && placement.parentId === payload.sectionId) {
    return getInsertPlanFromPlacement(payload, getSectionLastSortOrder() + 10)
  }

  return {
    sortOrder: getSectionLastSortOrder() + 10,
    moveUpAfterCreate: false,
  }
}

function getNestedInsertPlan(
  payload: InsertCreateSubmitPayload,
  fallbackSortOrder: number,
): SectionItemInsertPlan {
  return getInsertPlanFromPlacement(payload, fallbackSortOrder)
}

function getInsertPlanFromPlacement(
  payload: InsertCreateSubmitPayload,
  fallbackSortOrder: number,
): SectionItemInsertPlan {
  const placement = payload.placement
  const afterSortOrder = placement?.afterSortOrder
  const beforeSortOrder = placement?.beforeSortOrder

  if (
    typeof afterSortOrder === 'number' &&
    typeof beforeSortOrder === 'number' &&
    beforeSortOrder - afterSortOrder > 1
  ) {
    return {
      sortOrder: Math.floor((afterSortOrder + beforeSortOrder) / 2),
      moveUpAfterCreate: false,
    }
  }

  if (typeof beforeSortOrder === 'number') {
    return {
      sortOrder: beforeSortOrder,
      moveUpAfterCreate: true,
    }
  }

  if (typeof afterSortOrder === 'number') {
    return {
      sortOrder: afterSortOrder + 10,
      moveUpAfterCreate: false,
    }
  }

  return {
    sortOrder: fallbackSortOrder,
    moveUpAfterCreate: false,
  }
}

function getSectionLastSortOrder() {
  const items = sectionWorkspaceFlowItems.value
  return items.reduce(
    (max, item) => Math.max(max, typeof item.sortOrder === 'number' ? item.sortOrder : 0),
    0,
  )
}

function getAtomicSectionBlockById(atomicSectionId?: number) {
  if (!atomicSectionId) {
    return undefined
  }

  const atomicItem = sectionWorkspaceFlowItems.value.find(
    (item) => item.kind === 'AtomicSection' && item.targetId === atomicSectionId,
  )

  return atomicItem?.kind === 'AtomicSection' ? atomicItem.block : undefined
}

function getAtomicSectionTitleById(atomicSectionId?: number) {
  return getAtomicSectionBlockById(atomicSectionId)?.title
}

function getAtomicSectionChildLastSortOrder(atomicSectionId: number) {
  const atomicBlock = getAtomicSectionBlockById(atomicSectionId)

  if (!atomicBlock) {
    return 0
  }

  return atomicBlock.children.reduce(
    (max, child) => Math.max(max, typeof child.sortOrder === 'number' ? child.sortOrder : 0),
    0,
  )
}

function findCompositeBlockInChildren(
  children: StructuredBlockChildModel[],
  contentBlockId: number,
): StructuredBlockModel | undefined {
  for (const child of children) {
    if (child.kind !== 'CompositeBlock') {
      continue
    }

    if (child.contentBlockId === contentBlockId || child.block.contentBlockId === contentBlockId) {
      return child.block
    }

    const nested = findCompositeBlockInChildren(child.block.children, contentBlockId)
    if (nested) {
      return nested
    }
  }

  return undefined
}

function getCompositeBlockById(contentBlockId?: number) {
  if (!contentBlockId) {
    return undefined
  }

  for (const item of sectionWorkspaceFlowItems.value) {
    if (item.kind === 'CompositeBlock' && item.targetId === contentBlockId) {
      return item.block
    }

    if (item.kind !== 'ContentBlock') {
      const nested = findCompositeBlockInChildren(item.block.children, contentBlockId)
      if (nested) {
        return nested
      }
    }
  }

  return undefined
}

function getCompositeBlockTitleById(contentBlockId?: number) {
  return getCompositeBlockById(contentBlockId)?.title
}

function getCompositeBlockChildLastSortOrder(contentBlockId: number) {
  const compositeBlock = getCompositeBlockById(contentBlockId)

  if (!compositeBlock) {
    return 0
  }

  return compositeBlock.children.reduce(
    (max, child) => Math.max(max, typeof child.sortOrder === 'number' ? child.sortOrder : 0),
    0,
  )
}

interface SectionTreeWorkspaceContext {
  node: SectionTreeNodeModel
  topLevelItem?: SectionWorkspaceFlowItemModel
  child?: StructuredBlockChildModel
}

function findStructuredChildWorkspaceContext(
  children: StructuredBlockChildModel[],
  nodeId: string,
  topLevelItem: SectionWorkspaceFlowItemModel,
): SectionTreeWorkspaceContext | undefined {
  for (const child of children) {
    if (child.nodeId === nodeId) {
      return {
        node: findSectionTreeNode(sectionTreeNodes.value, nodeId)!,
        topLevelItem,
        child,
      }
    }

    if (child.kind === 'CompositeBlock') {
      const nested = findStructuredChildWorkspaceContext(child.block.children, nodeId, topLevelItem)
      if (nested) {
        return nested
      }
    }
  }

  return undefined
}

function findSectionTreeWorkspaceContext(node: SectionTreeNodeModel): SectionTreeWorkspaceContext {
  for (const item of sectionWorkspaceFlowItems.value) {
    if (item.nodeId === node.id) {
      return { node, topLevelItem: item }
    }

    if (item.kind !== 'ContentBlock') {
      const nested = findStructuredChildWorkspaceContext(item.block.children, node.id, item)
      if (nested) {
        return nested
      }
    }
  }

  return { node }
}

function getSectionEndPlacement(): InsertRequestModel['placement'] {
  const sectionId = getCurrentNumericSectionId()
  const lastItem = sectionWorkspaceFlowItems.value.at(-1)

  if (!sectionId) {
    return undefined
  }

  return {
    parentType: 'Section',
    parentId: sectionId,
    afterItemId: lastItem?.sectionItemId,
    afterSortOrder: lastItem?.sortOrder,
  }
}

function getSectionSiblingPlacementAfter(
  item?: SectionWorkspaceFlowItemModel,
): InsertRequestModel['placement'] {
  const sectionId = getCurrentNumericSectionId()

  if (!sectionId) {
    return undefined
  }

  return {
    parentType: 'Section',
    parentId: sectionId,
    afterItemId: item?.sectionItemId,
    afterSortOrder: item?.sortOrder,
  }
}

function getAtomicSectionEndPlacement(
  item: SectionWorkspaceFlowItemModel,
): InsertRequestModel['placement'] {
  if (item.kind !== 'AtomicSection') {
    return undefined
  }

  const parentId = item.block.atomicSectionId ?? item.targetId
  const lastChild = item.block.children.at(-1)

  if (!parentId) {
    return undefined
  }

  return {
    parentType: 'AtomicSection',
    parentId,
    afterItemId: lastChild?.atomicSectionItemId,
    afterSortOrder: lastChild?.sortOrder,
  }
}

function getCompositeBlockEndPlacement(
  item: SectionWorkspaceFlowItemModel,
): InsertRequestModel['placement'] {
  if (item.kind !== 'CompositeBlock') {
    return undefined
  }

  const parentId = item.block.contentBlockId ?? item.targetId
  const lastChild = item.block.children.at(-1)

  if (!parentId) {
    return undefined
  }

  return {
    parentType: 'CompositeBlock',
    parentId,
    afterItemId: lastChild?.relationId,
    afterSortOrder: lastChild?.sortOrder,
  }
}

function getNestedSiblingPlacementAfter(
  child?: StructuredBlockChildModel,
): InsertRequestModel['placement'] {
  if (!child) {
    return undefined
  }

  if (child.atomicSectionId && child.atomicSectionItemId) {
    return {
      parentType: 'AtomicSection',
      parentId: child.atomicSectionId,
      afterItemId: child.atomicSectionItemId,
      afterSortOrder: child.sortOrder,
    }
  }

  if (child.parentBlockId && child.relationId) {
    return {
      parentType: 'CompositeBlock',
      parentId: child.parentBlockId,
      afterItemId: child.relationId,
      afterSortOrder: child.sortOrder,
    }
  }

  return undefined
}

function createSectionTreeContextInsertRequest(
  node: SectionTreeNodeModel,
  actionType: 'CreateContentBlock' | 'CreateAtomicSection',
): InsertRequestModel {
  const context = findSectionTreeWorkspaceContext(node)
  let placement: InsertRequestModel['placement']

  if (node.kind === 'Section') {
    placement = getSectionEndPlacement()
  } else if (actionType === 'CreateAtomicSection') {
    placement = getSectionSiblingPlacementAfter(context.topLevelItem)
  } else if (context.child) {
    placement = getNestedSiblingPlacementAfter(context.child)
  } else if (context.topLevelItem?.kind === 'AtomicSection') {
    placement = getAtomicSectionEndPlacement(context.topLevelItem)
  } else if (context.topLevelItem?.kind === 'CompositeBlock') {
    placement = getCompositeBlockEndPlacement(context.topLevelItem)
  } else {
    placement = getSectionSiblingPlacementAfter(context.topLevelItem)
  }

  return {
    insertPointId: `section-tree-context-${node.id}-${actionType}`,
    actionType,
    placement,
  }
}

function getContextChildTitle(child: StructuredBlockChildModel) {
  if (child.kind === 'ContentBlock') {
    return child.block.title || child.block.role || 'ContentBlock'
  }

  return child.block.title || child.block.typeLabel || 'CompositeBlock'
}

function parseSectionItemNodeId(nodeId: string) {
  const match = /^section-item-(\d+)$/.exec(nodeId)
  return match ? Number(match[1]) : undefined
}

async function removeTopLevelSectionItemFromTreeNode(node: SectionTreeNodeModel) {
  const currentSectionId = getCurrentNumericSectionId()
  const sectionItemId = parseSectionItemNodeId(node.id)

  if (!currentSectionId || !sectionItemId) {
    sectionPageError.value = t('sectionPage.workspace.sectionItemActions.removeTargetMissing')
    return
  }

  const confirmed = window.confirm(
    t('sectionPage.workspace.sectionItemActions.removeConfirm', {
      title: node.title || node.typeLabel || 'SectionItem',
    }),
  )

  if (!confirmed) {
    return
  }

  try {
    await sectionItemActions.removeSectionItemReference(currentSectionId, sectionItemId)
    selectedStructureNodeId.value = undefined
    workspaceScrollTargetNodeId.value = selectedStructureNodeId.value
    workspaceScrollRequestKey.value += 1
  } catch (error) {
    sectionPageError.value = resolveSectionItemRemoveError(
      error,
      t('sectionPage.workspace.sectionItemActions.operationFailed'),
    )
  }
}

async function removeSectionTreeContextNode(node: SectionTreeNodeModel) {
  const context = findSectionTreeWorkspaceContext(node)

  if (context.child) {
    if (context.child.atomicSectionId && context.child.atomicSectionItemId && context.child.contentBlockId) {
      await requestAtomicSectionItemRemove({
        nodeId: context.child.nodeId,
        atomicSectionId: context.child.atomicSectionId,
        atomicSectionItemId: context.child.atomicSectionItemId,
        contentBlockId: context.child.contentBlockId,
        title: getContextChildTitle(context.child),
      })
      return
    }

    if (context.child.parentBlockId && context.child.relationId && context.child.contentBlockId) {
      await requestContentBlockRelationRemove({
        nodeId: context.child.nodeId,
        parentBlockId: context.child.parentBlockId,
        relationId: context.child.relationId,
        contentBlockId: context.child.contentBlockId,
        title: getContextChildTitle(context.child),
      })
      return
    }

    sectionPageError.value = t('sectionPage.workspace.sectionItemActions.removeTargetMissing')
    return
  }

  const item = context.topLevelItem
  if (!item?.sectionItemId || !item.targetId) {
    await removeTopLevelSectionItemFromTreeNode(node)
    return
  }

  if (item.kind === 'AtomicSection') {
    await requestAtomicRemove({
      nodeId: item.nodeId,
      sectionItemId: item.sectionItemId,
      atomicSectionId: item.targetId,
      title: item.block.title,
    })
    return
  }

  await requestContentBlockRemove({
    nodeId: item.nodeId,
    sectionItemId: item.sectionItemId,
    contentBlockId: item.targetId,
    title: item.kind === 'ContentBlock' ? item.block.title : item.block.title,
  })
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

function getFlowItemBySectionItemId(sectionItemId: number) {
  return sectionWorkspaceFlowItems.value.find((item) => item.sectionItemId === sectionItemId)
}

function getSectionVariantCandidateTitle(
  candidate: CmsV2SectionVariantSelectionCandidateDto,
  flowItem?: SectionWorkspaceFlowItemModel,
) {
  if (!flowItem) {
    return `${candidate.targetType} #${candidate.targetId}`
  }

  if (flowItem.kind === 'ContentBlock') {
    return flowItem.block.title || flowItem.block.role || 'ContentBlock'
  }

  return flowItem.block.title || flowItem.block.blockKind
}

function getSectionVariantCandidateDisplayType(
  candidate: CmsV2SectionVariantSelectionCandidateDto,
  flowItem?: SectionWorkspaceFlowItemModel,
) {
  if (!flowItem) {
    return candidate.targetType
  }

  return flowItem.kind
}

function mapSectionVariantSelectionCandidate(
  candidate: CmsV2SectionVariantSelectionCandidateDto,
): SectionVariantSelectionCandidateModel {
  const flowItem = getFlowItemBySectionItemId(candidate.sectionItemId)

  return {
    sectionItemId: candidate.sectionItemId,
    targetType: candidate.targetType,
    title: getSectionVariantCandidateTitle(candidate, flowItem),
    displayType: getSectionVariantCandidateDisplayType(candidate, flowItem),
    resolvedDifficulty: candidate.resolvedDifficulty,
    defaultSelected: candidate.defaultSelected,
    selected: candidate.selectable && candidate.defaultSelected,
    selectable: candidate.selectable,
    unavailableReason: candidate.unavailableReason ?? undefined,
  }
}

function openSectionVariantCreatePanel() {
  const currentSectionId = getCurrentNumericSectionId()

  if (!currentSectionId) {
    insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackMissingSection')
    return
  }

  cancelWrapSelectionMode()
  activeInsertPointId.value = undefined
  activeCreatePanel.value = null
  insertFeedback.value = ''
  insertCreateError.value = ''
  sectionVariantSelectionMode.value = false
  sectionVariantSelectionFeedback.value = ''
  sectionVariantCreateError.value = ''
  sectionVariantCandidates.value = []
  sectionVariantPreviewState.value = 'idle'
  sectionVariantPreviewError.value = ''
  sectionVariantCreateMetadata.value = {
    sectionId: currentSectionId,
    title: '',
    type: 'Lecture',
    difficulty: 'Basic',
    description: '',
  }
  sectionVariantCreatePanelOpen.value = true
}

function clearSectionVariantCreationFlow() {
  sectionVariantCreateMetadata.value = null
  sectionVariantCreatePanelOpen.value = false
  sectionVariantSelectionMode.value = false
  sectionVariantSelectionFeedback.value = ''
  sectionVariantCreateError.value = ''
  sectionVariantCandidates.value = []
  sectionVariantPreviewState.value = 'idle'
  sectionVariantPreviewError.value = ''
  isCreatingSectionVariant.value = false
}

function closeSectionVariantCreatePanel() {
  clearSectionVariantCreationFlow()
}

async function requestSectionVariantSelectionPreview(metadata: SectionVariantCreateMetadata) {
  if (isCreatingSectionVariant.value) {
    return
  }

  sectionVariantCreateMetadata.value = { ...metadata }
  sectionVariantCandidates.value = []
  sectionVariantSelectionMode.value = false
  sectionVariantSelectionFeedback.value = ''
  sectionVariantCreateError.value = ''
  sectionVariantPreviewState.value = 'loading'
  sectionVariantPreviewError.value = ''

  try {
    const candidates = await cmsV2Api.previewSectionVariantSelection({
      sectionId: metadata.sectionId,
      difficulty: metadata.difficulty,
    })

    sectionVariantCandidates.value = candidates.map(mapSectionVariantSelectionCandidate)
    sectionVariantPreviewState.value = 'ready'
    sectionVariantCreatePanelOpen.value = false
    sectionVariantSelectionMode.value = true
    sectionVariantSelectionFeedback.value = t('sectionPage.workspace.variantSelection.feedbackReady', {
      count: sectionVariantSelectedCandidateCount.value,
    })
  } catch (error) {
    sectionVariantPreviewState.value = 'error'
    sectionVariantPreviewError.value =
      error instanceof Error ? error.message : t('sectionPage.sectionVariantCreate.previewFailed')
  }
}

function toggleSectionVariantSelection(sectionItemId: number) {
  if (isCreatingSectionVariant.value) {
    return
  }

  const candidate = sectionVariantCandidates.value.find(
    (item) => item.sectionItemId === sectionItemId,
  )

  if (!candidate?.selectable) {
    sectionVariantCreateError.value = ''
    sectionVariantSelectionFeedback.value =
      candidate?.unavailableReason ?? t('sectionPage.workspace.variantSelection.unavailableItem')
    return
  }

  sectionVariantCreateError.value = ''
  sectionVariantCandidates.value = sectionVariantCandidates.value.map((item) =>
    item.sectionItemId === sectionItemId ? { ...item, selected: !item.selected } : item,
  )
  sectionVariantSelectionFeedback.value = t('sectionPage.workspace.variantSelection.feedbackUpdated', {
    count: sectionVariantSelectedCandidateCount.value,
  })
}

async function submitCompositeBlockChildContentBlock(payload: InsertCreateSubmitPayload) {
  const contentBlockTitle = payload.title.trim()
  const insertPlan = getNestedInsertPlan(
    payload,
    getCompositeBlockChildLastSortOrder(payload.compositeBlockId!) + 10,
  )
  const created = await contentBlockRelationActions.createContentBlockInsideCompositeBlock({
    parentBlockId: payload.compositeBlockId!,
    sectionId: payload.sectionId,
    title: contentBlockTitle,
    blockType: mapInsertContentBlockType(payload.contentBlockType),
    difficulty: mapInsertDifficulty(payload.difficulty),
    sortOrder: insertPlan.sortOrder,
  })

  if (insertPlan.moveUpAfterCreate) {
    await cmsV2Api.moveContentBlockRelation(
      payload.compositeBlockId!,
      created.relation.id,
      { direction: 'Up' },
    )
    await loadCurrentSectionPage()
  }

  activeCreatePanel.value = null
  activeInsertPointId.value = undefined
  insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackCreateAtomicChildSubmitted', {
    title: contentBlockTitle || 'ContentBlock',
  })
}

function clearSectionVariantSelection() {
  if (isCreatingSectionVariant.value) {
    return
  }

  sectionVariantCandidates.value = sectionVariantCandidates.value.map((candidate) =>
    candidate.selectable ? { ...candidate, selected: false } : candidate,
  )
  sectionVariantCreateError.value = ''
  sectionVariantSelectionFeedback.value = t('sectionPage.workspace.variantSelection.feedbackCleared')
}

function cancelSectionVariantSelectionMode() {
  if (isCreatingSectionVariant.value) {
    return
  }

  sectionVariantSelectionMode.value = false
  sectionVariantSelectionFeedback.value = ''
  sectionVariantCreateError.value = ''
  sectionVariantCandidates.value = []
  sectionVariantCreateMetadata.value = null
  sectionVariantCreatePanelOpen.value = false
}

async function confirmSectionVariantSelection() {
  if (!sectionVariantCreateMetadata.value) {
    return
  }

  const payload = {
    ...sectionVariantCreateMetadata.value,
    selectedSectionItemIds: sectionVariantCandidates.value
      .filter((candidate) => candidate.selectable && candidate.selected)
      .map((candidate) => candidate.sectionItemId),
  }

  try {
    isCreatingSectionVariant.value = true
    sectionVariantCreateError.value = ''
    sectionVariantSelectionFeedback.value = t('sectionPage.sectionVariantCreate.creatingFeedback', {
      count: payload.selectedSectionItemIds.length,
    })

    await cmsV2Api.createSectionVariant(payload)
    sectionVariantSelectionMode.value = false
    sectionVariantSelectionFeedback.value = ''
    sectionVariantCreateError.value = ''
    sectionVariantCandidates.value = []
    sectionVariantCreateMetadata.value = null
    sectionVariantCreatePanelOpen.value = false
    sectionVariantPreviewState.value = 'idle'
    sectionVariantPreviewError.value = ''
    insertFeedback.value = t('sectionPage.sectionVariantCreate.createdFeedback', {
      title: payload.title,
      count: payload.selectedSectionItemIds.length,
    })
  } catch (error) {
    const message =
      error instanceof Error ? error.message : t('sectionPage.sectionVariantCreate.createFailed')
    sectionVariantSelectionFeedback.value = t('sectionPage.sectionVariantCreate.createFailed')
    sectionVariantCreateError.value = message
    return
  } finally {
    isCreatingSectionVariant.value = false
  }

  await loadCurrentSectionPage()
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

async function handleSectionTreeContextMenuAction(payload: SectionTreeContextMenuActionPayload) {
  const contextNode = sectionTreeContextMenu.value?.node
  closeSectionTreeContextMenu()

  if (!contextNode) {
    return
  }

  if (payload.actionType === 'CreateSectionVariant') {
    if (contextNode.kind === 'Section') {
      openSectionVariantCreatePanel()
    }
    return
  }

  if (payload.actionType === 'CreateContentBlock' || payload.actionType === 'CreateAtomicSection') {
    requestInsert(createSectionTreeContextInsertRequest(contextNode, payload.actionType))
    return
  }

  activeCreatePanel.value = null

  if (payload.actionType === 'SearchExistingBlock') {
    insertFeedback.value = t('sectionPage.workspace.insertPanel.feedbackSearchExistingBlock')
    return
  }

  if (payload.actionType === 'Remove') {
    await removeSectionTreeContextNode(contextNode)
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

  if (event.key === 'Escape' && sectionVariantSelectionMode.value && !sectionVariantCreatePanelOpen.value) {
    cancelSectionVariantSelectionMode()
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
        :flow-items="visibleWorkspaceFlowItems"
        :selected-node-id="selectedStructureNodeId"
        :workspace-node-map="workspaceNodeMap"
        :scroll-target-node-id="workspaceScrollTargetNodeId"
        :scroll-request-key="workspaceScrollRequestKey"
        :active-insert-point-id="activeInsertPointId"
        :insert-feedback="insertFeedback"
        :wrap-selection-mode="wrapSelectionMode"
        :wrap-selected-node-ids="wrapSelectedNodeIds"
        :wrap-selection-feedback="wrapSelectionFeedback"
        :variant-selection-mode="sectionVariantSelectionMode"
        :variant-selection-candidates="sectionVariantCandidates"
        :variant-selection-feedback="sectionVariantSelectionFeedback"
        :variant-selection-error="sectionVariantCreateError"
        :variant-selection-submitting="isCreatingSectionVariant"
        :read-only-mode="sectionVariantViewMode"
        :read-only-label="sectionVariantReadOnlyLabel"
        :read-only-description="sectionVariantViewMode ? t('sectionPage.sectionVariantView.readOnlyDescription') : ''"
        :view-loading="isLoadingSectionVariantItems"
        :view-error="sectionVariantViewError"
        :empty-title="sectionVariantViewMode ? t('sectionPage.sectionVariantView.emptyTitle') : ''"
        :empty-description="sectionVariantViewMode ? t('sectionPage.sectionVariantView.emptyDescription') : ''"
        :collapsed-workspace-node-ids="collapsedWorkspaceNodeIdList"
        @select-node="selectWorkspaceNode"
        @toggle-workspace-node-collapse="toggleWorkspaceNodeCollapse"
        @request-insert="requestInsert"
        @enter-wrap-selection-mode="enterWrapSelectionMode"
        @cancel-wrap-selection-mode="cancelWrapSelectionMode"
        @clear-wrap-selection="clearWrapSelection"
        @toggle-wrap-node-selection="toggleWrapNodeSelection"
        @request-wrap-as-atomic-section="requestWrapAsAtomicSection"
        @toggle-variant-selection="toggleSectionVariantSelection"
        @clear-variant-selection="clearSectionVariantSelection"
        @cancel-variant-selection="cancelSectionVariantSelectionMode"
        @confirm-variant-selection="confirmSectionVariantSelection"
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
        <SectionInspector
          class="min-h-0 flex-1"
          :node="selectedStructureNode"
          :section="sectionShell"
          :variant-item-count="sectionVariantItemCount"
        />
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

    <Teleport to="body">
      <div
        v-if="sectionVariantCreatePanelOpen && sectionVariantCreateMetadata"
        class="fixed inset-0 z-[60] flex min-h-screen items-center justify-center p-4"
        role="dialog"
        aria-modal="true"
        :aria-label="t('sectionPage.sectionVariantCreate.dialogLabel')"
      >
        <button
          type="button"
          class="absolute inset-0 bg-background/70 backdrop-blur-sm"
          :aria-label="t('sectionPage.sectionVariantCreate.closeLabel')"
          @click="closeSectionVariantCreatePanel"
        />

        <div class="relative z-10 grid w-full max-w-3xl gap-3">
          <SectionVariantCreatePanel
            :initial-metadata="sectionVariantCreateMetadata"
            :candidates="sectionVariantCandidates"
            :section-title="sectionShell.title"
            :preview-state="sectionVariantPreviewState"
            :preview-error="sectionVariantPreviewError"
            selection-mode="workspace"
            @cancel="closeSectionVariantCreatePanel"
            @request-preview="requestSectionVariantSelectionPreview"
          />
        </div>
      </div>
    </Teleport>

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

    <Teleport to="body">
      <div
        v-if="isCreatingSectionVariant"
        class="fixed inset-0 z-[70] flex min-h-screen items-center justify-center bg-background/70 p-4 backdrop-blur-sm"
        role="status"
        aria-live="assertive"
      >
        <div class="w-full max-w-sm rounded-lg border bg-card p-4 text-card-foreground">
          <p class="text-sm font-semibold">
            {{ t('sectionPage.sectionVariantCreate.blockingTitle') }}
          </p>
          <p class="mt-1 text-sm text-muted-foreground">
            {{ t('sectionPage.sectionVariantCreate.blockingDescription') }}
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
