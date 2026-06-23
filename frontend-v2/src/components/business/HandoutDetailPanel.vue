<script setup lang="ts">
import { Archive, Edit3, ExternalLink, Plus } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import EmptyState from '@/components/presentation/EmptyState.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import type { CmsV2HandoutDto, CmsV2HandoutVersionDto } from '@/apis/cmsV2Client'

defineProps<{
  handout: CmsV2HandoutDto | null
  versions: CmsV2HandoutVersionDto[]
  loadingVersions?: boolean
}>()

const emit = defineEmits<{
  editHandout: []
  archiveHandout: []
  createVersion: []
  editVersion: [versionId: number]
  archiveVersion: [versionId: number]
  openVersion: [versionId: number]
}>()

const { t } = useI18n()
</script>

<template>
  <section class="flex h-full min-h-0 flex-col gap-4" :aria-label="t('handoutManagement.detail.title')">
    <EmptyState
      v-if="!handout"
      :title="t('handoutManagement.detail.emptyTitle')"
      :description="t('handoutManagement.detail.emptyDescription')"
    />

    <template v-else>
      <header class="rounded-lg border bg-background p-4">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0">
            <div class="flex min-w-0 items-center gap-2">
              <h2 class="truncate text-base font-semibold">{{ handout.title }}</h2>
              <StatusPill :label="handout.status" tone="neutral" />
            </div>
            <p class="mt-2 text-sm text-muted-foreground">
              {{ handout.description || t('handoutManagement.detail.noDescription') }}
            </p>
            <p class="mt-2 text-xs text-muted-foreground">
              {{ t('handoutManagement.detail.updatedTime', { value: handout.updatedTime }) }}
            </p>
          </div>
          <div class="flex shrink-0 items-center gap-2">
            <Button type="button" size="sm" variant="outline" @click="emit('editHandout')">
              <Edit3 class="size-4" />
              {{ t('handoutManagement.actions.edit') }}
            </Button>
            <Button
              type="button"
              size="sm"
              variant="outline"
              :disabled="handout.status === 'Archived'"
              @click="emit('archiveHandout')"
            >
              <Archive class="size-4" />
              {{ t('handoutManagement.actions.archive') }}
            </Button>
          </div>
        </div>
      </header>

      <section class="flex min-h-0 flex-1 flex-col gap-3 rounded-lg border bg-background p-4">
        <div class="flex items-center justify-between gap-3">
          <div class="min-w-0">
            <h3 class="truncate text-sm font-semibold">
              {{ t('handoutManagement.versions.title') }}
            </h3>
            <p class="truncate text-xs text-muted-foreground">
              {{ t('handoutManagement.versions.description', { count: versions.length }) }}
            </p>
          </div>
          <Button
            type="button"
            size="sm"
            variant="outline"
            :disabled="handout.status === 'Archived'"
            @click="emit('createVersion')"
          >
            <Plus class="size-4" />
            {{ t('handoutManagement.actions.createVersion') }}
          </Button>
        </div>

        <div class="min-h-0 flex-1 overflow-y-auto">
          <EmptyState
            v-if="!versions.length"
            :title="t('handoutManagement.versions.emptyTitle')"
            :description="t('handoutManagement.versions.emptyDescription')"
          />

          <div v-else class="space-y-2">
            <article
              v-for="version in versions"
              :key="version.id"
              class="rounded-md border bg-background px-3 py-2"
            >
              <div class="flex items-start justify-between gap-3">
                <div class="min-w-0">
                  <div class="flex min-w-0 items-center gap-2">
                    <h4 class="truncate text-sm font-medium">{{ version.title }}</h4>
                    <StatusPill :label="version.status" tone="neutral" />
                  </div>
                  <p class="mt-1 truncate text-xs text-muted-foreground">
                    {{ version.type }} · {{ t('handoutManagement.versions.sortOrder', { value: version.sortOrder }) }}
                  </p>
                  <p class="mt-1 line-clamp-2 text-xs text-muted-foreground">
                    {{ version.description || t('handoutManagement.detail.noDescription') }}
                  </p>
                </div>
                <div class="flex shrink-0 items-center gap-1">
                  <Button type="button" size="icon" variant="ghost" @click="emit('openVersion', version.id)">
                    <ExternalLink class="size-4" />
                    <span class="sr-only">{{ t('handoutManagement.actions.openVersion') }}</span>
                  </Button>
                  <Button type="button" size="icon" variant="ghost" @click="emit('editVersion', version.id)">
                    <Edit3 class="size-4" />
                    <span class="sr-only">{{ t('handoutManagement.actions.editVersion') }}</span>
                  </Button>
                  <Button
                    type="button"
                    size="icon"
                    variant="ghost"
                    :disabled="version.status === 'Archived'"
                    @click="emit('archiveVersion', version.id)"
                  >
                    <Archive class="size-4" />
                    <span class="sr-only">{{ t('handoutManagement.actions.archiveVersion') }}</span>
                  </Button>
                </div>
              </div>
            </article>
          </div>
        </div>

        <p v-if="loadingVersions" class="text-xs text-muted-foreground">
          {{ t('handoutManagement.versions.loading') }}
        </p>
      </section>
    </template>
  </section>
</template>
