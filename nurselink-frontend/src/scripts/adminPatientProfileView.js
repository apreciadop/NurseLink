import { onMounted } from 'vue'
import ReportModal from '../views/ReportModal.vue'
import { useAdminPatientProfile } from '../composables/useAdminPatientProfile'

export default {
  name: 'AdminPatientProfileView',

  components: {
    ReportModal
  },

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