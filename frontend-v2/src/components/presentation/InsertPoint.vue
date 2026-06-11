<script setup lang="ts">
import { Plus } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type { InsertPointModel } from '@/types'

const props = defineProps<{
  point: InsertPointModel
}>()

const emit = defineEmits<{
  insert: [id: string]
}>()

const { t } = useI18n()

function emitInsert() {
  if (!props.point.disabled) {
    emit('insert', props.point.id)
  }
}
</script>

<template>
  <div
    class="group flex min-h-5 items-center gap-2 rounded-md border border-transparent px-1 transition-colors delay-500 hover:border-border focus-within:border-border focus-within:delay-0"
    :class="point.disabled ? 'opacity-50' : ''"
  >
    <div class="h-px flex-1 border-t border-dashed border-border opacity-30 transition-opacity delay-500 group-hover:opacity-100 group-focus-within:opacity-100 group-focus-within:delay-0" />
    <div class="flex min-h-5 shrink-0 items-center gap-1 opacity-0 transition-opacity delay-500 group-hover:opacity-100 group-focus-within:opacity-100 group-focus-within:delay-0">
      <slot :point="point" :insert="emitInsert">
        <Button
          type="button"
          size="sm"
          variant="outline"
          class="h-6 px-2 text-xs"
          :aria-label="point.label"
          :disabled="point.disabled"
          @click="emitInsert"
        >
          <Plus class="size-3.5" />
          {{ t('components.insertPoint.insert') }}
        </Button>
      </slot>
    </div>
    <div class="h-px flex-1 border-t border-dashed border-border opacity-30 transition-opacity delay-500 group-hover:opacity-100 group-focus-within:opacity-100 group-focus-within:delay-0" />
  </div>
</template>
