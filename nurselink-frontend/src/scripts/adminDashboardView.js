import { onMounted } from 'vue'
import { useAdminDashboard } from '../composables/useAdminDashboard'

export default {
  name: 'AdminDashboardView',

  setup() {
    const {
      totalPatients,
      totalNurses,
      totalAlerts,
      totalUnassigned,
      patientsWithAlerts,
      unassignedPatients,
      nurses,
      loading,
      errorMessage,
      isAssignModalOpen,
      selectedPatient,
      selectedNurseId,
      assignErrorMessage,
      assignLoading,
      loadDashboardData,
      openAssignModal,
      closeAssignModal,
      submitAssignment
    } = useAdminDashboard()

    onMounted(() => {
      loadDashboardData()
    })

    return {
      totalPatients,
      totalNurses,
      totalAlerts,
      totalUnassigned,
      patientsWithAlerts,
      unassignedPatients,
      nurses,
      loading,
      errorMessage,
      isAssignModalOpen,
      selectedPatient,
      selectedNurseId,
      assignErrorMessage,
      assignLoading,
      openAssignModal,
      closeAssignModal,
      submitAssignment
    }
  }
}