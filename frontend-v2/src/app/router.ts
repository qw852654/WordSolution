import { createRouter, createWebHistory } from 'vue-router'
import ComponentLabPage from '@/pages/ComponentLabPage.vue'
import ContentBlocksPage from '@/pages/ContentBlocksPage.vue'
import HandoutManagementPage from '@/pages/HandoutManagementPage.vue'
import HandoutPage from '@/pages/HandoutPage.vue'
import HomePage from '@/pages/HomePage.vue'
import PlaceholderPage from '@/pages/PlaceholderPage.vue'
import SectionPage from '@/pages/SectionPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomePage,
    },
    {
      path: '/lab',
      name: 'lab',
      component: ComponentLabPage,
      meta: {
        layout: 'standalone',
      },
    },
    {
      path: '/topics',
      name: 'topics',
      component: PlaceholderPage,
      props: {
        eyebrowKey: 'routes.topics.eyebrow',
        titleKey: 'routes.topics.title',
        descriptionKey: 'routes.topics.description',
        emptyTitleKey: 'routes.topics.emptyTitle',
        emptyDescriptionKey: 'routes.topics.emptyDescription',
      },
    },
    {
      path: '/sections/:sectionId',
      name: 'section-page',
      component: SectionPage,
      meta: {
        layout: 'standalone',
      },
    },
    {
      path: '/handouts',
      name: 'handout-management',
      component: HandoutManagementPage,
      meta: {
        layout: 'standalone',
      },
    },
    {
      path: '/handouts/:handoutVersionId',
      name: 'handout-page',
      component: HandoutPage,
      meta: {
        layout: 'standalone',
      },
    },
    {
      path: '/content-blocks',
      name: 'content-blocks',
      component: ContentBlocksPage,
    },
    {
      path: '/content-blocks/:contentBlockId',
      name: 'content-block-placeholder',
      component: PlaceholderPage,
      props: {
        eyebrowKey: 'routes.contentBlockDetail.eyebrow',
        titleKey: 'routes.contentBlockDetail.title',
        descriptionKey: 'routes.contentBlockDetail.description',
        emptyTitleKey: 'routes.contentBlockDetail.emptyTitle',
        emptyDescriptionKey: 'routes.contentBlockDetail.emptyDescription',
        paramName: 'contentBlockId',
      },
    },
    {
      path: '/outputs/:outputFormId',
      name: 'output-placeholder',
      component: PlaceholderPage,
      props: {
        eyebrowKey: 'routes.outputs.eyebrow',
        titleKey: 'routes.outputs.title',
        descriptionKey: 'routes.outputs.description',
        emptyTitleKey: 'routes.outputs.emptyTitle',
        emptyDescriptionKey: 'routes.outputs.emptyDescription',
        paramName: 'outputFormId',
      },
    },
  ],
  scrollBehavior() {
    return { top: 0 }
  },
})

export default router
