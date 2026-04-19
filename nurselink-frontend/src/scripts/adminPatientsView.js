import { onMounted, watch } from 'vue'
import { useAdminPatients } from '../composables/useAdminPatients'

export default {
  name: 'AdminPatientsView',

  setup() {
    const adminPatients = useAdminPatients()

    watch([adminPatients.searchTerm, adminPatients.activeFilter], () => {
      adminPatients.resetCurrentPage()
    })

    watch(adminPatients.totalPages, (newTotalPages) => {
      if (adminPatients.currentPage.value > newTotalPages) {
        adminPatients.currentPage.value = newTotalPages
      }
    })

    onMounted(() => {
      adminPatients.loadPatients()
    })

    return {
      ...adminPatients
    }
  }
}