<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { AlertCircle, FileText, RefreshCw } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import HandoutOverviewFlyout from '@/components/business/HandoutOverviewFlyout.vue'
import HandoutInspector from '@/components/business/HandoutInspector.vue'
import HandoutOccurrenceEditDialog from '@/components/business/HandoutOccurrenceEditDialog.vue'
import HandoutOutputPanel from '@/components/business/HandoutOutputPanel.vue'
import SectionVariantSelectionDialog from '@/components/business/SectionVariantSelectionDialog.vue'
import HandoutStructureContextMenu from '@/components/business/HandoutStructureContextMenu.vue'
import HandoutStructurePanel from '@/components/business/HandoutStructurePanel.vue'
import HandoutTargetPicker from '@/components/business/HandoutTargetPicker.vue'
import HandoutWorkspace from '@/components/business/HandoutWorkspace.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import { cmsV2Api } from '@/apis/cmsV2Client'
import { usePageTitle } from '@/composables/usePageTitle'
import { getNewVariantIds } from '@/utils/sectionVariantTreeSelection'
import {
  mockGeneratedFiles,
  mockHandoutInspector,
  mockHandoutTreeNodes,
  mockHandoutWorkspaceItems,
  mockOutputForms,
} from '@/mocks'
import type {
  CmsV2GeneratedFileDto,
  CmsV2AtomicSectionDto,
  CmsV2ContentBlockDto,
  CmsV2HandoutDto,
  CmsV2HandoutVersionWorkspaceDto,
  CmsV2HandoutVersionDto,
  CmsV2HandoutWorkspaceItemDto,
  CmsV2HandoutWorkspaceNodeDto,
  CmsV2OutputFormDto,
  CmsV2SectionVariantSelectionTreeTopicDto,
} from '@/apis/cmsV2Client'
import type {
  GeneratedFileRowModel,
  HandoutInspectorModel,
  HandoutOverviewNodeModel,
  HandoutTreeContextMenuActionPayload,
  HandoutTreeContextMenuModel,
  HandoutTreeContextMenuPayload,
  HandoutTreeNodeKind,
  HandoutTreeNodeModel,
  HandoutTargetPickerCandidateModel,
  HandoutWorkspaceChildModel,
  HandoutWorkspaceItemModel,
  OutputFormCardModel,
} from '@/types'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()

const workspace = ref<CmsV2HandoutVersionWorkspaceDto | null>(null)
const selectedNodeId = ref<string>('')
const handoutWorkspaceScrollTargetNodeId = ref('')
const handoutWorkspaceScrollRequestKey = ref(0)
const handoutOverviewOpen = ref(false)
const handoutOverviewNodes = ref<HandoutOverviewNodeModel[]>([])
const handoutOverviewLoading = ref(false)
const handoutOverviewError = ref('')
const sectionVariantDialogOpen = ref(false)
const sectionVariantDialogLoading = ref(false)
const sectionVariantDialogError = ref('')
const sectionVariantTree = ref<CmsV2SectionVariantSelectionTreeTopicDto[]>([])
const selectedSectionVariantIds = ref<number[]>([])
const sectionVariantInsertAfterItemId = ref<number | null>(null)
const handoutContextMenu = ref<HandoutTreeContextMenuModel | null>(null)
const targetPickerOpen = ref(false)
const targetPickerKind = ref<'AtomicSection' | 'ContentBlock' | null>(null)
const targetPickerCandidates = ref<HandoutTargetPickerCandidateModel[]>([])
const targetPickerLoading = ref(false)
const targetPickerError = ref('')
const targetPickerInsertAfterItemId = ref<number | null>(null)
const occurrenceEditItem = ref<HandoutWorkspaceItemModel | null>(null)
const isLoading = ref(false)
const operationPending = ref(false)
const errorMessage = ref('')
const feedback = ref('')

const routeHandoutVersionId = computed(() => String(route.params.handoutVersionId ?? ''))
const numericHandoutVersionId = computed(() => {
  const parsed = Number(routeHandoutVersionId.value)
  return Number.isInteger(parsed) && parsed > 0 ? parsed : null
})
const isDemoRoute = computed(() => routeHandoutVersionId.value === 'demo-handout')
const handoutPageTitleDetail = computed(() =>
  isDemoRoute.value
    ? 'Mock Data'
    : workspace.value?.version.title || `HandoutVersion ${routeHandoutVersionId.value}`,
)
usePageTitle('HandoutPage', handoutPageTitleDetail)
let handoutOverviewTimer: number | undefined

