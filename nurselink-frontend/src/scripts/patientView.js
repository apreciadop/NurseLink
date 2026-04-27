import { logoutUser } from '../services/authService'
import {
  getConversationMessages,
  getOrCreateConversationForPatient
} from '../services/patientService'

export default {
  name: 'PatientView',

  data() {
    return {
      isSidebarOpen: false,
      showLogoutConfirm: false,
      unreadMessagesCount: 0,
      unreadPollingId: null,
      isRefreshingUnread: false
    }
  },

  computed: {
    isDashboardSection() {
      return this.$route.path === '/patient/dashboard'
    },

    isMessagesSection() {
      return this.$route.path.startsWith('/patient/messages')
    },

    currentSectionTitle() {
      if (this.isMessagesSection) {
        return 'Messages'
      }

      return 'Dashboard'
    },

    currentSectionIcon() {
      if (this.isMessagesSection) {
        return '/icons/messagesBlue.png'
      }

      return '/icons/dashboardBlue.png'
    },

    patientName() {
      return localStorage.getItem('name') || ''
    },

    unreadMessagesBadgeText() {
      return this.unreadMessagesCount > 9 ? '9+' : String(this.unreadMessagesCount)
    }
  },

  watch: {
    '$route.path': {
      immediate: true,
      async handler() {
        await this.refreshUnreadMessagesCount()
      }
    }
  },

  mounted() {
    this.refreshUnreadMessagesCount()
    this.startUnreadPolling()
  },

  beforeUnmount() {
    this.stopUnreadPolling()
  },

  methods: {
    toggleSidebar() {
      this.isSidebarOpen = !this.isSidebarOpen
    },

    closeSidebar() {
      this.isSidebarOpen = false
    },

    openLogoutConfirm() {
      this.showLogoutConfirm = true
    },

    cancelLogout() {
      this.showLogoutConfirm = false
    },

    confirmLogout() {
      logoutUser()
      this.showLogoutConfirm = false
      this.$router.push('/login')
    },

    getCurrentPatientId() {
      const userId = localStorage.getItem('userId')

      if (userId && userId !== 'undefined' && userId !== 'null') {
        return Number(userId)
      }

      return null
    },

    async refreshUnreadMessagesCount() {
      const patientId = this.getCurrentPatientId()

      if (!patientId || this.isRefreshingUnread) {
        return
      }

      this.isRefreshingUnread = true

      try {
        const conversation = await getOrCreateConversationForPatient(patientId)

        if (!conversation?.conversationId) {
          this.unreadMessagesCount = 0
          return
        }

        const messagesResponse = await getConversationMessages(
          conversation.conversationId,
          { patientId }
        )

        const messages = messagesResponse?.messages ?? []

        this.unreadMessagesCount = messages.filter(message => {
          return !message.messageSenderIsPatient && !message.messageRead
        }).length
      } catch (error) {
        console.error('Error refreshing unread messages count:', error)
      } finally {
        this.isRefreshingUnread = false
      }
    },

    startUnreadPolling() {
      this.stopUnreadPolling()

      this.unreadPollingId = window.setInterval(() => {
        if (document.hidden) {
          return
        }

        this.refreshUnreadMessagesCount()
      }, 5000)
    },

    stopUnreadPolling() {
      if (this.unreadPollingId) {
        window.clearInterval(this.unreadPollingId)
        this.unreadPollingId = null
      }
    }
  }
}