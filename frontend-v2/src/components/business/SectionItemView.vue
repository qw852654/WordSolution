<script setup lang="ts">
import { computed } from 'vue'
import type { Component } from 'vue'
import {
  ArrowDown,
  ArrowLeft,
  ArrowRight,
  ArrowUp,
  FileText,
  Pencil,
  Plus,
  Trash2,
} from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type { SectionItemVariantSelectionState, SectionItemViewAction } from '@/types'

const props = defineProps<{
  itemId: string
  selected?: boolean
  upgradeSelected?: boolean
  variantSelectionState?: SectionItemVariantSelectionState
  variantUnavailableReason?: string
  disabled?: boolean
  actions?: SectionItemViewAction[]
  ariaLabel?: string
}>()

const emit = defineEmits<{
  select: [id: string, event?: MouseEvent]
  insertBefore: [id: string]
  insertAfter: [id: string]
  insertChildContentBlock: [id: string]
  moveUp: [id: string]
  moveDown: [id: string]
  rename: [id: string]
  indent: [id: string]
  outdent: [id: string]
  remove: [id: string]
  openWord: [id: string]
}>()

const { t } = useI18n()

const defaultActions: SectionItemViewAction[] = [
  'InsertBefore',
  'InsertAfter',
  'OpenWord',
  'MoveUp',
  'MoveDown',
  'Outdent',
  'Indent',
  'Remove',
]

const actionDefinitions: Record<
  SectionItemViewAction,
  {
    labelKey: string
    icon: Component
  }
> = {
  InsertBefore: {
    labelKey: 'components.sectionItemView.insertBefore',
    icon: Plus,
  },
  InsertAfter: {
    labelKey: 'components.sectionItemView.insertAfter',
    icon: Plus,
  },
  InsertChildContentBlock: {
    labelKey: 'components.sectionItemView.insertChildContentBlock',
    icon: Plus,
  },
  OpenWord: {
    labelKey: 'components.sectionItemView.openWord',
    icon: FileText,
  },
  MoveUp: {
    labelKey: 'components.sectionItemView.moveUp',
    icon: ArrowUp,
  },
  MoveDown: {
    labelKey: 'components.sectionItemView.moveDown',
    icon: ArrowDown,
  },
  Rename: {
    labelKey: 'components.sectionItemView.rename',
    icon: Pencil,
  },
  Indent: {
    labelKey: 'components.sectionItemView.indent',
    icon: ArrowRight,
  },
  Outdent: {
    labelKey: 'components.sectionItemView.outdent',
    icon: ArrowLeft,
  },
  Remove: {
    labelKey: 'components.sectionItemView.remove',
    icon: Trash2,
  },
}

const visibleActions = computed(() => props.actions ?? defaultActions)
const variantSelected = computed(() => props.variantSelectionState === 'selected')
const variantUnavailable = computed(() => props.variantSelectionState === 'unavailable')
const actionRailHeight = computed(() => {
  const actionCount = visibleActions.value.length
  const gapCount = Math.max(actionCount - 1, 0)

  return `calc((${actionCount} * 2rem) + (${gapCount} * 0.25rem) + (2 * 0.25rem))`
})

function emitIfEnabled(eventName: 'select' | SectionItemViewAction, event?: MouseEvent) {
  if (props.disabled) {
    return
  }

  switch (eventName) {
    case 'select':
      emit('select', props.itemId, event)
      break
    case 'InsertBefore':
      emit('insertBefore', props.itemId)
      break
    case 'InsertAfter':
      emit('insertAfter', props.itemId)
      break
    case 'InsertChildContentBlock':
      emit('insertChildContentBlock', props.itemId)
      break
    case 'MoveUp':
      emit('moveUp', props.itemId)
      break
    case 'MoveDown':
      emit('moveDown', props.itemId)
      break
    case 'Rename':
      emit('rename', props.itemId)
      break
    case 'Indent':
      emit('indent', props.itemId)
      break
    case 'Outdent':
      emit('outdent', props.itemId)
      break
    case 'Remove':
      emit('remove', props.itemId)
      break
    case 'OpenWord':
      emit('openWord', props.itemId)
      break
  }
}
</script>

<template>
  <article
    :class="[
      'section-item-view relative w-full overflow-visible rounded-md border border-transparent bg-background transition-colors',
      selected && !upgradeSelected ? 'bg-muted/30' : '',
      upgradeSelected ? 'section-item-view-upgrade-selected' : '',
      variantSelected ? 'section-item-view-variant-selected' : '',
      variantUnavailable ? 'section-item-view-variant-unavailable' : '',
      disabled ? 'opacity-60' : '',
    ]"
    :aria-label="ariaLabel ?? t('components.sectionItemView.containerLabel')"
    :aria-disabled="disabled ? 'true' : undefined"
    :aria-selected="selected || upgradeSelected || variantSelected ? 'true' : 'false'"
    role="group"
    :style="{ '--section-item-action-rail-height': actionRailHeight }"
    @click.stop="emitIfEnabled('select', $event)"
  >
    <div class="min-w-0 p-2">
      <slot />
      <p
        v-if="variantUnavailable && variantUnavailableReason"
        class="mx-2 mb-2 rounded-md border bg-muted/20 px-2 py-1 text-xs text-muted-foreground"
      >
        {{ variantUnavailableReason }}
      </p>
    </div>

    <div
      v-if="visibleActions.length"
      class="section-item-view-actions absolute right-0 top-0 z-10 flex min-h-full w-10 flex-col items-center gap-1 overflow-visible border-l border-transparent bg-muted/20 p-1 opacity-0 transition-opacity"
      :aria-label="t('components.sectionItemView.actionRailLabel')"
    >
      <Button
        v-for="action in visibleActions"
        :key="action"
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t(actionDefinitions[action].labelKey)"
        :disabled="disabled"
        @click.stop="emitIfEnabled(action)"
      >
        <component :is="actionDefinitions[action].icon" class="size-4" />
      </Button>
    </div>
  </article>
</template>

<style scoped>
.section-item-view-actions {
  min-height: max(100%, var(--section-item-action-rail-height));
}

.section-item-view-upgrade-selected {
  background-color: var(--section-item-upgrade-selection);
  box-shadow: inset 0 0 0 1px var(--section-item-upgrade-selection-ring);
}

.section-item-view-upgrade-selected::before {
  position: absolute;
  z-index: 20;
  inset-block: 0.35rem;
  inset-inline-start: 0.25rem;
  width: 0.1875rem;
  border-radius: 9999px;
  background: var(--section-item-upgrade-selection-marker);
  content: "";
  pointer-events: none;
}

.section-item-view-variant-selected {
  background-color: hsl(var(--accent));
  box-shadow: inset 0 0 0 1px hsl(var(--primary));
}

.section-item-view-variant-selected::before {
  position: absolute;
  z-index: 20;
  inset-block: 0.35rem;
  inset-inline-start: 0.25rem;
  width: 0.1875rem;
  border-radius: 9999px;
  background: hsl(var(--primary));
  content: "";
  pointer-events: none;
}

.section-item-view-variant-unavailable {
  opacity: 0.55;
}

.section-item-view:has(.section-item-view-actions:hover),
.section-item-view:has(.section-item-view-actions:focus-within) {
  border-color: var(--destructive);
  min-height: var(--section-item-action-rail-height);
}

.section-item-view-actions:hover,
.section-item-view-actions:focus-within {
  border-color: var(--destructive);
  opacity: 1;
}
</style>
