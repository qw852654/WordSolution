<script setup lang="ts">
defineProps<{
  title: string
  description?: string
  eyebrow?: string
}>()
</script>

<template>
  <header class="flex flex-col gap-4 border-b pb-5 md:flex-row md:items-start md:justify-between">
    <div class="min-w-0 space-y-2">
      <p v-if="eyebrow" class="text-sm text-muted-foreground">
        {{ eyebrow }}
      </p>
      <h1 class="text-2xl font-semibold tracking-normal text-foreground">
        {{ title }}
      </h1>
      <p v-if="description" class="max-w-3xl text-sm leading-6 text-muted-foreground">
        {{ description }}
      </p>
    </div>
    <div v-if="$slots.actions" class="flex shrink-0 flex-wrap gap-2">
      <slot name="actions" />
    </div>
  </header>
</template>
