<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { AlertCircle, FileText, RefreshCw } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import HandoutInspector from '@/components/business/HandoutInspector.vue'
import HandoutOutputPanel from '@/components/business/HandoutOutputPanel.vue'
import HandoutStructurePanel from '@/components/business/HandoutStructurePanel.vue'
import HandoutWorkspace from '@/components/business/HandoutWorkspace.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import { cmsV2Api } from '@/apis/cmsV2Client'
import {
  mockGeneratedFiles,
  mockHandoutInspector,
  mockHandoutTreeNodes,
  mockHandoutWorkspaceItems,
  mockOutputForms,
} from '@/mocks'
import type {
  CmsV2GeneratedFileDto,
  CmsV2HandoutVersionWorkspaceDto,
  CmsV2HandoutWorkspaceItemDto,
  CmsV2HandoutWorkspaceNodeDto,
  CmsV2OutputFormDto,
} from '@/apis/cmsV2Client'
import type {
  GeneratedFileRowModel,
  HandoutInspectorModel,
  HandoutTreeNodeKind,
  HandoutTreeNodeModel,
  HandoutWorkspaceChildModel,
  HandoutWorkspaceItemModel,
  OutputFormCardModel,
} from '@/types'

const route = useRoute()
const { t } = useI18n()

const workspace = ref<CmsV2HandoutVersionWorkspaceDto | null>(null)
const selectedNodeId = ref<string>('')
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
}

function handleSelectWorkspaceItem(itemId: string) {
  const item = workspaceItems.value.find((entry) => entry.id === itemId || entry.nodeId === itemId)
  selectedNodeId.value = item?.nodeId ?? itemId
}

function showDeferredFeedback(actionKey: string, id?: number | string) {
  feedback.value = t(actionKey, { id }) as string
}

async function withOperation(action: () => Promise<void>) {
  if (isDemoRoute.value || !numericHandoutVersionId.value) {
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
  await promptAndAddHandoutVersionItem(null)
}

async function promptAndAddHandoutVersionItem(afterHandoutVersionItemId: number | null) {
  await withOperation(async () => {
    const targetType = window.prompt(
      t('handoutPage.prompt.targetType') as string,
      'SectionVariant',
    )
    if (
      targetType !== 'SectionVariant' &&
      targetType !== 'AtomicSection' &&
      targetType !== 'ContentBlock'
    ) {
      feedback.value = t('handoutPage.prompt.invalidTargetType') as string
      return
    }

    const targetIdText = window.prompt(t('handoutPage.prompt.targetId') as string, '')
    if (targetIdText === null) {
      return
    }

    const targetId = Number(targetIdText)
    if (!Number.isInteger(targetId) || targetId <= 0) {
      feedback.value = t('handoutPage.prompt.invalidTargetId') as string
      return
    }

    await cmsV2Api.addHandoutVersionItem(numericHandoutVersionId.value!, {
      targetType,
      targetId,
      afterHandoutVersionItemId,
    })
    await loadHandoutWorkspace()
    feedback.value = t('handoutPage.feedback.addedItem') as string
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

  const titleOverride = window.prompt(
    t('handoutPage.prompt.titleOverride') as string,
    item.titleOverride ?? '',
  )
  if (titleOverride === null) {
    return
  }

  const note = window.prompt(t('handoutPage.prompt.note') as string, item.note ?? '')
  if (note === null) {
    return
  }

  await withOperation(async () => {
    await cmsV2Api.updateHandoutVersionItem(
      numericHandoutVersionId.value!,
      item.handoutVersionItemId,
      {
        titleOverride: titleOverride.trim() || null,
        note: note.trim() || null,
      },
    )
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
    <aside class="min-h-0 rounded-lg border bg-background p-3">
      <HandoutStructurePanel
        :nodes="treeNodes"
        :selected-node-id="selectedNodeId"
        :read-only="isDemoRoute || operationPending"
        @select-node="handleSelectNode"
        @add-to-end="handleAddToEnd"
        @node-context-menu="showDeferredFeedback('handoutPage.feedback.contextMenu')"
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
        :read-only="isDemoRoute || operationPending"
        @select-item="handleSelectWorkspaceItem"
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
        @generate-word="handleGenerateWord"
        @download-generated-file="handleDownloadGeneratedFile"
        @view-manifest="handleViewManifest"
        @delete-generated-file="handleDeleteGeneratedFile"
      />
    </aside>
  </main>
</template>
