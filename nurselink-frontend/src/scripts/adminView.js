import { logoutUser } from '../services/authService'

export default {
  name: 'AdminView',

  data() {
    return {
      isSidebarOpen: false,
      showLogoutConfirm: false
    }
  },

  computed: {
    isDashboardSection() {
      return this.$route.path === '/admin/dashboard'
    },

    isNursesSection() {
      return this.$route.path.startsWith('/admin/nurses')
    },

    isPatientsSection() {
      return this.$route.path.startsWith('/admin/patients')
    },

    isAssignmentsSection() {
      return this.$route.path.startsWith('/admin/assignments')
    },

    currentSectionTitle() {
      if (this.isNursesSection) {
        return 'Nurses'
      }

      if (this.isPatientsSection) {
        return 'Patients'
      }

      if (this.isAssignmentsSection) {
        return 'Assignments'
      }

      return 'Dashboard'
    },

    currentSectionIcon() {
      if (this.isNursesSection) {
        return '/icons/nurseBlue.png'
      }

      if (this.isPatientsSection) {
        return '/icons/patientBlue.png'
      }

      if (this.isAssignmentsSection) {
        return '/icons/assignmentBlue.png'
      }

      return '/icons/dashboardBlue.png'
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