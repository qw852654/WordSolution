<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { AppNavItem } from '@/types'

defineProps<{
  item: AppNavItem
  active?: boolean
}>()
</script>

<template>
  <RouterLink
    :to="item.to"
    class="group flex min-h-12 items-start gap-3 rounded-lg border border-transparent px-3 py-2 text-sm transition-colors hover:border-border hover:bg-muted/60 focus-visible:border-ring focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
    :class="active ? 'border-border bg-muted text-foreground' : 'text-muted-foreground'"
  >
    <component
      :is="item.icon"
      v-if="item.icon"
      class="mt-0.5 size-4 shrink-0"
      aria-hidden="true"
    />
    <span class="min-w-0">
      <span class="block truncate font-medium text-foreground">{{ item.label }}</span>
      <span v-if="item.description" class="mt-0.5 block truncate text-xs text-muted-foreground">
        {{ item.description }}
      </span>
    </span>
  </RouterLink>
</template>