const treeNodes = computed<HandoutTreeNodeModel[]>(() => {
  if (isDemoRoute.value) {
    return markSelectedTreeNodes(mockHandoutTreeNodes)
  }

  if (!workspace.value) {
    return []
  }

  return markSelectedTreeNodes([toRootTreeNode(workspace.value)])
})

const workspaceItems = computed<HandoutWorkspaceItemModel[]>(() => {
  if (isDemoRoute.value) {
    return mockHandoutWorkspaceItems.map((item) => ({
      ...item,
      selected: item.nodeId === selectedNodeId.value || item.id === selectedNodeId.value,
      children: item.children?.map(markSelectedWorkspaceChild),
    }))
  }

  if (!workspace.value) {
    return []
  }

  return workspace.value.items.map(toWorkspaceItem)
})

const outputForms = computed<OutputFormCardModel[]>(() => {
  if (isDemoRoute.value) {
    return mockOutputForms
  }

  return workspace.value?.outputForms.map(toOutputFormCard) ?? []
})

const generatedFiles = computed<GeneratedFileRowModel[]>(() => {
  if (isDemoRoute.value) {
    return mockGeneratedFiles
  }

  return workspace.value?.generatedFiles.map(toGeneratedFileRow) ?? []
})

const existingSectionVariantIds = computed(() =>
  workspaceItems.value
    .filter((item) => item.targetType === 'SectionVariant')
    .map((item) => item.targetId),
)
const pageReadOnly = computed(
  () =>
    isDemoRoute.value ||
    operationPending.value ||
    workspace.value?.handout.status === 'Archived' ||
    workspace.value?.version.status === 'Archived',
)
const targetPickerTitle = computed(() =>
  targetPickerKind.value === 'AtomicSection'
    ? (t('handoutTargetPicker.atomicSectionTitle') as string)
    : (t('handoutTargetPicker.contentBlockTitle') as string),
)
const targetPickerDescription = computed(() =>
  targetPickerKind.value === 'AtomicSection'
    ? (t('handoutTargetPicker.atomicSectionDescription') as string)
    : (t('handoutTargetPicker.contentBlockDescription') as string),
)

const inspectorModel = computed<HandoutInspectorModel | null>(() => {
  if (isDemoRoute.value) {
    if (!selectedNodeId.value) {
      return mockHandoutInspector
    }

    const node = findTreeNode(mockHandoutTreeNodes, selectedNodeId.value)
    if (node) {
      return inspectorFromTreeNode(node)
    }

    const item = mockHandoutWorkspaceItems.find(
      (entry) => entry.id === selectedNodeId.value || entry.nodeId === selectedNodeId.value,
    )
    return item ? inspectorFromWorkspaceItem(item) : mockHandoutInspector
  }

  if (!workspace.value || !selectedNodeId.value) {
    return null
  }

  const node = findTreeNode(treeNodes.value, selectedNodeId.value)
  if (node) {
    return inspectorFromTreeNode(node)
  }

  const item = workspaceItems.value.find(
    (entry) => entry.id === selectedNodeId.value || entry.nodeId === selectedNodeId.value,
  )
  if (item) {
    return inspectorFromWorkspaceItem(item)
  }

  return null
})

watch(
  () => routeHandoutVersionId.value,
  async () => {
    await loadHandoutWorkspace()
  },
  { immediate: true },
)

onMounted(() => {
  document.addEventListener('keydown', handleDocumentKeydown)
})

onBeforeUnmount(() => {
  stopHandoutOverviewTimer()
  document.removeEventListener('keydown', handleDocumentKeydown)
})

function handleDocumentKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape' && handoutOverviewOpen.value) {
    closeHandoutOverview()
  }
}

function startHandoutOverviewTimer() {
  stopHandoutOverviewTimer()
  handoutOverviewTimer = window.setTimeout(() => {
    void openHandoutOverview()
  }, 2000)
}

function stopHandoutOverviewTimer() {
  if (handoutOverviewTimer) {
    window.clearTimeout(handoutOverviewTimer)
    handoutOverviewTimer = undefined
  }
}

async function openHandoutOverview() {
  stopHandoutOverviewTimer()
  handoutOverviewOpen.value = true
  await loadHandoutOverview()
}

function closeHandoutOverview() {
  stopHandoutOverviewTimer()
  handoutOverviewOpen.value = false
}

async function loadHandoutOverview() {
  handoutOverviewLoading.value = true
  handoutOverviewError.value = ''

  try {
    const handouts = await cmsV2Api.listHandouts()
    const versionGroups = await Promise.all(
      handouts.map(async (handout) => ({
        handout,
        versions: await cmsV2Api.listHandoutVersions(handout.id),
      })),
    )
    handoutOverviewNodes.value = versionGroups
      .sort((left, right) => left.handout.title.localeCompare(right.handout.title))
      .map(({ handout, versions }) => toHandoutOverviewNode(handout, versions))
  } catch (error) {
    handoutOverviewError.value =
      error instanceof Error ? error.message : (t('handoutOverview.loadFailed') as string)
  } finally {
    handoutOverviewLoading.value = false
  }
}

