<script setup lang="ts">
import { computed, ref } from 'vue'
import { Check, Minus, Search } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import BasicTree from '@/components/business/BasicTree.vue'
import BasicTreeNodeView from '@/components/presentation/BasicTreeNodeView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import { Button } from '@/components/ui/button'
import {
  buildExistingVariantIds,
  deriveNodeCheckState,
  filterTree,
  getSectionVariantIds,
  getTopicVariantIds,
  toggleGroup,
  toggleVariant,
  type SectionVariantTreeCheckState,
} from '@/utils/sectionVariantTreeSelection'
import type {
  CmsV2SectionVariantDto,
  CmsV2SectionVariantSelectionTreeSectionDto,
  CmsV2SectionVariantSelectionTreeTopicDto,
} from '@/apis/cmsV2Client'
import type { BasicTreeNode } from '@/types'

type SelectionPayload =
  | { kind: 'Topic'; title: string; status?: string; variantIds: number[] }
  | { kind: 'Section'; title: string; status?: string; variantIds: number[] }
  | { kind: 'SectionVariant'; variant: CmsV2SectionVariantDto; variantIds: number[] }

const props = defineProps<{
  open: boolean
  tree: CmsV2SectionVariantSelectionTreeTopicDto[]
  selectedVariantIds: number[]
  existingVariantIds: number[]
  loading?: boolean
  error?: string
}>()

const emit = defineEmits<{
  close: []
  toggleVariant: [sectionVariantId: number]
  submit: []
}>()

const { t } = useI18n()
const searchText = ref('')

const selectedSet = computed(() => new Set(props.selectedVariantIds))
const existingSet = computed(() => buildExistingVariantIds(props.existingVariantIds))
const selectedNewCount = computed(
  () => props.selectedVariantIds.filter((id) => !existingSet.value.has(id)).length,
)
const basicNodes = computed(() =>
  filterTree(props.tree, searchText.value)
    .map(toTopicNode)
    .filter((node): node is BasicTreeNode => Boolean(node)),
)

function toTopicNode(topic: CmsV2SectionVariantSelectionTreeTopicDto): BasicTreeNode | undefined {
  const sectionNodes = topic.sections
    .map(toSectionNode)
    .filter((node): node is BasicTreeNode => Boolean(node))
  const childTopicNodes = topic.children
    .map(toTopicNode)
    .filter((node): node is BasicTreeNode => Boolean(node))
  const children = [...sectionNodes, ...childTopicNodes]

  if (!children.length) {
    return undefined
  }

  return {
    id: `topic:${topic.teachingTopic.id}`,
    label: topic.teachingTopic.name,
    payload: {
      kind: 'Topic',
      title: topic.teachingTopic.name,
      status: topic.teachingTopic.status,
      variantIds: getTopicVariantIds(topic),
    } satisfies SelectionPayload,
    expanded: true,
    children,
  }
}

function toSectionNode(
  section: CmsV2SectionVariantSelectionTreeSectionDto,
): BasicTreeNode | undefined {
  const children = section.sectionVariants.map(toVariantNode)

  if (!children.length) {
    return undefined
  }

  return {
    id: `section:${section.section.id}`,
    label: section.section.title,
    payload: {
      kind: 'Section',
      title: section.section.title,
      status: section.section.status,
      variantIds: getSectionVariantIds(section),
    } satisfies SelectionPayload,
    expanded: true,
    children,
  }
}

function toVariantNode(variant: CmsV2SectionVariantDto): BasicTreeNode {
  return {
    id: `variant:${variant.id}`,
    label: variant.title,
    meta: `${variant.type} · ${variant.difficulty} · ${variant.status}`,
    payload: {
      kind: 'SectionVariant',
      variant,
      variantIds: [variant.id],
    } satisfies SelectionPayload,
    disabled: existingSet.value.has(variant.id),
  }
}

function getPayload(node: BasicTreeNode) {
  return node.payload as SelectionPayload
}

function handleSelect(nodeId: string) {
  const node = findBasicNode(basicNodes.value, nodeId)
  if (!node) {
    return
  }

  const payload = getPayload(node)
  const next =
    payload.kind === 'SectionVariant'
      ? toggleVariant(selectedSet.value, existingSet.value, payload.variant.id)
      : toggleGroup(selectedSet.value, existingSet.value, payload.variantIds)

  emitSelectionChange(next)
}

function findBasicNode(nodes: BasicTreeNode[], nodeId: string): BasicTreeNode | undefined {
  for (const node of nodes) {
    if (node.id === nodeId) {
      return node
    }

    const child = node.children ? findBasicNode(node.children, nodeId) : undefined
    if (child) {
      return child
    }
  }

  return undefined
}

function emitSelectionChange(next: Set<number>) {
  const current = selectedSet.value
  const ids = Array.from(new Set([...current, ...next]))

  for (const id of ids) {
    if (current.has(id) !== next.has(id)) {
      emit('toggleVariant', id)
    }
  }
}

function getCheckState(node: BasicTreeNode): SectionVariantTreeCheckState {
  return deriveNodeCheckState(getPayload(node).variantIds, selectedSet.value, existingSet.value)
}

