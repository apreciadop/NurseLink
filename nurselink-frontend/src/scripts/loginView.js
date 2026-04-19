import { useAuth } from '../composables/useAuth'

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

    return {
      email,
      password,
      errorMessage,
      loading,
      handleLogin
    }
  }
}
