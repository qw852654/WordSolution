import { computed, toValue, watchEffect, type MaybeRefOrGetter } from 'vue'

export function formatPageTitle(pageName: string, detailName?: string | null) {
  const normalizedPageName = pageName.trim()
  const normalizedDetailName = detailName?.trim()

  return normalizedDetailName ? `${normalizedPageName} - ${normalizedDetailName}` : normalizedPageName
}

export function usePageTitle(
  pageName: MaybeRefOrGetter<string>,
  detailName?: MaybeRefOrGetter<string | null | undefined>,
) {
  const title = computed(() =>
    formatPageTitle(toValue(pageName), detailName === undefined ? undefined : toValue(detailName)),
  )

  watchEffect(() => {
    document.title = title.value
  })

  return title
}
