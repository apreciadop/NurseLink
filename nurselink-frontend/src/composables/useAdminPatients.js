import { computed, reactive, ref } from 'vue'
import {
  createAssignment,
  createPatient,
  getNurses,
  getPatientsDetailed,
  getSurgeryTypes
} from '../services/adminService'
import {
  formatDate,
  formatDateForInput
} from '../utils/dateUtils'

export function useAdminPatients() {
  const loading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')
  const searchTerm = ref('')
  const activeFilter = ref('all')
  const statusFilter = ref('all')
  const patients = ref([])
  const surgeryTypes = ref([])
  const nurses = ref([])

  const currentPage = ref(1)
  const itemsPerPage = 8

  const isPatientModalOpen = ref(false)
  const patientFormLoading = ref(false)
  const patientFormErrorMessage = ref('')
  const photoPreview = ref('')

  const isAssignModalOpen = ref(false)
  const selectedPatient = ref(null)
  const selectedNurseId = ref('')
  const assignErrorMessage = ref('')
  const assignLoading = ref(false)

  const patientForm = reactive({
    name: '',
    surname: '',
    email: '',
    password: '',
    birthdate: '',
    phone: '',
    photo: '',
    patientObservations: '',
    surgeryTypeId: '',
    surgeryDate: ''
  })

  const clearAdminPatientsFeedback = () => {
    successMessage.value = ''
    patientFormErrorMessage.value = ''
    assignErrorMessage.value = ''
  }

  const resetPatientForm = () => {
    patientForm.name = ''
    patientForm.surname = ''
    patientForm.email = ''
    patientForm.password = ''
    patientForm.birthdate = ''
    patientForm.phone = ''
    patientForm.photo = ''
    patientForm.patientObservations = ''
    patientForm.surgeryTypeId = ''
    patientForm.surgeryDate = ''
    photoPreview.value = ''
    patientFormErrorMessage.value = ''
  }

  const normalizeStatusLabel = (status, alertCount = 0) => {
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

    if (statusText === 'stable') {
      return 'Stable'
    }

    if (statusText === 'warning') {
      return 'Warning'
    }

    if (statusText === 'alert') {
      return 'Alert'
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

  const mapPatient = (patient) => {
    const alertCount = patient.alertCount ?? patient.alerts ?? 0

    return {
      ...patient,
      birthdate: formatDate(patient.birthdate),
      birthdateValue: formatDateForInput(patient.birthdate),
      surgeryDate: formatDate(patient.surgeryDate),
      surgeryDateValue: formatDateForInput(patient.surgeryDate),
      statusLabel: normalizeStatusLabel(patient.status ?? patient.statusLabel, alertCount),
      alertCount
    }
  }

  const loadPatients = async () => {
    loading.value = true
    errorMessage.value = ''

    try {
      const data = await getPatientsDetailed()
      patients.value = (data ?? []).map(mapPatient)
    } catch (error) {
      errorMessage.value = error.message || 'Error loading patients.'
    } finally {
      loading.value = false
    }
  }

  const loadSurgeryTypes = async () => {
    try {
      const data = await getSurgeryTypes()
      surgeryTypes.value = data ?? []
    } catch (error) {
      patientFormErrorMessage.value = error.message || 'Error loading surgery types.'
    }
  }

  const loadNurses = async () => {
    try {
      const data = await getNurses()
      nurses.value = (data ?? [])
        .filter(nurse => nurse.active)
        .sort((a, b) => {
          const nameComparison = (a.name ?? '').localeCompare(b.name ?? '')

          return nameComparison !== 0
            ? nameComparison
            : (a.surname ?? '').localeCompare(b.surname ?? '')
        })
    } catch (error) {
      errorMessage.value = error.message || 'Error loading nurses.'
    }
  }

  const openCreatePatientModal = async () => {
    clearAdminPatientsFeedback()
    resetPatientForm()
    await loadSurgeryTypes()
    isPatientModalOpen.value = true
  }

  const closePatientModal = () => {
    isPatientModalOpen.value = false
    resetPatientForm()
  }

  const openAssignModal = async (patient) => {
    clearAdminPatientsFeedback()
    selectedPatient.value = patient
    selectedNurseId.value = ''
    await loadNurses()
    isAssignModalOpen.value = true
  }

  const closeAssignModal = () => {
    isAssignModalOpen.value = false
    selectedPatient.value = null
    selectedNurseId.value = ''
    assignErrorMessage.value = ''
  }

  const submitAssignment = async () => {
    clearAdminPatientsFeedback()

    if (!selectedPatient.value?.patientId) {
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

      closeAssignModal()
      await loadPatients()
      successMessage.value = 'Patient assigned successfully.'
    } catch (error) {
      assignErrorMessage.value = error.message || 'Error assigning patient.'
    } finally {
      assignLoading.value = false
    }
  }

  const handlePhotoChange = (event) => {
    const file = event.target.files?.[0]

    if (!file) {
      return
    }

    if (!file.type.startsWith('image/')) {
      patientFormErrorMessage.value = 'Please select a valid image file.'
      event.target.value = ''
      return
    }

    clearAdminPatientsFeedback()

    const reader = new FileReader()

    reader.onload = () => {
      patientForm.photo = typeof reader.result === 'string' ? reader.result : ''
      photoPreview.value = patientForm.photo
    }

    reader.onerror = () => {
      patientFormErrorMessage.value = 'Error reading the selected image.'
    }

    reader.readAsDataURL(file)
  }

  const submitCreatePatient = async () => {
    clearAdminPatientsFeedback()

    if (!patientForm.name.trim()) {
      patientFormErrorMessage.value = 'Name is required.'
      return
    }

    if (!patientForm.surname.trim()) {
      patientFormErrorMessage.value = 'Surname is required.'
      return
    }

    if (!patientForm.email.trim()) {
      patientFormErrorMessage.value = 'Email is required.'
      return
    }

    if (!patientForm.password.trim()) {
      patientFormErrorMessage.value = 'Password is required.'
      return
    }

    if (!patientForm.birthdate) {
      patientFormErrorMessage.value = 'Birthdate is required.'
      return
    }

    if (!patientForm.surgeryTypeId) {
      patientFormErrorMessage.value = 'Surgery type is required.'
      return
    }

    if (!patientForm.surgeryDate) {
      patientFormErrorMessage.value = 'Surgery date is required.'
      return
    }

    patientFormLoading.value = true

    try {
      await createPatient({
        name: patientForm.name.trim(),
        surname: patientForm.surname.trim(),
        email: patientForm.email.trim(),
        password: patientForm.password,
        birthdate: patientForm.birthdate,
        phone: patientForm.phone.trim() || null,
        photo: patientForm.photo || null,
        patientObservations: patientForm.patientObservations.trim() || null,
        surgeryTypeId: Number(patientForm.surgeryTypeId),
        surgeryDate: patientForm.surgeryDate
      })

      closePatientModal()
      await loadPatients()
      successMessage.value = 'Patient created successfully.'
    } catch (error) {
      patientFormErrorMessage.value = error.message || 'Error creating patient.'
    } finally {
      patientFormLoading.value = false
    }
  }

  const activePatients = computed(() => {
    return patients.value.filter(patient => patient.active)
  })

  const totalPatients = computed(() => {
    return patients.value.length
  })

  const stablePatients = computed(() => {
    return activePatients.value.filter(patient => patient.statusLabel === 'Stable').length
  })

  const patientsWithAlerts = computed(() => {
    return activePatients.value.filter(patient => (patient.alertCount ?? 0) > 0).length
  })

  const filteredPatients = computed(() => {
    const value = searchTerm.value.trim().toLowerCase()

    let result = [...patients.value]

    if (activeFilter.value === 'active') {
      result = result.filter(patient => patient.active)
    }

    if (activeFilter.value === 'inactive') {
      result = result.filter(patient => !patient.active)
    }

    if (statusFilter.value !== 'all') {
      result = result.filter(patient => {
        return patient.statusLabel.toLowerCase() === statusFilter.value
      })
    }

    if (!value) {
      return result
    }

    return result.filter(patient => {
      const fullName = `${patient.name ?? ''} ${patient.surname ?? ''}`.toLowerCase()
      const phone = (patient.phone ?? '').toLowerCase()

      return fullName.includes(value) || phone.includes(value)
    })
  })

  const totalPages = computed(() => {
    return Math.max(1, Math.ceil(filteredPatients.value.length / itemsPerPage))
  })

  const paginatedPatients = computed(() => {
    const start = (currentPage.value - 1) * itemsPerPage
    const end = start + itemsPerPage

    return filteredPatients.value.slice(start, end)
  })

  const resetCurrentPage = () => {
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

  return {
    loading,
    errorMessage,
    successMessage,
    searchTerm,
    activeFilter,
    statusFilter,
    patients,
    filteredPatients,
    paginatedPatients,
    currentPage,
    totalPages,
    resetCurrentPage,
    goToPreviousPage,
    goToNextPage,
    totalPatients,
    stablePatients,
    patientsWithAlerts,
    isPatientModalOpen,
    patientForm,
    patientFormLoading,
    patientFormErrorMessage,
    photoPreview,
    surgeryTypes,
    nurses,
    isAssignModalOpen,
    selectedPatient,
    selectedNurseId,
    assignErrorMessage,
    assignLoading,
    loadPatients,
    loadSurgeryTypes,
    loadNurses,
    openCreatePatientModal,
    closePatientModal,
    handlePhotoChange,
    submitCreatePatient,
    openAssignModal,
    closeAssignModal,
    submitAssignment,
    clearAdminPatientsFeedback
  }
}