import { computed, ref, watch } from 'vue'
import {
  getAssignedPatientsByNurse,
  getReportsByPatient
} from '../services/nurseService'
import {
  formatDate,
  formatDateTime
} from '../utils/dateUtils'

export function useNurseDashboard() {
  const patients = ref([])
  const reports = ref([])

  const selectedPatient = ref(null)

  const searchTerm = ref('')
  const activeFilter = ref('all')
  const statusFilter = ref('all')

  const currentPage = ref(1)
  const pageSize = ref(5)

  const reportsCurrentPage = ref(1)
  const reportsPageSize = ref(5)

  const loading = ref(false)
  const errorMessage = ref('')

  const reportsLoading = ref(false)
  const reportsErrorMessage = ref('')

  const getCurrentNurseId = () => {
    const userId = localStorage.getItem('userId')

    if (userId && userId !== 'undefined' && userId !== 'null') {
      return Number(userId)
    }

    return null
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

  const mapPatient = (patient) => {
    const alertCount = patient.alertCount ?? patient.alerts ?? 0

    return {
      patientId: patient.patientId ?? patient.id,
      name: patient.name ?? '',
      surname: patient.surname ?? '',
      photo: patient.photo ?? patient.photoUrl ?? '',
      surgery:
        patient.surgery ??
        patient.surgeryTypeName ??
        patient.surgeryName ??
        patient.surgeryType?.name ??
        '',
      surgeryDate: formatDate(patient.surgeryDate),
      phone: patient.phone ?? patient.phoneNumber ?? '',
      age: patient.age,
      active: patient.active ?? patient.isActive ?? true,
      statusLabel: normaliseStatus(
        patient.status ??
        patient.statusLabel ??
        calculateStatusFromAlerts(alertCount)
      ),
      alertCount
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

  const filteredPatients = computed(() => {
    const search = searchTerm.value.trim().toLowerCase()

    return patients.value.filter((patient) => {
      const matchesSearch =
        !search ||
        `${patient.name} ${patient.surname}`.toLowerCase().includes(search) ||
        patient.surgery.toLowerCase().includes(search) ||
        patient.phone.toLowerCase().includes(search) ||
        patient.statusLabel.toLowerCase().includes(search)

      const matchesActiveFilter =
        activeFilter.value === 'all' ||
        (activeFilter.value === 'active' && patient.active) ||
        (activeFilter.value === 'inactive' && !patient.active)

      const matchesStatusFilter =
        statusFilter.value === 'all' ||
        patient.statusLabel.toLowerCase() === statusFilter.value

      return matchesSearch && matchesActiveFilter && matchesStatusFilter
    })
  })

  const totalPages = computed(() => {
    return Math.max(1, Math.ceil(filteredPatients.value.length / pageSize.value))
  })

  const paginatedPatients = computed(() => {
    const start = (currentPage.value - 1) * pageSize.value
    const end = start + pageSize.value

    return filteredPatients.value.slice(start, end)
  })

  const reportsTotalPages = computed(() => {
    return Math.max(1, Math.ceil(reports.value.length / reportsPageSize.value))
  })

  const paginatedReports = computed(() => {
    const start = (reportsCurrentPage.value - 1) * reportsPageSize.value
    const end = start + reportsPageSize.value

    return reports.value.slice(start, end)
  })

  const loadPatientReports = async (patientId) => {
    reportsErrorMessage.value = ''
    reportsLoading.value = true
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

  const selectPatient = async (patient) => {
    if (!patient?.patientId) {
      selectedPatient.value = null
      reports.value = []
      reportsCurrentPage.value = 1
      reportsErrorMessage.value = ''
      return
    }

    selectedPatient.value = patient
    await loadPatientReports(patient.patientId)
  }

  const selectFirstFilteredPatient = async () => {
    currentPage.value = 1
    reportsCurrentPage.value = 1

    const firstPatient = filteredPatients.value[0]

    if (!firstPatient) {
      selectedPatient.value = null
      reports.value = []
      return
    }

    await selectPatient(firstPatient)
  }

  const loadAssignedPatients = async () => {
    errorMessage.value = ''
    loading.value = true

    try {
      const nurseId = getCurrentNurseId()

      if (!nurseId) {
        throw new Error('Nurse identifier was not found.')
      }

      const data = await getAssignedPatientsByNurse(nurseId)
      patients.value = (data ?? []).map(mapPatient)

      await selectFirstFilteredPatient()
    } catch (error) {
      errorMessage.value = error.message || 'Error loading assigned patients.'
      console.error('Assigned patients error:', error)
    } finally {
      loading.value = false
    }
  }

  const resetPatientsPage = () => {
    currentPage.value = 1
  }

  const goToPreviousPage = () => {
    if (currentPage.value > 1) {
      currentPage.value -= 1
    }
  }

  const goToNextPage = () => {
    if (currentPage.value < totalPages.value) {
      currentPage.value += 1
    }
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

  watch(
    [searchTerm, activeFilter, statusFilter],
    async () => {
      if (loading.value) {
        return
      }

      await selectFirstFilteredPatient()
    }
  )

  return {
    patients,
    reports,
    selectedPatient,
    searchTerm,
    activeFilter,
    statusFilter,
    currentPage,
    reportsCurrentPage,
    loading,
    errorMessage,
    reportsLoading,
    reportsErrorMessage,
    filteredPatients,
    totalPages,
    paginatedPatients,
    reportsTotalPages,
    paginatedReports,
    loadAssignedPatients,
    loadPatientReports,
    selectPatient,
    resetPatientsPage,
    goToPreviousPage,
    goToNextPage,
    goToPreviousReportsPage,
    goToNextReportsPage
  }
}