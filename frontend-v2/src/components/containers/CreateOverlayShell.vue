<script setup lang="ts">
import { Button } from '@/components/ui/button'

const props = defineProps<{
  open: boolean
  dialogLabel: string
  title: string
  description?: string
  eyebrow?: string
  positionLabel?: string
  cancelLabel: string
  submitLabel: string
  submitDisabled?: boolean
  busy?: boolean
  errorMessage?: string
  maxWidthClass?: string
}>()

const emit = defineEmits<{
  cancel: []
  submit: []
}>()

const panelWidthClass = props.maxWidthClass ?? 'max-w-xl'
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex min-h-screen items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      :aria-label="dialogLabel"
    >
      <div class="absolute inset-0 bg-background/70 backdrop-blur-sm" aria-hidden="true" />

      <form
        class="relative z-10 flex w-full flex-col gap-4 rounded-lg border bg-card p-4 text-card-foreground"
        :class="panelWidthClass"
        @submit.prevent="emit('submit')"
      >
        <header class="space-y-1">
          <p v-if="eyebrow" class="text-xs font-medium text-muted-foreground">
            {{ eyebrow }}
          </p>
          <div class="flex flex-col gap-1 sm:flex-row sm:items-baseline sm:justify-between">
            <h2 class="text-lg font-semibold tracking-normal">{{ title }}</h2>
            <p v-if="positionLabel" class="text-sm text-muted-foreground">{{ positionLabel }}</p>
          </div>
          <p v-if="description" class="text-sm text-muted-foreground">
            {{ description }}
          </p>
        </header>

        <slot />

        <p
          v-if="errorMessage"
          class="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive"
          role="alert"
        >
          {{ errorMessage }}
        </p>

        <footer class="flex justify-end gap-2">
          <Button type="button" variant="outline" :disabled="busy" @click="emit('cancel')">
            {{ cancelLabel }}
          </Button>
          <Button type="submit" :disabled="submitDisabled || busy">
            {{ submitLabel }}
          </Button>
        </footer>
      </form>
    </div>
  </Teleport>
</template>
