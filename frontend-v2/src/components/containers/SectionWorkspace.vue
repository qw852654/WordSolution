<script setup lang="ts">
import { PanelsTopLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import SectionItemView from '@/components/business/SectionItemView.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import { Card } from '@/components/ui/card'
import type { SectionItemViewShellModel, SectionPageShellModel } from '@/types'

const { t } = useI18n()

withDefaults(
  defineProps<{
    section: SectionPageShellModel
    items?: SectionItemViewShellModel[]
    teachingNoteMode?: boolean
  }>(),
  {
    items: () => [],
    teachingNoteMode: false,
  },
)
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
        <div v-if="items.length" class="space-y-2">
          <SectionItemView
            v-for="item in items"
            :key="item.id"
            :item-id="item.id"
            :selected="item.selected"
            :disabled="item.disabled"
          >
            <div class="rounded-md border border-dashed bg-muted/20 px-3 py-2">
              <p class="text-sm font-medium">{{ t(item.placeholderTitleKey) }}</p>
              <p class="mt-1 text-sm leading-6 text-muted-foreground">
                {{ t(item.placeholderDescriptionKey) }}
              </p>
            </div>
            <div v-if="item.children?.length" class="mt-2 space-y-2 border-l pl-3">
              <SectionItemView
                v-for="child in item.children"
                :key="child.id"
                :item-id="child.id"
                :selected="child.selected"
                :disabled="child.disabled"
              >
                <div class="rounded-md border border-dashed bg-muted/20 px-3 py-2">
                  <p class="text-sm font-medium">{{ t(child.placeholderTitleKey) }}</p>
                  <p class="mt-1 text-sm leading-6 text-muted-foreground">
                    {{ t(child.placeholderDescriptionKey) }}
                  </p>
                </div>
              </SectionItemView>
            </div>
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
