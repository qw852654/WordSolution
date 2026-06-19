<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { ListPlus, Plus, Trash2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type {
  TeachingTopicTreeContextActionType,
  TeachingTopicTreeContextMenuActionPayload,
  TeachingTopicTreeContextMenuModel,
} from '@/types'

const props = defineProps<{
  model: TeachingTopicTreeContextMenuModel | null
  open: boolean
}>()

const emit = defineEmits<{
  close: []
  requestAction: [payload: TeachingTopicTreeContextMenuActionPayload]
}>()

const { t } = useI18n()
const menuRef = ref<HTMLElement | null>(null)

const menuStyle = computed(() => {
  if (!props.model) {
    return {}
  }

  const menuWidth = 208
  const menuHeight = 148
  const viewportWidth = typeof window === 'undefined' ? props.model.position.x + menuWidth : window.innerWidth
  const viewportHeight =
    typeof window === 'undefined' ? props.model.position.y + menuHeight : window.innerHeight
  const left = Math.max(8, Math.min(props.model.position.x, viewportWidth - menuWidth - 8))
  const top = Math.max(8, Math.min(props.model.position.y, viewportHeight - menuHeight - 8))

  return {
    left: `${left}px`,
    top: `${top}px`,
  }
})

const actions = computed<
  {
    type: TeachingTopicTreeContextActionType
    icon: typeof Plus
    destructive?: boolean
    disabled?: boolean
  }[]
>(() => {
  const node = props.model?.node
  const disabled = Boolean(node?.disabled || node?.readOnly || node?.kind === 'SectionVariant')
  const deleteDisabled = disabled || node?.canDelete === false

  return [
    { type: 'AddChild', icon: Plus, disabled },
    { type: 'AddAfter', icon: ListPlus, disabled },
    { type: 'Delete', icon: Trash2, destructive: true, disabled: deleteDisabled },
  ]
})

function handleDocumentPointerDown(event: PointerEvent) {
  if (!props.open) {
    return
  }

  const target = event.target as Node | null

  if (target && menuRef.value?.contains(target)) {
    return
  }

  emit('close')
}

function handleDocumentKeydown(event: KeyboardEvent) {
  if (props.open && event.key === 'Escape') {
    emit('close')
  }
}

function requestAction(actionType: TeachingTopicTreeContextActionType) {
  if (!props.model) {
    return
  }

  emit('requestAction', {
    nodeId: props.model.node.id,
    actionType,
  })
}

onMounted(() => {
  document.addEventListener('pointerdown', handleDocumentPointerDown, true)
  document.addEventListener('keydown', handleDocumentKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', handleDocumentPointerDown, true)
  document.removeEventListener('keydown', handleDocumentKeydown)
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open && model"
      ref="menuRef"
      class="fixed z-50 w-52 rounded-md border bg-popover p-1 text-popover-foreground"
      :style="menuStyle"
      role="menu"
      :aria-label="t('components.teachingTopicTreeContextMenu.label')"
      @click.stop
      @contextmenu.prevent
    >
      <div class="border-b px-2 py-1.5">
        <p class="truncate text-xs text-muted-foreground">
          {{ t('components.teachingTopicTreeContextMenu.target') }}
        </p>
        <p class="truncate text-sm font-medium">
          {{ model.node.title }}
        </p>
      </div>

      <div class="py-1">
        <Button
          v-for="action in actions"
          :key="action.type"
          type="button"
          variant="ghost"
          size="sm"
          class="w-full justify-start gap-2"
          :class="action.destructive ? 'text-destructive hover:bg-destructive/10 hover:text-destructive' : ''"
          :disabled="action.disabled"
          role="menuitem"
          @click="requestAction(action.type)"
        >
          <component :is="action.icon" class="size-4" aria-hidden="true" />
          {{ t(`components.teachingTopicTreeContextMenu.actions.${action.type}`) }}
        </Button>
      </div>
    </div>
  </Teleport>
</template>
