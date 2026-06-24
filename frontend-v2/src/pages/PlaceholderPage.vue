<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import { Braces } from 'lucide-vue-next'
import EmptyState from '@/components/presentation/EmptyState.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { usePageTitle } from '@/composables/usePageTitle'

const props = defineProps<{
  eyebrowKey: string
  titleKey: string
  descriptionKey: string
  emptyTitleKey: string
  emptyDescriptionKey: string
  paramName?: string
}>()

const route = useRoute()
const { t } = useI18n()
usePageTitle(() => t(props.titleKey))

const routeParam = computed(() => {
  if (!props.paramName) {
    return undefined
  }

  const value = route.params[props.paramName]
  return Array.isArray(value) ? value.join('/') : value
})
</script>

<template>
  <div class="space-y-6">
    <PageHeader
      :eyebrow="t(eyebrowKey)"
      :title="t(titleKey)"
      :description="t(descriptionKey)"
    />

    <div class="grid grid-cols-[minmax(0,1fr)] gap-4 xl:grid-cols-[minmax(0,1fr)_320px]">
      <EmptyState
        :title="t(emptyTitleKey)"
        :description="t(emptyDescriptionKey)"
      >
        <template #icon>
          <Braces class="size-5" aria-hidden="true" />
        </template>
      </EmptyState>

      <Card>
        <CardHeader>
          <CardTitle>{{ t('routes.placeholder.contextTitle') }}</CardTitle>
          <CardDescription>{{ t('routes.placeholder.contextDescription') }}</CardDescription>
        </CardHeader>
        <CardContent class="space-y-3 text-sm">
          <div class="rounded-lg border bg-muted/30 px-3 py-2">
            <p class="text-xs text-muted-foreground">
              {{ t('routes.placeholder.pathLabel') }}
            </p>
            <p class="mt-1 break-all font-mono">{{ route.path }}</p>
          </div>
          <div v-if="routeParam" class="rounded-lg border bg-muted/30 px-3 py-2">
            <p class="text-xs text-muted-foreground">
              {{ t('routes.placeholder.paramLabel') }}
            </p>
            <p class="mt-1 break-all font-mono">{{ routeParam }}</p>
          </div>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
