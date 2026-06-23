<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { AlertCircle } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import HandoutDetailPanel from '@/components/business/HandoutDetailPanel.vue'
import HandoutListPanel from '@/components/business/HandoutListPanel.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import { cmsV2Api } from '@/apis/cmsV2Client'
import type { CmsV2HandoutDto, CmsV2HandoutVersionDto } from '@/apis/cmsV2Client'

const router = useRouter()
const { t } = useI18n()

const handouts = ref<CmsV2HandoutDto[]>([])
const versions = ref<CmsV2HandoutVersionDto[]>([])
const selectedHandoutId = ref<number | null>(null)
const isLoadingHandouts = ref(false)
const isLoadingVersions = ref(false)
const operationPending = ref(false)
const errorMessage = ref('')
const feedback = ref('')

const visibleHandouts = computed(() =>
  handouts.value
    .filter((handout) => handout.status !== 'Archived')
    .sort((left, right) => left.title.localeCompare(right.title)),
)
const selectedHandout = computed(
  () => handouts.value.find((handout) => handout.id === selectedHandoutId.value) ?? null,
)

watch(
  () => selectedHandoutId.value,
  async (handoutId) => {
    if (handoutId) {
      await loadVersions(handoutId)
    } else {
      versions.value = []
    }
  },
)

onMounted(async () => {
  await loadHandouts()
})

async function loadHandouts(preferredHandoutId?: number) {
  isLoadingHandouts.value = true
  errorMessage.value = ''

  try {
    handouts.value = await cmsV2Api.listHandouts()
    const visible = visibleHandouts.value
    selectedHandoutId.value =
      preferredHandoutId && visible.some((handout) => handout.id === preferredHandoutId)
        ? preferredHandoutId
        : visible[0]?.id ?? null
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : (t('handoutManagement.loadFailed') as string)
  } finally {
    isLoadingHandouts.value = false
  }
}

async function loadVersions(handoutId: number) {
  isLoadingVersions.value = true
  try {
    versions.value = (await cmsV2Api.listHandoutVersions(handoutId)).sort(
      (left, right) => left.sortOrder - right.sortOrder || left.id - right.id,
    )
  } catch (error) {
    versions.value = []
    errorMessage.value =
      error instanceof Error ? error.message : (t('handoutManagement.versions.loadFailed') as string)
  } finally {
    isLoadingVersions.value = false
  }
}

async function withOperation(action: () => Promise<void>) {
  if (operationPending.value) {
    return
  }

  operationPending.value = true
  errorMessage.value = ''
  feedback.value = ''

  try {
    await action()
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : (t('handoutManagement.operationFailed') as string)
  } finally {
    operationPending.value = false
  }
}

async function handleCreateHandout() {
  const title = window.prompt(t('handoutManagement.prompt.handoutTitle') as string, '')
  if (title === null) {
    return
  }

  const normalizedTitle = title.trim()
  if (!normalizedTitle) {
    feedback.value = t('handoutManagement.prompt.titleRequired') as string
    return
  }

  const description = window.prompt(t('handoutManagement.prompt.description') as string, '')
  if (description === null) {
    return
  }

  await withOperation(async () => {
    const result = await cmsV2Api.createHandout({
      title: normalizedTitle,
      description: description.trim() || null,
      status: 'Draft',
    })
    await loadHandouts(result.id)
    feedback.value = t('handoutManagement.feedback.handoutCreated', { title: normalizedTitle }) as string
  })
}

async function handleEditHandout() {
  if (!selectedHandout.value) {
    return
  }

  const title = window.prompt(
    t('handoutManagement.prompt.handoutTitle') as string,
    selectedHandout.value.title,
  )
  if (title === null) {
    return
  }

  const normalizedTitle = title.trim()
  if (!normalizedTitle) {
    feedback.value = t('handoutManagement.prompt.titleRequired') as string
    return
  }

  const description = window.prompt(
    t('handoutManagement.prompt.description') as string,
    selectedHandout.value.description ?? '',
  )
  if (description === null) {
    return
  }

  const currentId = selectedHandout.value.id
  await withOperation(async () => {
    await cmsV2Api.updateHandout(currentId, {
      title: normalizedTitle,
      description: description.trim() || null,
      status: selectedHandout.value?.status ?? 'Draft',
    })
    await loadHandouts(currentId)
    feedback.value = t('handoutManagement.feedback.handoutUpdated', { title: normalizedTitle }) as string
  })
}

