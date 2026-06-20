<script setup lang="ts">
import { Layers, Plus, Search } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type { InsertActionType, InsertPointModel } from '@/types'

const props = defineProps<{
  point: InsertPointModel
  selected?: boolean
}>()

const emit = defineEmits<{
  requestAction: [{ insertPointId: string; actionType: InsertActionType }]
}>()

const { t } = useI18n()

function emitAction(actionType: InsertActionType) {
  if (!props.point.disabled) {
    emit('requestAction', { insertPointId: props.point.id, actionType })
  }
}
</script>

<template>
  <div
    class="group flex min-h-5 items-center gap-2 rounded-md border border-transparent px-1 transition-colors delay-500 hover:border-border focus-within:border-border focus-within:delay-0"
    :class="[
      point.disabled ? 'opacity-50' : '',
      selected ? 'border-primary/40 bg-primary/5 delay-0' : '',
    ]"
  >
    <div
      class="h-px flex-1 border-t border-dashed border-border opacity-30 transition-opacity delay-500 group-hover:opacity-100 group-focus-within:opacity-100 group-focus-within:delay-0"
      :class="selected ? 'opacity-100 delay-0' : ''"
    />
    <div
      class="flex min-h-5 shrink-0 flex-wrap items-center justify-center gap-1 opacity-0 transition-opacity delay-500 group-hover:opacity-100 group-focus-within:opacity-100 group-focus-within:delay-0"
      :class="selected ? 'opacity-100 delay-0' : ''"
    >
      <slot :point="point" :request-action="emitAction">
        <Button
          type="button"
          size="sm"
          variant="outline"
          class="h-6 px-2 text-xs"
          :aria-label="t('components.insertPoint.createContentBlock')"
          :disabled="point.disabled"
          @click="emitAction('CreateContentBlock')"
        >
          <Plus class="size-3.5" />
          {{ t('components.insertPoint.createContentBlock') }}
        </Button>
        <Button
          type="button"
          size="sm"
          variant="outline"
          class="h-6 px-2 text-xs"
          :aria-label="t('components.insertPoint.createAtomicSection')"
          :disabled="point.disabled"
          @click="emitAction('CreateAtomicSection')"
        >
          <Layers class="size-3.5" />
          {{ t('components.insertPoint.createAtomicSection') }}
        </Button>
        <Button
          type="button"
          size="sm"
          variant="outline"
          class="h-6 px-2 text-xs"
          :aria-label="t('components.insertPoint.searchExistingBlock')"
          :disabled="point.disabled"
          @click="emitAction('SearchExistingBlock')"
        >
          <Search class="size-3.5" />
          {{ t('components.insertPoint.searchExistingBlock') }}
        </Button>
      </slot>
    </div>
    <div
      class="h-px flex-1 border-t border-dashed border-border opacity-30 transition-opacity delay-500 group-hover:opacity-100 group-focus-within:opacity-100 group-focus-within:delay-0"
      :class="selected ? 'opacity-100 delay-0' : ''"
    />
  </div>
</template>
