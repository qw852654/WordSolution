<template>
  <RouterView v-if="isStandaloneLayout" />
  <AppShell v-else />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import AppShell from '@/components/containers/AppShell.vue'

const route = useRoute()

const isStandaloneLayout = computed(() => route.meta.layout === 'standalone')
</script>
