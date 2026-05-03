import { computed, nextTick, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  getAssignedPatientsByNurse,
  getConversationDetail,
  getConversationMessages,
  getNurseConversations,
  getOrCreateConversation,
  markConversationAsRead,
  sendConversationMessage
} from '../services/nurseService'
import { formatDateTime, formatDateForInput } from '../utils/dateUtils'

export function useNurseMessages() {
  const router = useRouter()
  const route = useRoute()

  const loading = ref(false)
  const errorMessage = ref('')

  const conversationsLoading = ref(false)
  const conversations = ref([])
  const assignedPatients = ref([])

  const selectedPatientId = ref('')
  const dateFilter = ref('')
  const messageFilter = ref('')

  const currentPage = ref(1)
  const itemsPerPage = 8

  const conversationDetailLoading = ref(false)
  const messagesLoading = ref(false)
  const conversationDetail = ref(null)
  const conversationMessages = ref([])
  const newMessageText = ref('')
  const sendLoading = ref(false)
  const startConversationLoading = ref(false)
  const messagesContainerRef = ref(null)

  let pollingId = null
  let isRefreshingList = false
  let isRefreshingChat = false
  let lastLoadedConversationId = null
  let lastLoadedMode = ''

  const conversationId = computed(() => {
    const value = route.params.conversationId

    if (!value) {
      return null
    }

    const parsedValue = Number(value)
    return Number.isNaN(parsedValue) ? null : parsedValue
  })

  const isConversationView = computed(() => Boolean(conversationId.value))

  const getCurrentNurseId = () => {
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

  const mapConversation = (conversation) => {
    const lastMessage = conversation.lastMessage ?? ''

    return {
      conversationId: conversation.conversationId,
      nurseId: conversation.nurseId,
      patientId: conversation.patientId,
      patientName: conversation.patientName ?? '',
      patientSurname: conversation.patientSurname ?? '',
      patientPhoto: conversation.patientPhoto ?? '',
      lastMessage,
      lastMessagePreview: lastMessage.length > 140
        ? `${lastMessage.slice(0, 140)}...`
        : lastMessage,
      lastMessageDate: conversation.lastMessageDate ?? null,
      lastMessageDateFormatted: conversation.lastMessageDate
        ? formatDateTime(conversation.lastMessageDate)
        : '',
      lastMessageDateValue: conversation.lastMessageDate
        ? formatDateForInput(conversation.lastMessageDate)
        : '',
      lastMessageSenderIsPatient: conversation.lastMessageSenderIsPatient ?? null,
      unreadCount: conversation.unreadCount ?? 0,
      createdAt: conversation.createdAt
    }
  }

  const mapAssignedPatient = (patient) => {
    return {
      patientId: patient.patientId ?? patient.id,
      label: `${patient.name ?? ''} ${patient.surname ?? ''}`.trim()
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
      isOwnMessage: !message.messageSenderIsPatient
    }
  }

  const patientOptions = computed(() => {
    return [...assignedPatients.value].sort((a, b) =>
      a.label.localeCompare(b.label)
    )
  })

  const filteredConversations = computed(() => {
    const patientValue = selectedPatientId.value
    const dateValue = dateFilter.value
    const messageValue = messageFilter.value.trim().toLowerCase()

    return conversations.value.filter(conversation => {
      const lastMessage = (conversation.lastMessage ?? '').toLowerCase()

      const matchesPatient =
        !patientValue || String(conversation.patientId) === patientValue

      const matchesDate =
        !dateValue || conversation.lastMessageDateValue === dateValue

      const matchesMessage =
        !messageValue || lastMessage.includes(messageValue)

      return matchesPatient && matchesDate && matchesMessage
    })
  })

  const totalPages = computed(() => {
    return Math.max(1, Math.ceil(filteredConversations.value.length / itemsPerPage))
  })

  const paginatedConversations = computed(() => {
    const start = (currentPage.value - 1) * itemsPerPage
    const end = start + itemsPerPage

    return filteredConversations.value.slice(start, end)
  })

  const loadAssignedPatients = async () => {
    const nurseId = getCurrentNurseId()

    if (!nurseId) {
      throw new Error('Nurse identifier was not found.')
    }

    const data = await getAssignedPatientsByNurse(nurseId)
    assignedPatients.value = (data ?? []).map(mapAssignedPatient)
  }

  const loadConversations = async ({ silent = false } = {}) => {
    const nurseId = getCurrentNurseId()

    if (!nurseId) {
      errorMessage.value = 'Nurse identifier was not found.'
      return
    }

    if (isRefreshingList) {
      return
    }

    isRefreshingList = true

    if (!silent) {
      loading.value = true
      conversationsLoading.value = true
      errorMessage.value = ''
    }

    try {
      if (!assignedPatients.value.length) {
        await loadAssignedPatients()
      }

      const data = await getNurseConversations(nurseId)
      conversations.value = (data ?? []).map(mapConversation)
    } catch (error) {
      if (!silent) {
        errorMessage.value = error.message || 'Error loading conversations.'
      }

      console.error('Load nurse conversations error:', error)
    } finally {
      if (!silent) {
        conversationsLoading.value = false
        loading.value = false
      }

      isRefreshingList = false
    }
  }

  const loadConversationDetailData = async ({ silent = false } = {}) => {
    const nurseId = getCurrentNurseId()

    if (!conversationId.value || !nurseId) {
      return
    }

    if (!silent) {
      conversationDetailLoading.value = true
      errorMessage.value = ''
    }

    try {
      const data = await getConversationDetail(conversationId.value)
      conversationDetail.value = mapConversationDetail(data)
    } catch (error) {
      if (!silent) {
        errorMessage.value = error.message || 'Error loading conversation detail.'
      }

      console.error('Load conversation detail error:', error)
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
    const nurseId = getCurrentNurseId()

    if (!conversationId.value || !nurseId) {
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
      const data = await getConversationMessages(conversationId.value, {
        nurseId
      })

      conversationMessages.value = (data.messages ?? []).map(mapMessage)

      await markConversationAsRead(conversationId.value, {
        readerIsPatient: false,
        nurseId
      })
    } catch (error) {
      if (!silent) {
        errorMessage.value = error.message || 'Error loading conversation messages.'
      }

      console.error('Load conversation messages error:', error)
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

  const loadConversationView = async () => {
    await Promise.all([
      loadConversationDetailData(),
      loadConversationMessagesData({ scrollToBottom: false })
    ])
  }

  const loadInitialData = async () => {
    loading.value = true
    errorMessage.value = ''

    try {
      if (isConversationView.value) {
        if (
          lastLoadedMode === 'conversation' &&
          lastLoadedConversationId === conversationId.value
        ) {
          return
        }

        lastLoadedMode = 'conversation'
        lastLoadedConversationId = conversationId.value

        await loadConversationView()
      } else {
        if (lastLoadedMode === 'list') {
          return
        }

        lastLoadedMode = 'list'
        lastLoadedConversationId = null

        await loadConversations()
      }
    } catch (error) {
      errorMessage.value = error.message || 'Error loading messages.'
      console.error('Load nurse messages error:', error)
    } finally {
      loading.value = false

      if (isConversationView.value) {
        await scrollMessagesToBottom()
      }
    }
  }

  const openConversation = async (conversation) => {
    if (!conversation?.patientId) {
      return
    }

    try {
      const response = await getOrCreateConversation({
        nurseId: conversation.nurseId,
        patientId: conversation.patientId
      })

      if (!response?.conversationId) {
        return
      }

      router.push(`/nurse/messages/${response.conversationId}`)
    } catch (error) {
      errorMessage.value = error.message || 'Error opening conversation.'
    }
  }

  const startConversationWithSelectedPatient = async () => {
    const nurseId = getCurrentNurseId()

    if (!selectedPatientId.value || !nurseId) {
      return
    }

    startConversationLoading.value = true
    errorMessage.value = ''

    try {
      const response = await getOrCreateConversation({
        nurseId,
        patientId: Number(selectedPatientId.value)
      })

      if (!response?.conversationId) {
        return
      }

      router.push(`/nurse/messages/${response.conversationId}`)
    } catch (error) {
      errorMessage.value = error.message || 'Error starting conversation.'
    } finally {
      startConversationLoading.value = false
    }
  }

  const sendMessage = async () => {
    const nurseId = getCurrentNurseId()

    if (!conversationId.value || !nurseId || !newMessageText.value.trim()) {
      return
    }

    sendLoading.value = true
    errorMessage.value = ''

    try {
      await sendConversationMessage(conversationId.value, {
        messageSenderIsPatient: false,
        nurseId,
        messageText: newMessageText.value.trim()
      })

      newMessageText.value = ''

      await loadConversationMessagesData({
        silent: true,
        scrollToBottom: true
      })

      await loadConversations({ silent: true })
    } catch (error) {
      errorMessage.value = error.message || 'Error sending message.'
    } finally {
      sendLoading.value = false
    }
  }

  const goBackToConversations = () => {
    lastLoadedMode = ''
    lastLoadedConversationId = null
    router.push('/nurse/messages')
  }

  const resetPage = () => {
    currentPage.value = 1
  }

  const goToPreviousPage = () => {
    if (currentPage.value > 1) {
      currentPage.value -= 1
    }
  }

  const goToNextPage = () => {
    if (currentPage.value < totalPages.value) {
      currentPage.value += 1
    }
  }

  const startPolling = () => {
    stopPolling()

    pollingId = window.setInterval(() => {
      if (document.hidden) {
        return
      }

      if (isConversationView.value) {
        loadConversationMessagesData({ silent: true, scrollToBottom: false })
        return
      }

      loadConversations({ silent: true })
    }, isConversationView.value ? 5000 : 10000)
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
    conversationsLoading,
    conversations,
    assignedPatients,
    selectedPatientId,
    patientOptions,
    dateFilter,
    messageFilter,
    currentPage,
    filteredConversations,
    paginatedConversations,
    totalPages,
    conversationId,
    isConversationView,
    conversationDetailLoading,
    messagesLoading,
    conversationDetail,
    conversationMessages,
    newMessageText,
    sendLoading,
    startConversationLoading,
    messagesContainerRef,
    loadAssignedPatients,
    loadConversations,
    loadInitialData,
    openConversation,
    startConversationWithSelectedPatient,
    sendMessage,
    goBackToConversations,
    resetPage,
    goToPreviousPage,
    goToNextPage,
    startPolling,
    stopPolling
  }
}