function toHandoutOverviewNode(
  handout: CmsV2HandoutDto,
  versions: CmsV2HandoutVersionDto[],
): HandoutOverviewNodeModel {
  const hasCurrentVersion = versions.some((version) => version.id === numericHandoutVersionId.value)

  return {
    id: `handout:${handout.id}`,
    title: handout.title,
    kind: 'Handout',
    handoutId: handout.id,
    status: handout.status,
    expanded: hasCurrentVersion,
    children: versions
      .sort((left, right) => left.sortOrder - right.sortOrder || left.id - right.id)
      .map((version) => ({
        id: `handout-version:${version.id}`,
        title: version.title,
        kind: 'HandoutVersion',
        handoutId: handout.id,
        handoutVersionId: version.id,
        status: `${version.type} · ${version.status}`,
      })),
  }
}

async function openOverviewVersion(handoutVersionId: number) {
  closeHandoutOverview()
  await router.push(`/handouts/${handoutVersionId}`)
}

async function openHandoutManagement() {
  closeHandoutOverview()
  await router.push('/handouts')
}

async function loadHandoutWorkspace(preferredSelectedNodeId?: string) {
  errorMessage.value = ''
  feedback.value = ''

  if (isDemoRoute.value) {
    workspace.value = null
    selectedNodeId.value = mockHandoutTreeNodes[0]?.id ?? ''
    return
  }

  if (!numericHandoutVersionId.value) {
    workspace.value = null
    selectedNodeId.value = ''
    errorMessage.value = t('handoutPage.invalidRoute') as string
    return
  }

  isLoading.value = true
  try {
    const data = await cmsV2Api.getHandoutVersionWorkspace(numericHandoutVersionId.value)
    workspace.value = data
    selectedNodeId.value = preferredSelectedNodeId || `handout-version:${data.version.id}`
  } catch (error) {
    workspace.value = null
    selectedNodeId.value = ''
    errorMessage.value = error instanceof Error ? error.message : (t('handoutPage.loadFailed') as string)
  } finally {
    isLoading.value = false
  }
}

function toRootTreeNode(data: CmsV2HandoutVersionWorkspaceDto): HandoutTreeNodeModel {
  return {
    id: `handout-version:${data.version.id}`,
    title: data.version.title,
    kind: 'HandoutVersion',
    handoutVersionId: data.version.id,
    status: data.version.status,
    metaItems: [
      data.handout.title,
      data.version.type,
      t('handoutPage.itemCount', { count: data.items.length }) as string,
    ],
    expanded: true,
    children: data.items.map(toItemTreeNode),
  }
}

function toItemTreeNode(item: CmsV2HandoutWorkspaceItemDto): HandoutTreeNodeModel {
  return {
    id: item.nodeId,
    title: item.titleOverride || item.title,
    kind: 'HandoutVersionItem',
    handoutVersionItemId: item.handoutVersionItemId,
    targetType: item.targetType,
    targetId: item.targetId,
    metaItems: [item.targetType, t('handoutPage.referenceView') as string],
    expanded: true,
    children: item.children.map(toDerivedTreeNode),
  }
}

function toDerivedTreeNode(node: CmsV2HandoutWorkspaceNodeDto): HandoutTreeNodeModel {
  return {
    id: node.nodeId,
    title: node.title,
    kind: toTreeNodeKind(node.nodeKind),
    targetId: node.sourceId,
    readOnly: true,
    metaItems: [node.nodeKind],
    expanded: true,
    children: node.children.map(toDerivedTreeNode),
  }
}

function toTreeNodeKind(nodeKind: string): HandoutTreeNodeKind {
  if (nodeKind === 'AtomicSection' || nodeKind === 'ContentBlock' || nodeKind === 'SectionVariant') {
    return nodeKind
  }

  return 'Derived'
}

function toWorkspaceItem(item: CmsV2HandoutWorkspaceItemDto): HandoutWorkspaceItemModel {
  return {
    id: `workspace-item:${item.handoutVersionItemId}`,
    nodeId: item.nodeId,
    handoutVersionItemId: item.handoutVersionItemId,
    kind: item.targetType,
    title: item.title,
    titleOverride: item.titleOverride,
    note: item.note,
    targetType: item.targetType,
    targetId: item.targetId,
    sourceLabel: `${item.targetType} · ${targetModeLabel(item.targetType)}`,
    status: t('handoutPage.readOnlyStatus') as string,
    sortOrder: item.sortOrder,
    selected: item.nodeId === selectedNodeId.value || `workspace-item:${item.handoutVersionItemId}` === selectedNodeId.value,
    children: item.children.map(toWorkspaceChild),
  }
}

