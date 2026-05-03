<script setup>
import { ref } from 'vue'
import { useAuth } from '../composables/useAuth'
import { forgotPassword } from '../services/authService'

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
</script>

<template>
  <section class="login-page">
    <article class="login-card">
      <img src="/logo.png" alt="NurseLink logo" class="logo" />

      <h2>Sign in to NurseLink</h2>

      <form class="form" @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="loginEmail">Email</label>
          <input id="loginEmail" v-model="email" type="email" placeholder="Enter your email" autocomplete="email" />
        </div>

        <div class="form-group">
          <label for="loginPassword">Password</label>
          <input id="loginPassword" v-model="password" type="password" placeholder="Enter your password" autocomplete="current-password" />
        </div>

        <button type="submit" class="app-button app-button-primary login-submit-button" :disabled="loading">{{ loading ? 'Signing in...' : 'Login' }}</button>
      </form>

      <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

      <button type="button" class="forgot-password" @click="openForgotPasswordModal">Forgot password?</button>
    </article>

    <section v-if="isForgotPasswordModalOpen" class="forgot-password-modal" @click.self="closeForgotPasswordModal">
      <article class="forgot-password-modalcard">
        <header class="forgot-password-modalhead">
          <h3>Forgot password</h3>

          <button type="button" class="forgot-password-modalclose" @click="closeForgotPasswordModal" aria-label="Close forgot password modal">×</button>
        </header>

        <form class="forgot-password-form" @submit.prevent="handleForgotPassword">
          <div class="form-group">
            <label for="forgotPasswordEmail">Email</label>
            <input id="forgotPasswordEmail" v-model="forgotPasswordEmail" type="email" placeholder="Enter your email" autocomplete="email" />
          </div>

          <div class="form-group">
            <label for="forgotPasswordNewPassword">New Password</label>
            <input id="forgotPasswordNewPassword" v-model="forgotPasswordNewPassword" type="password" placeholder="Enter your new password" autocomplete="new-password" />
          </div>

          <div class="form-group">
            <label for="forgotPasswordConfirmPassword">Confirm Password</label>
            <input id="forgotPasswordConfirmPassword" v-model="forgotPasswordConfirmPassword" type="password" placeholder="Confirm your new password" autocomplete="new-password" />
          </div>

          <p v-if="forgotPasswordErrorMessage" class="error forgot-password-error">{{ forgotPasswordErrorMessage }}</p>

          <div class="forgot-password-actions">
            <button type="button" class="app-button app-button-secondary" @click="closeForgotPasswordModal">Cancel</button>

            <button type="submit" class="app-button app-button-primary" :disabled="forgotPasswordLoading">{{ forgotPasswordLoading ? 'Updating...' : 'Update Password' }}</button>
          </div>
        </form>
      </article>
    </section>

    <section v-if="isForgotPasswordSuccessModalOpen" class="forgot-password-modal">
      <article class="forgot-password-success-card">
        <h3>Password updated</h3>
        <p>{{ forgotPasswordSuccessMessage }}</p>

        <button type="button" class="app-button app-button-primary" @click="closeForgotPasswordSuccessModal">Accept</button>
      </article>
    </section>
  </section>
</template>

<style src="../styles/login.css"></style>