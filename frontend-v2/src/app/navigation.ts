import { Blocks, FileText, FlaskConical, FolderOutput, Library, Network } from 'lucide-vue-next'
import type { AppNavItem } from '@/types'

export function createAppNavigation(t: (key: string) => string): AppNavItem[] {
  return [
    {
      id: 'topics',
      to: '/topics',
      label: t('navigation.topics'),
      description: t('navigation.descriptions.topics'),
      icon: Network,
    },
    {
      id: 'sections',
      to: '/sections/demo-section',
      label: t('navigation.sections'),
      description: t('navigation.descriptions.sections'),
      icon: Blocks,
    },
    {
      id: 'handouts',
      to: '/handouts/demo-handout',
      label: t('navigation.handouts'),
      description: t('navigation.descriptions.handouts'),
      icon: FileText,
    },
    {
      id: 'contentBlocks',
      to: '/content-blocks',
      label: t('navigation.contentBlocks'),
      description: t('navigation.descriptions.contentBlocks'),
      icon: Library,
    },
    {
      id: 'outputs',
      to: '/outputs/demo-output',
      label: t('navigation.outputs'),
      description: t('navigation.descriptions.outputs'),
      icon: FolderOutput,
    },
    {
      id: 'lab',
      to: '/lab',
      label: t('navigation.lab'),
      description: t('navigation.descriptions.lab'),
      icon: FlaskConical,
    },
  ]
}
