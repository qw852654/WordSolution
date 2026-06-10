<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  label: string
  tone?: 'neutral' | 'active' | 'muted' | 'danger'
}>(), {
  tone: 'neutral',
})

const toneClass = computed(() => {
  switch (props.tone) {
    case 'active':
      return 'border-primary/30 bg-primary/5 text-foreground'
    case 'muted':
      return 'border-border bg-muted text-muted-foreground'
    case 'danger':
      return 'border-destructive/30 bg-destructive/10 text-destructive'
    default:
      return 'border-border bg-background text-foreground'
  }
})
</script>

<template>
  <span
    class="inline-flex h-6 items-center rounded-md border px-2 text-xs font-medium"
    :class="toneClass"
  >
    {{ label }}
  </span>
</template>