function toWorkspaceChild(node: CmsV2HandoutWorkspaceNodeDto): HandoutWorkspaceChildModel {
  return {
    id: node.nodeId,
    title: node.title,
    kind: node.nodeKind === 'AtomicSection' ? 'AtomicSection' : 'ContentBlock',
    typeLabel: node.nodeKind,
    sourceLabel: t('handoutPage.derivedReadOnly') as string,
    readOnly: true,
    selected: node.nodeId === selectedNodeId.value,
    children: node.children.map(toWorkspaceChild),
  }
}

function targetModeLabel(targetType: string) {
  return targetType === 'SectionVariant'
    ? (t('handoutPage.referenceExpansion') as string)
    : (t('handoutPage.directReference') as string)
}

function toOutputFormCard(outputForm: CmsV2OutputFormDto): OutputFormCardModel {
  return {
    id: outputForm.id,
    title: outputForm.title,
    audience: outputForm.audience,
    outputFormat: outputForm.outputFormat,
    visibilityMode: outputForm.visibilityMode,
    templateTitle: `OutputTemplate #${outputForm.outputTemplateId}`,
    status: outputForm.status,
  }
}

function toGeneratedFileRow(file: CmsV2GeneratedFileDto): GeneratedFileRowModel {
  const outputForm = outputForms.value.find((entry) => entry.id === file.outputFormId)
  return {
    id: file.id,
    fileName: file.filePath.split(/[\\/]/).pop() || file.filePath,
    generatedTime: file.generatedTime,
    outputFormTitle: outputForm?.title ?? `OutputForm #${file.outputFormId}`,
    manifestSummary: summarizeManifest(file.versionManifestJson),
  }
}

function summarizeManifest(manifestJson: string) {
  try {
    const payload = JSON.parse(manifestJson) as Record<string, unknown>
    const keys = Object.keys(payload)
    return keys.length
      ? `${t('handoutPage.manifestFields')}: ${keys.slice(0, 4).join(', ')}`
      : t('handoutPage.emptyManifest')
  } catch {
    return manifestJson.length > 80 ? `${manifestJson.slice(0, 80)}...` : manifestJson
  }
}

function markSelectedTreeNodes(nodes: HandoutTreeNodeModel[]): HandoutTreeNodeModel[] {
  return nodes.map((node) => ({
    ...node,
    disabled: node.disabled,
    children: node.children ? markSelectedTreeNodes(node.children) : undefined,
  }))
}

function markSelectedWorkspaceChild(child: HandoutWorkspaceChildModel): HandoutWorkspaceChildModel {
  return {
    ...child,
    selected: child.id === selectedNodeId.value,
    children: child.children?.map(markSelectedWorkspaceChild),
  }
}

function findTreeNode(
  nodes: HandoutTreeNodeModel[],
  nodeId: string,
): HandoutTreeNodeModel | undefined {
  for (const node of nodes) {
    if (node.id === nodeId) {
      return node
    }

    const child = node.children ? findTreeNode(node.children, nodeId) : undefined
    if (child) {
      return child
    }
  }

  return undefined
}

function inspectorFromTreeNode(node: HandoutTreeNodeModel): HandoutInspectorModel {
  return {
    nodeId: node.id,
    title: node.title,
    kind: node.kind,
    description: node.readOnly
      ? (t('handoutPage.inspector.derivedDescription') as string)
      : (t('handoutPage.inspector.treeDescription') as string),
    fields: [
      { label: t('handoutPage.fields.nodeKind') as string, value: node.kind },
      { label: t('handoutPage.fields.status') as string, value: node.status ?? '-' },
      { label: t('handoutPage.fields.targetType') as string, value: node.targetType ?? '-' },
      { label: t('handoutPage.fields.targetId') as string, value: String(node.targetId ?? '-') },
    ],
  }
}

function inspectorFromWorkspaceItem(item: HandoutWorkspaceItemModel): HandoutInspectorModel {
  return {
    nodeId: item.nodeId,
    title: item.titleOverride || item.title,
    kind: item.kind,
    description: t('handoutPage.inspector.itemDescription') as string,
    editableOccurrence: true,
    fields: [
      { label: t('handoutPage.fields.targetType') as string, value: item.targetType },
      { label: t('handoutPage.fields.targetId') as string, value: String(item.targetId) },
      { label: t('handoutPage.fields.sortOrder') as string, value: String(item.sortOrder) },
      { label: 'TitleOverride', value: item.titleOverride || '-' },
      { label: 'Note', value: item.note || '-' },
    ],
  }
}

