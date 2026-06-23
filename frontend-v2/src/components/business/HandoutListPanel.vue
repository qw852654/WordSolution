<script setup lang="ts">
import { Plus, RefreshCw } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import type { CmsV2HandoutDto } from '@/apis/cmsV2Client'

const props = defineProps<{
  handouts: CmsV2HandoutDto[]
  selectedHandoutId?: number | null
  loading?: boolean
}>()

const emit = defineEmits<{
  refresh: []
  createHandout: []
  selectHandout: [handoutId: number]
}>()

const { t } = useI18n()
</script>

<template>
  <section class="flex h-full min-h-0 flex-col gap-3" :aria-label="t('handoutManagement.list.title')">
    <div class="flex items-center justify-between gap-2">
      <div class="min-w-0">
        <h1 class="truncate text-sm font-semibold">
          {{ t('handoutManagement.list.title') }}
        </h1>
        <p class="truncate text-xs text-muted-foreground">
          {{ t('handoutManagement.list.description', { count: props.handouts.length }) }}
        </p>
      </div>
      <div class="flex items-center gap-1">
        <Button type="button" size="icon" variant="ghost" :disabled="loading" @click="emit('refresh')">
          <RefreshCw class="size-4" />
          <span class="sr-only">{{ t('handoutManagement.actions.refresh') }}</span>
        </Button>
        <Button type="button" size="icon" variant="outline" @click="emit('createHandout')">
          <Plus class="size-4" />
          <span class="sr-only">{{ t('handoutManagement.actions.createHandout') }}</span>
        </Button>
      </div>
    </div>

    <div class="min-h-0 flex-1 overflow-y-auto">
      <EmptyState
        v-if="!handouts.length"
        :title="t('handoutManagement.list.emptyTitle')"
        :description="t('handoutManagement.list.emptyDescription')"
      />

      <div v-else class="space-y-2">
        <button
          v-for="handout in handouts"
          :key="handout.id"
          type="button"
          class="w-full rounded-md border bg-background px-3 py-2 text-left text-sm transition-colors hover:bg-muted/30"
          :class="handout.id === selectedHandoutId ? 'border-primary bg-muted/40' : 'border-border'"
          @click="emit('selectHandout', handout.id)"
        >
          <div class="flex min-w-0 items-center justify-between gap-2">
            <span class="truncate font-medium">{{ handout.title }}</span>
            <StatusPill :label="handout.status" tone="neutral" />
          </div>
          <p class="mt-1 line-clamp-2 text-xs text-muted-foreground">
            {{ handout.description || t('handoutManagement.list.noDescription') }}
          </p>
        </button>
      </div>
    </div>
  </section>
</template>
