<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import GeneratedFileRow from '@/components/business/GeneratedFileRow.vue'
import OutputFormCard from '@/components/business/OutputFormCard.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import type { GeneratedFileRowModel, OutputFormCardModel } from '@/types'

defineProps<{
  outputForms: OutputFormCardModel[]
  generatedFiles: GeneratedFileRowModel[]
}>()

defineEmits<{
  generateWord: [id: number]
  downloadGeneratedFile: [id: number]
  viewManifest: [id: number]
}>()

const { t } = useI18n()
</script>

<template>
  <section class="space-y-4 rounded-lg border bg-background p-4">
    <div>
      <h2 class="text-sm font-semibold">{{ t('components.handoutOutput.title') }}</h2>
      <p class="mt-1 text-xs text-muted-foreground">
        {{ t('components.handoutOutput.description') }}
      </p>
    </div>

    <div class="space-y-2">
      <h3 class="text-xs font-medium text-muted-foreground">
        {{ t('components.handoutOutput.outputForms') }}
      </h3>
      <OutputFormCard
        v-for="outputForm in outputForms"
        :key="outputForm.id"
        :output-form="outputForm"
        @generate-word="$emit('generateWord', $event)"
      />
      <EmptyState
        v-if="!outputForms.length"
        :title="t('components.handoutOutput.emptyOutputFormsTitle')"
        :description="t('components.handoutOutput.emptyOutputFormsDescription')"
      />
    </div>

    <div class="space-y-2">
      <h3 class="text-xs font-medium text-muted-foreground">
        {{ t('components.handoutOutput.generatedFiles') }}
      </h3>
      <GeneratedFileRow
        v-for="file in generatedFiles"
        :key="file.id"
        :file="file"
        @download="$emit('downloadGeneratedFile', $event)"
        @view-manifest="$emit('viewManifest', $event)"
      />
      <EmptyState
        v-if="!generatedFiles.length"
        :title="t('components.handoutOutput.emptyGeneratedFilesTitle')"
        :description="t('components.handoutOutput.emptyGeneratedFilesDescription')"
      />
    </div>
  </section>
</template>
