<script setup lang="ts">
import { ref } from 'vue'
import { RefreshCw, Undo2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'

const { t } = useI18n()

const selectedQuestionBankId = ref('test')
const questionBanks = [
  { id: 'test', label: 'TEST' },
  { id: 'work', label: 'WORK' },
  { id: 'official', label: '正式题库' },
]
</script>

<template>
  <header
    class="rounded-lg border bg-card p-2 text-card-foreground"
    :aria-label="t('sectionPage.toolbar.areaLabel')"
  >
    <div class="flex flex-wrap items-center justify-end gap-1.5">
      <Button type="button" size="sm" variant="outline" class="h-8 px-2 text-xs" disabled>
        <Undo2 class="size-3.5" />
        {{ t('sectionPage.toolbar.backToTopic') }}
      </Button>
      <Button type="button" size="sm" variant="outline" class="h-8 px-2 text-xs" disabled>
        <RefreshCw class="size-3.5" />
        {{ t('sectionPage.toolbar.refresh') }}
      </Button>
      <select
        v-model="selectedQuestionBankId"
        class="h-8 w-24 rounded-md border border-input bg-background px-2 text-xs text-foreground outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
        :aria-label="t('sectionPage.toolbar.questionBankSelectLabel')"
      >
        <option
          v-for="questionBank in questionBanks"
          :key="questionBank.id"
          :value="questionBank.id"
        >
          {{ questionBank.label }}
        </option>
      </select>
    </div>
  </header>
</template>
