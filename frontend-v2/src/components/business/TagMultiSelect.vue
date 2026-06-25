<script setup lang="ts">
import { computed, ref } from 'vue'
import { Loader2, Plus, Save, Search } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import TagBadge from '@/components/business/TagBadge.vue'
import { Button } from '@/components/ui/button'
import type { TagBindingTargetType, TagModel } from '@/types'

const props = withDefaults(defineProps<{
  targetType: TagBindingTargetType
  targetId: number
  modelValue: TagModel[]
  searchResults?: TagModel[]
  loading?: boolean
  error?: string
  disabled?: boolean
}>(), {
  searchResults: () => [],
  loading: false,
  error: '',
  disabled: false,
})

const emit = defineEmits<{
  search: [keyword: string]
  'create-tag': [name: string]
  'update:modelValue': [tags: TagModel[]]
  save: [tagIds: number[]]
}>()

const { t } = useI18n()
const keyword = ref('')

const selectedIds = computed(() => new Set(props.modelValue.map((tag) => tag.id)))
const trimmedKeyword = computed(() => keyword.value.trim())
const targetLabel = computed(() => `${props.targetType} #${props.targetId}`)
const hasExactMatch = computed(() =>
  props.searchResults.some(
    (tag) => tag.name.trim().toLocaleLowerCase() === trimmedKeyword.value.toLocaleLowerCase(),
  ),
)
const selectableResults = computed(() =>
  props.searchResults.map((tag) => ({
    tag,
    selected: selectedIds.value.has(tag.id),
    disabled: props.disabled || tag.status === 'Archived',
  })),
)
const canCreate = computed(
  () =>
    !props.disabled &&
    trimmedKeyword.value.length > 0 &&
    !hasExactMatch.value &&
    !props.loading,
)

function updateKeyword(value: string) {
  keyword.value = value
  emit('search', value)
}

function selectTag(tag: TagModel) {
  if (props.disabled || tag.status === 'Archived' || selectedIds.value.has(tag.id)) {
    return
  }

  emit('update:modelValue', [...props.modelValue, tag])
}

function removeTag(tagId: number) {
  if (props.disabled) {
    return
  }

  emit('update:modelValue', props.modelValue.filter((tag) => tag.id !== tagId))
}

function createTag() {
  if (!canCreate.value) {
    return
  }

  emit('create-tag', trimmedKeyword.value)
}

function saveTags() {
  emit('save', props.modelValue.map((tag) => tag.id))
}
</script>

<template>
  <section
    class="grid gap-3 rounded-md border bg-card p-3 text-card-foreground"
    :aria-label="t('tag.multiSelectLabel', { target: targetLabel })"
  >
    <div class="flex items-center justify-between gap-3">
      <div class="min-w-0">
        <h3 class="text-sm font-semibold">
          {{ t('tag.title') }}
        </h3>
        <p class="text-xs text-muted-foreground">
          {{ t('tag.targetSummary', { target: targetLabel }) }}
        </p>
      </div>
      <Button
        type="button"
        size="sm"
        :disabled="disabled"
        @click="saveTags"
      >
        <Save class="size-4" aria-hidden="true" />
        {{ t('tag.save') }}
      </Button>
    </div>

    <div class="flex min-h-9 flex-wrap items-center gap-2 rounded-md border bg-background px-2 py-1">
      <TagBadge
        v-for="tag in modelValue"
        :key="tag.id"
        :tag="tag"
        removable
        :disabled="disabled"
        @remove="removeTag"
      />
      <span v-if="!modelValue.length" class="text-sm text-muted-foreground">
        {{ t('tag.emptySelected') }}
      </span>
    </div>

    <label class="grid gap-1">
      <span class="text-xs font-medium text-muted-foreground">
        {{ t('tag.search') }}
      </span>
      <span class="flex items-center gap-2 rounded-md border bg-background px-2 focus-within:ring-2 focus-within:ring-ring">
        <Search class="size-4 text-muted-foreground" aria-hidden="true" />
        <input
          :value="keyword"
          class="h-9 min-w-0 flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground disabled:cursor-not-allowed"
          :placeholder="t('tag.searchPlaceholder')"
          :disabled="disabled"
          @input="updateKeyword(($event.target as HTMLInputElement).value)"
        />
        <Loader2 v-if="loading" class="size-4 animate-spin text-muted-foreground" aria-hidden="true" />
      </span>
    </label>

    <div v-if="error" class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
      {{ error }}
    </div>

    <div v-else-if="loading" class="rounded-md border bg-muted/30 px-3 py-3 text-sm text-muted-foreground">
      {{ t('tag.loading') }}
    </div>

    <EmptyState
      v-else-if="!searchResults.length && trimmedKeyword"
      :title="t('tag.noResults')"
      :description="t('tag.noResultsDescription')"
      :action-label="canCreate ? t('tag.createFromKeyword', { name: trimmedKeyword }) : undefined"
      @action="createTag"
    >
    </EmptyState>

    <div v-else class="grid gap-2">
      <button
        v-for="result in selectableResults"
        :key="result.tag.id"
        type="button"
        class="flex items-center justify-between gap-3 rounded-md border bg-background px-3 py-2 text-left transition-colors hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="result.disabled || result.selected"
        @click="selectTag(result.tag)"
      >
        <TagBadge :tag="result.tag" :selected="result.selected" />
        <span class="text-xs text-muted-foreground">
          {{ result.selected ? t('tag.selected') : t('tag.select') }}
        </span>
      </button>

      <Button
        v-if="canCreate"
        type="button"
        variant="outline"
        size="sm"
        class="justify-start"
        @click="createTag"
      >
        <Plus class="size-4" aria-hidden="true" />
        {{ t('tag.createFromKeyword', { name: trimmedKeyword }) }}
      </Button>
    </div>
  </section>
</template>
