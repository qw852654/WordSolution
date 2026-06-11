<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, Blocks, FileText, GitBranchPlus, MousePointer2 } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import AtomicSectionBlock from '@/components/business/AtomicSectionBlock.vue'
import CompositeBlock from '@/components/business/CompositeBlock.vue'
import ContentBlockDisplay from '@/components/business/ContentBlockDisplay.vue'
import SectionItemView from '@/components/business/SectionItemView.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import InsertPoint from '@/components/presentation/InsertPoint.vue'
import { Button } from '@/components/ui/button'
import { mockContentBlockDisplays, mockInsertPoints, mockStructuredBlocks } from '@/mocks'

const { t } = useI18n()

const atomicBlocks = computed(() => mockStructuredBlocks.filter((block) => block.blockKind === 'AtomicSection'))
const compositeBlocks = computed(() => mockStructuredBlocks.filter((block) => block.blockKind === 'CompositeBlock'))
const selectedContentBlock = computed(() => mockContentBlockDisplays[0])
const selectedAtomicBlock = computed(() => atomicBlocks.value[0])
const selectedCompositeBlock = computed(() => compositeBlocks.value[0])
</script>

<template>
  <main class="min-h-screen bg-background px-4 py-6 text-foreground sm:px-6 lg:px-8">
    <PageHeader
      :eyebrow="t('lab.eyebrow')"
      :title="t('lab.title')"
      :description="t('lab.description')"
    >
      <template #actions>
        <Button variant="outline" as-child>
          <RouterLink to="/">
            <ArrowLeft class="size-4" />
            {{ t('lab.backHome') }}
          </RouterLink>
        </Button>
      </template>
    </PageHeader>

    <section class="mt-6 space-y-6">
      <section class="space-y-3" :aria-label="t('lab.sections.contentBlockDisplay.title')">
        <div class="flex items-center gap-2">
          <FileText class="size-4" aria-hidden="true" />
          <div>
            <h2 class="text-base font-semibold">{{ t('lab.sections.contentBlockDisplay.title') }}</h2>
            <p class="text-sm text-muted-foreground">{{ t('lab.sections.contentBlockDisplay.description') }}</p>
          </div>
        </div>
        <div class="grid gap-3 xl:grid-cols-2">
          <ContentBlockDisplay
            v-for="block in mockContentBlockDisplays"
            :key="block.id"
            :block="block"
          />
        </div>
      </section>

      <section class="space-y-3" :aria-label="t('lab.sections.structuredBlocks.title')">
        <div class="flex items-center gap-2">
          <Blocks class="size-4" aria-hidden="true" />
          <div>
            <h2 class="text-base font-semibold">{{ t('lab.sections.structuredBlocks.title') }}</h2>
            <p class="text-sm text-muted-foreground">{{ t('lab.sections.structuredBlocks.description') }}</p>
          </div>
        </div>
        <div class="grid gap-3 xl:grid-cols-2">
          <AtomicSectionBlock
            v-for="block in atomicBlocks"
            :key="block.id"
            :block="block"
          />
          <CompositeBlock
            v-for="block in compositeBlocks"
            :key="block.id"
            :block="block"
          />
        </div>
      </section>

      <section class="space-y-3" :aria-label="t('lab.sections.insertPoint.title')">
        <div class="flex items-center gap-2">
          <GitBranchPlus class="size-4" aria-hidden="true" />
          <div>
            <h2 class="text-base font-semibold">{{ t('lab.sections.insertPoint.title') }}</h2>
            <p class="text-sm text-muted-foreground">{{ t('lab.sections.insertPoint.description') }}</p>
          </div>
        </div>
        <div class="space-y-2 rounded-lg border bg-muted/20 p-3">
          <InsertPoint
            v-for="point in mockInsertPoints"
            :key="point.id"
            :point="point"
          >
            <template #default="{ insert }">
              <Button
                type="button"
                size="sm"
                variant="outline"
                class="h-6 px-2 text-xs"
                :disabled="point.disabled"
                @click="insert"
              >
                {{ t('components.insertPoint.contentBlock') }}
              </Button>
              <Button
                type="button"
                size="sm"
                variant="outline"
                class="h-6 px-2 text-xs"
                :disabled="point.disabled"
                @click="insert"
              >
                {{ t('components.insertPoint.atomicSection') }}
              </Button>
              <Button
                type="button"
                size="sm"
                variant="outline"
                class="h-6 px-2 text-xs"
                :disabled="point.disabled"
                @click="insert"
              >
                {{ t('components.insertPoint.compositeBlock') }}
              </Button>
            </template>
          </InsertPoint>
        </div>
      </section>

      <section class="space-y-3" :aria-label="t('lab.sections.sectionItemComposition.title')">
        <div class="flex items-center gap-2">
          <MousePointer2 class="size-4" aria-hidden="true" />
          <div>
            <h2 class="text-base font-semibold">{{ t('lab.sections.sectionItemComposition.title') }}</h2>
            <p class="text-sm text-muted-foreground">{{ t('lab.sections.sectionItemComposition.description') }}</p>
          </div>
        </div>
        <div class="rounded-lg border bg-background p-2">
          <SectionItemView
            v-if="selectedContentBlock"
            item-id="lab-section-item-content-block"
            selected
          >
            <ContentBlockDisplay :block="selectedContentBlock" />
          </SectionItemView>

          <InsertPoint v-if="mockInsertPoints[0]" :point="mockInsertPoints[0]">
            <template #default="{ insert }">
              <Button type="button" size="sm" variant="outline" class="h-6 px-2 text-xs" @click="insert">
                {{ t('components.insertPoint.contentBlock') }}
              </Button>
              <Button type="button" size="sm" variant="outline" class="h-6 px-2 text-xs" @click="insert">
                {{ t('components.insertPoint.atomicSection') }}
              </Button>
              <Button type="button" size="sm" variant="outline" class="h-6 px-2 text-xs" @click="insert">
                {{ t('components.insertPoint.compositeBlock') }}
              </Button>
            </template>
          </InsertPoint>

          <SectionItemView
            v-if="selectedAtomicBlock"
            item-id="lab-section-item-atomic-section"
          >
            <AtomicSectionBlock :block="selectedAtomicBlock" />
          </SectionItemView>

          <InsertPoint v-if="mockInsertPoints[0]" :point="mockInsertPoints[0]">
            <template #default="{ insert }">
              <Button type="button" size="sm" variant="outline" class="h-6 px-2 text-xs" @click="insert">
                {{ t('components.insertPoint.contentBlock') }}
              </Button>
              <Button type="button" size="sm" variant="outline" class="h-6 px-2 text-xs" @click="insert">
                {{ t('components.insertPoint.atomicSection') }}
              </Button>
              <Button type="button" size="sm" variant="outline" class="h-6 px-2 text-xs" @click="insert">
                {{ t('components.insertPoint.compositeBlock') }}
              </Button>
            </template>
          </InsertPoint>

          <SectionItemView
            v-if="selectedCompositeBlock"
            item-id="lab-section-item-composite-block"
          >
            <CompositeBlock :block="selectedCompositeBlock" />
          </SectionItemView>
        </div>
      </section>
    </section>
  </main>
</template>
