import { computed, ref, watch } from 'vue'
import { createNurse, getNursesDetailed } from '../services/adminService'

export function useAdminNurses() {
  const nurses = ref([])
  const loading = ref(false)
  const errorMessage = ref('')
  const successMessage = ref('')
  const searchTerm = ref('')
  const statusFilter = ref('all')
  const currentPage = ref(1)
  const pageSize = ref(8)

  const isNurseModalOpen = ref(false)
  const nurseModalMode = ref('create')
  const nurseFormErrorMessage = ref('')
  const nurseFormLoading = ref(false)

  const nurseForm = ref({
    name: '',
    surname: '',
    email: '',
    password: '',
    birthdate: '',
    phone: '',
    photo: ''
  })

  const photoPreview = ref('')

  const clearAdminNursesFeedback = () => {
    successMessage.value = ''
    nurseFormErrorMessage.value = ''
  }

  const filteredNurses = computed(() => {
    const value = searchTerm.value.trim().toLowerCase()

    let result = nurses.value

    if (statusFilter.value === 'active') {
      result = result.filter(nurse => nurse.active === true)
    }

    if (statusFilter.value === 'inactive') {
      result = result.filter(nurse => nurse.active === false)
    }

    if (!value) {
      return result
    }

    return result.filter(nurse => {
      const fullName = `${nurse.name ?? ''} ${nurse.surname ?? ''}`.toLowerCase()
      const phone = (nurse.phoneNumber ?? nurse.phone ?? '').toLowerCase()

      return fullName.includes(value) || phone.includes(value)
    })
  })

  const totalPages = computed(() => {
    const total = Math.ceil(filteredNurses.value.length / pageSize.value)
    return total > 0 ? total : 1
  })

  const paginatedNurses = computed(() => {
    const startIndex = (currentPage.value - 1) * pageSize.value
    const endIndex = startIndex + pageSize.value

    return filteredNurses.value.slice(startIndex, endIndex)
  })

  watch(searchTerm, () => {
    currentPage.value = 1
  })

  watch(statusFilter, () => {
    currentPage.value = 1
  })

  watch(totalPages, (newTotalPages) => {
    if (currentPage.value > newTotalPages) {
      currentPage.value = newTotalPages
    }
  })

  const resetNurseForm = () => {
    nurseForm.value = {
      name: '',
      surname: '',
      email: '',
      password: '',
      birthdate: '',
      phone: '',
      photo: ''
    }

    photoPreview.value = ''
    nurseFormErrorMessage.value = ''
  }

  const openCreateNurseModal = () => {
    clearAdminNursesFeedback()
    nurseModalMode.value = 'create'
    resetNurseForm()
    isNurseModalOpen.value = true
  }

  const closeNurseModal = () => {
    isNurseModalOpen.value = false
    nurseFormErrorMessage.value = ''
    nurseFormLoading.value = false
  }

  const handlePhotoChange = (event) => {
    const file = event.target.files?.[0]

    if (!file) {
      nurseForm.value.photo = ''
      photoPreview.value = ''
      return
    }

    if (!file.type.startsWith('image/')) {
      nurseFormErrorMessage.value = 'Please select a valid image file.'
      nurseForm.value.photo = ''
      photoPreview.value = ''
      event.target.value = ''
      return
    }

    clearAdminNursesFeedback()

    const reader = new FileReader()

    reader.onload = () => {
      const result = typeof reader.result === 'string' ? reader.result : ''
      nurseForm.value.photo = result
      photoPreview.value = result
    }

    reader.onerror = () => {
      nurseFormErrorMessage.value = 'Error reading the selected image.'
      nurseForm.value.photo = ''
      photoPreview.value = ''
    }

    reader.readAsDataURL(file)
  }

  const loadNursesDetailed = async () => {
    errorMessage.value = ''
    loading.value = true

    try {
      const data = await getNursesDetailed()
      nurses.value = data ?? []
    } catch (error) {
      errorMessage.value = error.message || 'Error loading nurses.'
      console.error('Nurses detailed data error:', error)
    } finally {
      loading.value = false
    }
  }

  const submitCreateNurse = async () => {
    clearAdminNursesFeedback()

    if (!nurseForm.value.name.trim()) {
      nurseFormErrorMessage.value = 'Name is required.'
      return
    }

    if (!nurseForm.value.surname.trim()) {
      nurseFormErrorMessage.value = 'Surname is required.'
      return
    }

    if (!nurseForm.value.email.trim()) {
      nurseFormErrorMessage.value = 'Email is required.'
      return
    }

    if (!nurseForm.value.password.trim()) {
      nurseFormErrorMessage.value = 'Password is required.'
      return
    }

    if (!nurseForm.value.birthdate) {
      nurseFormErrorMessage.value = 'Birthdate is required.'
      return
    }

    nurseFormLoading.value = true

    try {
      await createNurse({
        name: nurseForm.value.name.trim(),
        surname: nurseForm.value.surname.trim(),
        email: nurseForm.value.email.trim(),
        password: nurseForm.value.password,
        birthdate: nurseForm.value.birthdate || null,
        phone: nurseForm.value.phone.trim() || null,
        photo: nurseForm.value.photo || null
      })

      await loadNursesDetailed()
      closeNurseModal()
      resetNurseForm()
      successMessage.value = 'Nurse created successfully.'
    } catch (error) {
      nurseFormErrorMessage.value = error.message || 'Error creating nurse.'
      console.error('Create nurse error:', error)
    } finally {
      nurseFormLoading.value = false
    }
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
    nurses,
    loading,
    errorMessage,
    successMessage,
    searchTerm,
    statusFilter,
    currentPage,
    totalPages,
    paginatedNurses,
    isNurseModalOpen,
    nurseModalMode,
    nurseForm,
    photoPreview,
    nurseFormErrorMessage,
    nurseFormLoading,
    openCreateNurseModal,
    closeNurseModal,
    handlePhotoChange,
    loadNursesDetailed,
    submitCreateNurse,
    goToPreviousPage,
    goToNextPage,
    clearAdminNursesFeedback
  }
}