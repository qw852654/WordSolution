import { createApp } from 'vue'
import i18n from '@/app/i18n'
import pinia from '@/app/pinia'
import router from '@/app/router'
import '@/styles/index.css'
import App from './App.vue'

createApp(App).use(pinia).use(router).use(i18n).mount('#app')
