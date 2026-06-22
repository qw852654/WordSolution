<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import SectionVariantCreatePanel from '@/components/business/SectionVariantCreatePanel.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { Button } from '@/components/ui/button'
import {
  mockSectionVariantCreateMetadata,
  mockSectionVariantSelectionCandidates,
} from '@/mocks'
import type { SectionVariantCreateSubmitPayload } from '@/types'

const { t } = useI18n()

const submittedPayload = ref<SectionVariantCreateSubmitPayload | null>(null)
const panelKey = ref(0)
const payloadText = computed(() =>
  submittedPayload.value ? JSON.stringify(submittedPayload.value, null, 2) : '',
)

function handleSubmit(payload: SectionVariantCreateSubmitPayload) {
  submittedPayload.value = payload
}

function handleCancel() {
  submittedPayload.value = null
  panelKey.value += 1
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

    <section class="mt-6 grid gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(18rem,24rem)]">
      <div class="space-y-4">
        <div class="rounded-lg border bg-background p-4">
          <h2 class="text-base font-semibold">SectionVariantCreatePanel</h2>
          <p class="mt-1 text-sm text-muted-foreground">
            {{ t('lab.sections.sectionVariantCreate.description') }}
          </p>
        </div>

        <SectionVariantCreatePanel
          :key="panelKey"
          :initial-metadata="mockSectionVariantCreateMetadata"
          :candidates="mockSectionVariantSelectionCandidates"
          :section-title="t('lab.sections.sectionVariantCreate.mockSectionTitle')"
          @submit="handleSubmit"
          @cancel="handleCancel"
        />
      </div>

      <aside class="rounded-lg border bg-background p-4" aria-live="polite">
        <div class="flex items-start justify-between gap-3">
          <div>
            <h2 class="text-sm font-semibold">
              {{ t('lab.sections.sectionVariantCreate.payloadTitle') }}
            </h2>
            <p class="mt-1 text-sm text-muted-foreground">
              {{ t('lab.sections.sectionVariantCreate.payloadDescription') }}
            </p>
          </div>
          <Button type="button" variant="outline" size="sm" @click="handleCancel">
            {{ t('lab.sections.sectionVariantCreate.reset') }}
          </Button>
        </div>

        <pre
          v-if="submittedPayload"
          class="mt-4 max-h-[28rem] overflow-auto rounded-md border bg-muted/30 p-3 text-xs text-foreground"
        >{{ payloadText }}</pre>
        <p v-else class="mt-4 text-sm text-muted-foreground">
          {{ t('lab.sections.sectionVariantCreate.emptyPayload') }}
        </p>
      </aside>
    </section>
  </main>
</template>
