<script setup lang="ts">
import { computed } from 'vue'
import { PanelsTopLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import AtomicSectionBlock from '@/components/business/AtomicSectionBlock.vue'
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import SectionItemView from '@/components/business/SectionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import { Card } from '@/components/ui/card'
import type {
  ContentBlockDisplayModel,
  SectionPageShellModel,
  StructuredBlockModel,
} from '@/types'

const { t } = useI18n()

const props = withDefaults(
  defineProps<{
    section: SectionPageShellModel
    contentBlocks?: ContentBlockDisplayModel[]
    structuredBlocks?: StructuredBlockModel[]
    teachingNoteMode?: boolean
  }>(),
  {
    contentBlocks: () => [],
    structuredBlocks: () => [],
    teachingNoteMode: false,
  },
)

type WorkspaceFlowItem =
  | {
      kind: 'ContentBlock'
      id: string
      selected?: boolean
      disabled?: boolean
      block: ContentBlockDisplayModel
    }
  | {
      kind: 'AtomicSection' | 'CompositeBlock'
      id: string
      selected?: boolean
      disabled?: boolean
      block: StructuredBlockModel
    }

const flowItems = computed<WorkspaceFlowItem[]>(() => {
  const [firstContentBlock, ...remainingContentBlocks] = props.contentBlocks
  const items: WorkspaceFlowItem[] = []

  if (firstContentBlock) {
    items.push({
      kind: 'ContentBlock',
      id: firstContentBlock.id,
      selected: firstContentBlock.selected,
      disabled: firstContentBlock.disabled,
      block: firstContentBlock,
    })
  }

  for (const block of props.structuredBlocks) {
    items.push({
      kind: block.blockKind,
      id: block.id,
      selected: block.selected,
      disabled: block.disabled,
      block,
    })
  }

  for (const block of remainingContentBlocks) {
    items.push({
      kind: 'ContentBlock',
      id: block.id,
      selected: block.selected,
      disabled: block.disabled,
      block,
    })
  }

  return items
})
</script>

<template>
  <Card class="flex h-full min-h-0 flex-col overflow-hidden">
    <div class="flex min-h-10 flex-wrap items-center gap-x-3 gap-y-1 border-b px-3 py-2 text-xs">
      <span class="font-medium">{{ section.title }}</span>
      <StatusPill :label="section.status" tone="active" />
      <span class="text-muted-foreground">{{ t('sectionPage.meta.sectionId') }}: {{ section.sectionId }}</span>
      <span class="text-muted-foreground">{{ t('sectionPage.meta.teachingTopic') }}: {{ section.teachingTopicTitle }}</span>
    </div>

    <div
      class="grid min-h-0 flex-1 gap-3 p-3"
      :class="teachingNoteMode ? 'lg:grid-cols-[minmax(0,1fr)_260px]' : 'grid-cols-[minmax(0,1fr)]'"
    >
      <WeakScrollArea class="rounded-md border bg-background p-3" :aria-label="t('sectionPage.workspace.mainColumnLabel')">
        <div v-if="flowItems.length" class="space-y-0">
          <SectionItemView
            v-for="item in flowItems"
            :key="item.id"
            :item-id="item.id"
            :selected="item.selected"
            :disabled="item.disabled"
          >
            <ContentBlockDisplay
              v-if="item.kind === 'ContentBlock'"
              :block="item.block"
            />
            <AtomicSectionBlock
              v-else-if="item.kind === 'AtomicSection'"
              :block="item.block"
            />
            <CompositeBlock
              v-else
              :block="item.block"
            />
          </SectionItemView>
        </div>

        <EmptyState
          v-else
          :title="t('sectionPage.workspace.emptyTitle')"
          :description="t('sectionPage.workspace.emptyDescription')"
        >
          <template #icon>
            <PanelsTopLeft class="size-5" aria-hidden="true" />
          </template>
        </EmptyState>
      </WeakScrollArea>

      <WeakScrollArea
        v-if="teachingNoteMode"
        class="rounded-md border bg-muted/20 p-3"
        :aria-label="t('sectionPage.workspace.teachingNoteColumnLabel')"
      >
        <p class="text-sm font-medium">{{ t('sectionPage.workspace.teachingNoteColumnLabel') }}</p>
        <p class="mt-1 text-sm leading-6 text-muted-foreground">
          {{ t('sectionPage.workspace.teachingNoteColumnDescription') }}
        </p>
      </WeakScrollArea>
    </div>
  </Card>
</template>
