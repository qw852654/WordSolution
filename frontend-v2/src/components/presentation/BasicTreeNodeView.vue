<script setup lang="ts">
defineProps<{
  title: string
  markerLabel?: string
  metaItems?: string[]
}>()
</script>

<template>
  <span class="grid w-full min-w-0 grid-cols-[minmax(0,1fr)_auto] items-center gap-2">
    <span class="flex min-w-0 items-center gap-1.5">
      <span
        v-if="markerLabel"
        class="h-3 w-0.5 shrink-0 rounded-full bg-primary"
        :title="markerLabel"
        aria-hidden="true"
      />
      <span class="min-w-0 truncate font-medium text-foreground">
        {{ title }}
      </span>
    </span>

    <span
      v-if="metaItems?.length"
      class="flex min-w-0 shrink-0 items-center gap-1.5 text-xs text-muted-foreground"
    >
      <template v-for="(metaItem, index) in metaItems" :key="`${metaItem}-${index}`">
        <span v-if="index > 0" aria-hidden="true">&middot;</span>
        <span class="whitespace-nowrap">{{ metaItem }}</span>
      </template>
    </span>
  </span>
</template>
