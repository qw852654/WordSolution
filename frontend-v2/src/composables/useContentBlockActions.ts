import { computed, onBeforeUnmount, ref } from 'vue'
import {
  cmsV2Api,
  type CmsV2ContentBlockEditSessionDto,
} from '@/apis/cmsV2Client'

export interface ContentBlockActionsOptions {
  setFeedback?: (message: string) => void
  refreshSection?: () => Promise<void> | void
  wordEditStartedMessage?: string
  wordEditSyncingMessage?: string
  wordEditSyncedMessage?: string
  wordEditNoChangesMessage?: string
  wordEditCancelledMessage?: string
  wordEditFailedMessage?: string
}

export function useContentBlockActions(options: ContentBlockActionsOptions = {}) {
  const activeEditSession = ref<CmsV2ContentBlockEditSessionDto | null>(null)
  const isWordEditBusy = ref(false)
  const isWordEditPolling = ref(false)
  let pollTimerId: number | undefined

  const canSyncActiveContentBlockEditSession = computed(
    () => activeEditSession.value !== null && !isWordEditBusy.value,
  )

  async function startContentBlockWordEdit(contentBlockId: number) {
    if (isWordEditBusy.value || activeEditSession.value) {
      return
    }

    isWordEditBusy.value = true

    try {
      const session = await cmsV2Api.createContentBlockEditSession(contentBlockId, {
        openWord: true,
      })

      activeEditSession.value = session
      options.setFeedback?.(
        options.wordEditStartedMessage || 'ContentBlock Word 编辑会话已启动。',
      )
      startPollingContentBlockEditSession()
    } finally {
      isWordEditBusy.value = false
    }
  }

  async function syncActiveContentBlockEditSession() {
    const session = activeEditSession.value

    if (!session || isWordEditBusy.value) {
      return
    }

    isWordEditBusy.value = true
    options.setFeedback?.(
      options.wordEditSyncingMessage || '正在同步 ContentBlock Word 编辑内容...',
    )

    try {
      const result = await cmsV2Api.syncContentBlockEditSession(session.sessionId)
      activeEditSession.value = null
      stopPollingContentBlockEditSession()
      options.setFeedback?.(
        result.changed
          ? options.wordEditSyncedMessage || 'ContentBlock Word 编辑内容已同步。'
          : options.wordEditNoChangesMessage || 'ContentBlock Word 编辑内容没有变化。',
      )
      await options.refreshSection?.()
    } finally {
      isWordEditBusy.value = false
    }
  }

  async function cancelActiveContentBlockEditSession() {
    const session = activeEditSession.value

    if (!session || isWordEditBusy.value) {
      return
    }

    isWordEditBusy.value = true

    try {
      const cancelled = await cmsV2Api.cancelContentBlockEditSession(session.sessionId)
      activeEditSession.value = null
      stopPollingContentBlockEditSession()
      options.setFeedback?.(cancelled.message || 'ContentBlock Word 编辑会话已取消。')
    } finally {
      isWordEditBusy.value = false
    }
  }

  function startPollingContentBlockEditSession() {
    stopPollingContentBlockEditSession()
    pollTimerId = window.setInterval(() => {
      void pollContentBlockEditSession()
    }, 1000)
  }

  function stopPollingContentBlockEditSession() {
    if (pollTimerId !== undefined) {
      window.clearInterval(pollTimerId)
      pollTimerId = undefined
    }
  }

  async function pollContentBlockEditSession() {
    const session = activeEditSession.value
    if (!session || isWordEditPolling.value) {
      return
    }

    isWordEditPolling.value = true

    try {
      const latest = await cmsV2Api.getContentBlockEditSession(session.sessionId)
      activeEditSession.value = latest

      if (latest.status === 'Synced') {
        activeEditSession.value = null
        stopPollingContentBlockEditSession()
        options.setFeedback?.(
          options.wordEditSyncedMessage || 'ContentBlock Word 编辑内容已同步。',
        )
        await options.refreshSection?.()
      }

      if (latest.status === 'Cancelled') {
        activeEditSession.value = null
        stopPollingContentBlockEditSession()
        options.setFeedback?.(
          options.wordEditCancelledMessage || 'ContentBlock Word 编辑会话已取消。',
        )
      }

      if (latest.status === 'Failed') {
        activeEditSession.value = null
        stopPollingContentBlockEditSession()
        options.setFeedback?.(
          options.wordEditFailedMessage || 'ContentBlock Word 编辑会话同步失败。',
        )
      }
    } finally {
      isWordEditPolling.value = false
    }
  }

  onBeforeUnmount(() => {
    stopPollingContentBlockEditSession()
  })

  return {
    activeEditSession,
    isWordEditBusy,
    isWordEditPolling,
    canSyncActiveContentBlockEditSession,
    startContentBlockWordEdit,
    syncActiveContentBlockEditSession,
    cancelActiveContentBlockEditSession,
  }
}
