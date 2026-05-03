import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

import './styles/variables.css'
import './styles/common/messages.css'
import './styles/common/pagination.css'
import './styles/common/badges.css'
import './styles/common/action-buttons.css'
import './styles/common/forms.css'

createApp(App).use(router).mount('#app')