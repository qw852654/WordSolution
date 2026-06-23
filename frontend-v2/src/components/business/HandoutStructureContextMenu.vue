<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { Box, FileText, GitBranch } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { Button } from '@/components/ui/button'
import type {
  HandoutTreeContextActionType,
  HandoutTreeContextMenuActionPayload,
  HandoutTreeContextMenuModel,
} from '@/types'

const props = defineProps<{
  model: HandoutTreeContextMenuModel | null
  open: boolean
}>()

const emit = defineEmits<{
  close: []
  requestAction: [payload: HandoutTreeContextMenuActionPayload]
}>()

const { t } = useI18n()
const menuRef = ref<HTMLElement | null>(null)

const menuStyle = computed(() => {
  if (!props.model) {
    return {}
  }

  const menuWidth = 240
  const menuHeight = 120
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
    type: HandoutTreeContextActionType
    icon: typeof GitBranch
  }[]
>(() => {
  const node = props.model?.node

  if (node?.kind === 'HandoutVersion') {
    return [
      { type: 'AddSectionVariantsToEnd', icon: GitBranch },
      { type: 'AddAtomicSectionToEnd', icon: Box },
      { type: 'AddContentBlockToEnd', icon: FileText },
    ]
  }

  if (node?.kind === 'HandoutVersionItem') {
    return [
      { type: 'AddSectionVariantsAfter', icon: GitBranch },
      { type: 'AddAtomicSectionAfter', icon: Box },
      { type: 'AddContentBlockAfter', icon: FileText },
    ]
  }

  return []
})

function requestAction(actionType: HandoutTreeContextActionType) {
  if (!props.model) {
    return
  }

  emit('requestAction', {
    nodeId: props.model.node.id,
    actionType,
  })
}

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
      class="fixed z-50 w-60 rounded-md border bg-popover p-1 text-popover-foreground"
      :style="menuStyle"
      role="menu"
      :aria-label="t('components.handoutStructureContextMenu.label')"
      @click.stop
      @contextmenu.prevent
    >
      <div class="border-b px-2 py-1.5">
        <p class="truncate text-xs text-muted-foreground">
          {{ t('components.handoutStructureContextMenu.target') }}
        </p>
        <p class="truncate text-sm font-medium">
          {{ model.node.title }}
        </p>
      </div>

      <div v-if="actions.length" class="py-1">
        <Button
          v-for="action in actions"
          :key="action.type"
          type="button"
          variant="ghost"
          size="sm"
          class="w-full justify-start gap-2"
          role="menuitem"
          @click="requestAction(action.type)"
        >
          <component :is="action.icon" class="size-4" aria-hidden="true" />
          {{ t(`components.handoutStructureContextMenu.actions.${action.type}`) }}
        </Button>
      </div>

      <p v-else class="px-2 py-2 text-xs text-muted-foreground">
        {{ t('components.handoutStructureContextMenu.noActions') }}
      </p>
    </div>
  </Teleport>
</template>
