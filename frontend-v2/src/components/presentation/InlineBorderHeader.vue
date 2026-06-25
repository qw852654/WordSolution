<script setup lang="ts">
defineProps<{
  title: string
  meta?: string
  difficultyMarkerClass?: string
  difficultyMarkerLabel?: string
}>()
</script>

<template>
  <div class="relative flex min-h-9 items-center gap-2 px-3">
    <div class="absolute inset-x-0 top-1/2 border-t" aria-hidden="true" />
    <div class="relative flex min-w-0 items-center gap-2 bg-background pr-2">
      <span
        v-if="difficultyMarkerClass"
        class="size-2 shrink-0 rounded-full"
        :class="difficultyMarkerClass"
        :aria-label="difficultyMarkerLabel"
        :title="difficultyMarkerLabel"
      />
      <span class="truncate text-sm font-medium">{{ title }}</span>
      <span v-if="meta" class="shrink-0 text-xs text-muted-foreground">{{ meta }}</span>
      <slot name="meta-extra" />
    </div>
    <div class="relative ml-auto flex shrink-0 items-center gap-1 bg-background pl-2">
      <slot name="actions" />
    </div>
  </div>
</template>
