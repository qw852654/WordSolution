<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import AtomicSectionBlock from '@/components/business/AtomicSectionBlock.vue'
import AtomicSectionPanelCreateOverlay from '@/components/business/AtomicSectionPanelCreateOverlay.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { mockAtomicSectionPanelBlock } from '@/mocks'
import type {
  AtomicSectionItemActionPayload,
  AtomicSectionItemMovePayload,
  AtomicSectionPanelActionPayload,
  AtomicSectionPanelCreateOverlayModel,
  AtomicSectionPanelCreateSubmitPayload,
  AtomicSectionPanelMovePayload,
  ContentBlockRelationActionPayload,
  ContentBlockRelationMovePayload,
  InsertRequestModel,
} from '@/types'

const { t } = useI18n()
const feedback = ref('')
const overlayOpen = ref(false)
const overlayModel = ref<AtomicSectionPanelCreateOverlayModel>({
  nodeId: 'lab-atomic-section',
  atomicSectionId: 101,
  atomicSectionTitle: t('lab.sections.atomicSectionPanelCreate.mockAtomicSectionTitle'),
  defaultTitle: t('lab.sections.atomicSectionPanelCreate.mockAtomicSectionTitle'),
})

function openOverlay(disabled = false) {
  overlayModel.value = {
    nodeId: disabled ? 'lab-atomic-section-disabled' : 'lab-atomic-section',
    atomicSectionId: disabled ? 102 : 101,
    atomicSectionTitle: t('lab.sections.atomicSectionPanelCreate.mockAtomicSectionTitle'),
    defaultTitle: t('lab.sections.atomicSectionPanelCreate.mockAtomicSectionTitle'),
    disabled,
  }
  overlayOpen.value = true
  feedback.value = ''
}

function cancelOverlay() {
  overlayOpen.value = false
  feedback.value = t('lab.sections.atomicSectionPanelCreate.cancelled')
}

function submitOverlay(payload: AtomicSectionPanelCreateSubmitPayload) {
  overlayOpen.value = false
  feedback.value = JSON.stringify(payload, null, 2)
}

function recordEvent(label: string, payload: unknown) {
  feedback.value = `${label}\n${JSON.stringify(payload, null, 2)}`
}

function recordPanelEvent(
  key: 'selectPanel' | 'renamePanel' | 'removePanel',
  payload: AtomicSectionPanelActionPayload,
) {
  recordEvent(t(`lab.sections.atomicSectionPanel.events.${key}`, { value: payload.title }), payload)
}

function recordPanelMoveEvent(payload: AtomicSectionPanelMovePayload) {
  recordEvent(
    t('lab.sections.atomicSectionPanel.events.movePanel', {
      value: payload.title,
      direction: payload.direction,
    }),
    payload,
  )
}

function recordAtomicSectionItemEvent(
  key: 'wordEdit' | 'removeItem',
  payload: AtomicSectionItemActionPayload,
) {
  recordEvent(t(`lab.sections.atomicSectionPanel.events.${key}`, { value: payload.title }), payload)
}

function recordAtomicSectionItemMoveEvent(payload: AtomicSectionItemMovePayload) {
  recordEvent(
    t('lab.sections.atomicSectionPanel.events.moveItem', {
      value: payload.title,
      direction: payload.direction,
    }),
    payload,
  )
}

function recordContentBlockRelationEvent(
  key: 'wordEdit' | 'removeItem',
  payload: ContentBlockRelationActionPayload,
) {
  recordEvent(t(`lab.sections.atomicSectionPanel.events.${key}`, { value: payload.title }), payload)
}

function recordContentBlockRelationMoveEvent(payload: ContentBlockRelationMovePayload) {
  recordEvent(
    t('lab.sections.atomicSectionPanel.events.moveItem', {
      value: payload.title,
      direction: payload.direction,
    }),
    payload,
  )
}

function recordInsertRequest(payload: InsertRequestModel) {
  recordEvent(t('lab.sections.atomicSectionPanel.events.insertPoint', { value: payload.insertPointId }), payload)
}
</script>