function handleSelectNode(nodeId: string) {
  selectedNodeId.value = nodeId
  requestHandoutWorkspaceScroll(nodeId)
}

function handleSelectWorkspaceItem(itemId: string) {
  selectedNodeId.value = resolveWorkspaceNodeId(itemId) ?? itemId
}

function requestHandoutWorkspaceScroll(nodeId: string) {
  handoutWorkspaceScrollTargetNodeId.value = nodeId
  handoutWorkspaceScrollRequestKey.value += 1
}

function resolveWorkspaceNodeId(nodeId: string): string | undefined {
  for (const item of workspaceItems.value) {
    if (item.id === nodeId || item.nodeId === nodeId) {
      return item.nodeId
    }

    const child = findWorkspaceChild(item.children ?? [], nodeId)
    if (child) {
      return child.id
    }
  }

  return undefined
}

function findWorkspaceChild(
  children: HandoutWorkspaceChildModel[],
  nodeId: string,
): HandoutWorkspaceChildModel | undefined {
  for (const child of children) {
    if (child.id === nodeId) {
      return child
    }

    const nested = findWorkspaceChild(child.children ?? [], nodeId)
    if (nested) {
      return nested
    }
  }

  return undefined
}

function showDeferredFeedback(actionKey: string, id?: number | string) {
  feedback.value = t(actionKey, { id }) as string
}

async function withOperation(action: () => Promise<void>) {
  if (
    isDemoRoute.value ||
    !numericHandoutVersionId.value ||
    workspace.value?.handout.status === 'Archived' ||
    workspace.value?.version.status === 'Archived'
  ) {
    showDeferredFeedback('handoutPage.feedback.readOnly')
    return
  }

  operationPending.value = true
  errorMessage.value = ''
  try {
    await action()
  } catch (error) {
    feedback.value = error instanceof Error ? error.message : (t('handoutPage.operationFailed') as string)
  } finally {
    operationPending.value = false
  }
}

async function handleAddToEnd() {
  await openSectionVariantSelectionDialog(null)
}

async function openSectionVariantSelectionDialog(afterHandoutVersionItemId: number | null) {
  if (isDemoRoute.value || !numericHandoutVersionId.value) {
    showDeferredFeedback('handoutPage.feedback.readOnly')
    return
  }

  sectionVariantDialogOpen.value = true
  sectionVariantDialogLoading.value = true
  sectionVariantDialogError.value = ''
  sectionVariantInsertAfterItemId.value = afterHandoutVersionItemId
  selectedSectionVariantIds.value = [...existingSectionVariantIds.value]

  try {
    sectionVariantTree.value = await cmsV2Api.getSectionVariantTree()
  } catch (error) {
    sectionVariantDialogError.value =
      error instanceof Error
        ? error.message
        : (t('sectionVariantSelectionDialog.loadFailed') as string)
  } finally {
    sectionVariantDialogLoading.value = false
  }
}

async function openHandoutTargetPicker(
  targetType: 'AtomicSection' | 'ContentBlock',
  afterHandoutVersionItemId: number | null,
) {
  if (isDemoRoute.value || !numericHandoutVersionId.value) {
    showDeferredFeedback('handoutPage.feedback.readOnly')
    return
  }

  targetPickerOpen.value = true
  targetPickerKind.value = targetType
  targetPickerInsertAfterItemId.value = afterHandoutVersionItemId
  targetPickerLoading.value = true
  targetPickerError.value = ''
  targetPickerCandidates.value = []

  try {
    if (targetType === 'AtomicSection') {
      const atomicSections = await cmsV2Api.listAtomicSections()
      targetPickerCandidates.value = atomicSections.map(toAtomicSectionCandidate)
    } else {
      const contentBlocks = await cmsV2Api.listContentBlocks()
      targetPickerCandidates.value = contentBlocks.map(toContentBlockCandidate)
    }
  } catch (error) {
    targetPickerError.value =
      error instanceof Error ? error.message : (t('handoutTargetPicker.loadFailed') as string)
  } finally {
    targetPickerLoading.value = false
  }
}

function toAtomicSectionCandidate(
  atomicSection: CmsV2AtomicSectionDto,
): HandoutTargetPickerCandidateModel {
  return {
    id: atomicSection.id,
    title: atomicSection.title || `AtomicSection #${atomicSection.id}`,
    metaItems: [atomicSection.type, atomicSection.difficulty, atomicSection.status],
    disabled: atomicSection.status === 'Archived',
  }
}

