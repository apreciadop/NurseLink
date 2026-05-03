<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { logoutUser } from '../services/authService'
import { getNurseConversations } from '../services/nurseService'

const route = useRoute()
const router = useRouter()

const isSidebarOpen = ref(false)
const showLogoutConfirm = ref(false)
const unreadMessagesCount = ref(0)

let unreadPollingId = null
let isRefreshingUnread = false

const isDashboardSection = computed(() => {
  return (
    route.path === '/nurse/dashboard' ||
    route.path.startsWith('/nurse/patients/')
  )
})

const isMessagesSection = computed(() => {
  return route.path.startsWith('/nurse/messages')
})

const isPatientProfileSection = computed(() => {
  return route.path.startsWith('/nurse/patients/')
})

const currentSectionTitle = computed(() => {
  if (isMessagesSection.value) {
    return 'Messages'
  }

  if (isPatientProfileSection.value) {
    return 'Patient Profile'
  }

  return 'Dashboard'
})

const currentSectionIcon = computed(() => {
  if (isMessagesSection.value) {
    return '/icons/messagesBlue.png'
  }

  return '/icons/dashboardBlue.png'
})

const nurseName = computed(() => {
  const storedName =
    localStorage.getItem('userName') ||
    localStorage.getItem('name') ||
    localStorage.getItem('fullName')

  if (storedName) {
    return storedName
  }

  const userData = localStorage.getItem('user')

  if (!userData) {
    return ''
  }

  try {
    const user = JSON.parse(userData)

    return (
      user.fullName ||
      user.name ||
      `${user.firstName || ''} ${user.lastName || ''}`.trim()
    )
  } catch {
    return ''
  }
})

const unreadMessagesBadgeText = computed(() => {
  return unreadMessagesCount.value > 9 ? '9+' : String(unreadMessagesCount.value)
})

const getCurrentNurseId = () => {
  const userId = localStorage.getItem('userId')

  if (userId && userId !== 'undefined' && userId !== 'null') {
    return Number(userId)
  }

  return null
}

const refreshUnreadMessagesCount = async () => {
  const nurseId = getCurrentNurseId()

  if (!nurseId || isRefreshingUnread) {
    return
  }

  isRefreshingUnread = true

  try {
    const conversations = await getNurseConversations(nurseId)

    unreadMessagesCount.value = (conversations ?? []).reduce((total, conversation) => {
      return total + Number(conversation.unreadCount ?? 0)
    }, 0)
  } catch (error) {
    console.error('Error refreshing nurse unread messages count:', error)
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
  <section class="nurse">
    <aside :class="['nurse-sidebar', isSidebarOpen ? 'nurse-sidebar-open' : '']">
      <div class="nurse-sidebarbrand">
        <img src="/logo.png" alt="NurseLink logo" class="nurse-sidebarlogo" />
      </div>

      <nav class="nurse-sidebarnav">
        <ul class="nurse-sidebarmenu">
          <li class="nurse-sidebaritem">
            <router-link to="/nurse/dashboard" :class="['nurse-sidebarlink', isDashboardSection ? 'nurse-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/dashboardBlack.png" alt="" class="nurse-sidebaricon nurse-sidebaricon-default" />
              <img src="/icons/dashboardBlue.png" alt="" class="nurse-sidebaricon nurse-sidebaricon-active" />
              <span>Dashboard</span>
            </router-link>
          </li>

          <li class="nurse-sidebaritem">
            <router-link to="/nurse/messages" :class="['nurse-sidebarlink', isMessagesSection ? 'nurse-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/messagesBlack.png" alt="" class="nurse-sidebaricon nurse-sidebaricon-default" />
              <img src="/icons/messagesBlue.png" alt="" class="nurse-sidebaricon nurse-sidebaricon-active" />
              <span>Messages</span>
              <span v-if="unreadMessagesCount > 0" class="nurse-sidebarbadge">{{ unreadMessagesBadgeText }}</span>
            </router-link>
          </li>
        </ul>
      </nav>
    </aside>

    <button v-if="isSidebarOpen" type="button" class="nurse-overlay" aria-label="Close sidebar" @click="closeSidebar"></button>

    <section class="nurse-main">
      <header class="nurse-topbar">
        <button type="button" class="nurse-topbarmenu-button" @click="toggleSidebar">Menu</button>

        <div class="nurse-topbarheading">
          <img :src="currentSectionIcon" alt="" class="nurse-topbarheading-icon" />
          <h1 class="nurse-topbartitle">{{ currentSectionTitle }}</h1>
          <span v-if="nurseName" class="nurse-topbarname">{{ nurseName }}</span>
        </div>

        <button type="button" class="nurse-topbarlogout" @click="openLogoutConfirm">Logout</button>
      </header>

      <main class="nurse-content">
        <router-view />
      </main>

      <footer class="nurse-footer">
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

<style src="../styles/nurse.css"></style>