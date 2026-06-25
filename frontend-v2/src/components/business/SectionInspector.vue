<script setup lang="ts">
import { computed } from 'vue'
import { PackageOpen, Search, Trash2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import TagMultiSelect from '@/components/business/TagMultiSelect.vue'
import TeachingNoteDeleteConfirm from '@/components/business/TeachingNoteDeleteConfirm.vue'
import TeachingNoteEditor from '@/components/business/TeachingNoteEditor.vue'
import TeachingNoteList from '@/components/business/TeachingNoteList.vue'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import type {
  AtomicSectionTeachingRole,
  SectionDifficultyChangePayload,
  SectionDifficultyEditableNodeKind,
  SectionPageShellModel,
  SectionTreeNodeModel,
  TagBindingTargetType,
  TagModel,
  TeachingNoteEditorValue,
  TeachingNoteEffectLevel,
  TeachingNoteListState,
  TeachingNoteModel,
  TeachingNoteTargetType,
} from '@/types'

const props = defineProps<{
  node?: SectionTreeNodeModel
  section?: SectionPageShellModel
  variantItemCount?: number
  deletingContentBlockCascade?: boolean
  updatingAtomicSectionItemClassification?: boolean
  updatingNodeDifficulty?: boolean
  tagTargetType?: TagBindingTargetType
  tagTargetId?: number
  tags?: TagModel[]
  tagSearchResults?: TagModel[]
  tagLoading?: boolean
  tagSaving?: boolean
  tagError?: string
  tagTargetSource?: 'Direct' | 'OccurrenceContentBlock'
  teachingNoteTargetType?: TeachingNoteTargetType
  teachingNoteTargetId?: number
  teachingNoteTargetSource?: string
  teachingNoteKeyword?: string
  teachingNoteEffectLevel?: TeachingNoteEffectLevel | ''
  teachingNotes?: TeachingNoteModel[]
  teachingNoteState?: TeachingNoteListState
  teachingNoteError?: string
  teachingNoteEditorValue?: TeachingNoteEditorValue | null
  teachingNoteEditorMode?: 'create' | 'edit'
  teachingNoteSaving?: boolean
  teachingNoteDeletingId?: number | null
  teachingNoteDeleteTarget?: TeachingNoteModel | null
  teachingNoteDeleteError?: string
}>()

const emit = defineEmits<{
  deleteContentBlockCascade: []
  changeAtomicSectionItemClassification: [payload: {
    atomicSectionId: number
    atomicSectionItemId: number
    teachingRole: AtomicSectionTeachingRole
    difficulty: string
  }]
  changeNodeDifficulty: [payload: SectionDifficultyChangePayload]
  searchTags: [keyword: string]
  createTag: [name: string]
  updateTags: [tags: TagModel[]]
  saveTags: [tagIds: number[]]
  createTeachingNote: []
  editTeachingNote: [note: TeachingNoteModel]
  submitTeachingNote: [value: TeachingNoteEditorValue]
  cancelTeachingNote: []
  searchTeachingNotes: [keyword: string]
  filterTeachingNoteEffectLevel: [effectLevel: TeachingNoteEffectLevel | '']
  requestDeleteTeachingNote: [note: TeachingNoteModel]
  confirmDeleteTeachingNote: [note: TeachingNoteModel]
  cancelDeleteTeachingNote: []
}>()

const { t } = useI18n()
const atomicSectionTeachingRoleOptions: AtomicSectionTeachingRole[] = [
  'Unclassified',
  'Knowledge',
  'Example',
  'Variant',
  'Practice',
  'Homework',
]
const difficultyOptions = ['Unset', 'Basic', 'Medium', 'Advanced', 'Top']
const teachingNoteEffectFilterOptions: TeachingNoteEffectLevel[] = [
  'Good',
  'Normal',
  'Weak',
  'Failed',
]

const displayTitle = computed(() => {
  if (!props.node) {
    return ''
  }

  if (props.node.kind === 'ContentBlock') {
    return props.node.typeLabel
  }

  return props.node.title || props.node.typeLabel
})

const kindLabel = computed(() => {
  if (!props.node) {
    return ''
  }

  return t(`components.sectionTree.kind.${props.node.kind}`)
})

const showContentBlockCascadeDelete = computed(() =>
  props.node?.kind === 'ContentBlock' || props.node?.kind === 'CompositeBlock',
)
const showAtomicSectionItemClassification = computed(() =>
  (props.node?.kind === 'ContentBlock' || props.node?.kind === 'CompositeBlock') &&
  typeof props.node.atomicSectionId === 'number' &&
  typeof props.node.atomicSectionItemId === 'number',
)
const editableDifficultyKind = computed<SectionDifficultyEditableNodeKind | undefined>(() => {
  const kind = props.node?.kind
  if (
    kind === 'ContentBlock' ||
    kind === 'CompositeBlock' ||
    kind === 'AtomicSection' ||
    kind === 'AtomicSectionPanel'
  ) {
    return kind
  }

  return undefined
})
const showNodeDifficultyEditor = computed(() => Boolean(editableDifficultyKind.value))
const showTagEditor = computed(() => Boolean(props.tagTargetType && props.tagTargetId))
const showTeachingNoteEditor = computed(() =>
  Boolean(props.teachingNoteTargetType && props.teachingNoteTargetId),
)
const teachingNoteBusy = computed(() =>
  Boolean(props.teachingNoteSaving || props.teachingNoteDeletingId),
)

function updateTeachingNoteKeyword(value: string) {
  emit('searchTeachingNotes', value)
}

function updateTeachingNoteEffectLevel(value: string) {
  emit('filterTeachingNoteEffectLevel', value as TeachingNoteEffectLevel | '')
}

function changeAtomicSectionItemClassification(
  next: Partial<{
    teachingRole: AtomicSectionTeachingRole
    difficulty: string
  }>,
) {
  const node = props.node
  if (
    !node ||
    typeof node.atomicSectionId !== 'number' ||
    typeof node.atomicSectionItemId !== 'number'
  ) {
    return
  }

  emit('changeAtomicSectionItemClassification', {
    atomicSectionId: node.atomicSectionId,
    atomicSectionItemId: node.atomicSectionItemId,
    teachingRole: next.teachingRole ?? node.teachingRole ?? 'Unclassified',
    difficulty: next.difficulty ?? node.difficultyValue ?? 'Unset',
  })
}

function changeNodeDifficulty(difficulty: string) {
  const node = props.node
  const kind = editableDifficultyKind.value
  if (!node || !kind || difficulty === (node.difficultyValue ?? 'Unset')) {
    return
  }

  emit('changeNodeDifficulty', {
    nodeId: node.id,
    kind,
    difficulty,
    atomicSectionId: node.atomicSectionId,
    atomicSectionPanelId: node.atomicSectionPanelId,
    title: node.title,
    teachingRole: node.teachingRole,
  })
}

const detailRows = computed(() => {
  const node = props.node
  if (!node) {
    return []
  }

  const notSet = t('components.sectionInspector.notSet')
  const row = (id: string, label: string, value?: string | number | null) => ({
    id,
    label,
    value: value === undefined || value === null || value === '' ? notSet : String(value),
  })
  const previewState = node.previewState
    ? t(`components.contentBlockDisplay.previewState.${node.previewState}`)
    : notSet
  const wordDocumentStatus =
    node.hasWordDocument === undefined
      ? notSet
      : node.hasWordDocument
        ? t('components.sectionInspector.yes')
        : t('components.sectionInspector.no')

  if (node.kind === 'Section') {
    return [
      row('title', t('components.sectionInspector.title'), node.title),
      row('status', t('components.sectionInspector.status'), node.status),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row(
        'teachingTopic',
        t('components.sectionInspector.teachingTopic'),
        node.teachingTopicTitle ?? props.section?.teachingTopicTitle,
      ),
      row('sectionId', t('components.sectionInspector.sectionId'), node.sectionId ?? props.section?.sectionId),
    ]
  }

  if (node.kind === 'ContentBlock') {
    return [
      row('type', t('components.sectionInspector.type'), node.typeLabel),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row('status', t('components.sectionInspector.status'), node.targetStatus ?? node.status),
      row('hasWordDocument', t('components.sectionInspector.hasWordDocument'), wordDocumentStatus),
      row('previewState', t('components.sectionInspector.previewState'), previewState),
    ]
  }

  if (node.kind === 'AtomicSection') {
    return [
      row('name', t('components.sectionInspector.name'), node.title),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row('status', t('components.sectionInspector.status'), node.targetStatus ?? node.status),
      row('childCount', t('components.sectionInspector.childCount'), node.itemCount ?? 0),
    ]
  }

  if (node.kind === 'AtomicSectionPanel') {
    return [
      row('name', t('components.sectionInspector.name'), node.title),
      row('type', t('components.sectionInspector.type'), node.typeLabel),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row('childCount', t('components.sectionInspector.childCount'), node.itemCount ?? 0),
    ]
  }

  if (node.kind === 'AtomicSectionUnassigned') {
    return [
      row('name', t('components.sectionInspector.name'), node.title),
      row('type', t('components.sectionInspector.type'), node.typeLabel),
      row('childCount', t('components.sectionInspector.childCount'), node.itemCount ?? 0),
    ]
  }

  if (node.kind === 'CompositeBlock') {
    return [
      row('title', t('components.sectionInspector.title'), node.title),
      row('groupType', t('components.sectionInspector.groupType'), node.typeLabel),
      row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
      row('status', t('components.sectionInspector.status'), node.targetStatus ?? node.status),
      row('hasWordDocument', t('components.sectionInspector.hasWordDocument'), wordDocumentStatus),
      row('childCount', t('components.sectionInspector.childCount'), node.itemCount ?? 0),
    ]
  }

  return [
    row('name', t('components.sectionInspector.name'), node.title),
    row('type', t('components.sectionInspector.type'), node.typeLabel),
    row('difficulty', t('components.sectionInspector.difficulty'), node.difficulty),
    row('status', t('components.sectionInspector.status'), node.status),
    row(
      'selectedItemCount',
      t('components.sectionInspector.selectedItemCount'),
      t('components.sectionInspector.selectedItemCountValue', {
        count: props.variantItemCount ?? 0,
      }),
    ),
  ]
})
</script>

<template>
  <EmptyState
    v-if="!node"
    class="h-full"
    :title="t('components.sectionInspector.emptyTitle')"
    :description="t('components.sectionInspector.emptyDescription')"
  >
    <template #icon>
      <PackageOpen class="size-5" aria-hidden="true" />
    </template>
  </EmptyState>

  <Card v-else class="flex h-full min-h-0 flex-col overflow-hidden border">
    <CardHeader class="gap-2 px-4 py-3">
      <div class="flex min-w-0 items-start justify-between gap-3">
        <div class="min-w-0 space-y-1">
          <p class="text-xs text-muted-foreground">{{ t('components.sectionInspector.currentSelection') }}</p>
          <CardTitle class="truncate text-sm">{{ displayTitle }}</CardTitle>
        </div>
        <StatusPill :label="kindLabel" :tone="node.disabled ? 'muted' : 'active'" />
      </div>
    </CardHeader>

    <WeakScrollArea class="flex-1 space-y-2 px-4 pb-4">
      <dl class="grid gap-2 text-sm">
        <div
          v-for="row in detailRows"
          :key="row.id"
          class="flex items-center justify-between gap-3 rounded-md border bg-muted/30 px-3 py-2"
        >
          <dt class="text-xs text-muted-foreground">{{ row.label }}</dt>
          <dd class="truncate font-medium">{{ row.value }}</dd>
        </div>
      </dl>

      <div v-if="showTeachingNoteEditor && teachingNoteTargetType && teachingNoteTargetId" class="grid gap-3 border-t pt-3">
        <p class="text-xs text-muted-foreground">
          {{
            t('teachingNote.currentTarget', {
              type: t(`teachingNote.targetType.${teachingNoteTargetType}`),
              id: teachingNoteTargetId,
            })
          }}
        </p>
        <div class="grid gap-2 rounded-md border bg-muted/20 p-2">
          <div class="min-w-0">
            <p class="text-xs font-medium">
              {{ t('teachingNote.filter.title') }}
            </p>
            <p class="text-xs text-muted-foreground">
              {{ t('teachingNote.filter.description') }}
            </p>
          </div>
          <label class="grid gap-1 text-xs text-muted-foreground">
            <span>{{ t('teachingNote.filter.keywordLabel') }}</span>
            <span class="flex items-center gap-2 rounded-md border bg-background px-2 focus-within:ring-2 focus-within:ring-ring">
              <Search class="size-4 text-muted-foreground" aria-hidden="true" />
              <input
                :value="teachingNoteKeyword ?? ''"
                class="h-9 min-w-0 flex-1 bg-transparent text-sm text-foreground outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed"
                :placeholder="t('teachingNote.filter.keywordPlaceholder')"
                :disabled="teachingNoteBusy"
                @input="updateTeachingNoteKeyword(($event.target as HTMLInputElement).value)"
              />
            </span>
          </label>
          <label class="grid gap-1 text-xs text-muted-foreground">
            <span>{{ t('teachingNote.filter.effectLabel') }}</span>
            <select
              class="h-9 rounded-md border bg-background px-2 text-sm text-foreground disabled:cursor-not-allowed"
              :value="teachingNoteEffectLevel ?? ''"
              :disabled="teachingNoteBusy"
              @change="updateTeachingNoteEffectLevel(($event.target as HTMLSelectElement).value)"
            >
              <option value="">
                {{ t('teachingNote.filter.effectAll') }}
              </option>
              <option
                v-for="effectLevel in teachingNoteEffectFilterOptions"
                :key="effectLevel"
                :value="effectLevel"
              >
                {{ t(`teachingNote.effectLevel.${effectLevel}`) }}
              </option>
            </select>
          </label>
        </div>
        <TeachingNoteEditor
          v-if="teachingNoteEditorValue"
          :model-value="teachingNoteEditorValue"
          :mode="teachingNoteEditorMode ?? 'create'"
          :saving="teachingNoteSaving"
          :disabled="Boolean(teachingNoteDeletingId)"
          :error="teachingNoteError"
          @submit="emit('submitTeachingNote', $event)"
          @cancel="emit('cancelTeachingNote')"
        />
        <TeachingNoteDeleteConfirm
          v-if="teachingNoteDeleteTarget"
          :note="teachingNoteDeleteTarget"
          :deleting="teachingNoteDeletingId === teachingNoteDeleteTarget.id"
          :disabled="teachingNoteSaving"
          :error="teachingNoteDeleteError"
          @confirm="emit('confirmDeleteTeachingNote', $event)"
          @cancel="emit('cancelDeleteTeachingNote')"
        />
        <TeachingNoteList
          :notes="teachingNotes ?? []"
          :state="teachingNoteState ?? 'idle'"
          :disabled="teachingNoteBusy"
          :deleting-note-id="teachingNoteDeletingId"
          :error="teachingNoteError"
          @create="emit('createTeachingNote')"
          @edit="emit('editTeachingNote', $event)"
          @delete="emit('requestDeleteTeachingNote', $event)"
        />
      </div>
    </WeakScrollArea>

    <div v-if="showTagEditor && tagTargetType && tagTargetId" class="border-t px-4 py-3">
      <TagMultiSelect
        :target-type="tagTargetType"
        :target-id="tagTargetId"
        :model-value="tags ?? []"
        :search-results="tagSearchResults ?? []"
        :loading="tagLoading"
        :error="tagError"
        :disabled="tagSaving"
        @search="emit('searchTags', $event)"
        @create-tag="emit('createTag', $event)"
        @update:model-value="emit('updateTags', $event)"
        @save="emit('saveTags', $event)"
      />
      <p
        v-if="tagTargetSource === 'OccurrenceContentBlock'"
        class="mt-2 text-xs text-muted-foreground"
      >
        {{ t('components.sectionInspector.tagOccurrenceContentBlockHint') }}
      </p>
    </div>

    <div v-if="showNodeDifficultyEditor" class="border-t px-4 py-3">
      <div class="grid gap-2">
        <p class="text-xs font-medium">
          {{ t('components.sectionInspector.nodeDifficulty') }}
        </p>
        <label class="grid gap-1 text-xs text-muted-foreground">
          <span>{{ t('components.sectionInspector.difficulty') }}</span>
          <select
            class="h-9 rounded-md border bg-background px-2 text-sm text-foreground"
            :value="node.difficultyValue ?? 'Unset'"
            :disabled="updatingNodeDifficulty"
            @change="changeNodeDifficulty(($event.target as HTMLSelectElement).value)"
          >
            <option
              v-for="difficulty in difficultyOptions"
              :key="difficulty"
              :value="difficulty"
            >
              {{ t(`common.difficulty.${difficulty}`) }}
            </option>
          </select>
        </label>
        <p class="text-xs text-muted-foreground">
          {{
            updatingNodeDifficulty
              ? t('components.sectionInspector.nodeDifficultyUpdating')
              : t('components.sectionInspector.nodeDifficultyDescription')
          }}
        </p>
      </div>
    </div>

    <div v-if="showAtomicSectionItemClassification" class="border-t px-4 py-3">
      <div class="grid gap-2">
        <p class="text-xs font-medium">
          {{ t('components.sectionInspector.atomicSectionItemClassification') }}
        </p>
        <label class="grid gap-1 text-xs text-muted-foreground">
          <span>{{ t('components.sectionInspector.teachingRole') }}</span>
          <select
            class="h-9 rounded-md border bg-background px-2 text-sm text-foreground"
            :value="node.teachingRole ?? 'Unclassified'"
            :disabled="updatingAtomicSectionItemClassification"
            @change="
              changeAtomicSectionItemClassification({
                teachingRole: ($event.target as HTMLSelectElement).value as AtomicSectionTeachingRole,
              })
            "
          >
            <option
              v-for="role in atomicSectionTeachingRoleOptions"
              :key="role"
              :value="role"
            >
              {{ t(`components.atomicSectionTeachingRole.${role}`) }}
            </option>
          </select>
        </label>
        <label class="grid gap-1 text-xs text-muted-foreground">
          <span>{{ t('components.sectionInspector.difficulty') }}</span>
          <select
            class="h-9 rounded-md border bg-background px-2 text-sm text-foreground"
            :value="node.difficultyValue ?? 'Unset'"
            :disabled="updatingAtomicSectionItemClassification"
            @change="
              changeAtomicSectionItemClassification({
                difficulty: ($event.target as HTMLSelectElement).value,
              })
            "
          >
            <option
              v-for="difficulty in difficultyOptions"
              :key="difficulty"
              :value="difficulty"
            >
              {{ t(`common.difficulty.${difficulty}`) }}
            </option>
          </select>
        </label>
      </div>
    </div>

    <div v-if="showContentBlockCascadeDelete" class="border-t px-4 py-3">
      <div class="grid gap-2">
        <p class="text-xs font-medium text-destructive">
          {{ t('components.sectionInspector.dangerZone') }}
        </p>
        <p class="text-xs text-muted-foreground">
          {{ t('components.sectionInspector.deleteContentBlockCascadeDescription') }}
        </p>
        <Button
          type="button"
          variant="destructive"
          size="sm"
          class="w-full"
          :disabled="deletingContentBlockCascade"
          @click="emit('deleteContentBlockCascade')"
        >
          <Trash2 class="size-4" aria-hidden="true" />
          {{
            deletingContentBlockCascade
              ? t('components.sectionInspector.deleteContentBlockCascadeBusy')
              : t('components.sectionInspector.deleteContentBlockCascade')
          }}
        </Button>
      </div>
    </div>
  </Card>
</template>
