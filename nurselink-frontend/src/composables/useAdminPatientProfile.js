import { computed, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import {
  getPatientById,
  getSurgeryTypes,
  getReportsByPatient,
  getReportById,
  updatePatient
} from '../services/adminService'
import {
  formatDateForInput,
  formatDateTime
} from '../utils/dateUtils'

export function useAdminPatientProfile() {
  const route = useRoute()

  const patientId = ref(Number(route.params.id))
  const loading = ref(false)
  const errorMessage = ref('')
  const saveLoading = ref(false)
  const saveMessage = ref('')
  const saveErrorMessage = ref('')
  const surgeryTypes = ref([])

  const reportsLoading = ref(false)
  const reportsErrorMessage = ref('')
  const reports = ref([])
  const allReports = ref([])
  const reportsCurrentPage = ref(1)
  const reportsTotalPages = ref(1)
  const reportsPageSize = 8

  const isReportModalOpen = ref(false)
  const reportDetailLoading = ref(false)
  const reportDetailErrorMessage = ref('')
  const selectedReport = ref(null)

  const painLevels = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

  const patientForm = reactive({
    patientId: 0,
    userId: 0,
    name: '',
    surname: '',
    email: '',
    password: '',
    birthdate: '',
    phone: '',
    photo: '',
    active: true,
    patientObservations: '',
    surgeryTypeId: 0,
    surgeryDate: '',
    assignedNurseId: null,
    assignedNurseName: '',
    alertCount: 0,
    statusLabel: 'Stable'
  })

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

    const statusText = String(status ?? '').toLowerCase()

    if (statusText === 'alert') {
      return 'Alert'
    }

    if (statusText === 'warning') {
      return 'Warning'
    }

    return calculateStatusFromAlerts(alertCount)
  }

  const mapReport = (report) => {
    const alertCount = report.alertCount ?? report.alerts ?? 0

    return {
      reportId: report.reportId ?? report.id,
      reportDate: formatDateTime(report.createdAt ?? report.reportDate),
      statusLabel: normalizeStatus(report.status ?? report.statusLabel, alertCount),
      painLevel: report.painLevel,
      hasFever: report.hasFever ?? report.fever ?? false,
      hasBleeding: report.hasBleeding ?? report.bleeding ?? false,
      hasSwelling: report.hasSwelling ?? report.swelling ?? false,
      observations: report.observations ?? '',
      nurseObservations: report.nurseObservations ?? '',
      alertCount
    }
  }

  const getPagedItems = (data) => {
    return data?.items ?? data?.reports ?? data?.data ?? data?.results ?? []
  }

  const getPagedTotalPages = (data, items) => {
    const totalPages = data?.totalPages ?? data?.pages ?? data?.pageCount

    if (totalPages) {
      return Math.max(1, Number(totalPages))
    }

    const totalItems = data?.totalItems ?? data?.totalCount ?? data?.count ?? items.length

    return Math.max(1, Math.ceil(Number(totalItems) / reportsPageSize))
  }

  const getPagedCurrentPage = (data, requestedPage) => {
    return Number(data?.currentPage ?? data?.pageNumber ?? data?.page ?? requestedPage)
  }

  const setClientReportsPage = (page) => {
    reportsCurrentPage.value = page
    reportsTotalPages.value = Math.max(1, Math.ceil(allReports.value.length / reportsPageSize))

    const start = (reportsCurrentPage.value - 1) * reportsPageSize
    reports.value = allReports.value.slice(start, start + reportsPageSize)
  }

  const loadPatient = async () => {
    const data = await getPatientById(patientId.value)
    const alertCount = data.alertCount ?? data.alerts ?? 0

    patientForm.patientId = data.patientId ?? 0
    patientForm.userId = data.userId ?? 0
    patientForm.name = data.name ?? ''
    patientForm.surname = data.surname ?? ''
    patientForm.email = data.email ?? ''
    patientForm.password = ''
    patientForm.birthdate = formatDateForInput(data.birthdate)
    patientForm.phone = data.phone ?? ''
    patientForm.photo = data.photo ?? ''
    patientForm.active = data.active ?? true
    patientForm.patientObservations = data.patientObservations ?? ''
    patientForm.surgeryTypeId = data.surgeryTypeId ?? 0
    patientForm.surgeryDate = formatDateForInput(data.surgeryDate)
    patientForm.assignedNurseId = data.assignedNurseId ?? null
    patientForm.assignedNurseName = data.assignedNurseName ?? ''
    patientForm.alertCount = alertCount
    patientForm.statusLabel = normalizeStatus(data.status ?? data.statusLabel, alertCount)
  }

  const loadSurgeryTypes = async () => {
    const data = await getSurgeryTypes()
    surgeryTypes.value = data ?? []
  }

  const loadReports = async (page = reportsCurrentPage.value) => {
    reportsLoading.value = true
    reportsErrorMessage.value = ''

    try {
      const data = await getReportsByPatient(patientId.value, page, reportsPageSize)

      if (Array.isArray(data)) {
        allReports.value = data.map(mapReport)
        setClientReportsPage(page)
        return
      }

      const items = getPagedItems(data)
      reports.value = items.map(mapReport)
      reportsCurrentPage.value = getPagedCurrentPage(data, page)
      reportsTotalPages.value = getPagedTotalPages(data, items)

      if (reportsCurrentPage.value > reportsTotalPages.value) {
        reportsCurrentPage.value = reportsTotalPages.value
        await loadReports(reportsCurrentPage.value)
      }
    } catch (error) {
      reportsErrorMessage.value = error.message || 'Error loading patient reports.'
      console.error('Admin patient reports error:', error)
    } finally {
      reportsLoading.value = false
    }
  }

  const goToPreviousReportsPage = async () => {
    if (reportsCurrentPage.value > 1) {
      if (allReports.value.length) {
        setClientReportsPage(reportsCurrentPage.value - 1)
        return
      }

      await loadReports(reportsCurrentPage.value - 1)
    }
  }

  const goToNextReportsPage = async () => {
    if (reportsCurrentPage.value < reportsTotalPages.value) {
      if (allReports.value.length) {
        setClientReportsPage(reportsCurrentPage.value + 1)
        return
      }

      await loadReports(reportsCurrentPage.value + 1)
    }
  }

  const loadProfileData = async () => {
    errorMessage.value = ''
    loading.value = true

    try {
      await Promise.all([
        loadPatient(),
        loadSurgeryTypes(),
        loadReports(1)
      ])
    } catch (error) {
      errorMessage.value = error.message || 'Error loading patient profile.'
      console.error('Load patient profile error:', error)
    } finally {
      loading.value = false
    }
  }

  const clearSaveFeedback = () => {
    saveMessage.value = ''
    saveErrorMessage.value = ''
  }

  const handlePhotoChange = (event) => {
    const file = event.target.files?.[0]

    if (!file) {
      return
    }

    if (!file.type.startsWith('image/')) {
      saveErrorMessage.value = 'Please select a valid image file.'
      event.target.value = ''
      return
    }

    clearSaveFeedback()

    const reader = new FileReader()

    reader.onload = () => {
      patientForm.photo = typeof reader.result === 'string' ? reader.result : ''
    }

    reader.onerror = () => {
      saveErrorMessage.value = 'Error reading the selected image.'
    }

    reader.readAsDataURL(file)
  }

  const submitUpdatePatient = async () => {
    clearSaveFeedback()

    if (!patientForm.name.trim()) {
      saveErrorMessage.value = 'Name is required.'
      return
    }

    if (!patientForm.surname.trim()) {
      saveErrorMessage.value = 'Surname is required.'
      return
    }

    if (!patientForm.email.trim()) {
      saveErrorMessage.value = 'Email is required.'
      return
    }

    if (!patientForm.birthdate) {
      saveErrorMessage.value = 'Birthdate is required.'
      return
    }

    if (!patientForm.surgeryTypeId) {
      saveErrorMessage.value = 'Surgery is required.'
      return
    }

    if (!patientForm.surgeryDate) {
      saveErrorMessage.value = 'Surgery date is required.'
      return
    }

    saveLoading.value = true

    try {
      await updatePatient(patientForm.patientId, {
        name: patientForm.name.trim(),
        surname: patientForm.surname.trim(),
        email: patientForm.email.trim(),
        password: patientForm.password.trim() || null,
        birthdate: patientForm.birthdate || null,
        phone: patientForm.phone.trim() || null,
        photo: patientForm.photo || null,
        active: patientForm.active,
        patientObservations: patientForm.patientObservations.trim() || null,
        surgeryTypeId: Number(patientForm.surgeryTypeId),
        surgeryDate: patientForm.surgeryDate || null
      })

      patientForm.password = ''
      saveMessage.value = 'Patient updated successfully.'
      await loadPatient()
    } catch (error) {
      saveErrorMessage.value = error.message || 'Error updating patient.'
      console.error('Update patient error:', error)
    } finally {
      saveLoading.value = false
    }
  }

  const openViewReportModal = async (report) => {
    if (!report?.reportId) {
      return
    }

    isReportModalOpen.value = true
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
    isReportModalOpen.value = false
    reportDetailLoading.value = false
    reportDetailErrorMessage.value = ''
    selectedReport.value = null
  }

  const hasReports = computed(() => reports.value.length > 0)

  return {
    patientId,
    loading,
    errorMessage,
    saveLoading,
    saveMessage,
    saveErrorMessage,
    patientForm,
    surgeryTypes,
    reports,
    reportsLoading,
    reportsErrorMessage,
    reportsCurrentPage,
    reportsTotalPages,
    reportsPageSize,
    hasReports,
    isReportModalOpen,
    reportDetailLoading,
    reportDetailErrorMessage,
    selectedReport,
    painLevels,
    loadPatient,
    loadSurgeryTypes,
    loadReports,
    goToPreviousReportsPage,
    goToNextReportsPage,
    loadProfileData,
    handlePhotoChange,
    submitUpdatePatient,
    openViewReportModal,
    closeViewReportModal,
    clearSaveFeedback
  }
}