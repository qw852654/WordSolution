import type { TagColorToken } from '@/types'

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

export const tagColorClasses: Record<TagColorToken, string> = {
  'tag-gray': 'border-tag-gray-border bg-tag-gray text-tag-gray-foreground',
  'tag-orange': 'border-tag-orange-border bg-tag-orange text-tag-orange-foreground',
  'tag-yellow': 'border-tag-yellow-border bg-tag-yellow text-tag-yellow-foreground',
  'tag-green': 'border-tag-green-border bg-tag-green text-tag-green-foreground',
  'tag-blue': 'border-tag-blue-border bg-tag-blue text-tag-blue-foreground',
  'tag-purple': 'border-tag-purple-border bg-tag-purple text-tag-purple-foreground',
  'tag-pink': 'border-tag-pink-border bg-tag-pink text-tag-pink-foreground',
  'tag-red': 'border-tag-red-border bg-tag-red text-tag-red-foreground',
}

export const tagColorSwatchClasses: Record<TagColorToken, string> = {
  'tag-gray': 'bg-tag-gray',
  'tag-orange': 'bg-tag-orange',
  'tag-yellow': 'bg-tag-yellow',
  'tag-green': 'bg-tag-green',
  'tag-blue': 'bg-tag-blue',
  'tag-purple': 'bg-tag-purple',
  'tag-pink': 'bg-tag-pink',
  'tag-red': 'bg-tag-red',
}
