import { computed, ref } from 'vue'
import {
  createAssignment,
  deleteAssignmentByPatient,
  getAssignedPatientsByNurse,
  getNurses,
  getUnassignedPatients
} from '../services/adminService'
import {
  formatDate,
  formatDateForInput
} from '../utils/dateUtils'

export function useAdminAssignments() {
  const loading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')

  const nurses = ref([])
  const selectedNurseId = ref('')

  const assignedPatients = ref([])
  const availablePatients = ref([])

  const assignedSearchTerm = ref('')
  const availableSearchTerm = ref('')

  const assignLoading = ref(false)
  const unassignLoading = ref(false)
  const assignErrorMessage = ref('')

  const isUnassignErrorModalOpen = ref(false)
  const unassignErrorModalMessage = ref('')

  const assignedCurrentPage = ref(1)
  const availableCurrentPage = ref(1)
  const itemsPerPage = 8

  const clearAssignmentFeedback = () => {
    successMessage.value = ''
    assignErrorMessage.value = ''
  }

  const closeUnassignErrorModal = () => {
    isUnassignErrorModalOpen.value = false
    unassignErrorModalMessage.value = ''
  }

  const openUnassignErrorModal = (message) => {
    unassignErrorModalMessage.value = message || 'Error unassigning patient.'
    isUnassignErrorModalOpen.value = true
  }

  const sortByNameAndSurname = (a, b, nameKey = 'name', surnameKey = 'surname') => {
    const nameComparison = (a[nameKey] ?? '').localeCompare(b[nameKey] ?? '')

    return nameComparison !== 0
      ? nameComparison
      : (a[surnameKey] ?? '').localeCompare(b[surnameKey] ?? '')
  }

  const mapAssignedPatient = (patient) => {
    return {
      ...patient,
      surgeryDate: formatDate(patient.surgeryDate),
      surgeryDateValue: formatDateForInput(patient.surgeryDate)
    }
  }

  const mapAvailablePatient = (patient) => {
    return {
      ...patient,
      surgeryDate: formatDate(patient.surgeryDate),
      surgeryDateValue: formatDateForInput(patient.surgeryDate)
    }
  }

  const loadNurses = async () => {
    const data = await getNurses()

    nurses.value = (data ?? [])
      .filter(nurse => nurse.active)
      .sort((a, b) => sortByNameAndSurname(a, b))

    if (!selectedNurseId.value && nurses.value.length) {
      selectedNurseId.value = nurses.value[0].nurseId
    }
  }

  const loadAssignedPatients = async () => {
    if (!selectedNurseId.value) {
      assignedPatients.value = []
      return
    }

    const data = await getAssignedPatientsByNurse(selectedNurseId.value)

    assignedPatients.value = (data ?? [])
      .map(mapAssignedPatient)
      .sort((a, b) => sortByNameAndSurname(a, b))
  }

  const loadAvailablePatients = async () => {
    const data = await getUnassignedPatients()

    availablePatients.value = (data ?? [])
      .map(mapAvailablePatient)
      .sort((a, b) => sortByNameAndSurname(a, b, 'patientName', 'patientSurname'))
  }

  const loadAssignmentsData = async () => {
    loading.value = true
    errorMessage.value = ''
    clearAssignmentFeedback()
    closeUnassignErrorModal()

    try {
      await loadNurses()

      await Promise.all([
        loadAssignedPatients(),
        loadAvailablePatients()
      ])
    } catch (error) {
      errorMessage.value = error.message || 'Error loading assignments.'
      console.error('Load assignments error:', error)
    } finally {
      loading.value = false
    }
  }

  const refreshTables = async () => {
    await Promise.all([
      loadAssignedPatients(),
      loadAvailablePatients()
    ])
  }

  const assignPatient = async (patient) => {
    clearAssignmentFeedback()
    closeUnassignErrorModal()

    if (!selectedNurseId.value) {
      assignErrorMessage.value = 'Please select a nurse.'
      return
    }

    if (!patient?.patientId) {
      assignErrorMessage.value = 'No patient selected.'
      return
    }

    assignLoading.value = true

    try {
      await createAssignment({
        patientId: patient.patientId,
        nurseId: Number(selectedNurseId.value)
      })

      assignedCurrentPage.value = 1
      availableCurrentPage.value = 1

      await refreshTables()

      successMessage.value = 'Patient assigned successfully.'
    } catch (error) {
      assignErrorMessage.value = error.message || 'Error assigning patient.'
    } finally {
      assignLoading.value = false
    }
  }

  const unassignPatient = async (patient) => {
    clearAssignmentFeedback()
    closeUnassignErrorModal()

    if (!patient?.patientId) {
      openUnassignErrorModal('No patient selected.')
      return
    }

    unassignLoading.value = true

    try {
      await deleteAssignmentByPatient(patient.patientId)

      assignedCurrentPage.value = 1
      availableCurrentPage.value = 1

      await refreshTables()

      successMessage.value = 'Patient unassigned successfully.'
    } catch (error) {
      const message = error.message || 'Error unassigning patient.'
      assignErrorMessage.value = message
      openUnassignErrorModal(message)
    } finally {
      unassignLoading.value = false
    }
  }

  const filteredAssignedPatients = computed(() => {
    const value = assignedSearchTerm.value.trim().toLowerCase()

    if (!value) {
      return assignedPatients.value
    }

    return assignedPatients.value.filter(patient => {
      const fullName = `${patient.name ?? ''} ${patient.surname ?? ''}`.toLowerCase()
      const surgery = (patient.surgery ?? '').toLowerCase()
      const surgeryDate = (patient.surgeryDate ?? '').toLowerCase()
      const surgeryDateValue = (patient.surgeryDateValue ?? '').toLowerCase()

      return (
        fullName.includes(value) ||
        surgery.includes(value) ||
        surgeryDate.includes(value) ||
        surgeryDateValue.includes(value)
      )
    })
  })

  const filteredAvailablePatients = computed(() => {
    const value = availableSearchTerm.value.trim().toLowerCase()

    if (!value) {
      return availablePatients.value
    }

    return availablePatients.value.filter(patient => {
      const fullName = `${patient.patientName ?? ''} ${patient.patientSurname ?? ''}`.toLowerCase()
      const surgery = (patient.surgeryTypeName ?? '').toLowerCase()
      const surgeryDate = (patient.surgeryDate ?? '').toLowerCase()
      const surgeryDateValue = (patient.surgeryDateValue ?? '').toLowerCase()

      return (
        fullName.includes(value) ||
        surgery.includes(value) ||
        surgeryDate.includes(value) ||
        surgeryDateValue.includes(value)
      )
    })
  })

  const assignedTotalPages = computed(() => {
    return Math.max(1, Math.ceil(filteredAssignedPatients.value.length / itemsPerPage))
  })

  const availableTotalPages = computed(() => {
    return Math.max(1, Math.ceil(filteredAvailablePatients.value.length / itemsPerPage))
  })

  const paginatedAssignedPatients = computed(() => {
    const start = (assignedCurrentPage.value - 1) * itemsPerPage
    const end = start + itemsPerPage

    return filteredAssignedPatients.value.slice(start, end)
  })

  const paginatedAvailablePatients = computed(() => {
    const start = (availableCurrentPage.value - 1) * itemsPerPage
    const end = start + itemsPerPage

    return filteredAvailablePatients.value.slice(start, end)
  })

  const resetAssignedPage = () => {
    assignedCurrentPage.value = 1
  }

  const resetAvailablePage = () => {
    availableCurrentPage.value = 1
  }

  const goToPreviousAssignedPage = () => {
    if (assignedCurrentPage.value > 1) {
      assignedCurrentPage.value -= 1
    }
  }

  const goToNextAssignedPage = () => {
    if (assignedCurrentPage.value < assignedTotalPages.value) {
      assignedCurrentPage.value += 1
    }
  }

  const goToPreviousAvailablePage = () => {
    if (availableCurrentPage.value > 1) {
      availableCurrentPage.value -= 1
    }
  }

  const goToNextAvailablePage = () => {
    if (availableCurrentPage.value < availableTotalPages.value) {
      availableCurrentPage.value += 1
    }
  }

  return {
    loading,
    errorMessage,
    successMessage,
    nurses,
    selectedNurseId,
    assignedPatients,
    availablePatients,
    assignedSearchTerm,
    availableSearchTerm,
    filteredAssignedPatients,
    filteredAvailablePatients,
    paginatedAssignedPatients,
    paginatedAvailablePatients,
    assignedCurrentPage,
    availableCurrentPage,
    assignedTotalPages,
    availableTotalPages,
    assignLoading,
    unassignLoading,
    assignErrorMessage,
    isUnassignErrorModalOpen,
    unassignErrorModalMessage,
    loadNurses,
    loadAssignedPatients,
    loadAvailablePatients,
    loadAssignmentsData,
    refreshTables,
    assignPatient,
    unassignPatient,
    closeUnassignErrorModal,
    clearAssignmentFeedback,
    resetAssignedPage,
    resetAvailablePage,
    goToPreviousAssignedPage,
    goToNextAssignedPage,
    goToPreviousAvailablePage,
    goToNextAvailablePage
  }
}