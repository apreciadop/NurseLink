import { computed, reactive, ref } from 'vue'
import {
  createReport,
  getPatientDashboardById,
  getReportById,
  getReportsByPatient,
  getSurgeryTypes
} from '../services/patientService'
import {
  formatDate,
  formatDateTime,
  formatDateForInput
} from '../utils/dateUtils'

export function usePatientDashboard() {
  const loading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')

  const reportsLoading = ref(false)
  const reportsErrorMessage = ref('')
  const reports = ref([])

  const reportDateFilter = ref('')
  const reportStatusFilter = ref('all')

  const reportsCurrentPage = ref(1)
  const reportsPageSize = ref(8)

  const surgeryTypes = ref([])

  const isCreateReportModalOpen = ref(false)
  const createReportLoading = ref(false)
  const createReportErrorMessage = ref('')

  const isViewReportModalOpen = ref(false)
  const reportDetailLoading = ref(false)
  const reportDetailErrorMessage = ref('')
  const selectedReport = ref(null)

  const painLevels = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

  const reportForm = reactive({
    painLevel: 0,
    hasFever: false,
    hasBleeding: false,
    hasSwelling: false,
    observations: ''
  })

  const patient = ref({
    patientId: 0,
    name: '',
    surname: '',
    email: '',
    phone: '',
    birthdate: '',
    age: null,
    photo: '',
    surgery: '',
    surgeryTypeId: null,
    surgeryDate: '',
    assignedNurseName: '',
    assignedNursePhoto: '',
    statusLabel: 'Stable',
    alertCount: 0
  })

  const getCurrentPatientId = () => {
    const userId = localStorage.getItem('userId')

    if (userId && userId !== 'undefined' && userId !== 'null') {
      return Number(userId)
    }

    return null
  }

  const calculateAge = (birthdate) => {
    if (!birthdate) {
      return null
    }

    let date = new Date(birthdate)

    if (Number.isNaN(date.getTime()) && typeof birthdate === 'string') {
      const parts = birthdate.split('/')

      if (parts.length === 3) {
        const [day, month, year] = parts
        date = new Date(Number(year), Number(month) - 1, Number(day))
      }
    }

    if (Number.isNaN(date.getTime())) {
      return null
    }

    const today = new Date()
    let age = today.getFullYear() - date.getFullYear()
    const monthDifference = today.getMonth() - date.getMonth()

    if (
      monthDifference < 0 ||
      (monthDifference === 0 && today.getDate() < date.getDate())
    ) {
      age -= 1
    }

    return age
  }

  const calculateStatusFromAlerts = (alertCount) => {
    if (Number(alertCount) === 0) {
      return 'Stable'
    }

    if (Number(alertCount) <= 2) {
      return 'Warning'
    }

    return 'Alert'
  }

  const normalizeStatus = (status, alertCount) => {
    if (status === 0 || status === 'Stable') {
      return 'Stable'
    }

    if (status === 1 || status === 'Warning') {
      return 'Warning'
    }

    if (status === 2 || status === 'Alert') {
      return 'Alert'
    }

    return calculateStatusFromAlerts(alertCount)
  }

  const getSurgeryName = (data) => {
    const directName =
      data.surgery ??
      data.surgeryTypeName ??
      data.surgeryName ??
      data.surgeryType?.name ??
      data.surgeryType?.Name ??
      ''

    if (directName) {
      return directName
    }

    const surgeryTypeId = data.surgeryTypeId ?? data.SurgeryTypeId

    if (!surgeryTypeId) {
      return ''
    }

    const surgeryType = surgeryTypes.value.find(type => {
      return Number(type.surgeryTypeId ?? type.id) === Number(surgeryTypeId)
    })

    return surgeryType?.name ?? surgeryType?.Name ?? ''
  }

  const mapPatient = (data) => {
    const alertCount = data.alertCount ?? data.alerts ?? 0
    const birthdateSource = data.birthdate ?? data.birthDate ?? data.userBirthdate ?? data.userBirthDate ?? null

    return {
      patientId: data.patientId ?? data.id ?? 0,
      name: data.name ?? '',
      surname: data.surname ?? '',
      email: data.email ?? '',
      phone: data.phone ?? '',
      birthdate: formatDate(birthdateSource),
      age: data.age ?? calculateAge(birthdateSource),
      photo: data.photo ?? data.photoUrl ?? '',
      surgery: getSurgeryName(data),
      surgeryTypeId: data.surgeryTypeId ?? data.SurgeryTypeId ?? null,
      surgeryDate: formatDate(data.surgeryDate),
      assignedNurseName: data.assignedNurseName ?? '',
      assignedNursePhoto: data.assignedNursePhoto ?? data.nursePhoto ?? '',
      statusLabel: normalizeStatus(data.status, alertCount),
      alertCount
    }
  }

  const mapReport = (report) => {
    const alertCount = report.alertCount ?? report.alerts ?? 0
    const nurseObservations = report.nurseObservations ?? ''

    return {
      reportId: report.reportId ?? report.id,
      reportDate: formatDateTime(report.createdAt ?? report.reportDate),
      reportDateValue: formatDateForInput(report.createdAt ?? report.reportDate),
      statusLabel: normalizeStatus(report.status ?? report.statusLabel, alertCount),
      painLevel: report.painLevel,
      hasFever: report.hasFever ?? report.fever ?? false,
      hasBleeding: report.hasBleeding ?? report.bleeding ?? false,
      hasSwelling: report.hasSwelling ?? report.swelling ?? false,
      observations: report.observations ?? '',
      nurseObservations,
      hasNurseObservations: Boolean(nurseObservations && nurseObservations.trim()),
      alertCount
    }
  }

  const filteredReports = computed(() => {
    return reports.value.filter(report => {
      const matchesDate =
        !reportDateFilter.value ||
        report.reportDateValue === reportDateFilter.value

      const matchesStatus =
        reportStatusFilter.value === 'all' ||
        report.statusLabel.toLowerCase() === reportStatusFilter.value

      return matchesDate && matchesStatus
    })
  })

  const reportsTotalPages = computed(() => {
    return Math.max(1, Math.ceil(filteredReports.value.length / reportsPageSize.value))
  })

  const paginatedReports = computed(() => {
    const start = (reportsCurrentPage.value - 1) * reportsPageSize.value
    const end = start + reportsPageSize.value

    return filteredReports.value.slice(start, end)
  })

  const createModalReport = computed(() => ({
    painLevel: reportForm.painLevel,
    hasFever: reportForm.hasFever,
    hasBleeding: reportForm.hasBleeding,
    hasSwelling: reportForm.hasSwelling,
    observations: reportForm.observations,
    nurseObservations: ''
  }))

  const clearPatientDashboardFeedback = () => {
    successMessage.value = ''
  }

  const loadSurgeryTypes = async () => {
    const data = await getSurgeryTypes()
    surgeryTypes.value = data ?? []
  }

  const loadReports = async (patientId) => {
    reportsLoading.value = true
    reportsErrorMessage.value = ''
    reports.value = []
    reportsCurrentPage.value = 1

    try {
      const data = await getReportsByPatient(patientId)
      reports.value = (data ?? []).map(mapReport)
    } catch (error) {
      reportsErrorMessage.value = error.message || 'Error loading patient reports.'
      console.error('Patient reports error:', error)
    } finally {
      reportsLoading.value = false
    }
  }

  const loadPatientDashboard = async () => {
    loading.value = true
    errorMessage.value = ''

    try {
      const patientId = getCurrentPatientId()

      if (!patientId) {
        throw new Error('Patient identifier was not found.')
      }

      await loadSurgeryTypes()

      const data = await getPatientDashboardById(patientId)
      patient.value = mapPatient(data)

      await loadReports(patientId)
    } catch (error) {
      errorMessage.value = error.message || 'Error loading patient dashboard.'
      console.error('Patient dashboard error:', error)
    } finally {
      loading.value = false
    }
  }

  const resetReportForm = () => {
    reportForm.painLevel = 0
    reportForm.hasFever = false
    reportForm.hasBleeding = false
    reportForm.hasSwelling = false
    reportForm.observations = ''
    createReportErrorMessage.value = ''
  }

  const openCreateReportModal = () => {
    successMessage.value = ''
    resetReportForm()
    isCreateReportModalOpen.value = true
  }

  const closeCreateReportModal = () => {
    isCreateReportModalOpen.value = false
    resetReportForm()
  }

  const submitCreateReport = async () => {
    createReportErrorMessage.value = ''
    successMessage.value = ''

    if (!patient.value.patientId) {
      createReportErrorMessage.value = 'Patient identifier was not found.'
      return
    }

    createReportLoading.value = true

    try {
      await createReport({
        patientId: patient.value.patientId,
        painLevel: Number(reportForm.painLevel),
        hasFever: reportForm.hasFever,
        hasBleeding: reportForm.hasBleeding,
        hasSwelling: reportForm.hasSwelling,
        observations: reportForm.observations.trim() || null
      })

      closeCreateReportModal()
      await loadPatientDashboard()
      successMessage.value = 'Report created successfully.'
    } catch (error) {
      createReportErrorMessage.value = error.message || 'Error creating report.'
    } finally {
      createReportLoading.value = false
    }
  }

  const openViewReportModal = async (report) => {
    if (!report?.reportId) {
      return
    }

    isViewReportModalOpen.value = true
    reportDetailLoading.value = true
    reportDetailErrorMessage.value = ''
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
    isViewReportModalOpen.value = false
    reportDetailLoading.value = false
    reportDetailErrorMessage.value = ''
    selectedReport.value = null
  }

  const resetReportsPage = () => {
    reportsCurrentPage.value = 1
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
    loading,
    errorMessage,
    successMessage,
    patient,
    reports,
    reportDateFilter,
    reportStatusFilter,
    reportsLoading,
    reportsErrorMessage,
    filteredReports,
    paginatedReports,
    reportsCurrentPage,
    reportsTotalPages,
    isCreateReportModalOpen,
    createReportLoading,
    createReportErrorMessage,
    isViewReportModalOpen,
    reportDetailLoading,
    reportDetailErrorMessage,
    selectedReport,
    painLevels,
    reportForm,
    createModalReport,
    loadPatientDashboard,
    loadReports,
    openCreateReportModal,
    closeCreateReportModal,
    submitCreateReport,
    openViewReportModal,
    closeViewReportModal,
    resetReportsPage,
    goToPreviousReportsPage,
    goToNextReportsPage,
    clearPatientDashboardFeedback
  }
}