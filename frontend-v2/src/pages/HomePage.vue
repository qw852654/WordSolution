<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowRight, FlaskConical, Network } from 'lucide-vue-next'
import { useI18n } from 'vue-i18n'
import { CMS_V2_API_BASE } from '@/apis/cmsV2Client'
import PageHeader from '@/components/presentation/PageHeader.vue'
import { scaffoldChecks } from '@/mocks'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

const { t } = useI18n()

const checklistIds = computed(() => scaffoldChecks.map((item) => item.id))
</script>

<template>
  <div class="space-y-6">
    <PageHeader
      :eyebrow="t('home.stageLabel')"
      :title="t('home.title')"
      :description="t('home.description')"
    >
      <template #actions>
        <Button as-child>
          <RouterLink to="/topics">
            <Network class="size-4" />
            {{ t('home.openTopics') }}
          </RouterLink>
        </Button>
        <Button variant="outline" as-child>
          <RouterLink to="/lab">
            <FlaskConical class="size-4" />
            {{ t('home.openLab') }}
          </RouterLink>
        </Button>
      </template>
    </PageHeader>

    <section class="grid grid-cols-[minmax(0,1fr)] gap-4 lg:grid-cols-[minmax(0,1fr)_320px]">
      <Card>
        <CardHeader>
          <CardTitle>{{ t('home.statusTitle') }}</CardTitle>
          <CardDescription>{{ t('home.statusDescription') }}</CardDescription>
        </CardHeader>
        <CardContent class="space-y-4">
          <div class="rounded-lg border bg-muted/30 px-4 py-3">
            <p class="text-xs uppercase tracking-wide text-muted-foreground">
              {{ t('home.apiBaseLabel') }}
            </p>
            <p class="mt-2 font-mono text-sm">{{ CMS_V2_API_BASE }}</p>
          </div>

          <ul class="space-y-2">
            <li
              v-for="checkId in checklistIds"
              :key="checkId"
              class="rounded-lg border px-3 py-2 text-sm"
            >
              {{ t(`home.checklist.${checkId}`) }}
            </li>
          </ul>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{{ t('home.boundaryTitle') }}</CardTitle>
          <CardDescription>{{ t('home.boundaryDescription') }}</CardDescription>
        </CardHeader>
        <CardContent class="space-y-3 text-sm text-muted-foreground">
          <div class="flex items-start gap-3 rounded-lg border px-3 py-3">
            <Network class="mt-0.5 size-4 shrink-0" />
            <p>{{ t('home.boundaryBody') }}</p>
          </div>
        </CardContent>
        <CardFooter>
          <Button variant="outline" as-child>
            <RouterLink to="/topics">
              {{ t('home.openTopics') }}
              <ArrowRight class="size-4" />
            </RouterLink>
          </Button>
        </CardFooter>
      </Card>
    </section>
  </div>
</template>
