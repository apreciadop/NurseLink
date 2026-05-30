import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  getPatientDashboardById,
  getReportsByPatient
} from '../services/patientService'
import {
  getReportById,
  nurseObservations
} from '../services/nurseService'
import {
  formatDate,
  formatDateTime
} from '../utils/dateUtils'

export function useNursePatientProfile() {
  const route = useRoute()
  const router = useRouter()

  const patient = ref(null)
  const reports = ref([])

  const loading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')

  const reportsLoading = ref(false)
  const reportsErrorMessage = ref('')

  const reportsCurrentPage = ref(1)
  const reportsPageSize = ref(8)

  const isReportModalOpen = ref(false)
  const reportDetailLoading = ref(false)
  const reportDetailErrorMessage = ref('')
  const reportSaveLoading = ref(false)
  const reportSaveErrorMessage = ref('')
  const selectedReport = ref(null)

  const painLevels = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

  const patientId = computed(() => Number(route.params.id))

  const calculateStatusFromAlerts = (alertCount) => {
    if (Number(alertCount) === 0) {
      return 'Stable'
    }

    if (Number(alertCount) <= 2) {
      return 'Warning'
    }

    return 'Alert'
  }

  const normaliseStatus = (status) => {
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

    return 'Stable'
  }

  const calculateAge = (birthdate) => {
    if (!birthdate) {
      return null
    }

    const date = new Date(birthdate)

    if (Number.isNaN(date.getTime())) {
      return null
    }

    const today = new Date()
    let age = today.getFullYear() - date.getFullYear()

    const monthDifference = today.getMonth() - date.getMonth()
    const dayDifference = today.getDate() - date.getDate()

    if (monthDifference < 0 || (monthDifference === 0 && dayDifference < 0)) {
      age -= 1
    }

    return age
  }

  const mapPatient = (data) => {
    const alertCount = data.alertCount ?? data.alerts ?? 0

    return {
      patientId: data.patientId ?? data.id,
      userId: data.userId,
      name: data.name ?? '',
      surname: data.surname ?? '',
      email: data.email ?? '',
      birthdate: formatDate(data.birthdate),
      age: calculateAge(data.birthdate),
      phone: data.phone ?? data.phoneNumber ?? '',
      photo: data.photo ?? data.photoUrl ?? '',
      active: data.active ?? data.isActive ?? true,
      patientObservations: data.patientObservations ?? '',
      surgeryName:
        data.surgeryName ??
        data.surgery ??
        data.surgeryTypeName ??
        '',
      surgeryDate: formatDate(data.surgeryDate),
      assignedNurseId: data.assignedNurseId ?? null,
      assignedNurseName: data.assignedNurseName ?? '',
      assignedNursePhoto: data.assignedNursePhoto ?? '',
      alertCount,
      statusLabel: normaliseStatus(
        data.status ??
        data.statusLabel ??
        calculateStatusFromAlerts(alertCount)
      )
    }
  }

  const mapReport = (report) => {
    const alertCount = report.alertCount ?? report.alerts ?? 0

    const rawDate =
      report.createdAt ??
      report.reportDate ??
      report.date

    return {
      reportId: report.reportId ?? report.id,
      reportDate: formatDateTime(rawDate),
      statusLabel: normaliseStatus(
        report.status ??
        report.statusLabel ??
        calculateStatusFromAlerts(alertCount)
      ),
      painLevel: report.painLevel,
      hasFever: report.hasFever ?? report.fever ?? false,
      hasBleeding: report.hasBleeding ?? report.bleeding ?? false,
      hasSwelling: report.hasSwelling ?? report.swelling ?? false,
      observations: report.observations ?? '',
      nurseObservations: report.nurseObservations ?? '',
      alertCount
    }
  }

  const reportsTotalPages = computed(() => {
    return Math.max(1, Math.ceil(reports.value.length / reportsPageSize.value))
  })

  const paginatedReports = computed(() => {
    const start = (reportsCurrentPage.value - 1) * reportsPageSize.value
    const end = start + reportsPageSize.value

    return reports.value.slice(start, end)
  })

  const clearNursePatientProfileFeedback = () => {
    successMessage.value = ''
    reportSaveErrorMessage.value = ''
  }

  const loadPatient = async () => {
    loading.value = true
    errorMessage.value = ''

    try {
      const data = await getPatientDashboardById(patientId.value)
      patient.value = mapPatient(data)
    } catch (error) {
      errorMessage.value = error.message || 'Error loading patient profile.'
      console.error('Patient profile error:', error)
    } finally {
      loading.value = false
    }
  }

  const loadReports = async () => {
    reportsLoading.value = true
    reportsErrorMessage.value = ''
    reports.value = []
    reportsCurrentPage.value = 1

    try {
      const data = await getReportsByPatient(patientId.value)
      reports.value = (data ?? []).map(mapReport)
    } catch (error) {
      reportsErrorMessage.value = error.message || 'Error loading patient reports.'
      console.error('Patient reports error:', error)
    } finally {
      reportsLoading.value = false
    }
  }

  const loadPatientProfileData = async () => {
    await loadPatient()
    await loadReports()
  }

  const openViewReportModal = async (report) => {
    if (!report?.reportId) {
      return
    }

    successMessage.value = ''
    isReportModalOpen.value = true
    reportDetailLoading.value = true
    reportDetailErrorMessage.value = ''
    reportSaveErrorMessage.value = ''
    selectedReport.value = null

    try {
      const data = await getReportById(report.reportId)
      selectedReport.value = mapReport(data)
    } catch (error) {
      reportDetailErrorMessage.value = error.message || 'Error loading report details.'
    } finally {
      reportDetailLoading.value = false
    }
  }

  const closeViewReportModal = () => {
    isReportModalOpen.value = false
    reportDetailLoading.value = false
    reportDetailErrorMessage.value = ''
    reportSaveErrorMessage.value = ''
    reportSaveLoading.value = false
    selectedReport.value = null
  }

  const saveNurseObservations = async () => {
    if (!selectedReport.value?.reportId) {
      return
    }

    reportSaveErrorMessage.value = ''
    successMessage.value = ''
    reportSaveLoading.value = true

    try {
      const data = await nurseObservations(selectedReport.value.reportId, {
        nurseObservations: selectedReport.value.nurseObservations?.trim() || null
      })

      selectedReport.value = mapReport(data)
      await loadReports()

      successMessage.value = 'Nurse observations saved successfully.'
      closeViewReportModal()
    } catch (error) {
      reportSaveErrorMessage.value = error.message || 'Error saving nurse observations.'
    } finally {
      reportSaveLoading.value = false
    }
  }

  const goBack = () => {
    router.push('/nurse/dashboard')
  }

  const goToPreviousReportsPage = () => {
    if (reportsCurrentPage.value > 1) {
      reportsCurrentPage.value -= 1
    }
  }

  const goToNextReportsPage = () => {
    if (reportsCurrentPage.value < reportsTotalPages.value) {
      reportsCurrentPage.value += 1
    }
  }

  return {
    patient,
    reports,
    loading,
    errorMessage,
    successMessage,
    reportsLoading,
    reportsErrorMessage,
    reportsCurrentPage,
    reportsTotalPages,
    paginatedReports,
    isReportModalOpen,
    reportDetailLoading,
    reportDetailErrorMessage,
    reportSaveLoading,
    reportSaveErrorMessage,
    selectedReport,
    painLevels,
    loadPatient,
    loadReports,
    loadPatientProfileData,
    openViewReportModal,
    closeViewReportModal,
    saveNurseObservations,
    clearNursePatientProfileFeedback,
    goBack,
    goToPreviousReportsPage,
    goToNextReportsPage
  }
}