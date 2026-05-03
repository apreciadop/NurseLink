import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  getPatientDashboardById,
  getReportsByPatient
} from '../services/patientService'
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

  const reportsLoading = ref(false)
  const reportsErrorMessage = ref('')

  const reportsCurrentPage = ref(1)
  const reportsPageSize = ref(5)

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
    reportsLoading,
    reportsErrorMessage,
    reportsCurrentPage,
    reportsTotalPages,
    paginatedReports,
    loadPatient,
    loadReports,
    loadPatientProfileData,
    goBack,
    goToPreviousReportsPage,
    goToNextReportsPage
  }
}