<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import AtomicSectionBlock from '@/components/business/AtomicSectionBlock.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { mockAtomicSectionPanelBlock } from '@/mocks'

const { t } = useI18n()
const feedback = ref('')

function setFeedback(message: string) {
  feedback.value = message
}
</script>

<template>
  <main class="min-h-screen bg-background px-4 py-6 text-foreground sm:px-6 lg:px-8">
    <PageHeader
      :eyebrow="t('lab.eyebrow')"
      :title="t('lab.sections.atomicSectionPanel.title')"
      :description="t('lab.sections.atomicSectionPanel.description')"
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

    <section class="mt-6 rounded-lg border bg-card p-4 text-card-foreground">
      <AtomicSectionBlock
        :block="mockAtomicSectionPanelBlock"
        @select="setFeedback(t('lab.sections.atomicSectionPanel.events.selectAtomicSection', { value: $event }))"
        @create-atomic-section-panel="setFeedback(t('lab.sections.atomicSectionPanel.events.createPanel', { value: $event.title }))"
        @select-atomic-section-panel="setFeedback(t('lab.sections.atomicSectionPanel.events.selectPanel', { value: $event.title }))"
        @rename-atomic-section-panel="setFeedback(t('lab.sections.atomicSectionPanel.events.renamePanel', { value: $event.title }))"
        @move-atomic-section-panel="setFeedback(t('lab.sections.atomicSectionPanel.events.movePanel', { value: $event.title, direction: $event.direction }))"
        @remove-atomic-section-panel="setFeedback(t('lab.sections.atomicSectionPanel.events.removePanel', { value: $event.title }))"
        @select-content-block="setFeedback(t('lab.sections.atomicSectionPanel.events.selectContentBlock', { value: $event }))"
        @request-insert="setFeedback(t('lab.sections.atomicSectionPanel.events.insertPoint', { value: $event.insertPointId }))"
        @open-atomic-section-item-word="setFeedback(t('lab.sections.atomicSectionPanel.events.wordEdit', { value: $event.title }))"
        @move-atomic-section-item="setFeedback(t('lab.sections.atomicSectionPanel.events.moveItem', { value: $event.title, direction: $event.direction }))"
        @remove-atomic-section-item="setFeedback(t('lab.sections.atomicSectionPanel.events.removeItem', { value: $event.title }))"
      />
    </section>

    <aside class="mt-4 rounded-lg border bg-muted/20 px-4 py-3 text-sm text-muted-foreground" aria-live="polite">
      <span class="font-medium text-foreground">{{ t('lab.sections.atomicSectionPanel.feedbackTitle') }}</span>
      <span class="ml-2">{{ feedback || t('lab.sections.atomicSectionPanel.emptyFeedback') }}</span>
    </aside>
  </main>
</template>
