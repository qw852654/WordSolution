<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, GitBranch } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import SectionTree from '@/components/business/SectionTree.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { emptySectionTreeNodes, mockSectionTreeNodes } from '@/mocks'

const { t } = useI18n()
const selectedNodeId = ref('section-tree-atomic-basics')
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

    <section class="mt-6 space-y-3" :aria-label="t('lab.sections.sectionTree.title')">
      <div class="flex items-center gap-2">
        <GitBranch class="size-4" aria-hidden="true" />
        <div>
          <h2 class="text-base font-semibold">{{ t('lab.sections.sectionTree.title') }}</h2>
          <p class="text-sm text-muted-foreground">{{ t('lab.sections.sectionTree.description') }}</p>
        </div>
      </div>

      <div class="grid gap-4 lg:grid-cols-[minmax(0,24rem)_minmax(0,1fr)]">
        <div class="space-y-2 rounded-lg border bg-background p-3">
          <SectionTree
            :nodes="mockSectionTreeNodes"
            :selected-node-id="selectedNodeId"
            @select-node="selectedNodeId = $event"
          />
        </div>

        <div class="space-y-4">
          <div class="rounded-lg border bg-muted/20 p-3">
            <h3 class="text-sm font-medium">{{ t('lab.sections.sectionTree.selectedTitle') }}</h3>
            <p class="mt-1 text-sm text-muted-foreground">
              {{ selectedNodeId }}
            </p>
          </div>

          <div class="rounded-lg border bg-background p-3">
            <SectionTree :nodes="emptySectionTreeNodes" />
          </div>
        </div>
      </div>
    </section>
  </main>
</template>
