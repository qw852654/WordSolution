<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { CMS_V2_API_BASE } from '@/apis/cmsV2Client'
import NavItem from '@/components/presentation/NavItem.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { createAppNavigation } from '@/app/navigation'

const route = useRoute()
const { t } = useI18n()

const navItems = computed(() => createAppNavigation(t))

function isActive(to: string) {
  if (to === '/') {
    return route.path === '/'
  }

  const root = `/${to.split('/').filter(Boolean)[0]}`
  return route.path === to || route.path.startsWith(`${root}/`) || route.path === root
}
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
