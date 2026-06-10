<script setup lang="ts">
import { Button } from '@/components/ui/button'

defineProps<{
  title: string
  description?: string
  actionLabel?: string
}>()

defineEmits<{
  action: []
}>()
</script>

<template>
  <section class="rounded-lg border bg-card px-5 py-6 text-card-foreground">
    <div class="flex max-w-2xl flex-col gap-3">
      <div v-if="$slots.icon" class="text-muted-foreground">
        <slot name="icon" />
      </div>
      <div class="space-y-1">
        <h2 class="text-base font-semibold tracking-normal">
          {{ title }}
        </h2>
        <p v-if="description" class="text-sm leading-6 text-muted-foreground">
          {{ description }}
        </p>
      </div>
      <Button v-if="actionLabel" type="button" class="w-fit" @click="$emit('action')">
        {{ actionLabel }}
      </Button>
    </div>
  </section>
</template>
