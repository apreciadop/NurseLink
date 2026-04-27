import { onMounted, watch } from 'vue'
import ReportModal from '../views/ReportModal.vue'
import { useNurseDashboard } from '../composables/useNurseDashboard'

export default {
  name: 'NurseDashboardView',

  components: {
    ReportModal
  },

  setup() {
    const nurseDashboard = useNurseDashboard()

    watch(nurseDashboard.searchTerm, () => {
      nurseDashboard.resetPatientsPage()
    })

    watch(nurseDashboard.activeFilter, () => {
      nurseDashboard.resetPatientsPage()
    })

    watch(nurseDashboard.statusFilter, () => {
      nurseDashboard.resetPatientsPage()
    })

    watch(nurseDashboard.totalPages, () => {
      if (nurseDashboard.currentPage.value > nurseDashboard.totalPages.value) {
        nurseDashboard.currentPage.value = nurseDashboard.totalPages.value
      }
    })

    watch(nurseDashboard.reportsTotalPages, () => {
      if (nurseDashboard.reportsCurrentPage.value > nurseDashboard.reportsTotalPages.value) {
        nurseDashboard.reportsCurrentPage.value = nurseDashboard.reportsTotalPages.value
      }
    })

    onMounted(() => {
      nurseDashboard.loadAssignedPatients()
    })

    return {
      ...nurseDashboard
    }
  }
}