function toContentBlockCandidate(
  contentBlock: CmsV2ContentBlockDto,
): HandoutTargetPickerCandidateModel {
  return {
    id: contentBlock.id,
    title: contentBlock.title || `ContentBlock #${contentBlock.id}`,
    metaItems: [contentBlock.blockType, contentBlock.difficulty, contentBlock.status],
    disabled: contentBlock.status === 'Archived',
  }
}

function closeHandoutTargetPicker() {
  targetPickerOpen.value = false
  targetPickerError.value = ''
}

async function selectHandoutTarget(targetId: number) {
  const targetType = targetPickerKind.value
  if (!targetType) {
    return
  }

  await withOperation(async () => {
    const result = await cmsV2Api.addHandoutVersionItem(numericHandoutVersionId.value!, {
      targetType,
      targetId,
      afterHandoutVersionItemId: targetPickerInsertAfterItemId.value,
    })
    closeHandoutTargetPicker()
    await loadHandoutWorkspace(`handout-item:${result.id}`)
    feedback.value = t('handoutPage.feedback.addedItem') as string
  })
}

function closeSectionVariantSelectionDialog() {
  sectionVariantDialogOpen.value = false
  sectionVariantDialogError.value = ''
}

function openHandoutContextMenu(payload: HandoutTreeContextMenuPayload) {
  if (pageReadOnly.value) {
    return
  }

  handoutContextMenu.value = {
    node: payload.node,
    position: {
      x: payload.x,
      y: payload.y,
    },
  }
}

function closeHandoutContextMenu() {
  handoutContextMenu.value = null
}

async function handleHandoutContextAction(payload: HandoutTreeContextMenuActionPayload) {
  const node = handoutContextMenu.value?.node
  closeHandoutContextMenu()

  if (!node || node.id !== payload.nodeId) {
    return
  }

  if (payload.actionType === 'AddSectionVariantsToEnd') {
    await openSectionVariantSelectionDialog(null)
    return
  }

  if (payload.actionType === 'AddAtomicSectionToEnd') {
    await openHandoutTargetPicker('AtomicSection', null)
    return
  }

  if (payload.actionType === 'AddContentBlockToEnd') {
    await openHandoutTargetPicker('ContentBlock', null)
    return
  }

  if (payload.actionType === 'AddSectionVariantsAfter' && node.handoutVersionItemId) {
    await openSectionVariantSelectionDialog(node.handoutVersionItemId)
    return
  }

  if (payload.actionType === 'AddAtomicSectionAfter' && node.handoutVersionItemId) {
    await openHandoutTargetPicker('AtomicSection', node.handoutVersionItemId)
    return
  }

  if (payload.actionType === 'AddContentBlockAfter' && node.handoutVersionItemId) {
    await openHandoutTargetPicker('ContentBlock', node.handoutVersionItemId)
  }
}

function toggleSectionVariantSelection(sectionVariantId: number) {
  const next = new Set(selectedSectionVariantIds.value)

  if (next.has(sectionVariantId)) {
    next.delete(sectionVariantId)
  } else {
    next.add(sectionVariantId)
  }

  selectedSectionVariantIds.value = Array.from(next)
}

async function confirmSectionVariantSelection() {
  const newVariantIds = getNewVariantIds(
    new Set(selectedSectionVariantIds.value),
    new Set(existingSectionVariantIds.value),
  )

  if (!newVariantIds.length) {
    feedback.value = t('sectionVariantSelectionDialog.noNewSelection') as string
    return
  }

  await withOperation(async () => {
    const result = await cmsV2Api.batchAddSectionVariantsToHandoutVersion(
      numericHandoutVersionId.value!,
      {
        sectionVariantIds: newVariantIds,
        insertAfterHandoutVersionItemId: sectionVariantInsertAfterItemId.value,
      },
    )
    closeSectionVariantSelectionDialog()
    await loadHandoutWorkspace(
      result.createdItemIds[0] ? `handout-item:${result.createdItemIds[0]}` : selectedNodeId.value,
    )
    feedback.value = t('sectionVariantSelectionDialog.batchAdded', {
      created: result.createdItemIds.length,
      skipped: result.skippedExistingVariantIds.length,
    }) as string
  })
}

async function handleMoveItem(itemId: string, direction: 'Up' | 'Down') {
  const item = findWorkspaceItemByAnyId(itemId)
  if (!item) {
    return
  }

  await withOperation(async () => {
    await cmsV2Api.moveHandoutVersionItem(
      numericHandoutVersionId.value!,
      item.handoutVersionItemId,
      { direction },
    )
    await loadHandoutWorkspace(item.nodeId)
    feedback.value = t('handoutPage.feedback.movedItem') as string
  })
}

