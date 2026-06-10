<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { ArrowLeft, LayoutPanelLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import SectionInspector from '@/components/business/SectionInspector.vue'
import SectionItemView from '@/components/business/SectionItemView.vue'
import SectionStructurePanel from '@/components/containers/SectionStructurePanel.vue'
import SectionTopToolbar from '@/components/containers/SectionTopToolbar.vue'
import SectionWorkspace from '@/components/containers/SectionWorkspace.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { componentLabScenarios } from '@/labs'
import { mockSectionItemViewShells, mockSectionPageShells } from '@/mocks'
import type { SectionPageShellModel } from '@/types'

const { t } = useI18n()
const sectionShell: SectionPageShellModel = mockSectionPageShells[0] ?? {
  sectionId: 'demo-section',
  title: '机械能守恒',
  teachingTopicTitle: '功能关系',
  status: '骨架验收',
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

    <section class="mt-6 space-y-4" :aria-label="t('lab.sections.sectionWorkspace.title')">
      <div class="flex flex-col gap-3 rounded-lg border bg-muted/30 px-4 py-3 md:flex-row md:items-center md:justify-between">
        <div class="min-w-0">
          <h2 class="flex items-center gap-2 text-base font-semibold">
            <LayoutPanelLeft class="size-4" aria-hidden="true" />
            {{ t('lab.sections.sectionWorkspace.title') }}
          </h2>
          <p class="mt-1 text-sm text-muted-foreground">
            {{ t('lab.sections.sectionWorkspace.description') }}
          </p>
        </div>
        <p class="shrink-0 rounded-md border bg-background px-3 py-2 text-sm text-muted-foreground">
          {{ t('lab.scenarioCount') }} {{ componentLabScenarios.length }}
        </p>
      </div>

      <section class="grid min-h-[720px] grid-cols-[minmax(0,1fr)] gap-3 xl:grid-cols-[240px_minmax(0,1fr)_280px]">
        <SectionStructurePanel />
        <SectionWorkspace :section="sectionShell" :items="mockSectionItemViewShells" />

        <aside class="flex min-h-0 flex-col gap-3">
          <SectionTopToolbar />
          <SectionInspector class="min-h-0 flex-1" />
        </aside>
      </section>

      <section class="rounded-lg border bg-muted/30 p-4" :aria-label="t('lab.sections.sectionItemView.title')">
        <div class="mb-3">
          <h2 class="text-base font-semibold">{{ t('lab.sections.sectionItemView.title') }}</h2>
          <p class="mt-1 text-sm text-muted-foreground">
            {{ t('lab.sections.sectionItemView.description') }}
          </p>
        </div>
        <div class="space-y-2">
          <SectionItemView
            v-for="item in mockSectionItemViewShells"
            :key="item.id"
            :item-id="item.id"
            :selected="item.selected"
            :disabled="item.disabled"
          >
            <div class="rounded-md border border-dashed bg-background px-3 py-2">
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
                <div class="rounded-md border border-dashed bg-background px-3 py-2">
                  <p class="text-sm font-medium">{{ t(child.placeholderTitleKey) }}</p>
                  <p class="mt-1 text-sm leading-6 text-muted-foreground">
                    {{ t(child.placeholderDescriptionKey) }}
                  </p>
                </div>
              </SectionItemView>
            </div>
          </SectionItemView>
        </div>
      </section>
    </section>
  </main>
</template>