<template>
  <main class="min-h-screen bg-background px-4 py-6 text-foreground sm:px-6 lg:px-8">
    <PageHeader
      :eyebrow="t('lab.eyebrow')"
      :title="t('lab.sections.atomicSectionPanelCreate.title')"
      :description="t('lab.sections.atomicSectionPanelCreate.description')"
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

    <section class="mt-6 grid gap-4 rounded-lg border bg-card p-4 text-card-foreground">
      <div class="grid gap-1">
        <h2 class="text-base font-semibold">
          {{ t('lab.sections.atomicSectionPanel.title') }}
        </h2>
        <p class="text-sm text-muted-foreground">
          {{ t('lab.sections.atomicSectionPanel.description') }}
        </p>
      </div>

      <div class="rounded-md border bg-background p-4">
        <AtomicSectionBlock
          :block="mockAtomicSectionPanelBlock"
          @select="recordEvent(t('lab.sections.atomicSectionPanel.events.selectAtomicSection', { value: mockAtomicSectionPanelBlock.title }), { id: $event })"
          @toggle-collapse="recordEvent('toggleCollapse', { id: $event })"
          @open-more="recordEvent('openMore', { id: $event })"
          @select-content-block="recordEvent(t('lab.sections.atomicSectionPanel.events.selectContentBlock', { value: $event }), { id: $event })"
          @create-atomic-section-panel="recordEvent(t('lab.sections.atomicSectionPanel.events.createPanel', { value: $event.title }), $event)"
          @select-atomic-section-panel="recordPanelEvent('selectPanel', $event)"
          @rename-atomic-section-panel="recordPanelEvent('renamePanel', $event)"
          @move-atomic-section-panel="recordPanelMoveEvent"
          @remove-atomic-section-panel="recordPanelEvent('removePanel', $event)"
          @open-atomic-section-item-word="recordAtomicSectionItemEvent('wordEdit', $event)"
          @move-atomic-section-item="recordAtomicSectionItemMoveEvent"
          @remove-atomic-section-item="recordAtomicSectionItemEvent('removeItem', $event)"
          @open-content-block-relation-word="recordContentBlockRelationEvent('wordEdit', $event)"
          @move-content-block-relation="recordContentBlockRelationMoveEvent"
          @remove-content-block-relation="recordContentBlockRelationEvent('removeItem', $event)"
          @request-insert="recordInsertRequest"
        />
      </div>
    </section>

    <section class="mt-6 grid gap-4 rounded-lg border bg-card p-4 text-card-foreground">
      <div class="flex flex-wrap gap-2">
        <Button type="button" @click="openOverlay(false)">
          {{ t('lab.sections.atomicSectionPanelCreate.open') }}
        </Button>
        <Button type="button" variant="outline" @click="openOverlay(true)">
          {{ t('lab.sections.atomicSectionPanelCreate.openDisabled') }}
        </Button>
      </div>

      <div class="grid gap-3 rounded-md border bg-background p-4">
        <div class="flex items-center justify-between gap-3">
          <div>
            <h2 class="text-base font-semibold">
              {{ t('lab.sections.atomicSectionPanelCreate.mockPageTitle') }}
            </h2>
            <p class="text-sm text-muted-foreground">
              {{ t('lab.sections.atomicSectionPanelCreate.mockPageDescription') }}
            </p>
          </div>
          <span class="rounded-md border px-2 py-1 text-xs text-muted-foreground">
            {{ t('lab.sections.atomicSectionPanelCreate.sharedShellLabel') }}
          </span>
        </div>
        <div class="min-h-40 rounded-md border border-dashed bg-muted/20 p-4 text-sm text-muted-foreground">
          {{ t('lab.sections.atomicSectionPanelCreate.mockWorkspace') }}
        </div>
      </div>
    </section>

    <aside
      class="mt-4 whitespace-pre-wrap rounded-lg border bg-muted/20 px-4 py-3 text-sm text-muted-foreground"
      aria-live="polite"
    >
      <span class="font-medium text-foreground">
        {{ t('lab.sections.atomicSectionPanelCreate.feedbackTitle') }}
      </span>
      <span class="ml-2">
        {{ feedback || t('lab.sections.atomicSectionPanelCreate.emptyFeedback') }}
      </span>
    </aside>

    <AtomicSectionPanelCreateOverlay
      :model="overlayModel"
      :open="overlayOpen"
      @cancel="cancelOverlay"
      @submit="submitOverlay"
    />
  </main>
</template>
