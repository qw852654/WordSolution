<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { CMS_V2_API_BASE, cmsV2Api, type CmsV2HealthDto } from '@/apis/cmsV2Client'
import NavItem from '@/components/presentation/NavItem.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { createAppNavigation } from '@/app/navigation'

const route = useRoute()
const { t } = useI18n()

const navItems = computed(() => createAppNavigation(t))
const currentBank = ref<CmsV2HealthDto | null>(null)
const currentBankLoading = ref(true)
const currentBankUnavailable = ref(false)

const currentBankLabel = computed(() => {
  if (currentBankLoading.value) {
    return t('shell.bank.loading')
  }

  if (currentBankUnavailable.value || !currentBank.value) {
    return t('shell.bank.unavailable')
  }

  return `${currentBank.value.bankDisplayName} · ${currentBank.value.bankKey}`
})

const currentBankKindLabel = computed(() => {
  if (!currentBank.value || currentBankLoading.value || currentBankUnavailable.value) {
    return ''
  }

  if (currentBank.value.bankKind === 'Production') {
    return t('shell.bank.kind.production')
  }

  if (currentBank.value.bankKind === 'Test') {
    return t('shell.bank.kind.test')
  }

  return currentBank.value.bankKind
})

async function loadCurrentBank() {
  currentBankLoading.value = true
  currentBankUnavailable.value = false

  try {
    currentBank.value = await cmsV2Api.getHealth()
  } catch {
    currentBank.value = null
    currentBankUnavailable.value = true
  } finally {
    currentBankLoading.value = false
  }
}

function isActive(to: string) {
  if (to === '/') {
    return route.path === '/'
  }

  const root = `/${to.split('/').filter(Boolean)[0]}`
  return route.path === to || route.path.startsWith(`${root}/`) || route.path === root
}

onMounted(() => {
  void loadCurrentBank()
})
</script>

<template>
  <div class="min-h-screen bg-background text-foreground">
    <header class="border-b bg-background">
      <div class="flex min-h-14 flex-col gap-3 px-4 py-3 sm:px-6 lg:flex-row lg:items-center lg:justify-between lg:px-8">
        <div class="min-w-0">
          <p class="text-sm font-semibold">{{ t('common.appName') }}</p>
          <p class="truncate text-xs text-muted-foreground">{{ t('shell.subtitle') }}</p>
        </div>
        <div class="flex flex-wrap items-center gap-2">
          <StatusPill :label="t('shell.stage')" tone="active" />
          <span
            class="inline-flex min-h-6 max-w-full items-center gap-2 rounded-md border bg-background px-2 text-xs text-foreground"
            :class="currentBankUnavailable || currentBankLoading ? 'text-muted-foreground' : ''"
          >
            <span class="truncate">{{ currentBankLabel }}</span>
            <StatusPill
              v-if="currentBankKindLabel"
              :label="currentBankKindLabel"
              :tone="currentBank?.bankKind === 'Production' ? 'active' : 'neutral'"
            />
          </span>
          <span class="rounded-md border bg-muted px-2 py-1 font-mono text-xs text-muted-foreground">
            {{ CMS_V2_API_BASE }}
          </span>
        </div>
      </div>
    </header>

    <div class="grid min-h-[calc(100vh-57px)] lg:grid-cols-[256px_minmax(0,1fr)]">
      <aside class="border-b bg-background px-3 py-3 lg:border-b-0 lg:border-r">
        <nav :aria-label="t('shell.primaryNavigation')" class="grid gap-1">
          <NavItem
            v-for="item in navItems"
            :key="item.id"
            :item="item"
            :active="isActive(item.to)"
          />
        </nav>
      </aside>

      <main class="min-w-0 px-4 py-6 sm:px-6 lg:px-8">
        <RouterView />
      </main>
    </div>
  </div>
</template>
