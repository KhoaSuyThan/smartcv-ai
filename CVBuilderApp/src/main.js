import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import CustomSectionItem from './components/CustomSectionItem.vue'

const app = createApp(App)
app.component('CustomSectionItem', CustomSectionItem)
app.mount('#app')
