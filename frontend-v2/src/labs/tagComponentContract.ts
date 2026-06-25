import type { TagColorToken, TagModel, TagPickerState } from '@/types'

export const tagColorTokens: TagColorToken[] = [
  'tag-gray',
  'tag-orange',
  'tag-yellow',
  'tag-green',
  'tag-blue',
  'tag-purple',
  'tag-pink',
  'tag-red',
]

export interface TagComponentLabScenario {
  id: string
  selectedTags: TagModel[]
  searchResults: TagModel[]
  state: TagPickerState
}
