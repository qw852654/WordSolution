<script setup lang="ts">
import { RefreshCw, Save, Undo2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'

const { t } = useI18n()
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
      <Button type="button" size="sm" class="h-8 px-2 text-xs" disabled>
        <Save class="size-3.5" />
        {{ t('sectionPage.toolbar.save') }}
      </Button>
    </div>
  </header>
</template>
