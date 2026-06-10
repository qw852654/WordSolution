<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import SectionInspector from '@/components/business/SectionInspector.vue'
import SectionStructurePanel from '@/components/containers/SectionStructurePanel.vue'
import SectionTopToolbar from '@/components/containers/SectionTopToolbar.vue'
import SectionWorkspace from '@/components/containers/SectionWorkspace.vue'
import { mockSectionItemViewShells, mockSectionPageShells } from '@/mocks'
import type { SectionPageShellModel } from '@/types'

const route = useRoute()

const sectionId = computed(() => {
  const value = route.params.sectionId
  return Array.isArray(value) ? value.join('/') : value
})

const sectionShell = computed<SectionPageShellModel>(() => {
  const id = sectionId.value || 'demo-section'
  const matched = mockSectionPageShells.find((section) => section.sectionId === id)

  return matched ?? {
    sectionId: id,
    title: `Section ${id}`,
    teachingTopicTitle: 'Mock Data',
    status: '骨架验收',
  }
})
</script>

<template>
  <main class="min-h-screen bg-background text-foreground xl:h-screen xl:overflow-hidden">
    <section class="grid min-h-screen grid-cols-[minmax(0,1fr)] gap-3 p-3 xl:h-full xl:min-h-0 xl:grid-cols-[240px_minmax(0,1fr)_280px]">
      <SectionStructurePanel />
      <SectionWorkspace :section="sectionShell" :items="mockSectionItemViewShells" />

      <aside class="flex min-h-0 flex-col gap-3">
        <SectionTopToolbar />
        <SectionInspector class="min-h-0 flex-1" />
      </aside>
    </section>
  </main>
</template>
