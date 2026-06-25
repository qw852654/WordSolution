import type { Config } from 'tailwindcss'
import animate from 'tailwindcss-animate'

export default {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,tsx,vue}'],
  theme: {
    extend: {
      colors: {
        border: 'var(--border)',
        input: 'var(--input)',
        ring: 'var(--ring)',
        background: 'var(--background)',
        foreground: 'var(--foreground)',
        primary: {
          DEFAULT: 'var(--primary)',
          foreground: 'var(--primary-foreground)',
        },
        secondary: {
          DEFAULT: 'var(--secondary)',
          foreground: 'var(--secondary-foreground)',
        },
        destructive: {
          DEFAULT: 'var(--destructive)',
          foreground: 'var(--primary-foreground)',
        },
        muted: {
          DEFAULT: 'var(--muted)',
          foreground: 'var(--muted-foreground)',
        },
        accent: {
          DEFAULT: 'var(--accent)',
          foreground: 'var(--accent-foreground)',
        },
        'section-tree-context-target': {
          DEFAULT: 'var(--section-tree-context-target)',
          foreground: 'var(--section-tree-context-target-foreground)',
          ring: 'var(--section-tree-context-target-ring)',
        },
        'difficulty-unset': 'var(--difficulty-unset)',
        'difficulty-basic': 'var(--difficulty-basic)',
        'difficulty-medium': 'var(--difficulty-medium)',
        'difficulty-advanced': 'var(--difficulty-advanced)',
        'difficulty-top': 'var(--difficulty-top)',
        'tag-gray': {
          DEFAULT: 'var(--tag-gray)',
          foreground: 'var(--tag-gray-foreground)',
          border: 'var(--tag-gray-border)',
        },
        'tag-orange': {
          DEFAULT: 'var(--tag-orange)',
          foreground: 'var(--tag-orange-foreground)',
          border: 'var(--tag-orange-border)',
        },
        'tag-yellow': {
          DEFAULT: 'var(--tag-yellow)',
          foreground: 'var(--tag-yellow-foreground)',
          border: 'var(--tag-yellow-border)',
        },
        'tag-green': {
          DEFAULT: 'var(--tag-green)',
          foreground: 'var(--tag-green-foreground)',
          border: 'var(--tag-green-border)',
        },
        'tag-blue': {
          DEFAULT: 'var(--tag-blue)',
          foreground: 'var(--tag-blue-foreground)',
          border: 'var(--tag-blue-border)',
        },
        'tag-purple': {
          DEFAULT: 'var(--tag-purple)',
          foreground: 'var(--tag-purple-foreground)',
          border: 'var(--tag-purple-border)',
        },
        'tag-pink': {
          DEFAULT: 'var(--tag-pink)',
          foreground: 'var(--tag-pink-foreground)',
          border: 'var(--tag-pink-border)',
        },
        'tag-red': {
          DEFAULT: 'var(--tag-red)',
          foreground: 'var(--tag-red-foreground)',
          border: 'var(--tag-red-border)',
        },
        popover: {
          DEFAULT: 'var(--popover)',
          foreground: 'var(--popover-foreground)',
        },
        card: {
          DEFAULT: 'var(--card)',
          foreground: 'var(--card-foreground)',
        },
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
    },
  },
  plugins: [animate],
} satisfies Config