async function handleArchiveHandout() {
  if (!selectedHandout.value) {
    return
  }

  const handout = selectedHandout.value
  if (!window.confirm(t('handoutManagement.prompt.archiveHandoutConfirm', { title: handout.title }) as string)) {
    return
  }

  await withOperation(async () => {
    await cmsV2Api.updateHandout(handout.id, {
      title: handout.title,
      description: handout.description ?? null,
      status: 'Archived',
    })
    await loadHandouts()
    feedback.value = t('handoutManagement.feedback.handoutArchived', { title: handout.title }) as string
  })
}

async function handleCreateVersion() {
  if (!selectedHandout.value) {
    return
  }

  const title = window.prompt(t('handoutManagement.prompt.versionTitle') as string, '')
  if (title === null) {
    return
  }

  const normalizedTitle = title.trim()
  if (!normalizedTitle) {
    feedback.value = t('handoutManagement.prompt.titleRequired') as string
    return
  }

  const description = window.prompt(t('handoutManagement.prompt.description') as string, '')
  if (description === null) {
    return
  }

  const handoutId = selectedHandout.value.id
  await withOperation(async () => {
    const result = await cmsV2Api.createHandoutVersion(handoutId, {
      title: normalizedTitle,
      description: description.trim() || null,
      type: 'Normal',
      status: 'Draft',
    })
    await loadHandouts(handoutId)
    await router.push(`/handouts/${result.id}`)
  })
}

async function handleEditVersion(versionId: number) {
  const version = versions.value.find((entry) => entry.id === versionId)
  if (!version) {
    return
  }

  const title = window.prompt(t('handoutManagement.prompt.versionTitle') as string, version.title)
  if (title === null) {
    return
  }

  const normalizedTitle = title.trim()
  if (!normalizedTitle) {
    feedback.value = t('handoutManagement.prompt.titleRequired') as string
    return
  }

  const description = window.prompt(t('handoutManagement.prompt.description') as string, version.description ?? '')
  if (description === null) {
    return
  }

  await withOperation(async () => {
    await cmsV2Api.updateHandoutVersion(version.id, {
      title: normalizedTitle,
      description: description.trim() || null,
      type: version.type,
      status: version.status,
      sortOrder: version.sortOrder,
    })
    await loadVersions(version.handoutId)
    feedback.value = t('handoutManagement.feedback.versionUpdated', { title: normalizedTitle }) as string
  })
}

async function handleArchiveVersion(versionId: number) {
  const version = versions.value.find((entry) => entry.id === versionId)
  if (!version) {
    return
  }

  if (!window.confirm(t('handoutManagement.prompt.archiveVersionConfirm', { title: version.title }) as string)) {
    return
  }

  await withOperation(async () => {
    await cmsV2Api.updateHandoutVersion(version.id, {
      title: version.title,
      description: version.description ?? null,
      type: version.type,
      status: 'Archived',
      sortOrder: version.sortOrder,
    })
    await loadVersions(version.handoutId)
    feedback.value = t('handoutManagement.feedback.versionArchived', { title: version.title }) as string
  })
}
</script>

<template>
  <main class="grid h-screen min-h-0 grid-cols-[320px_minmax(0,1fr)] gap-3 bg-background p-3">
    <aside class="min-h-0 rounded-lg border bg-background p-3">
      <HandoutListPanel
        :handouts="visibleHandouts"
        :selected-handout-id="selectedHandoutId"
        :loading="isLoadingHandouts || operationPending"
        @refresh="loadHandouts(selectedHandoutId ?? undefined)"
        @create-handout="handleCreateHandout"
        @select-handout="selectedHandoutId = $event"
      />
    </aside>

    <section class="min-h-0 rounded-lg border bg-background p-3">
      <div v-if="errorMessage" class="mb-3">
        <EmptyState :title="t('handoutManagement.errorTitle')" :description="errorMessage">
          <template #icon>
            <AlertCircle class="size-5" />
          </template>
        </EmptyState>
      </div>

      <p v-if="feedback" class="mb-3 rounded-md border bg-muted/20 px-3 py-2 text-xs text-muted-foreground">
        {{ feedback }}
      </p>

      <HandoutDetailPanel
        :handout="selectedHandout"
        :versions="versions"
        :loading-versions="isLoadingVersions"
        @edit-handout="handleEditHandout"
        @archive-handout="handleArchiveHandout"
        @create-version="handleCreateVersion"
        @edit-version="handleEditVersion"
        @archive-version="handleArchiveVersion"
        @open-version="router.push(`/handouts/${$event}`)"
      />
    </section>
  </main>
</template>