async function handleEditOccurrence(itemId: string) {
  const item = findWorkspaceItemByAnyId(itemId)
  if (!item) {
    return
  }

  occurrenceEditItem.value = item
}

function closeOccurrenceEditDialog() {
  occurrenceEditItem.value = null
}

async function submitOccurrenceEdit(payload: { titleOverride: string | null; note: string | null }) {
  const item = occurrenceEditItem.value
  if (!item) {
    return
  }

  await withOperation(async () => {
    await cmsV2Api.updateHandoutVersionItem(
      numericHandoutVersionId.value!,
      item.handoutVersionItemId,
      {
        titleOverride: payload.titleOverride,
        note: payload.note,
      },
    )
    closeOccurrenceEditDialog()
    await loadHandoutWorkspace(item.nodeId)
    feedback.value = t('handoutPage.feedback.updatedItem') as string
  })
}

async function handleRemoveItem(itemId: string) {
  const items = workspaceItems.value
  const itemIndex = items.findIndex((entry) => entry.id === itemId || entry.nodeId === itemId)
  const item = itemIndex >= 0 ? items[itemIndex] : undefined
  if (!item) {
    return
  }

  if (!window.confirm(t('handoutPage.prompt.removeConfirm', { title: item.titleOverride || item.title }) as string)) {
    return
  }

  const nextSelection = items[itemIndex + 1]?.nodeId ?? items[itemIndex - 1]?.nodeId
  await withOperation(async () => {
    await cmsV2Api.removeHandoutVersionItem(
      numericHandoutVersionId.value!,
      item.handoutVersionItemId,
    )
    await loadHandoutWorkspace(nextSelection)
    feedback.value = t('handoutPage.feedback.removedItem') as string
  })
}

async function handleGenerateWord(outputFormId: number) {
  await withOperation(async () => {
    const result = await cmsV2Api.generateHandoutWord(outputFormId, {})
    await loadHandoutWorkspace(selectedNodeId.value)
    feedback.value = t('handoutPage.feedback.generatedWord', {
      id: result.generatedFileId,
    }) as string
  })
}

function handleDownloadGeneratedFile(generatedFileId: number) {
  window.location.href = cmsV2Api.getGeneratedFileDownloadUrl(generatedFileId)
}

async function handleViewManifest(generatedFileId: number) {
  await withOperation(async () => {
    const manifest = await cmsV2Api.getGeneratedFileManifest(generatedFileId)
    window.alert(manifest)
  })
}

async function handleDeleteGeneratedFile(generatedFileId: number) {
  if (!window.confirm(t('handoutPage.prompt.deleteGeneratedFileConfirm') as string)) {
    return
  }

  await withOperation(async () => {
    await cmsV2Api.deleteGeneratedFile(generatedFileId)
    await loadHandoutWorkspace(selectedNodeId.value)
    feedback.value = t('handoutPage.feedback.deletedGeneratedFile') as string
  })
}

function findWorkspaceItemByAnyId(itemId: string) {
  return workspaceItems.value.find((entry) => entry.id === itemId || entry.nodeId === itemId)
}
</script>

