<script setup>
import '../styles/login.css'
import { useAuth } from '../composables/useAuth'

const {
  email,
  password,
  errorMessage,
  loading,
  handleLogin
} = useAuth()
</script>

<template>
    <main class="login-page">
        <section class="login-card">

        <img src="/logo.png" alt="NurseLink Logo" class="logo" />

        <h2>Sign in to NurseLink</h2>

        <form @submit.prevent="handleLogin" class="form">
            <div class="form-group">
            <label>Email</label>
                <input v-model="email" type="email" placeholder="Email" />
            </div>

            <div class="form-group">
                <label>Password</label>
                <input v-model="password" type="password" placeholder="Password" />
            </div>

            <button type="submit">
                {{ loading ? 'Signing in...' : 'Log in' }}
            </button>

            <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
        </form>

        <a href="#" class="forgot-password">Forgot Password?</a>
    </section>
  </main>
</template>