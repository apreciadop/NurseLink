import { computed, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import {
  getAssignedPatientsByNurse,
  getNurseById,
  updateNurse
} from '../services/adminService'
import { formatDate, formatDateForInput } from '../utils/dateUtils'

export function useAdminNurseProfile() {
  const route = useRoute()

  const nurseId = ref(route.params.id)
  const loading = ref(false)
  const errorMessage = ref('')
  const saveLoading = ref(false)
  const saveMessage = ref('')
  const saveErrorMessage = ref('')
  const assignedPatients = ref([])
  const patientSearchTerm = ref('')

  const nurseForm = reactive({
    nurseId: 0,
    userId: 0,
    name: '',
    surname: '',
    email: '',
    password: '',
    birthdate: '',
    phone: '',
    photo: '',
    active: true
  })

  const getPatientStatus = (alertCount) => {
    const alerts = Number(alertCount ?? 0)

    if (alerts === 0) {
      return 'Stable'
    }

    if (alerts >= 1 && alerts <= 2) {
      return 'Warning'
    }

    return 'Alert'
  }

  const mapAssignedPatient = (patient) => {
    return {
      ...patient,
      surgeryDate: formatDate(patient.surgeryDate),
      surgeryDateValue: formatDateForInput(patient.surgeryDate),
      status: getPatientStatus(patient.alertCount)
    }
  }

  const filteredAssignedPatients = computed(() => {
    const value = patientSearchTerm.value.trim().toLowerCase()

    const result = assignedPatients.value.map(mapAssignedPatient)

    if (!value) {
      return result
    }

    return result.filter(patient => {
      const fullName = `${patient.name ?? ''} ${patient.surname ?? ''}`.toLowerCase()
      const surgery = (patient.surgery ?? '').toLowerCase()
      const status = (patient.status ?? '').toLowerCase()
      const surgeryDate = (patient.surgeryDate ?? '').toLowerCase()
      const surgeryDateValue = (patient.surgeryDateValue ?? '').toLowerCase()
      const alertsText = String(patient.alertCount ?? 0)

      return (
        fullName.includes(value) ||
        surgery.includes(value) ||
        status.includes(value) ||
        surgeryDate.includes(value) ||
        surgeryDateValue.includes(value) ||
        alertsText.includes(value)
      )
    })
  })

  const assignedPatientsCount = computed(() => {
    return assignedPatients.value.length
  })

  const assignedAlertsCount = computed(() => {
    return assignedPatients.value.reduce((total, patient) => {
      return total + Number(patient.alertCount ?? 0)
    }, 0)
  })

  const loadNurse = async () => {
    const data = await getNurseById(nurseId.value)

    nurseForm.nurseId = data.nurseId ?? 0
    nurseForm.userId = data.userId ?? 0
    nurseForm.name = data.name ?? ''
    nurseForm.surname = data.surname ?? ''
    nurseForm.email = data.email ?? ''
    nurseForm.password = ''
    nurseForm.birthdate = formatDateForInput(data.birthdate)
    nurseForm.phone = data.phone ?? ''
    nurseForm.photo = data.photo ?? ''
    nurseForm.active = data.active ?? true
  }

  const loadAssignedPatients = async () => {
    const data = await getAssignedPatientsByNurse(nurseId.value)
    assignedPatients.value = data ?? []
  }

  const loadProfileData = async () => {
    errorMessage.value = ''
    loading.value = true

    try {
      await Promise.all([
        loadNurse(),
        loadAssignedPatients()
      ])
    } catch (error) {
      errorMessage.value = error.message || 'Error loading nurse profile.'
      console.error('Load nurse profile error:', error)
    } finally {
      loading.value = false
    }
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

    saveErrorMessage.value = ''
    saveMessage.value = ''

    const reader = new FileReader()

    reader.onload = () => {
      nurseForm.photo = typeof reader.result === 'string' ? reader.result : ''
    }

    reader.onerror = () => {
      saveErrorMessage.value = 'Error reading the selected image.'
    }

    reader.readAsDataURL(file)
  }

  const submitUpdateNurse = async () => {
    saveMessage.value = ''
    saveErrorMessage.value = ''

    if (!nurseForm.name.trim()) {
      saveErrorMessage.value = 'Name is required.'
      return
    }

    if (!nurseForm.surname.trim()) {
      saveErrorMessage.value = 'Surname is required.'
      return
    }

    if (!nurseForm.email.trim()) {
      saveErrorMessage.value = 'Email is required.'
      return
    }

    saveLoading.value = true

    try {
      await updateNurse(nurseForm.nurseId, {
        name: nurseForm.name.trim(),
        surname: nurseForm.surname.trim(),
        email: nurseForm.email.trim(),
        password: nurseForm.password.trim() || null,
        birthdate: nurseForm.birthdate || null,
        phone: nurseForm.phone.trim() || null,
        photo: nurseForm.photo || null,
        active: nurseForm.active
      })

      nurseForm.password = ''
      saveMessage.value = 'Nurse updated successfully.'
    } catch (error) {
      saveErrorMessage.value = error.message || 'Error updating nurse.'
      console.error('Update nurse error:', error)
    } finally {
      saveLoading.value = false
    }
  }

  return {
    nurseId,
    loading,
    errorMessage,
    saveLoading,
    saveMessage,
    saveErrorMessage,
    nurseForm,
    assignedPatients,
    filteredAssignedPatients,
    assignedPatientsCount,
    assignedAlertsCount,
    patientSearchTerm,
    loadNurse,
    loadAssignedPatients,
    loadProfileData,
    handlePhotoChange,
    submitUpdateNurse
  }
}