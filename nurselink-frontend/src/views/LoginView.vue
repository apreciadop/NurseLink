<script src="../scripts/loginView.js"></script>

<template>
  <section class="login-page">
    <article class="login-card">
      <img src="/logo.png" alt="NurseLink logo" class="logo" />

      <h2>Sign in to NurseLink</h2>

      <form class="form" @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="loginEmail">Email</label>
          <input id="loginEmail" v-model="email" type="email" placeholder="Enter your email" autocomplete="email"/>
        </div>

        <div class="form-group">
          <label for="loginPassword">Password</label>
          <input id="loginPassword" v-model="password" type="password" placeholder="Enter your password" autocomplete="current-password"/>
        </div>

        <button type="submit" :disabled="loading">{{ loading ? 'Signing in...' : 'Login' }}</button>
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
            <input id="forgotPasswordEmail" v-model="forgotPasswordEmail" type="email" placeholder="Enter your email" autocomplete="email"/>
          </div>

          <div class="form-group">
            <label for="forgotPasswordNewPassword">New Password</label>
            <input id="forgotPasswordNewPassword" v-model="forgotPasswordNewPassword" type="password" placeholder="Enter your new password" autocomplete="new-password"/>
          </div>

          <div class="form-group">
            <label for="forgotPasswordConfirmPassword">Confirm Password</label>
            <input id="forgotPasswordConfirmPassword" v-model="forgotPasswordConfirmPassword" type="password" placeholder="Confirm your new password" autocomplete="new-password"/>
          </div>

          <p v-if="forgotPasswordErrorMessage" class="error forgot-password-error">{{ forgotPasswordErrorMessage }}</p>

          <div class="forgot-password-actions">
            <button type="button" class="forgot-password-cancel" @click="closeForgotPasswordModal">Cancel</button>

            <button type="submit" class="forgot-password-submit" :disabled="forgotPasswordLoading">{{ forgotPasswordLoading ? 'Updating...' : 'Update Password' }}</button>
          </div>
        </form>
      </article>
    </section>

    <section v-if="isForgotPasswordSuccessModalOpen" class="forgot-password-modal">
      <article class="forgot-password-success-card">
        <h3>Password updated</h3>
        <p>{{ forgotPasswordSuccessMessage }}</p>

        <button type="button" class="forgot-password-success-button" @click="closeForgotPasswordSuccessModal">Accept</button>
      </article>
    </section>
  </section>
</template>

<style src="../styles/login.css"></style>