import { computed, nextTick, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  getConversationDetail,
  getConversationMessages,
  getOrCreateConversationForPatient,
  markConversationAsRead,
  sendConversationMessage
} from '../services/patientService'
import { formatDateTime } from '../utils/dateUtils'

export function usePatientMessages() {
  const router = useRouter()
  const route = useRoute()

  const loading = ref(false)
  const errorMessage = ref('')

  const conversationDetailLoading = ref(false)
  const messagesLoading = ref(false)

  const conversationDetail = ref(null)
  const conversationMessages = ref([])

  const newMessageText = ref('')
  const sendLoading = ref(false)
  const messagesContainerRef = ref(null)

  let pollingId = null
  let isRefreshingChat = false

  const conversationId = computed(() => {
    const value = route.params.conversationId

    if (!value) {
      return null
    }

    const parsedValue = Number(value)
    return Number.isNaN(parsedValue) ? null : parsedValue
  })

  const getCurrentPatientId = () => {
    const userId = localStorage.getItem('userId')

    if (userId && userId !== 'undefined' && userId !== 'null') {
      return Number(userId)
    }

    return null
  }

  const scrollMessagesToBottom = async () => {
    await nextTick()

    const container = messagesContainerRef.value

    if (container) {
      container.scrollTop = container.scrollHeight
    }
  }

  const mapConversationDetail = (detail) => {
    return {
      conversationId: detail.conversationId,
      nurseId: detail.nurseId,
      nurseName: detail.nurseName ?? '',
      nurseSurname: detail.nurseSurname ?? '',
      nursePhoto: detail.nursePhoto ?? '',
      patientId: detail.patientId,
      patientName: detail.patientName ?? '',
      patientSurname: detail.patientSurname ?? '',
      patientPhoto: detail.patientPhoto ?? '',
      createdAt: detail.createdAt
    }
  }

  const mapMessage = (message) => {
    return {
      messageId: message.messageId,
      conversationId: message.conversationId,
      messageDate: message.messageDate,
      messageDateFormatted: formatDateTime(message.messageDate),
      messageSenderIsPatient: message.messageSenderIsPatient,
      messageText: message.messageText ?? '',
      messageRead: message.messageRead ?? false,
      createdAt: message.createdAt,
      isOwnMessage: Boolean(message.messageSenderIsPatient)
    }
  }

  const ensureConversation = async () => {
    const patientId = getCurrentPatientId()

    if (!patientId) {
      throw new Error('Patient identifier was not found.')
    }

    if (conversationId.value) {
      return conversationId.value
    }

    const response = await getOrCreateConversationForPatient(patientId)

    if (!response?.conversationId) {
      throw new Error('Conversation could not be prepared.')
    }

    await router.replace(`/patient/messages/${response.conversationId}`)

    return response.conversationId
  }

  const loadConversationDetailData = async ({ silent = false } = {}) => {
    const activeConversationId = await ensureConversation()

    if (!activeConversationId) {
      return
    }

    if (!silent) {
      conversationDetailLoading.value = true
      errorMessage.value = ''
    }

    try {
      const data = await getConversationDetail(activeConversationId)
      conversationDetail.value = mapConversationDetail(data)
    } catch (error) {
      if (!silent) {
        errorMessage.value = error.message || 'Error loading conversation detail.'
      }
      console.error('Load patient conversation detail error:', error)
    } finally {
      if (!silent) {
        conversationDetailLoading.value = false
      }
    }
  }

  const loadConversationMessagesData = async ({
    silent = false,
    scrollToBottom = false
  } = {}) => {
    const patientId = getCurrentPatientId()
    const activeConversationId = await ensureConversation()

    if (!patientId || !activeConversationId) {
      return
    }

    if (isRefreshingChat) {
      return
    }

    isRefreshingChat = true

    if (!silent) {
      messagesLoading.value = true
      errorMessage.value = ''
    }

    try {
      const data = await getConversationMessages(activeConversationId, {
        patientId
      })

      conversationMessages.value = (data.messages ?? []).map(mapMessage)

      await markConversationAsRead(activeConversationId, {
        readerIsPatient: true,
        patientId
      })
    } catch (error) {
      if (!silent) {
        errorMessage.value = error.message || 'Error loading conversation messages.'
      }
      console.error('Load patient conversation messages error:', error)
    } finally {
      if (!silent) {
        messagesLoading.value = false
      }

      isRefreshingChat = false

      if (scrollToBottom) {
        await scrollMessagesToBottom()
      }
    }
  }

  const loadInitialData = async () => {
    loading.value = true
    errorMessage.value = ''

    try {
      await ensureConversation()

      await Promise.all([
        loadConversationDetailData(),
        loadConversationMessagesData({ scrollToBottom: false })
      ])
    } finally {
      loading.value = false
      await scrollMessagesToBottom()
    }
  }

  const sendMessage = async () => {
    const patientId = getCurrentPatientId()
    const activeConversationId = conversationId.value || await ensureConversation()

    if (!patientId || !activeConversationId || !newMessageText.value.trim()) {
      return
    }

    sendLoading.value = true
    errorMessage.value = ''

    try {
      await sendConversationMessage(activeConversationId, {
        messageSenderIsPatient: true,
        patientId,
        messageText: newMessageText.value.trim()
      })

      newMessageText.value = ''

      await loadConversationMessagesData({
        silent: true,
        scrollToBottom: true
      })
    } catch (error) {
      errorMessage.value = error.message || 'Error sending message.'
    } finally {
      sendLoading.value = false
    }
  }

  const startPolling = () => {
    stopPolling()

    pollingId = window.setInterval(() => {
      if (document.hidden) {
        return
      }

      loadConversationMessagesData({
        silent: true,
        scrollToBottom: false
      })
    }, 3000)
  }

  const stopPolling = () => {
    if (pollingId) {
      window.clearInterval(pollingId)
      pollingId = null
    }
  }

  return {
    loading,
    errorMessage,
    conversationId,
    conversationDetailLoading,
    messagesLoading,
    conversationDetail,
    conversationMessages,
    newMessageText,
    sendLoading,
    messagesContainerRef,
    loadInitialData,
    sendMessage,
    startPolling,
    stopPolling
  }
}