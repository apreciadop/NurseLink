import { onMounted } from 'vue'
import { useAdminPatientProfile } from '../composables/useAdminPatientProfile'

export default {
  name: 'AdminPatientProfileView',

  setup() {
    const adminPatientProfile = useAdminPatientProfile()

    onMounted(() => {
      adminPatientProfile.loadProfileData()
    })

    return {
      ...adminPatientProfile
    }
  }
}