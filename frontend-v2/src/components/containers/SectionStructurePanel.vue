<script setup lang="ts">
import { ListTree } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import WeakScrollArea from '@/components/presentation/WeakScrollArea.vue'
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

const { t } = useI18n()
</script>

<template>
  <Card class="flex h-full min-h-0 flex-col overflow-hidden">
    <CardHeader class="gap-2 px-4 py-3">
      <CardTitle class="text-base">{{ t('sectionPage.structure.title') }}</CardTitle>
      <CardDescription class="text-xs">{{ t('sectionPage.structure.description') }}</CardDescription>
    </CardHeader>
    <WeakScrollArea class="px-4 pb-4">
      <EmptyState
        :title="t('sectionPage.structure.emptyTitle')"
        :description="t('sectionPage.structure.emptyDescription')"
      >
        <template #icon>
          <ListTree class="size-5" aria-hidden="true" />
        </template>
      </EmptyState>
    </WeakScrollArea>
  </Card>
</template>