<template>
  <main class="grid h-screen min-h-0 grid-cols-[280px_minmax(0,1fr)_340px] gap-3 bg-background p-3">
    <button
      type="button"
      class="fixed inset-y-0 left-0 z-30 w-3 cursor-default bg-transparent focus-visible:bg-muted/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
      :aria-label="t('handoutOverview.triggerLabel')"
      @mouseenter="startHandoutOverviewTimer"
      @mouseleave="stopHandoutOverviewTimer"
      @focus="openHandoutOverview"
    />

    <aside class="min-h-0 rounded-lg border bg-background p-3">
      <HandoutStructurePanel
        :nodes="treeNodes"
        :selected-node-id="selectedNodeId"
        :context-target-node-id="handoutContextMenu?.node.id"
        :read-only="pageReadOnly"
        @select-node="handleSelectNode"
        @add-to-end="handleAddToEnd"
        @node-context-menu="openHandoutContextMenu"
      />
    </aside>

    <section class="flex min-h-0 flex-col rounded-lg border bg-background">
      <header class="flex items-center justify-between gap-3 border-b px-4 py-3">
        <div class="min-w-0">
          <div class="flex min-w-0 items-center gap-2">
            <FileText class="size-4 shrink-0 text-muted-foreground" />
            <h1 class="truncate text-sm font-semibold">
              {{
                isDemoRoute
                  ? t('handoutPage.demoTitle')
                  : workspace?.version.title || t('handoutPage.titleFallback')
              }}
            </h1>
            <StatusPill
              v-if="workspace?.version.status || isDemoRoute"
              :label="workspace?.version.status || 'Mock Data'"
              tone="neutral"
            />
          </div>
          <p class="mt-1 truncate text-xs text-muted-foreground">
            {{
              isDemoRoute
                ? t('handoutPage.demoDescription')
                : workspace
                  ? t('handoutPage.loadedDescription', {
                      handout: workspace.handout.title,
                      versionId: workspace.version.id,
                    })
                  : t('handoutPage.descriptionFallback')
            }}
          </p>
        </div>
        <Button
          type="button"
          size="sm"
          variant="outline"
          :disabled="isLoading || operationPending || isDemoRoute"
          @click="loadHandoutWorkspace"
        >
          <RefreshCw class="size-4" />
          {{ t('handoutPage.refresh') }}
        </Button>
      </header>

      <div v-if="isLoading" class="p-4">
        <EmptyState
          :title="t('handoutPage.loadingTitle')"
          :description="t('handoutPage.loadingDescription')"
        />
      </div>

      <div v-else-if="errorMessage" class="p-4">
        <EmptyState :title="t('handoutPage.errorTitle')" :description="errorMessage">
          <template #icon>
            <AlertCircle class="size-5" />
          </template>
        </EmptyState>
      </div>

      <HandoutWorkspace
        v-else
        class="m-3 min-h-0 flex-1"
        :items="workspaceItems"
        :read-only="pageReadOnly"
        :selected-node-id="selectedNodeId"
        :scroll-target-node-id="handoutWorkspaceScrollTargetNodeId"
        :scroll-request-key="handoutWorkspaceScrollRequestKey"
        @select-item="handleSelectWorkspaceItem"
        @add-initial-content="openSectionVariantSelectionDialog(null)"
        @move-up="handleMoveItem($event, 'Up')"
        @move-down="handleMoveItem($event, 'Down')"
        @edit-occurrence="handleEditOccurrence"
        @remove="handleRemoveItem"
      />

      <p v-if="feedback" class="mx-3 mb-3 rounded-md border bg-muted/20 px-3 py-2 text-xs text-muted-foreground">
        {{ feedback }}
      </p>
    </section>

    <aside class="min-h-0 space-y-3 overflow-hidden">
      <HandoutInspector :model="inspectorModel" />
      <HandoutOutputPanel
        :output-forms="outputForms"
        :generated-files="generatedFiles"
        :read-only="pageReadOnly"
        @generate-word="handleGenerateWord"
        @download-generated-file="handleDownloadGeneratedFile"
        @view-manifest="handleViewManifest"
        @delete-generated-file="handleDeleteGeneratedFile"
      />
    </aside>

    <Teleport to="body">
      <HandoutOverviewFlyout
        v-if="handoutOverviewOpen"
        :nodes="handoutOverviewNodes"
        :current-handout-version-id="numericHandoutVersionId"
        :loading="handoutOverviewLoading"
        :error="handoutOverviewError"
        @close="closeHandoutOverview"
        @open-version="openOverviewVersion"
        @open-management="openHandoutManagement"
      />
    </Teleport>

    <SectionVariantSelectionDialog
      :open="sectionVariantDialogOpen"
      :tree="sectionVariantTree"
      :selected-variant-ids="selectedSectionVariantIds"
      :existing-variant-ids="existingSectionVariantIds"
      :loading="sectionVariantDialogLoading"
      :error="sectionVariantDialogError"
      @close="closeSectionVariantSelectionDialog"
      @toggle-variant="toggleSectionVariantSelection"
      @submit="confirmSectionVariantSelection"
    />

    <HandoutStructureContextMenu
      :open="Boolean(handoutContextMenu)"
      :model="handoutContextMenu"
      @close="closeHandoutContextMenu"
      @request-action="handleHandoutContextAction"
    />

    <HandoutTargetPicker
      :open="targetPickerOpen"
      :title="targetPickerTitle"
      :description="targetPickerDescription"
      :candidates="targetPickerCandidates"
      :loading="targetPickerLoading"
      :error="targetPickerError"
      @close="closeHandoutTargetPicker"
      @select="selectHandoutTarget"
    />

    <HandoutOccurrenceEditDialog
      :open="Boolean(occurrenceEditItem)"
      :item-title="occurrenceEditItem?.titleOverride || occurrenceEditItem?.title || ''"
      :title-override="occurrenceEditItem?.titleOverride"
      :note="occurrenceEditItem?.note"
      @close="closeOccurrenceEditDialog"
      @submit="submitOccurrenceEdit"
    />
  </main>
</template>
