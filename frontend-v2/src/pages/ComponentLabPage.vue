<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, FlaskConical, PackageOpen } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import ContentBlockCard from '@/components/business/ContentBlockCard.vue'
import FocusTree from '@/components/business/FocusTree.vue'
import SectionVariantCard from '@/components/business/SectionVariantCard.vue'
import EmptyState from '@/components/presentation/EmptyState.vue'
import PageHeader from '@/components/presentation/PageHeader.vue'
import StatusPill from '@/components/presentation/StatusPill.vue'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { componentLabScenarios } from '@/labs'
import {
  emptyFocusTreeNodes,
  mockContentBlocks,
  mockFocusTreeNodes,
  mockSectionVariants,
  scaffoldChecks,
} from '@/mocks'

const { t } = useI18n()

const scenarioIds = computed(() => componentLabScenarios.map((item) => item.id))
const checkIds = computed(() => scaffoldChecks.map((item) => item.id))
const selectedBlockId = ref(mockContentBlocks[0]?.id)
const selectedVariantId = ref(mockSectionVariants[0]?.id)
const selectedTreeNodeId = ref('node-law')
</script>

<template>
  <div class="space-y-6">
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

    <section class="grid grid-cols-[minmax(0,1fr)] gap-4 lg:grid-cols-[320px_minmax(0,1fr)]">
      <Card>
        <CardHeader>
          <CardTitle>{{ t('lab.summaryTitle') }}</CardTitle>
          <CardDescription>{{ t('lab.summaryDescription') }}</CardDescription>
        </CardHeader>
        <CardContent class="space-y-2">
          <div
            v-for="checkId in checkIds"
            :key="checkId"
            class="rounded-lg border px-3 py-2 text-sm"
          >
            {{ t(`lab.checks.${checkId}`) }}
          </div>
          <div class="rounded-lg border bg-muted/30 px-3 py-2 text-sm text-muted-foreground">
            {{ t('lab.scenarioCount') }} {{ scenarioIds.length }}
          </div>
        </CardContent>
      </Card>

      <div class="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>{{ t('lab.sections.presentation.title') }}</CardTitle>
            <CardDescription>{{ t('lab.sections.presentation.description') }}</CardDescription>
          </CardHeader>
          <CardContent class="grid grid-cols-[minmax(0,1fr)] gap-4 xl:grid-cols-2">
            <EmptyState
              :title="t('emptyState.lab.title')"
              :description="t('emptyState.lab.description')"
              :action-label="t('emptyState.lab.action')"
            >
              <template #icon>
                <PackageOpen class="size-5" aria-hidden="true" />
              </template>
            </EmptyState>
            <div class="rounded-lg border bg-card p-4">
              <p class="mb-3 text-sm font-medium">{{ t('lab.statusPillTitle') }}</p>
              <div class="flex flex-wrap gap-2">
                <StatusPill :label="t('lab.status.ready')" tone="active" />
                <StatusPill :label="t('lab.status.neutral')" />
                <StatusPill :label="t('lab.status.muted')" tone="muted" />
                <StatusPill :label="t('lab.status.danger')" tone="danger" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{{ t('lab.sections.contentBlockCard.title') }}</CardTitle>
            <CardDescription>{{ t('lab.sections.contentBlockCard.description') }}</CardDescription>
          </CardHeader>
          <CardContent class="grid grid-cols-[minmax(0,1fr)] gap-4 xl:grid-cols-2">
            <ContentBlockCard
              v-for="block in mockContentBlocks"
              :key="block.id"
              :block="block"
              :selected="selectedBlockId === block.id"
              @select="selectedBlockId = $event"
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{{ t('lab.sections.sectionVariantCard.title') }}</CardTitle>
            <CardDescription>{{ t('lab.sections.sectionVariantCard.description') }}</CardDescription>
          </CardHeader>
          <CardContent class="grid grid-cols-[minmax(0,1fr)] gap-4 xl:grid-cols-3">
            <SectionVariantCard
              v-for="variant in mockSectionVariants"
              :key="variant.id"
              :variant="variant"
              :selected="selectedVariantId === variant.id"
              @select="selectedVariantId = $event"
            />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle class="flex items-center gap-2">
              <FlaskConical class="size-4" />
              {{ t('lab.sections.focusTree.title') }}
            </CardTitle>
            <CardDescription>
              {{ t('lab.sections.focusTree.description') }}
            </CardDescription>
          </CardHeader>
          <CardContent class="grid grid-cols-[minmax(0,1fr)] gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
            <FocusTree
              :nodes="mockFocusTreeNodes"
              :selected-node-id="selectedTreeNodeId"
              :expand-label="t('components.focusTree.expand')"
              :collapse-label="t('components.focusTree.collapse')"
              @select="selectedTreeNodeId = $event"
            />
            <EmptyState
              v-if="emptyFocusTreeNodes.length === 0"
              :title="t('components.focusTree.emptyTitle')"
              :description="t('components.focusTree.emptyDescription')"
            />
          </CardContent>
          <CardFooter class="justify-between border-t pt-4">
            <p class="text-sm text-muted-foreground">
              {{ t('lab.selectedNodeLabel') }} {{ selectedTreeNodeId }}
            </p>
            <Button variant="outline" size="sm">
              {{ t('lab.previewAction') }}
            </Button>
          </CardFooter>
        </Card>
      </div>
    </section>
  </div>
</template>
