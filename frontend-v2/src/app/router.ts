import { createRouter, createWebHistory } from 'vue-router'
import ComponentLabPage from '@/pages/ComponentLabPage.vue'
import HomePage from '@/pages/HomePage.vue'
import PlaceholderPage from '@/pages/PlaceholderPage.vue'

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
      name: 'section-placeholder',
      component: PlaceholderPage,
      props: {
        eyebrowKey: 'routes.sections.eyebrow',
        titleKey: 'routes.sections.title',
        descriptionKey: 'routes.sections.description',
        emptyTitleKey: 'routes.sections.emptyTitle',
        emptyDescriptionKey: 'routes.sections.emptyDescription',
        paramName: 'sectionId',
      },
    },
    {
      path: '/handouts/:handoutVersionId',
      name: 'handout-placeholder',
      component: PlaceholderPage,
      props: {
        eyebrowKey: 'routes.handouts.eyebrow',
        titleKey: 'routes.handouts.title',
        descriptionKey: 'routes.handouts.description',
        emptyTitleKey: 'routes.handouts.emptyTitle',
        emptyDescriptionKey: 'routes.handouts.emptyDescription',
        paramName: 'handoutVersionId',
      },
    },
    {
      path: '/content-blocks',
      name: 'content-blocks',
      component: PlaceholderPage,
      props: {
        eyebrowKey: 'routes.contentBlocks.eyebrow',
        titleKey: 'routes.contentBlocks.title',
        descriptionKey: 'routes.contentBlocks.description',
        emptyTitleKey: 'routes.contentBlocks.emptyTitle',
        emptyDescriptionKey: 'routes.contentBlocks.emptyDescription',
      },
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
