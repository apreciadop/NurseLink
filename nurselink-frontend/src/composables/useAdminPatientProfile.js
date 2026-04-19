import { reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import {
  getPatientById,
  getSurgeryTypes,
  updatePatient
} from '../services/adminService'
import { formatDateForInput } from '../utils/dateUtils'

export function useAdminPatientProfile() {
  const route = useRoute()

  const patientId = ref(Number(route.params.id))
  const loading = ref(false)
  const errorMessage = ref('')
  const saveLoading = ref(false)
  const saveMessage = ref('')
  const saveErrorMessage = ref('')
  const surgeryTypes = ref([])

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
    assignedNurseName: ''
  })

  const loadPatient = async () => {
    const data = await getPatientById(patientId.value)

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
  }

  const loadSurgeryTypes = async () => {
    const data = await getSurgeryTypes()
    surgeryTypes.value = data ?? []
  }

  const loadProfileData = async () => {
    errorMessage.value = ''
    loading.value = true

    try {
      await Promise.all([
        loadPatient(),
        loadSurgeryTypes()
      ])
    } catch (error) {
      errorMessage.value = error.message || 'Error loading patient profile.'
      console.error('Load patient profile error:', error)
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
      patientForm.photo = typeof reader.result === 'string' ? reader.result : ''
    }

    reader.onerror = () => {
      saveErrorMessage.value = 'Error reading the selected image.'
    }

    reader.readAsDataURL(file)
  }

  const submitUpdatePatient = async () => {
    saveMessage.value = ''
    saveErrorMessage.value = ''

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
    } catch (error) {
      saveErrorMessage.value = error.message || 'Error updating patient.'
      console.error('Update patient error:', error)
    } finally {
      saveLoading.value = false
    }
  }

  return {
    patientId,
    loading,
    errorMessage,
    saveLoading,
    saveMessage,
    saveErrorMessage,
    patientForm,
    surgeryTypes,
    loadPatient,
    loadSurgeryTypes,
    loadProfileData,
    handlePhotoChange,
    submitUpdatePatient
  }
}