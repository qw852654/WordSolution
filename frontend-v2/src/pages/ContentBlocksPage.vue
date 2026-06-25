<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Search, X } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import ContentBlockCard from '@/components/business/ContentBlockCard.vue'
import TagBadge from '@/components/business/TagBadge.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { usePageTitle } from '@/composables/usePageTitle'
import { useTagActions } from '@/composables/useTagActions'
import { mapTagBindingsToTags } from '@/utils/tagTargets'
import type { ContentBlockCardModel, TagModel } from '@/types'

const { t } = useI18n()
const tagActions = useTagActions()
usePageTitle(() => t('routes.contentBlocks.title'))

const contentBlocks = ref<ContentBlockCardModel[]>([])
const selectedFilterTags = ref<TagModel[]>([])
const tagSearchResults = ref<TagModel[]>([])
const tagKeyword = ref('')
const loadingBlocks = ref(false)
const loadingTags = ref(false)
const errorMessage = ref('')

const selectedFilterTagIds = computed(() => new Set(selectedFilterTags.value.map((tag) => tag.id)))
const visibleTagSearchResults = computed(() =>
  tagSearchResults.value.filter((tag) => !selectedFilterTagIds.value.has(tag.id)),
)

function mapBlockType(value?: string | null) {
  const labels: Record<string, string> = {
    KnowledgePoint: '知识点',
    Explanation: '说明',
    Question: '题目',
    Answer: '答案',
    Analysis: '解析',
    MethodSummary: '方法总结',
    CommonMistake: '易错点',
    Analogy: '类比说明',
    DiagramNote: '图示说明',
    ExampleGroup: '例题组',
    ExerciseGroup: '练习组',
    VariantGroup: '变式题组',
    GeneralText: '普通文本',
  }

  return value ? labels[value] ?? value : 'ContentBlock'
}

function mapDifficulty(value?: string | null) {
  const labels: Record<string, string> = {
    Unset: '未设置',
    Basic: '基础',
    Medium: '中档',
    Advanced: '提高',
    Top: '压轴',
  }

  return value ? labels[value] ?? value : '未设置'
}

function mapStatus(value?: string | null) {
  const labels: Record<string, string> = {
    Active: '可用',
    Archived: '归档',
    Draft: '草稿',
  }

  return value ? labels[value] ?? value : '未设置'
}

async function toCardModel(block: Awaited<ReturnType<typeof tagActions.listContentBlocksByTags>>[number]) {
  const tags = mapTagBindingsToTags(
    await tagActions.loadTargetTags({
      targetType: 'ContentBlock',
      targetId: block.id,
    }),
  )

  return {
    id: String(block.id),
    title: block.title || t('contentBlocks.untitled'),
    role: mapBlockType(block.blockType),
    blockType: 'ContentBlock',
    difficulty: mapDifficulty(block.difficulty),
    status: mapStatus(block.status),
    version: block.currentVersionId ? `#${block.currentVersionId}` : t('contentBlocks.noVersion'),
    summary: block.summary || t('contentBlocks.emptySummary'),
    tags,
    disabled: block.status === 'Archived',
  }
}

async function loadContentBlocks() {
  loadingBlocks.value = true
  errorMessage.value = ''

  try {
    const blocks = await tagActions.listContentBlocksByTags(
      selectedFilterTags.value.map((tag) => tag.id),
    )
    contentBlocks.value = await Promise.all(blocks.map(toCardModel))
  } catch (error) {
    contentBlocks.value = []
    errorMessage.value = error instanceof Error ? error.message : t('contentBlocks.loadFailed')
  } finally {
    loadingBlocks.value = false
  }
}

async function searchTags(keyword: string) {
  tagKeyword.value = keyword
  loadingTags.value = true
  errorMessage.value = ''

  try {
    tagSearchResults.value = await tagActions.searchTags(keyword)
  } catch (error) {
    tagSearchResults.value = []
    errorMessage.value = error instanceof Error ? error.message : t('tag.searchFailed')
  } finally {
    loadingTags.value = false
  }
}