function getCheckStateLabel(state: SectionVariantTreeCheckState) {
  if (state === 'checked') {
    return t('sectionVariantSelectionDialog.checked')
  }

  if (state === 'mixed') {
    return t('sectionVariantSelectionDialog.mixed')
  }

  if (state === 'locked') {
    return t('sectionVariantSelectionDialog.locked')
  }

  return t('sectionVariantSelectionDialog.unchecked')
}

function getCheckboxClasses(state: SectionVariantTreeCheckState) {
  if (state === 'checked' || state === 'locked') {
    return 'border-primary bg-primary text-primary-foreground'
  }

  if (state === 'mixed') {
    return 'border-primary bg-primary/10 text-primary'
  }

  return 'border-border bg-background text-muted-foreground'
}

function isChecked(state: SectionVariantTreeCheckState) {
  return state === 'checked' || state === 'locked'
}

function getTitle(node: BasicTreeNode) {
  const payload = getPayload(node)
  return payload.kind === 'SectionVariant' ? payload.variant.title : payload.title
}

function getMetaItems(node: BasicTreeNode) {
  const payload = getPayload(node)

  if (payload.kind === 'SectionVariant') {
    return [
      payload.variant.type,
      payload.variant.difficulty,
      existingSet.value.has(payload.variant.id)
        ? t('sectionVariantSelectionDialog.existing')
        : '',
    ].filter(Boolean) as string[]
  }

  return [payload.status || ''].filter(Boolean) as string[]
}

function getMarkerClass(node: BasicTreeNode) {
  const payload = getPayload(node)

  if (payload.kind === 'SectionVariant') {
    return 'bg-primary'
  }

  if (payload.kind === 'Section') {
    return 'bg-muted-foreground'
  }

  return 'bg-border'
}
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-[65] flex min-h-screen items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-label="t('sectionVariantSelectionDialog.dialogLabel')"
    >
      <button
        type="button"
        class="absolute inset-0 bg-background/70 backdrop-blur-sm"
        :aria-label="t('sectionVariantSelectionDialog.close')"
        @click="emit('close')"
      />

      <section class="relative z-10 flex max-h-[calc(100vh-2rem)] w-full max-w-3xl flex-col rounded-lg border bg-card text-card-foreground">
        <header class="border-b px-4 py-3">
          <div class="flex items-start justify-between gap-3">
            <div class="min-w-0">
              <h2 class="truncate text-lg font-semibold">
                {{ t('sectionVariantSelectionDialog.title') }}
              </h2>
              <p class="mt-1 text-sm text-muted-foreground">
                {{ t('sectionVariantSelectionDialog.description') }}
              </p>
            </div>
            <p class="shrink-0 text-sm text-muted-foreground">
              {{ t('sectionVariantSelectionDialog.selectedCount', { count: selectedNewCount }) }}
            </p>
          </div>
        </header>

        <div class="flex min-h-0 flex-1 flex-col gap-3 p-4">
          <label class="flex items-center gap-2 rounded-md border bg-background px-3 py-2 text-sm">
            <Search class="size-4 text-muted-foreground" aria-hidden="true" />
            <input
              v-model="searchText"
              class="min-w-0 flex-1 bg-transparent outline-none placeholder:text-muted-foreground"
              :placeholder="t('sectionVariantSelectionDialog.searchPlaceholder')"
            />
          </label>

          <EmptyState
            v-if="loading"
            :title="t('sectionVariantSelectionDialog.loadingTitle')"
            :description="t('sectionVariantSelectionDialog.loadingDescription')"
          />

          <EmptyState
            v-else-if="error"
            :title="t('sectionVariantSelectionDialog.errorTitle')"
            :description="error"
          />

          <div v-else-if="basicNodes.length" class="min-h-0 overflow-auto">
            <BasicTree
              :nodes="basicNodes"
              :expand-label="t('components.basicTree.expand')"
              :collapse-label="t('components.basicTree.collapse')"
              @select="handleSelect"
            >
              <template #default="{ node }">
                <span class="flex min-w-0 flex-1 items-center gap-2">
                  <span
                    class="flex size-4 shrink-0 items-center justify-center rounded-sm border text-[10px]"
                    :class="getCheckboxClasses(getCheckState(node))"
                    :aria-label="getCheckStateLabel(getCheckState(node))"
                    role="img"
                  >
                    <Check v-if="isChecked(getCheckState(node))" class="size-3" aria-hidden="true" />
                    <Minus
                      v-else-if="getCheckState(node) === 'mixed'"
                      class="size-3"
                      aria-hidden="true"
                    />
                  </span>
                  <BasicTreeNodeView
                    :title="getTitle(node)"
                    :marker-label="getPayload(node).kind"
                    :marker-class="getMarkerClass(node)"
                    :meta-items="getMetaItems(node)"
                  />
                </span>
              </template>
            </BasicTree>
          </div>

          <EmptyState
            v-else
            :title="t('sectionVariantSelectionDialog.emptyTitle')"
            :description="t('sectionVariantSelectionDialog.emptyDescription')"
          />
        </div>

        <footer class="flex justify-end gap-2 border-t px-4 py-3">
          <Button type="button" variant="outline" @click="emit('close')">
            {{ t('sectionVariantSelectionDialog.close') }}
          </Button>
          <Button type="button" :disabled="loading || Boolean(error)" @click="emit('submit')">
            {{ t('sectionVariantSelectionDialog.submit') }}
          </Button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>
