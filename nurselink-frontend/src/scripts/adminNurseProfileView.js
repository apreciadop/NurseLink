import { onMounted } from 'vue'
import { useAdminNurseProfile } from '../composables/useAdminNurseProfile'

export default {
  name: 'AdminNurseProfileView',

  setup() {
    const adminNurseProfile = useAdminNurseProfile()

    onMounted(() => {
      adminNurseProfile.loadProfileData()
    })

    return {
      ...adminNurseProfile
    }
  }
}