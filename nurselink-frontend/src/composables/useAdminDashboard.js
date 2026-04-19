import { ref } from 'vue'
import {
  getDashboardKpis,
  getPatientsWithAlerts,
  getUnassignedPatients,
  getNurses,
  createAssignment
} from '../services/adminService'
import {
  formatDate,
  formatDateTime,
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

  const loading = ref(false)
  const errorMessage = ref('')

  const isAssignModalOpen = ref(false)
  const selectedPatient = ref(null)
  const selectedNurseId = ref('')
  const assignErrorMessage = ref('')
  const assignLoading = ref(false)

  const mapPatientWithAlert = (patient) => {
    const reportDisplayDate =
      patient.createdAt ??
      patient.reportCreatedAt ??
      patient.reportDate

    return {
      ...patient,
      reportDate: formatDateTime(reportDisplayDate),
      reportDateValue: formatDateForInput(reportDisplayDate)
    }
  }

  const mapUnassignedPatient = (patient) => {
    return {
      ...patient,
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
    patientsWithAlerts.value = (data ?? []).map(mapPatientWithAlert)
  }

  const loadUnassignedPatients = async () => {
    const data = await getUnassignedPatients()
    unassignedPatients.value = (data ?? []).map(mapUnassignedPatient)
  }

  const loadNurses = async () => {
    const data = await getNurses()
    nurses.value = (data ?? []).filter(nurse => nurse.active === true)
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

  const openAssignModal = async (patient) => {
    selectedPatient.value = patient
    selectedNurseId.value = ''
    assignErrorMessage.value = ''
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
    if (!selectedPatient.value) {
      assignErrorMessage.value = 'No patient selected.'
      return
    }

    if (!selectedNurseId.value) {
      assignErrorMessage.value = 'Please select a nurse.'
      return
    }

    assignErrorMessage.value = ''
    assignLoading.value = true

    try {
      await createAssignment({
        patientId: selectedPatient.value.patientId,
        nurseId: Number(selectedNurseId.value)
      })

      await loadDashboardData()
      closeAssignModal()
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
    nurses,
    loading,
    errorMessage,
    isAssignModalOpen,
    selectedPatient,
    selectedNurseId,
    assignErrorMessage,
    assignLoading,
    loadDashboardKpis,
    loadPatientsWithAlerts,
    loadUnassignedPatients,
    loadDashboardData,
    openAssignModal,
    closeAssignModal,
    submitAssignment
  }
}