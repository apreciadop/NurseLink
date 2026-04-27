import { onBeforeUnmount, onMounted, watch } from 'vue'
import { useNurseMessages } from '../composables/useNurseMessages'

export default {
  name: 'NurseMessagesView',

  setup() {
    const nurseMessages = useNurseMessages()

    watch(
      [
        nurseMessages.selectedPatientId,
        nurseMessages.dateFilter,
        nurseMessages.messageFilter
      ],
      () => {
        nurseMessages.resetPage()
      }
    )

    watch(nurseMessages.totalPages, () => {
      if (nurseMessages.currentPage.value > nurseMessages.totalPages.value) {
        nurseMessages.currentPage.value = nurseMessages.totalPages.value
      }
    })

    watch(
      nurseMessages.conversationId,
      async () => {
        nurseMessages.stopPolling()
        await nurseMessages.loadInitialData()
        nurseMessages.startPolling()
      }
    )

    onMounted(async () => {
      await nurseMessages.loadInitialData()
      nurseMessages.startPolling()
    })

    onBeforeUnmount(() => {
      nurseMessages.stopPolling()
    })

    return {
      ...nurseMessages
    }
  }
}