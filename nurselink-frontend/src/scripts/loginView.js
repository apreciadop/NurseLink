import { ref } from 'vue'
import { useAuth } from '../composables/useAuth'
import { forgotPassword } from '../services/authService'

export default {
  name: 'LoginView',

  setup() {
    const {
      email,
      password,
      errorMessage,
      loading,
      handleLogin
    } = useAuth()

    const isForgotPasswordModalOpen = ref(false)
    const forgotPasswordEmail = ref('')
    const forgotPasswordNewPassword = ref('')
    const forgotPasswordConfirmPassword = ref('')
    const forgotPasswordErrorMessage = ref('')
    const forgotPasswordSuccessMessage = ref('')
    const forgotPasswordLoading = ref(false)

    const isForgotPasswordSuccessModalOpen = ref(false)

    const resetForgotPasswordForm = () => {
      forgotPasswordEmail.value = ''
      forgotPasswordNewPassword.value = ''
      forgotPasswordConfirmPassword.value = ''
      forgotPasswordErrorMessage.value = ''
      forgotPasswordSuccessMessage.value = ''
      forgotPasswordLoading.value = false
    }

    const openForgotPasswordModal = () => {
      resetForgotPasswordForm()
      forgotPasswordEmail.value = email.value || ''
      isForgotPasswordModalOpen.value = true
    }

    const closeForgotPasswordModal = () => {
      isForgotPasswordModalOpen.value = false
      resetForgotPasswordForm()
    }

    const closeForgotPasswordSuccessModal = () => {
      isForgotPasswordSuccessModalOpen.value = false
      forgotPasswordSuccessMessage.value = ''
      password.value = ''
    }

    const handleForgotPassword = async () => {
      forgotPasswordErrorMessage.value = ''
      forgotPasswordSuccessMessage.value = ''

      if (!forgotPasswordEmail.value.trim()) {
        forgotPasswordErrorMessage.value = 'Email is required.'
        return
      }

      if (!forgotPasswordNewPassword.value.trim()) {
        forgotPasswordErrorMessage.value = 'New password is required.'
        return
      }

      if (!forgotPasswordConfirmPassword.value.trim()) {
        forgotPasswordErrorMessage.value = 'Confirm password is required.'
        return
      }

      forgotPasswordLoading.value = true

      try {
        const response = await forgotPassword({
          email: forgotPasswordEmail.value.trim(),
          newPassword: forgotPasswordNewPassword.value,
          confirmPassword: forgotPasswordConfirmPassword.value
        })

        forgotPasswordSuccessMessage.value =
          response?.message || 'Password updated successfully.'

        email.value = forgotPasswordEmail.value.trim()
        password.value = ''

        isForgotPasswordModalOpen.value = false
        isForgotPasswordSuccessModalOpen.value = true
      } catch (error) {
        forgotPasswordErrorMessage.value =
          error.message || 'Error updating password.'
      } finally {
        forgotPasswordLoading.value = false
      }
    }

    return {
      email,
      password,
      errorMessage,
      loading,
      handleLogin,
      isForgotPasswordModalOpen,
      forgotPasswordEmail,
      forgotPasswordNewPassword,
      forgotPasswordConfirmPassword,
      forgotPasswordErrorMessage,
      forgotPasswordSuccessMessage,
      forgotPasswordLoading,
      isForgotPasswordSuccessModalOpen,
      openForgotPasswordModal,
      closeForgotPasswordModal,
      closeForgotPasswordSuccessModal,
      handleForgotPassword
    }
  }
}