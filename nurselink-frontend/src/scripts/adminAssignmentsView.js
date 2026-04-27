import { onMounted, watch } from 'vue'
import { useAdminAssignments } from '../composables/useAdminAssignments'

export default {
  name: 'AdminAssignmentsView',

  setup() {
    const adminAssignments = useAdminAssignments()

    watch(adminAssignments.selectedNurseId, async () => {
      adminAssignments.resetAssignedPage()
      adminAssignments.assignErrorMessage.value = ''
      adminAssignments.closeUnassignErrorModal()

      try {
        await adminAssignments.loadAssignedPatients()
      } catch (error) {
        adminAssignments.errorMessage.value = error.message || 'Error loading assigned patients.'
      }
    })

    watch(adminAssignments.assignedSearchTerm, () => {
      adminAssignments.resetAssignedPage()
    })

    watch(adminAssignments.availableSearchTerm, () => {
      adminAssignments.resetAvailablePage()
    })

    watch(adminAssignments.assignedTotalPages, (newTotalPages) => {
      if (adminAssignments.assignedCurrentPage.value > newTotalPages) {
        adminAssignments.assignedCurrentPage.value = newTotalPages
      }
    })

    watch(adminAssignments.availableTotalPages, (newTotalPages) => {
      if (adminAssignments.availableCurrentPage.value > newTotalPages) {
        adminAssignments.availableCurrentPage.value = newTotalPages
      }
    })

    onMounted(() => {
      adminAssignments.loadAssignmentsData()
    })

    return {
      ...adminAssignments
    }
  }
}