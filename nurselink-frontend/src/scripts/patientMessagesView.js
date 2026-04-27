import { onBeforeUnmount, onMounted, watch } from 'vue'
import { usePatientMessages } from '../composables/usePatientMessages'

export default {
  name: 'PatientMessagesView',

  setup() {
    const patientMessages = usePatientMessages()

    watch(
      patientMessages.conversationId,
      async () => {
        patientMessages.stopPolling()
        await patientMessages.loadInitialData()
        patientMessages.startPolling()
      }
    )

    onMounted(async () => {
      await patientMessages.loadInitialData()
      patientMessages.startPolling()
    })

    onBeforeUnmount(() => {
      patientMessages.stopPolling()
    })

    return {
      ...patientMessages
    }
  }
}