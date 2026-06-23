import type {
  CmsV2SectionVariantSelectionTreeSectionDto,
  CmsV2SectionVariantSelectionTreeTopicDto,
} from '@/apis/cmsV2Client'

export type SectionVariantTreeCheckState = 'checked' | 'unchecked' | 'mixed' | 'locked'

export function collectSelectableVariantIds(
  tree: CmsV2SectionVariantSelectionTreeTopicDto[],
  existingVariantIds: Set<number>,
) {
  const ids: number[] = []

  forEachVariant(tree, (variantId) => {
    if (!existingVariantIds.has(variantId)) {
      ids.push(variantId)
    }
  })

  return ids
}

export function deriveNodeCheckState(
  variantIds: number[],
  selectedVariantIds: Set<number>,
  existingVariantIds: Set<number>,
): SectionVariantTreeCheckState {
  if (!variantIds.length) {
    return 'unchecked'
  }

  const selectableIds = variantIds.filter((id) => !existingVariantIds.has(id))
  const lockedIds = variantIds.filter((id) => existingVariantIds.has(id))
  const selectedSelectableCount = selectableIds.filter((id) => selectedVariantIds.has(id)).length
  const lockedSelectedCount = lockedIds.filter((id) => selectedVariantIds.has(id)).length

  if (!selectableIds.length && lockedIds.length && lockedSelectedCount === lockedIds.length) {
    return 'locked'
  }

  if (selectedSelectableCount === 0 && lockedSelectedCount === 0) {
    return 'unchecked'
  }

  if (
    selectedSelectableCount === selectableIds.length &&
    lockedSelectedCount === lockedIds.length
  ) {
    return 'checked'
  }

  return 'mixed'
}

export function toggleVariant(
  selectedVariantIds: Set<number>,
  existingVariantIds: Set<number>,
  variantId: number,
) {
  const next = new Set(selectedVariantIds)

  if (existingVariantIds.has(variantId)) {
    return next
  }

  if (next.has(variantId)) {
    next.delete(variantId)
  } else {
    next.add(variantId)
  }

  return next
}

export function toggleGroup(
  selectedVariantIds: Set<number>,
  existingVariantIds: Set<number>,
  variantIds: number[],
) {
  const next = new Set(selectedVariantIds)
  const selectableIds = variantIds.filter((id) => !existingVariantIds.has(id))
  const allSelected = selectableIds.length > 0 && selectableIds.every((id) => next.has(id))

  for (const id of selectableIds) {
    if (allSelected) {
      next.delete(id)
    } else {
      next.add(id)
    }
  }

  return next
}

export function buildExistingVariantIds(existingVariantIds: number[]) {
  return new Set(existingVariantIds)
}

export function getNewVariantIds(
  selectedVariantIds: Set<number>,
  existingVariantIds: Set<number>,
) {
  return Array.from(selectedVariantIds).filter((id) => !existingVariantIds.has(id))
}

export function filterTree(
  tree: CmsV2SectionVariantSelectionTreeTopicDto[],
  searchText: string,
) {
  const query = normalize(searchText)

  if (!query) {
    return tree
  }

  return tree
    .map((topic) => filterTopic(topic, query))
    .filter((topic): topic is CmsV2SectionVariantSelectionTreeTopicDto => Boolean(topic))
}

export function getTopicVariantIds(topic: CmsV2SectionVariantSelectionTreeTopicDto) {
  const ids: number[] = []
  forEachTopicVariant(topic, (variantId) => ids.push(variantId))
  return ids
}

export function getSectionVariantIds(section: CmsV2SectionVariantSelectionTreeSectionDto) {
  return section.sectionVariants.map((variant) => variant.id)
}

function normalize(value: string) {
  return value.trim().toLocaleLowerCase()
}

function filterTopic(
  topic: CmsV2SectionVariantSelectionTreeTopicDto,
  query: string,
): CmsV2SectionVariantSelectionTreeTopicDto | undefined {
  const sections = topic.sections
    .map((section) => filterSection(section, query))
    .filter((section): section is CmsV2SectionVariantSelectionTreeSectionDto => Boolean(section))
  const children = topic.children
    .map((child) => filterTopic(child, query))
    .filter((child): child is CmsV2SectionVariantSelectionTreeTopicDto => Boolean(child))
  const topicMatches = normalize(topic.teachingTopic.name).includes(query)

  if (!topicMatches && !sections.length && !children.length) {
    return undefined
  }

  return {
    ...topic,
    sections: topicMatches ? topic.sections : sections,
    children: topicMatches ? topic.children : children,
  }
}

function filterSection(
  section: CmsV2SectionVariantSelectionTreeSectionDto,
  query: string,
): CmsV2SectionVariantSelectionTreeSectionDto | undefined {
  const sectionMatches = normalize(section.section.title).includes(query)
  const variants = section.sectionVariants.filter((variant) =>
    normalize(variant.title).includes(query),
  )

  if (!sectionMatches && !variants.length) {
    return undefined
  }

  return {
    ...section,
    sectionVariants: sectionMatches ? section.sectionVariants : variants,
  }
}

function forEachVariant(
  tree: CmsV2SectionVariantSelectionTreeTopicDto[],
  callback: (variantId: number) => void,
) {
  for (const topic of tree) {
    forEachTopicVariant(topic, callback)
  }
}

function forEachTopicVariant(
  topic: CmsV2SectionVariantSelectionTreeTopicDto,
  callback: (variantId: number) => void,
) {
  for (const section of topic.sections) {
    for (const variant of section.sectionVariants) {
      callback(variant.id)
    }
  }

  for (const child of topic.children) {
    forEachTopicVariant(child, callback)
  }
}
