<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, GitBranch, MousePointer2, Network } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import TeachingTopicTree from '@/components/business/TeachingTopicTree.vue'
import TeachingTopicTreeContextMenu from '@/components/business/TeachingTopicTreeContextMenu.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { mockTeachingTopicTreeNodes } from '@/mocks'
import type {
  TeachingTopicTreeContextMenuActionPayload,
  TeachingTopicTreeContextMenuModel,
  TeachingTopicTreeContextMenuPayload,
  TeachingTopicTreeNodeModel,
} from '@/types'

const { t } = useI18n()

const selectedTopicId = ref('topic-mechanical-energy')
const contextMenu = ref<TeachingTopicTreeContextMenuModel | null>(null)
const feedback = ref('')

const selectedTopic = computed(() =>
  findTeachingTopicNode(mockTeachingTopicTreeNodes, selectedTopicId.value),
)
const contextTargetTopicId = computed(() => contextMenu.value?.node.id)

function findTeachingTopicNode(
  nodes: TeachingTopicTreeNodeModel[],
  nodeId?: string,
): TeachingTopicTreeNodeModel | undefined {
  if (!nodeId) {
    return undefined
  }

  for (const node of nodes) {
    if (node.id === nodeId) {
      return node
    }

    const childResult = findTeachingTopicNode(node.children ?? [], nodeId)

    if (childResult) {
      return childResult
    }
  }

  return undefined
}

function selectTopic(topicId: string) {
  selectedTopicId.value = topicId
  contextMenu.value = null
}

function openContextMenu(payload: TeachingTopicTreeContextMenuPayload) {
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

function handleMenuAction(payload: TeachingTopicTreeContextMenuActionPayload) {
  const targetNode = contextMenu.value?.node
  const actionLabel = t(`components.teachingTopicTreeContextMenu.actions.${payload.actionType}`)

  feedback.value = t('lab.sections.teachingTopicTree.contextFeedback', {
    action: actionLabel,
    node: targetNode?.title ?? payload.nodeId,
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

    <section class="mt-6 space-y-4" :aria-label="t('lab.sections.teachingTopicTree.title')">
      <div class="flex items-start gap-2">
        <Network class="mt-0.5 size-4" aria-hidden="true" />
        <div>
          <h2 class="text-base font-semibold">{{ t('lab.sections.teachingTopicTree.title') }}</h2>
          <p class="text-sm text-muted-foreground">
            {{ t('lab.sections.teachingTopicTree.description') }}
          </p>
        </div>
      </div>

      <div class="grid gap-4 lg:grid-cols-[minmax(18rem,24rem)_minmax(0,1fr)]">
        <section class="rounded-lg border bg-background p-4" :aria-label="t('components.teachingTopicTree.title')">
          <TeachingTopicTree
            :nodes="mockTeachingTopicTreeNodes"
            :selected-topic-id="selectedTopicId"
            :context-target-topic-id="contextTargetTopicId"
            @select-topic="selectTopic"
            @node-context-menu="openContextMenu"
          />
        </section>

        <aside class="space-y-4">
          <div class="rounded-lg border bg-background p-4">
            <div class="mb-3 flex items-center gap-2">
              <GitBranch class="size-4" aria-hidden="true" />
              <h3 class="text-sm font-medium">
                {{ t('lab.sections.teachingTopicTree.selectedTitle') }}
              </h3>
            </div>

            <div v-if="selectedTopic" class="space-y-3 text-sm">
              <div>
                <p class="text-xs text-muted-foreground">
                  {{ t('lab.sections.teachingTopicTree.nameLabel') }}
                </p>
                <p class="font-medium">{{ selectedTopic.title }}</p>
              </div>
              <div class="grid gap-2 sm:grid-cols-2">
                <div>
                  <p class="text-xs text-muted-foreground">
                    {{ t('components.teachingTopicTree.status') }}
                  </p>
                  <p>{{ selectedTopic.status ?? t('components.sectionInspector.notSet') }}</p>
                </div>
                <div>
                  <p class="text-xs text-muted-foreground">
                    {{ t('components.teachingTopicTree.sectionCountLabel') }}
                  </p>
                  <p>
                    {{
                      typeof selectedTopic.sectionCount === 'number'
                        ? selectedTopic.sectionCount
                        : t('components.sectionInspector.notSet')
                    }}
                  </p>
                </div>
                <div>
                  <p class="text-xs text-muted-foreground">
                    {{ t('components.teachingTopicTree.handoutCountLabel') }}
                  </p>
                  <p>
                    {{
                      typeof selectedTopic.handoutCount === 'number'
                        ? selectedTopic.handoutCount
                        : t('components.sectionInspector.notSet')
                    }}
                  </p>
                </div>
                <div>
                  <p class="text-xs text-muted-foreground">
                    {{ t('components.teachingTopicTree.archivedLabel') }}
                  </p>
                  <p>
                    {{
                      selectedTopic.archived
                        ? t('components.sectionInspector.yes')
                        : t('components.sectionInspector.no')
                    }}
                  </p>
                </div>
              </div>
            </div>

            <p v-else class="text-sm text-muted-foreground">
              {{ t('lab.sections.teachingTopicTree.emptySelected') }}
            </p>
          </div>

          <div class="grid gap-4 md:grid-cols-2">
            <div class="rounded-lg border bg-background p-4">
              <div class="mb-3 flex items-center gap-2">
                <MousePointer2 class="size-4" aria-hidden="true" />
                <h3 class="text-sm font-medium">
                  {{ t('lab.sections.teachingTopicTree.contextTargetTitle') }}
                </h3>
              </div>
              <p class="truncate text-sm text-foreground">
                {{ contextMenu?.node.title ?? t('lab.sections.teachingTopicTree.emptyContextTarget') }}
              </p>
              <p class="mt-1 text-xs text-muted-foreground">
                {{ t('lab.sections.teachingTopicTree.contextRule') }}
              </p>
            </div>

            <div class="rounded-lg border bg-background p-4" aria-live="polite">
              <h3 class="text-sm font-medium">
                {{ t('lab.sections.teachingTopicTree.feedbackTitle') }}
              </h3>
              <p class="mt-3 text-sm text-muted-foreground">
                {{ feedback || t('lab.sections.teachingTopicTree.emptyFeedback') }}
              </p>
            </div>
          </div>
        </aside>
      </div>
    </section>

    <TeachingTopicTreeContextMenu
      :model="contextMenu"
      :open="contextMenu !== null"
      @close="closeContextMenu"
      @request-action="handleMenuAction"
    />
  </main>
</template>
