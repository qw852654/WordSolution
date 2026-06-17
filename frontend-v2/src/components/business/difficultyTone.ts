const difficultyMarkerClasses = {
  unset: 'bg-difficulty-unset',
  basic: 'bg-difficulty-basic',
  medium: 'bg-difficulty-medium',
  advanced: 'bg-difficulty-advanced',
  top: 'bg-difficulty-top',
} as const

const difficultyAliases: Record<string, keyof typeof difficultyMarkerClasses> = {
  unset: 'unset',
  '未设置': 'unset',
  basic: 'basic',
  foundation: 'basic',
  '基础': 'basic',
  medium: 'medium',
  intermediate: 'medium',
  '中档': 'medium',
  advanced: 'advanced',
  '提高': 'advanced',
  top: 'top',
  '压轴': 'top',
}

export function getDifficultyMarkerClass(difficulty?: string | null) {
  const normalizedDifficulty = difficulty?.trim().toLowerCase()
  const tone = normalizedDifficulty ? difficultyAliases[normalizedDifficulty] : undefined

  return difficultyMarkerClasses[tone ?? 'unset']
}
