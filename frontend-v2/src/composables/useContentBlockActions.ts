export interface ContentBlockActionsOptions {
  setFeedback?: (message: string) => void
  contentBlockWordEditApiPendingMessage?: string
}

export function useContentBlockActions(options: ContentBlockActionsOptions = {}) {
  async function startContentBlockWordEdit(contentBlockId: number) {
    void contentBlockId

    options.setFeedback?.(
      options.contentBlockWordEditApiPendingMessage ?? 'ContentBlock Word 编辑 API 尚未接入',
    )
  }

  return {
    startContentBlockWordEdit,
  }
}
