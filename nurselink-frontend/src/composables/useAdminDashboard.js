import { computed, ref } from 'vue'
import {
  getDashboardKpis,
  getPatientsWithAlerts,
  getUnassignedPatients,
  getNurses,
  createAssignment
} from '../services/adminService'
import {
  formatDate,
  formatDateForInput
} from '../utils/dateUtils'

export function useAdminDashboard() {
  const totalPatients = ref(0)
  const totalNurses = ref(0)
  const totalAlerts = ref(0)
  const totalUnassigned = ref(0)

  const patientsWithAlerts = ref([])
  const unassignedPatients = ref([])
  const nurses = ref([])

  const alertsCurrentPage = ref(1)
  const unassignedCurrentPage = ref(1)
  const dashboardPageSize = 8

  const loading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')

  const isAssignModalOpen = ref(false)
  const selectedPatient = ref(null)
  const selectedNurseId = ref('')
  const assignErrorMessage = ref('')
  const assignLoading = ref(false)

  const clearDashboardFeedback = () => {
    successMessage.value = ''
    assignErrorMessage.value = ''
  }

  const normalizeReportStatus = (status, alertCount) => {
    if (status === 0 || status === 'Stable') {
      return 'Stable'
    }

    if (status === 1 || status === 'Warning') {
      return 'Warning'
    }

    if (status === 2 || status === 'Alert') {
      return 'Alert'
    }

    const statusText = String(status ?? '').toLowerCase()

    if (statusText === 'alert') {
      return 'Alert'
    }

    if (statusText === 'warning') {
      return 'Warning'
    }

    const alerts = Number(alertCount ?? 0)

    if (alerts === 0) {
      return 'Stable'
    }

    if (alerts <= 2) {
      return 'Warning'
    }

    return 'Alert'
  }

  const mapPatientWithAlert = (patient) => {
    const alertCount = patient.alertCount ?? patient.alerts ?? 0

    const reportDisplayDate =
      patient.reportDate ??
      patient.createdAt ??
      patient.reportCreatedAt ??
      patient.lastReportDate

    return {
      patientId: patient.patientId ?? patient.id,
      patientName: patient.patientName ?? patient.name ?? '',
      patientSurname: patient.patientSurname ?? patient.surname ?? '',
      nurseName: patient.nurseName ?? '',
      nurseSurname: patient.nurseSurname ?? '',
      reportDate: formatDate(reportDisplayDate),
      reportDateValue: formatDateForInput(reportDisplayDate),
      reportStatus: normalizeReportStatus(patient.reportStatus ?? patient.status ?? patient.statusLabel, alertCount),
      reportPain: patient.reportPain ?? patient.painLevel ?? '-',
      reportFever: patient.reportFever ?? patient.hasFever ?? false,
      reportBleeding: patient.reportBleeding ?? patient.hasBleeding ?? false,
      reportSwelling: patient.reportSwelling ?? patient.hasSwelling ?? false,
      alertCount
    }
  }

  const mapUnassignedPatient = (patient) => {
    return {
      ...patient,
      patientName: patient.patientName ?? patient.name ?? '',
      patientSurname: patient.patientSurname ?? patient.surname ?? '',
      surgeryTypeName: patient.surgeryTypeName ?? patient.surgery ?? '',
      surgeryDate: formatDate(patient.surgeryDate),
      surgeryDateValue: formatDateForInput(patient.surgeryDate)
    }
  }

  const loadDashboardKpis = async () => {
    const data = await getDashboardKpis()

    totalPatients.value = data.totalPatients ?? 0
    totalNurses.value = data.totalNurses ?? 0
    totalAlerts.value = data.totalAlerts ?? 0
    totalUnassigned.value = data.unassignedPatients ?? 0
  }

  const loadPatientsWithAlerts = async () => {
    const data = await getPatientsWithAlerts()

    patientsWithAlerts.value = (data ?? [])
      .map(mapPatientWithAlert)
      .sort((a, b) => {
        const nameComparison = (a.patientName ?? '').localeCompare(b.patientName ?? '')

        return nameComparison !== 0
          ? nameComparison
          : (a.patientSurname ?? '').localeCompare(b.patientSurname ?? '')
      })

    alertsCurrentPage.value = 1
  }

  const loadUnassignedPatients = async () => {
    const data = await getUnassignedPatients()

    unassignedPatients.value = (data ?? [])
      .map(mapUnassignedPatient)
      .sort((a, b) => {
        const nameComparison = (a.patientName ?? '').localeCompare(b.patientName ?? '')

        return nameComparison !== 0
          ? nameComparison
          : (a.patientSurname ?? '').localeCompare(b.patientSurname ?? '')
      })

    unassignedCurrentPage.value = 1
  }

  const loadNurses = async () => {
    const data = await getNurses()

    nurses.value = (data ?? [])
      .filter(nurse => nurse.active === true)
      .sort((a, b) => {
        const nameComparison = (a.name ?? '').localeCompare(b.name ?? '')

        return nameComparison !== 0
          ? nameComparison
          : (a.surname ?? '').localeCompare(b.surname ?? '')
      })
  }

  const loadDashboardData = async () => {
    errorMessage.value = ''
    loading.value = true

    try {
      await Promise.all([
        loadDashboardKpis(),
        loadPatientsWithAlerts(),
        loadUnassignedPatients()
      ])
    } catch (error) {
      errorMessage.value = error.message || 'Error loading Administrator dashboard data.'
      console.error('Dashboard data error:', error)
    } finally {
      loading.value = false
    }
  }

  const patientsWithAlertsTotalPages = computed(() => {
    return Math.max(1, Math.ceil(patientsWithAlerts.value.length / dashboardPageSize))
  })

  const unassignedPatientsTotalPages = computed(() => {
    return Math.max(1, Math.ceil(unassignedPatients.value.length / dashboardPageSize))
  })

  const paginatedPatientsWithAlerts = computed(() => {
    const start = (alertsCurrentPage.value - 1) * dashboardPageSize
    const end = start + dashboardPageSize

    return patientsWithAlerts.value.slice(start, end)
  })

  const paginatedUnassignedPatients = computed(() => {
    const start = (unassignedCurrentPage.value - 1) * dashboardPageSize
    const end = start + dashboardPageSize

    return unassignedPatients.value.slice(start, end)
  })

  const goToPreviousAlertsPage = () => {
    if (alertsCurrentPage.value > 1) {
      alertsCurrentPage.value -= 1
    }
  }

  const goToNextAlertsPage = () => {
    if (alertsCurrentPage.value < patientsWithAlertsTotalPages.value) {
      alertsCurrentPage.value += 1
    }
  }

  const goToPreviousUnassignedPage = () => {
    if (unassignedCurrentPage.value > 1) {
      unassignedCurrentPage.value -= 1
    }
  }

  const goToNextUnassignedPage = () => {
    if (unassignedCurrentPage.value < unassignedPatientsTotalPages.value) {
      unassignedCurrentPage.value += 1
    }
  }

  const openAssignModal = async (patient) => {
    clearDashboardFeedback()
    selectedPatient.value = patient
    selectedNurseId.value = ''
    isAssignModalOpen.value = true

    try {
      await loadNurses()
    } catch (error) {
      assignErrorMessage.value = error.message || 'Error loading nurses.'
      console.error('Load nurses error:', error)
    }
  }

  const closeAssignModal = () => {
    isAssignModalOpen.value = false
    selectedPatient.value = null
    selectedNurseId.value = ''
    assignErrorMessage.value = ''
  }

  const submitAssignment = async () => {
    clearDashboardFeedback()

    if (!selectedPatient.value) {
      assignErrorMessage.value = 'No patient selected.'
      return
    }

    if (!selectedNurseId.value) {
      assignErrorMessage.value = 'Please select a nurse.'
      return
    }

    assignLoading.value = true

    try {
      await createAssignment({
        patientId: selectedPatient.value.patientId,
        nurseId: Number(selectedNurseId.value)
      })

      await loadDashboardData()
      closeAssignModal()
      successMessage.value = 'Patient assigned successfully.'
    } catch (error) {
      assignErrorMessage.value = error.message || 'Error creating assignment.'
      console.error('Create assignment error:', error)
    } finally {
      assignLoading.value = false
    }
  }

  return {
    totalPatients,
    totalNurses,
    totalAlerts,
    totalUnassigned,
    patientsWithAlerts,
    unassignedPatients,
    paginatedPatientsWithAlerts,
    paginatedUnassignedPatients,
    alertsCurrentPage,
    unassignedCurrentPage,
    patientsWithAlertsTotalPages,
    unassignedPatientsTotalPages,
    nurses,
    loading,
    errorMessage,
    successMessage,
    isAssignModalOpen,
    selectedPatient,
    selectedNurseId,
    assignErrorMessage,
    assignLoading,
    loadDashboardKpis,
    loadPatientsWithAlerts,
    loadUnassignedPatients,
    loadDashboardData,
    goToPreviousAlertsPage,
    goToNextAlertsPage,
    goToPreviousUnassignedPage,
    goToNextUnassignedPage,
    openAssignModal,
    closeAssignModal,
    submitAssignment,
    clearDashboardFeedback
  }
}