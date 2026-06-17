<script setup lang="ts">
defineProps<{
  title: string
  markerLabel?: string
  markerClass?: string
  metaItems?: string[]
  truncateTitle?: boolean
}>()
</script>

<template>
  <span
    class="grid items-center gap-2"
    :class="
      truncateTitle === false
        ? 'w-max min-w-full grid-cols-[max-content_auto]'
        : 'w-full min-w-0 grid-cols-[minmax(0,1fr)_auto]'
    "
  >
    <span class="flex min-w-0 items-center gap-1.5">
      <span
        v-if="markerLabel"
        class="h-3 w-0.5 shrink-0 rounded-full"
        :class="markerClass ?? 'bg-muted-foreground'"
        :title="markerLabel"
        aria-hidden="true"
      />
      <span
        class="font-medium text-foreground"
        :class="truncateTitle === false ? 'whitespace-nowrap' : 'min-w-0 truncate'"
      >
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
