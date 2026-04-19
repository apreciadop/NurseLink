import { logoutUser } from '../services/authService'

export default {
  name: 'PatientView',

  data() {
    return {
      isSidebarOpen: false,
      showLogoutConfirm: false
    }
  },

  computed: {
    isDashboardSection() {
      return this.$route.path === '/patient/dashboard'
    },

    currentSectionTitle() {
      return 'Dashboard'
    },

    currentSectionIcon() {
      return '/icons/dashboardBlue.png'
    },

    patientName() {
      return localStorage.getItem('name') || ''
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