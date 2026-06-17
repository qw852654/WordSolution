<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, GitBranch, MousePointer2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import SectionTree from '@/components/business/SectionTree.vue'
import SectionTreeContextMenu from '@/components/business/SectionTreeContextMenu.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { mockSectionTreeNodes } from '@/mocks'
import type {
  SectionTreeContextMenuActionPayload,
  SectionTreeContextMenuModel,
  SectionTreeContextMenuPayload,
  SectionTreeNodeModel,
} from '@/types'

const { t } = useI18n()

const selectedNodeId = ref('section-tree-atomic-basics')
const contextMenu = ref<SectionTreeContextMenuModel | null>(null)
const feedback = ref('')

const selectedNode = computed(() => findSectionTreeNode(mockSectionTreeNodes, selectedNodeId.value))
const contextTargetNodeId = computed(() => contextMenu.value?.node.id)

function findSectionTreeNode(nodes: SectionTreeNodeModel[], nodeId?: string): SectionTreeNodeModel | undefined {
  if (!nodeId) {
    return undefined
  }

  for (const node of nodes) {
    if (node.id === nodeId) {
      return node
    }

    const childResult = findSectionTreeNode(node.children ?? [], nodeId)

    if (childResult) {
      return childResult
    }
  }

  return undefined
}

function selectNode(nodeId: string) {
  selectedNodeId.value = nodeId
  contextMenu.value = null
}

function openContextMenu(payload: SectionTreeContextMenuPayload) {
  contextMenu.value = {
    node: payload.node,
    position: {
      x: payload.x,
      y: payload.y,
    },
  }
}

function closeContextMenu() {
  contextMenu.value = null
}

function handleMenuAction(payload: SectionTreeContextMenuActionPayload) {
  const targetNode = contextMenu.value?.node
  const actionLabel = t(`components.sectionTreeContextMenu.actions.${payload.actionType}`)

  feedback.value = t('lab.sections.sectionTreeContextMenu.feedback', {
    action: actionLabel,
    node: targetNode?.typeLabel ?? payload.nodeId,
  })
  contextMenu.value = null
}
</script>

<template>
  <main class="min-h-screen bg-background px-4 py-6 text-foreground sm:px-6 lg:px-8">
    <PageHeader
      :eyebrow="t('lab.eyebrow')"
      :title="t('lab.title')"
      :description="t('lab.description')"
    >
      <template #actions>
        <Button variant="outline" as-child>
          <RouterLink to="/">
            <ArrowLeft class="size-4" />
            {{ t('lab.backHome') }}
          </RouterLink>
        </Button>
      </template>
    </PageHeader>

    <section class="mt-6 space-y-4" :aria-label="t('lab.sections.sectionTreeContextMenu.title')">
      <div class="flex items-start gap-2">
        <MousePointer2 class="mt-0.5 size-4" aria-hidden="true" />
        <div>
          <h2 class="text-base font-semibold">{{ t('lab.sections.sectionTreeContextMenu.title') }}</h2>
          <p class="text-sm text-muted-foreground">
            {{ t('lab.sections.sectionTreeContextMenu.description') }}
          </p>
        </div>
      </div>

      <div class="grid gap-4 lg:grid-cols-[minmax(18rem,24rem)_minmax(0,1fr)]">
        <section class="rounded-lg border bg-background p-4" :aria-label="t('components.sectionTree.title')">
          <SectionTree
            :nodes="mockSectionTreeNodes"
            :selected-node-id="selectedNodeId"
            :context-target-node-id="contextTargetNodeId"
            @select-node="selectNode"
            @node-context-menu="openContextMenu"
          />
        </section>

        <section class="grid gap-4 md:grid-cols-3">
          <div class="rounded-lg border bg-background p-4">
            <div class="mb-3 flex items-center gap-2">
              <GitBranch class="size-4" aria-hidden="true" />
              <h3 class="text-sm font-medium">
                {{ t('lab.sections.sectionTreeContextMenu.selectedTitle') }}
              </h3>
            </div>
            <p class="truncate text-sm text-foreground">
              {{ selectedNode?.typeLabel ?? t('lab.sections.sectionTreeContextMenu.emptySelected') }}
            </p>
            <p class="mt-1 truncate text-xs text-muted-foreground">
              {{ selectedNode?.kind ?? t('components.sectionInspector.notSet') }}
            </p>
          </div>

          <div class="rounded-lg border bg-background p-4">
            <h3 class="text-sm font-medium">
              {{ t('lab.sections.sectionTreeContextMenu.contextTargetTitle') }}
            </h3>
            <p class="mt-3 truncate text-sm text-foreground">
              {{ contextMenu?.node.typeLabel ?? t('lab.sections.sectionTreeContextMenu.emptyContextTarget') }}
            </p>
            <p class="mt-1 text-xs text-muted-foreground">
              {{ t('lab.sections.sectionTreeContextMenu.contextRule') }}
            </p>
          </div>

          <div class="rounded-lg border bg-background p-4" aria-live="polite">
            <h3 class="text-sm font-medium">
              {{ t('lab.sections.sectionTreeContextMenu.feedbackTitle') }}
            </h3>
            <p class="mt-3 text-sm text-muted-foreground">
              {{ feedback || t('lab.sections.sectionTreeContextMenu.emptyFeedback') }}
            </p>
          </div>
        </section>
      </div>
    </section>

    <SectionTreeContextMenu
      :model="contextMenu"
      :open="contextMenu !== null"
      @close="closeContextMenu"
      @request-action="handleMenuAction"
    />
  </main>
</template>
