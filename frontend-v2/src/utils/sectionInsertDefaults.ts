export interface AtomicSectionChildTitleInput {
  inputTitle: string
  atomicSectionTitle: string
}

export function resolveAtomicSectionChildContentBlockTitle(input: AtomicSectionChildTitleInput): string {
  const trimmedInputTitle = input.inputTitle.trim()

  if (trimmedInputTitle) {
    return trimmedInputTitle
  }

  return input.atomicSectionTitle.trim()
}
