<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { logoutUser } from '../services/authService'
import {
  getConversationMessages,
  getOrCreateConversationForPatient
} from '../services/patientService'

const route = useRoute()
const router = useRouter()

const isSidebarOpen = ref(false)
const showLogoutConfirm = ref(false)
const unreadMessagesCount = ref(0)

let unreadPollingId = null
let isRefreshingUnread = false

const isDashboardSection = computed(() => {
  return route.path === '/patient/dashboard'
})

const isMessagesSection = computed(() => {
  return route.path.startsWith('/patient/messages')
})

const currentSectionTitle = computed(() => {
  if (isMessagesSection.value) {
    return 'Messages'
  }

  return 'Dashboard'
})

const currentSectionIcon = computed(() => {
  if (isMessagesSection.value) {
    return '/icons/messagesBlue.png'
  }

  return '/icons/dashboardBlue.png'
})

const patientName = computed(() => {
  return localStorage.getItem('name') || ''
})

const unreadMessagesBadgeText = computed(() => {
  return unreadMessagesCount.value > 9 ? '9+' : String(unreadMessagesCount.value)
})

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const closeSidebar = () => {
  isSidebarOpen.value = false
}

const openLogoutConfirm = () => {
  showLogoutConfirm.value = true
}

const cancelLogout = () => {
  showLogoutConfirm.value = false
}

const confirmLogout = () => {
  logoutUser()
  showLogoutConfirm.value = false
  router.push('/login')
}

const getCurrentPatientId = () => {
  const userId = localStorage.getItem('userId')

  if (userId && userId !== 'undefined' && userId !== 'null') {
    return Number(userId)
  }

  return null
}

const refreshUnreadMessagesCount = async () => {
  const patientId = getCurrentPatientId()

  if (!patientId || isRefreshingUnread) {
    return
  }

  isRefreshingUnread = true

  try {
    const conversation = await getOrCreateConversationForPatient(patientId)

    if (!conversation?.conversationId) {
      unreadMessagesCount.value = 0
      return
    }

    const messagesResponse = await getConversationMessages(
      conversation.conversationId,
      { patientId }
    )

    const messages = messagesResponse?.messages ?? []

    unreadMessagesCount.value = messages.filter(message => {
      return !message.messageSenderIsPatient && !message.messageRead
    }).length
  } catch (error) {
    console.error('Error refreshing unread messages count:', error)
  } finally {
    isRefreshingUnread = false
  }
}

const startUnreadPolling = () => {
  stopUnreadPolling()

  unreadPollingId = window.setInterval(() => {
    if (document.hidden) {
      return
    }

    refreshUnreadMessagesCount()
  }, 5000)
}

const stopUnreadPolling = () => {
  if (unreadPollingId) {
    window.clearInterval(unreadPollingId)
    unreadPollingId = null
  }
}

watch(
  () => route.path,
  async () => {
    await refreshUnreadMessagesCount()
  },
  { immediate: true }
)

onMounted(() => {
  refreshUnreadMessagesCount()
  startUnreadPolling()
})

onBeforeUnmount(() => {
  stopUnreadPolling()
})
</script>

<template>
  <section class="patient">
    <aside :class="['patient-sidebar', isSidebarOpen ? 'patient-sidebar-open' : '']">
      <div class="patient-sidebarbrand">
        <img src="/logo.png" alt="NurseLink logo" class="patient-sidebarlogo" />
      </div>

      <nav class="patient-sidebarnav">
        <ul class="patient-sidebarmenu">
          <li class="patient-sidebaritem">
            <router-link to="/patient/dashboard" :class="['patient-sidebarlink', isDashboardSection ? 'patient-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/dashboardBlack.png" alt="" class="patient-sidebaricon patient-sidebaricon-default" />
              <img src="/icons/dashboardBlue.png" alt="" class="patient-sidebaricon patient-sidebaricon-active" />
              <span>Dashboard</span>
            </router-link>
          </li>

          <li class="patient-sidebaritem">
            <router-link to="/patient/messages" :class="['patient-sidebarlink', isMessagesSection ? 'patient-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/messagesBlack.png" alt="" class="patient-sidebaricon patient-sidebaricon-default" />
              <img src="/icons/messagesBlue.png" alt="" class="patient-sidebaricon patient-sidebaricon-active" />
              <span>Messages</span>
              <span v-if="unreadMessagesCount > 0" class="patient-sidebarbadge">{{ unreadMessagesBadgeText }}</span>
            </router-link>
          </li>
        </ul>
      </nav>
    </aside>

    <button v-if="isSidebarOpen" type="button" class="patient-overlay" aria-label="Close sidebar" @click="closeSidebar"></button>

    <section class="patient-main">
      <header class="patient-topbar">
        <button type="button" class="patient-topbarmenu-button" @click="toggleSidebar">Menu</button>

        <div class="patient-topbarheading">
          <img :src="currentSectionIcon" alt="" class="patient-topbarheading-icon" />
          <h1 class="patient-topbartitle">{{ currentSectionTitle }}</h1>
          <span v-if="patientName" class="patient-topbarname">{{ patientName }}</span>
        </div>

        <button type="button" class="patient-topbarlogout" @click="openLogoutConfirm">Logout</button>
      </header>

      <main class="patient-content">
        <router-view />
      </main>

      <footer class="patient-footer">
        <span>© 2026 NurseLink - Version 1.0</span>
      </footer>
    </section>

    <section v-if="showLogoutConfirm" class="logout-modal">
      <article class="logout-modalcard">
        <p>Are you sure you want to log out?</p>

        <div class="logout-modalactions">
          <button type="button" class="logout-modalcancel" @click="cancelLogout">Cancel</button>
          <button type="button" class="logout-modalconfirm" @click="confirmLogout">Logout</button>
        </div>
      </article>
    </section>
  </section>
</template>

<style src="../styles/patient.css"></style>