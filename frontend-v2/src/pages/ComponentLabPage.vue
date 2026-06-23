<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import HandoutInspector from '@/components/business/HandoutInspector.vue'
import HandoutOutputPanel from '@/components/business/HandoutOutputPanel.vue'
import HandoutStructurePanel from '@/components/business/HandoutStructurePanel.vue'
import HandoutWorkspace from '@/components/business/HandoutWorkspace.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import {
  mockGeneratedFiles,
  mockHandoutInspector,
  mockHandoutTreeNodes,
  mockHandoutWorkspaceItems,
  mockOutputForms,
} from '@/mocks'
import type { HandoutInspectorModel } from '@/types'

const { t } = useI18n()

const selectedNodeId = ref('handout-item:102')
const feedback = ref('')

const selectedInspector = computed<HandoutInspectorModel>(() => {
  if (selectedNodeId.value === 'handout-item:102') {
    return mockHandoutInspector
  }

  return {
    nodeId: selectedNodeId.value,
    title: selectedNodeId.value,
    kind: 'Derived',
    description: t('lab.sections.handoutPage.mockInspectorFallback'),
    fields: [
      { label: 'NodeId', value: selectedNodeId.value },
      { label: 'Mode', value: 'Mock Data' },
    ],
  }
})

function setFeedback(message: string) {
  feedback.value = message
}
</script>

<template>
  <main class="min-h-screen bg-background px-4 py-6 text-foreground sm:px-6 lg:px-8">
    <PageHeader
      :eyebrow="t('lab.eyebrow')"
      :title="t('lab.sections.handoutPage.title')"
      :description="t('lab.sections.handoutPage.description')"
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

    <section class="mt-6 grid min-h-[42rem] gap-4 xl:grid-cols-[18rem_minmax(0,1fr)_24rem]">
      <HandoutStructurePanel
        class="min-h-0"
        :nodes="mockHandoutTreeNodes"
        :selected-node-id="selectedNodeId"
        @select-node="selectedNodeId = $event"
        @add-to-end="setFeedback(t('lab.sections.handoutPage.feedback.addToEnd'))"
        @node-context-menu="setFeedback(t('lab.sections.handoutPage.feedback.contextMenu', { node: $event.node.title }))"
      />

      <HandoutWorkspace
        :items="mockHandoutWorkspaceItems"
        @select-item="selectedNodeId = mockHandoutWorkspaceItems.find((item) => item.id === $event)?.nodeId ?? selectedNodeId"
        @move-up="setFeedback(t('lab.sections.handoutPage.feedback.moveUp', { id: $event }))"
        @move-down="setFeedback(t('lab.sections.handoutPage.feedback.moveDown', { id: $event }))"
        @edit-occurrence="setFeedback(t('lab.sections.handoutPage.feedback.editOccurrence', { id: $event }))"
        @remove="setFeedback(t('lab.sections.handoutPage.feedback.remove', { id: $event }))"
      />

      <div class="min-h-0 space-y-4">
        <HandoutInspector :model="selectedInspector" />
        <HandoutOutputPanel
          :output-forms="mockOutputForms"
          :generated-files="mockGeneratedFiles"
          @generate-word="setFeedback(t('lab.sections.handoutPage.feedback.generateWord', { id: $event }))"
          @download-generated-file="setFeedback(t('lab.sections.handoutPage.feedback.download', { id: $event }))"
          @view-manifest="setFeedback(t('lab.sections.handoutPage.feedback.manifest', { id: $event }))"
        />
      </div>
    </section>

    <aside class="mt-4 rounded-lg border bg-muted/20 px-4 py-3 text-sm text-muted-foreground" aria-live="polite">
      <span class="font-medium text-foreground">{{ t('lab.sections.handoutPage.feedbackTitle') }}</span>
      <span class="ml-2">{{ feedback || t('lab.sections.handoutPage.emptyFeedback') }}</span>
    </aside>
  </main>
</template>
