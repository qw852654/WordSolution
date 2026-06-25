<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import InlineBorderHeader from '@/components/presentation/InlineBorderHeader.vue'
import { cn } from '@/lib/utils'

const props = defineProps<{
  title: string
  meta?: string
  difficultyMarkerClass?: string
  difficultyMarkerLabel?: string
  selected?: boolean
  disabled?: boolean
  class?: HTMLAttributes['class']
}>()

const emit = defineEmits<{
  selectTitle: [event: MouseEvent | KeyboardEvent]
}>()

function emitTitleSelect(event: MouseEvent | KeyboardEvent) {
  if (props.disabled) {
    return
  }

  emit('selectTitle', event)
}
</script>

<template>
  <section
    :class="cn(
      'rounded-md border bg-background transition-colors',
      selected ? 'border-primary/40 bg-primary/5' : 'border-border',
      disabled ? 'opacity-60' : '',
      props.class,
    )"
    :aria-disabled="disabled ? 'true' : undefined"
  >
    <div
      role="button"
      :tabindex="disabled ? -1 : 0"
      :class="disabled ? '' : 'cursor-pointer'"
      @click.stop="emitTitleSelect"
      @keydown.enter.stop="emitTitleSelect"
      @keydown.space.prevent.stop="emitTitleSelect"
    >
      <InlineBorderHeader
        :title="title"
        :meta="meta"
        :difficulty-marker-class="difficultyMarkerClass"
        :difficulty-marker-label="difficultyMarkerLabel"
      >
        <template #meta-extra>
          <slot name="meta-extra" />
        </template>
        <template #actions>
          <slot name="actions" />
        </template>
      </InlineBorderHeader>
    </div>
    <div class="space-y-2 px-3 pb-3 pt-1">
      <slot />
    </div>
  </section>
</template>