function addFilterTag(tag: TagModel) {
  if (selectedFilterTagIds.value.has(tag.id)) {
    return
  }

  selectedFilterTags.value = [...selectedFilterTags.value, tag]
  void loadContentBlocks()
}

function removeFilterTag(tagId: number) {
  selectedFilterTags.value = selectedFilterTags.value.filter((tag) => tag.id !== tagId)
  void loadContentBlocks()
}

function clearFilterTags() {
  selectedFilterTags.value = []
  void loadContentBlocks()
}

onMounted(() => {
  void searchTags('')
  void loadContentBlocks()
})
</script>

<template>
  <div class="space-y-5">
    <PageHeader
      :eyebrow="t('routes.contentBlocks.eyebrow')"
      :title="t('routes.contentBlocks.title')"
      :description="t('routes.contentBlocks.description')"
    />

    <section class="grid gap-3 rounded-lg border bg-card p-3 text-card-foreground">
      <div class="flex min-w-0 flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
        <div class="min-w-0">
          <h2 class="text-sm font-semibold">{{ t('contentBlocks.tagFilterTitle') }}</h2>
          <p class="mt-1 text-xs text-muted-foreground">
            {{ t('contentBlocks.tagFilterDescription') }}
          </p>
        </div>
        <Button
          type="button"
          variant="outline"
          size="sm"
          :disabled="!selectedFilterTags.length"
          @click="clearFilterTags"
        >
          <X class="size-4" aria-hidden="true" />
          {{ t('contentBlocks.clearTagFilter') }}
        </Button>
      </div>

      <div class="flex min-h-9 flex-wrap items-center gap-2 rounded-md border bg-background px-2 py-1">
        <TagBadge
          v-for="tag in selectedFilterTags"
          :key="tag.id"
          :tag="tag"
          removable
          @remove="removeFilterTag"
        />
        <span v-if="!selectedFilterTags.length" class="text-sm text-muted-foreground">
          {{ t('contentBlocks.noTagFilter') }}
        </span>
      </div>

      <label class="grid gap-1">
        <span class="text-xs font-medium text-muted-foreground">
          {{ t('tag.search') }}
        </span>
        <span class="flex items-center gap-2 rounded-md border bg-background px-2 focus-within:ring-2 focus-within:ring-ring">
          <Search class="size-4 text-muted-foreground" aria-hidden="true" />
          <input
            :value="tagKeyword"
            class="h-9 min-w-0 flex-1 bg-transparent text-sm outline-none placeholder:text-muted-foreground"
            :placeholder="t('tag.searchPlaceholder')"
            @input="searchTags(($event.target as HTMLInputElement).value)"
          />
        </span>
      </label>

      <div class="flex min-h-9 flex-wrap gap-2">
        <span v-if="loadingTags" class="text-sm text-muted-foreground">
          {{ t('tag.loading') }}
        </span>
        <template v-else>
          <button
            v-for="tag in visibleTagSearchResults"
            :key="tag.id"
            type="button"
            class="rounded-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            @click="addFilterTag(tag)"
          >
            <TagBadge :tag="tag" />
          </button>
        </template>
      </div>
    </section>

    <div v-if="errorMessage" class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
      {{ errorMessage }}
    </div>

    <EmptyState
      v-if="!loadingBlocks && !contentBlocks.length"
      :title="t('routes.contentBlocks.emptyTitle')"
      :description="t('routes.contentBlocks.emptyDescription')"
    />

    <div v-else class="grid grid-cols-[minmax(0,1fr)] gap-3 xl:grid-cols-2">
      <div v-if="loadingBlocks" class="rounded-md border bg-muted/30 px-3 py-4 text-sm text-muted-foreground">
        {{ t('contentBlocks.loading') }}
      </div>
      <template v-else>
        <ContentBlockCard
          v-for="block in contentBlocks"
          :key="block.id"
          :block="block"
        />
      </template>
    </div>
  </div>
</template>
