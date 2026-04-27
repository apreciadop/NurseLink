import { logoutUser } from '../services/authService'

export default {
  name: 'NurseView',

  data() {
    return {
      isSidebarOpen: false,
      showLogoutConfirm: false
    }
  },

  computed: {
    isDashboardSection() {
      return (
        this.$route.path === '/nurse/dashboard' ||
        this.$route.path.startsWith('/nurse/patients/')
      )
    },

    isMessagesSection() {
      return this.$route.path.startsWith('/nurse/messages')
    },

    isPatientProfileSection() {
      return this.$route.path.startsWith('/nurse/patients/')
    },

    currentSectionTitle() {
      if (this.isMessagesSection) {
        return 'Messages'
      }

      if (this.isPatientProfileSection) {
        return 'Patient Profile'
      }

      return 'Dashboard'
    },

    currentSectionIcon() {
      if (this.isMessagesSection) {
        return '/icons/messagesBlue.png'
      }

      return '/icons/dashboardBlue.png'
    },

    nurseName() {
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
    }
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
    }
  }
}