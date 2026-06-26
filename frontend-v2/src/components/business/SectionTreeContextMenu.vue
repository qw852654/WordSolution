<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { GitBranch, Layers, Pencil, Plus, Search, Trash2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type {
  SectionTreeContextActionType,
  SectionTreeContextMenuActionPayload,
  SectionTreeContextMenuModel,
} from '@/types'

const props = defineProps<{
  model: SectionTreeContextMenuModel | null
  open: boolean
}>()

const emit = defineEmits<{
  close: []
  requestAction: [payload: SectionTreeContextMenuActionPayload]
}>()

const { t } = useI18n()
const menuRef = ref<HTMLElement | null>(null)

const menuStyle = computed(() => {
  if (!props.model) {
    return {}
  }

  const menuWidth = 220
  const menuHeight = 220
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
    type: SectionTreeContextActionType
    icon: typeof Plus
    destructive?: boolean
    disabled?: boolean
    disabledReason?: string
  }[]
>(() => {
  const node = props.model?.node

  if (node?.kind === 'SectionVariant') {
    return [
      {
        type: 'DeleteSectionVariant',
        icon: Trash2,
        destructive: true,
        disabled: Boolean(node.disabled),
      },
    ]
  }

  if (node?.kind === 'AtomicSectionPanel' || node?.kind === 'AtomicSectionUnassigned') {
    return []
  }

  const disabled = Boolean(node?.disabled)
  const removeDisabled = disabled || node?.kind === 'Section'

  const baseActions: {
    type: SectionTreeContextActionType
    icon: typeof Plus
    destructive?: boolean
    disabled?: boolean
    disabledReason?: string
  }[] = [
    { type: 'CreateContentBlock', icon: Plus, disabled },
    { type: 'CreateAtomicSection', icon: Layers, disabled },
    { type: 'SearchExistingBlock', icon: Search, disabled },
    {
      type: 'Remove',
      icon: Trash2,
      destructive: true,
      disabled: removeDisabled,
      disabledReason:
        node?.kind === 'Section'
          ? t('components.sectionTreeContextMenu.rootRemoveDisabled')
          : undefined,
    },
  ]

  return node?.kind === 'Section'
    ? [
        { type: 'CreateSectionVariant', icon: GitBranch, disabled },
        { type: 'RenameSection', icon: Pencil, disabled },
        ...baseActions,
      ]
    : baseActions
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

function requestAction(actionType: SectionTreeContextActionType) {
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
      v-if="open && model && actions.length"
      ref="menuRef"
      class="fixed z-50 w-[13.75rem] rounded-md border bg-popover p-1 text-popover-foreground"
      :style="menuStyle"
      role="menu"
      :aria-label="t('components.sectionTreeContextMenu.label')"
      @click.stop
      @contextmenu.prevent
    >
      <div class="border-b px-2 py-1.5">
        <p class="truncate text-xs text-muted-foreground">
          {{ t('components.sectionTreeContextMenu.target') }}
        </p>
        <p class="truncate text-sm font-medium">
          {{ model.node.typeLabel }}
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
          :title="action.disabledReason"
          role="menuitem"
          @click="requestAction(action.type)"
        >
          <component :is="action.icon" class="size-4" aria-hidden="true" />
          {{ t(`components.sectionTreeContextMenu.actions.${action.type}`) }}
        </Button>
      </div>
    </div>
  </Teleport>
</template>
