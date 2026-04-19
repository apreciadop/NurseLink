import { onMounted, watch } from 'vue'
import { usePatientDashboard } from '../composables/usePatientDashboard'

export default {
  name: 'PatientDashboardView',

  setup() {
    const patientDashboard = usePatientDashboard()

    watch(patientDashboard.reportDateFilter, () => {
      patientDashboard.resetReportsPage()
    })

    watch(patientDashboard.reportStatusFilter, () => {
      patientDashboard.resetReportsPage()
    })

    watch(patientDashboard.reportsTotalPages, () => {
      if (patientDashboard.reportsCurrentPage.value > patientDashboard.reportsTotalPages.value) {
        patientDashboard.reportsCurrentPage.value = patientDashboard.reportsTotalPages.value
      }
    })

    onMounted(() => {
      patientDashboard.loadPatientDashboard()
    })

    return {
      ...patientDashboard
    }
  }
}