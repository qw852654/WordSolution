<script setup lang="ts">
import {
  ArrowDown,
  ArrowLeft,
  ArrowRight,
  ArrowUp,
  FileText,
  Plus,
  Trash2,
} from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'

const props = defineProps<{
  itemId: string
  selected?: boolean
  disabled?: boolean
  ariaLabel?: string
}>()

const emit = defineEmits<{
  select: [id: string]
  insertBefore: [id: string]
  insertAfter: [id: string]
  moveUp: [id: string]
  moveDown: [id: string]
  indent: [id: string]
  outdent: [id: string]
  remove: [id: string]
  openWord: [id: string]
}>()

const { t } = useI18n()

function emitIfEnabled(
  eventName:
    | 'select'
    | 'insertBefore'
    | 'insertAfter'
    | 'moveUp'
    | 'moveDown'
    | 'indent'
    | 'outdent'
    | 'remove'
    | 'openWord',
) {
  if (props.disabled) {
    return
  }

  switch (eventName) {
    case 'select':
      emit('select', props.itemId)
      break
    case 'insertBefore':
      emit('insertBefore', props.itemId)
      break
    case 'insertAfter':
      emit('insertAfter', props.itemId)
      break
    case 'moveUp':
      emit('moveUp', props.itemId)
      break
    case 'moveDown':
      emit('moveDown', props.itemId)
      break
    case 'indent':
      emit('indent', props.itemId)
      break
    case 'outdent':
      emit('outdent', props.itemId)
      break
    case 'remove':
      emit('remove', props.itemId)
      break
    case 'openWord':
      emit('openWord', props.itemId)
      break
  }
}
</script>

<template>
  <article
    :class="[
      'section-item-view relative w-full overflow-visible rounded-md border border-transparent bg-background transition-colors',
      selected ? 'bg-muted/30' : '',
      disabled ? 'opacity-60' : '',
    ]"
    :aria-label="ariaLabel ?? t('components.sectionItemView.containerLabel')"
    :aria-disabled="disabled ? 'true' : undefined"
    :aria-selected="selected ? 'true' : 'false'"
    role="group"
    @click.stop="emitIfEnabled('select')"
  >
    <div class="min-w-0 p-2">
      <slot />
    </div>

    <div
      class="section-item-view-actions absolute right-0 top-0 z-10 flex min-h-full w-10 flex-col items-center gap-1 overflow-visible border-l border-transparent bg-muted/20 p-1 opacity-0 transition-opacity"
      :aria-label="t('components.sectionItemView.actionRailLabel')"
    >
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.insertBefore')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('insertBefore')"
      >
        <Plus class="size-4" />
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.insertAfter')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('insertAfter')"
      >
        <Plus class="size-4 rotate-90" />
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.openWord')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('openWord')"
      >
        <FileText class="size-4" />
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.moveUp')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('moveUp')"
      >
        <ArrowUp class="size-4" />
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.moveDown')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('moveDown')"
      >
        <ArrowDown class="size-4" />
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.outdent')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('outdent')"
      >
        <ArrowLeft class="size-4" />
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.indent')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('indent')"
      >
        <ArrowRight class="size-4" />
      </Button>
      <Button
        type="button"
        size="icon"
        variant="ghost"
        class="size-8"
        :aria-label="t('components.sectionItemView.remove')"
        :disabled="disabled"
        @click.stop="emitIfEnabled('remove')"
      >
        <Trash2 class="size-4" />
      </Button>
    </div>
  </article>
</template>

<style scoped>
.section-item-view:has(.section-item-view-actions:hover),
.section-item-view:has(.section-item-view-actions:focus-within) {
  border-color: var(--destructive);
}

.section-item-view-actions:hover,
.section-item-view-actions:focus-within {
  border-color: var(--destructive);
  opacity: 1;
}
</style>
