import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { loginUser } from '../services/authService'

export function useAuth() {
  const router = useRouter()

  const email = ref('')
  const password = ref('')
  const errorMessage = ref('')
  const loading = ref(false)

  const handleLogin = async () => {
    errorMessage.value = ''

    if (!email.value || !password.value) {
      errorMessage.value = 'Please complete all fields.'
      return
    }

    loading.value = true

    try {
      const data = await loginUser(
        email.value,
        password.value
      )

      const nurseId =
        data.nurseId ??
        data.NurseId ??
        data.user?.nurseId ??
        data.user?.NurseId ??
        null

      const patientId =
        data.patientId ??
        data.PatientId ??
        data.user?.patientId ??
        data.user?.PatientId ??
        null

      localStorage.setItem('token', data.token)
      localStorage.setItem('role', data.role)
      localStorage.setItem('userId', data.userId ?? '')
      localStorage.setItem('email', data.email ?? '')
      localStorage.setItem('name', data.name ?? '')

      if (nurseId) {
        localStorage.setItem('nurseId', String(nurseId))
      } else {
        localStorage.removeItem('nurseId')
      }

      if (patientId) {
        localStorage.setItem('patientId', String(patientId))
      } else {
        localStorage.removeItem('patientId')
      }

      if (data.role === 'Admin') router.push('/admin')
      else if (data.role === 'Nurse') router.push('/nurse')
      else if (data.role === 'Patient') router.push('/patient')
      else router.push('/')
    } catch (error) {
      errorMessage.value = error.message || 'Login failed.'
    } finally {
      loading.value = false
    }
  }

  return {
    email,
    password,
    errorMessage,
    loading,
    handleLogin
  }
}