<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, Plus, Rows3 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import InsertCreateOverlay from '@/components/containers/InsertCreateOverlay.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import { mockInsertCreatePanels } from '@/mocks'
import type { InsertCreatePanelModel, InsertCreateSubmitPayload } from '@/types'

const { t } = useI18n()

const activePanel = ref<InsertCreatePanelModel | null>(null)
const feedback = ref('')

function openPanel(model: InsertCreatePanelModel) {
  activePanel.value = { ...model }
  feedback.value = ''
}

function handleCancel() {
  const targetType = activePanel.value?.targetType ?? 'ContentBlock'
  feedback.value = t('lab.sections.insertCreateOverlay.cancelled', { targetType })
  activePanel.value = null
}

function handleSubmit(payload: InsertCreateSubmitPayload) {
  feedback.value = t('lab.sections.insertCreateOverlay.submitted', {
    targetType: payload.targetType,
    title: payload.title,
  })
  activePanel.value = null
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

    <section class="mt-6 space-y-4" :aria-label="t('lab.sections.insertCreateOverlay.title')">
      <div class="flex items-start gap-2">
        <Plus class="mt-0.5 size-4" aria-hidden="true" />
        <div>
          <h2 class="text-base font-semibold">{{ t('lab.sections.insertCreateOverlay.title') }}</h2>
          <p class="text-sm text-muted-foreground">
            {{ t('lab.sections.insertCreateOverlay.description') }}
          </p>
        </div>
      </div>

      <div class="flex flex-wrap gap-2">
        <Button type="button" variant="outline" @click="openPanel(mockInsertCreatePanels.contentBlock)">
          {{ t('lab.sections.insertCreateOverlay.openContentBlock') }}
        </Button>
        <Button type="button" variant="outline" @click="openPanel(mockInsertCreatePanels.atomicSection)">
          {{ t('lab.sections.insertCreateOverlay.openAtomicSection') }}
        </Button>
        <Button type="button" variant="outline" @click="openPanel(mockInsertCreatePanels.disabled)">
          {{ t('lab.sections.insertCreateOverlay.openDisabled') }}
        </Button>
      </div>

      <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_20rem]">
        <div class="rounded-lg border bg-background p-4">
          <div class="mb-4 flex items-start gap-2">
            <Rows3 class="mt-0.5 size-4" aria-hidden="true" />
            <div>
              <h3 class="text-sm font-medium">
                {{ t('lab.sections.insertCreateOverlay.mockSectionTitle') }}
              </h3>
              <p class="text-sm text-muted-foreground">
                {{ t('lab.sections.insertCreateOverlay.mockSectionDescription') }}
              </p>
            </div>
          </div>

          <div class="grid min-h-[28rem] gap-3 lg:grid-cols-[14rem_minmax(0,1fr)_14rem]">
            <aside class="rounded-md border bg-muted/20 p-3">
              <div class="h-3 w-24 rounded-sm bg-muted" />
              <div class="mt-4 space-y-2">
                <div class="h-7 rounded-md border bg-background" />
                <div class="h-7 rounded-md border bg-background" />
                <div class="h-7 rounded-md border bg-background" />
              </div>
            </aside>

            <div class="rounded-md border bg-background p-3">
              <div class="h-4 w-40 rounded-sm bg-muted" />
              <div class="mt-4 space-y-0">
                <div class="min-h-24 border border-dashed p-3">
                  <div class="h-3 w-28 rounded-sm bg-muted" />
                  <div class="mt-4 h-3 w-full rounded-sm bg-muted" />
                  <div class="mt-2 h-3 w-3/4 rounded-sm bg-muted" />
                </div>
                <div class="flex h-8 items-center justify-center border-x border-dashed text-xs text-muted-foreground">
                  {{ t('lab.sections.insertCreateOverlay.mockInsertPointLabel') }}
                </div>
                <div class="min-h-28 border border-dashed p-3">
                  <div class="h-3 w-32 rounded-sm bg-muted" />
                  <div class="mt-4 h-3 w-full rounded-sm bg-muted" />
                  <div class="mt-2 h-3 w-2/3 rounded-sm bg-muted" />
                </div>
              </div>
            </div>

            <aside class="rounded-md border bg-muted/20 p-3">
              <div class="h-3 w-24 rounded-sm bg-muted" />
              <div class="mt-4 space-y-2">
                <div class="h-6 rounded-md border bg-background" />
                <div class="h-6 rounded-md border bg-background" />
                <div class="h-6 rounded-md border bg-background" />
              </div>
            </aside>
          </div>
        </div>

        <aside class="rounded-lg border bg-background p-4" aria-live="polite">
          <h3 class="text-sm font-medium">{{ t('lab.sections.insertCreateOverlay.feedbackTitle') }}</h3>
          <p class="mt-2 text-sm text-muted-foreground">
            {{ feedback || t('lab.sections.insertCreateOverlay.emptyFeedback') }}
          </p>
        </aside>
      </div>
    </section>

    <InsertCreateOverlay
      v-if="activePanel"
      :model="activePanel"
      :open="activePanel !== null"
      @cancel="handleCancel"
      @submit="handleSubmit"
    />
  </main>
</template>
