<script setup>
import { onBeforeUnmount, onMounted, watch } from 'vue'
import { useNurseMessages } from '../composables/useNurseMessages'

const {
  loading,
  errorMessage,
  conversationsLoading,
  selectedPatientId,
  patientOptions,
  dateFilter,
  messageFilter,
  currentPage,
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
} = useNurseMessages()

watch([selectedPatientId, dateFilter, messageFilter], () => {
  resetPage()
})

watch(totalPages, () => {
  if (currentPage.value > totalPages.value) {
    currentPage.value = totalPages.value
  }
})

watch(conversationId, async () => {
  stopPolling()
  await loadInitialData()
  startPolling()
})

onMounted(async () => {
  await loadInitialData()
  startPolling()
})

onBeforeUnmount(() => {
  stopPolling()
})
</script>

<template>
  <section class="nurse-messages">
    <p v-if="loading" class="nurse-messages-message">Loading messages...</p>

    <p v-else-if="errorMessage" class="nurse-messages-message nurse-messages-message-error">{{ errorMessage }}</p>

    <section v-else-if="!isConversationView" class="nurse-messages-card">
      <header class="nurse-messages-card-head">
        <section class="nurse-messages-filters">
          <div class="nurse-messages-filterfield">
            <label for="patientFilter">Patient</label>
            <select id="patientFilter" v-model="selectedPatientId" class="nurse-messages-filterinput">
              <option value="">All patients</option>
              <option v-for="patient in patientOptions" :key="patient.patientId" :value="String(patient.patientId)">{{ patient.label }}</option>
            </select>
          </div>

          <div class="nurse-messages-filterfield">
            <label for="dateFilter">Date</label>
            <input id="dateFilter" v-model="dateFilter" type="date" class="nurse-messages-filterinput" />
          </div>

          <div class="nurse-messages-filterfield nurse-messages-filterfield-message">
            <label for="messageFilter">Message</label>
            <input id="messageFilter" v-model="messageFilter" type="text" class="nurse-messages-filterinput" placeholder="Search by message..." />
          </div>
        </section>

        <div class="nurse-messages-head-actions">
          <button type="button" class="nurse-messages-start-button" :disabled="!selectedPatientId || startConversationLoading" @click="startConversationWithSelectedPatient">
            {{ startConversationLoading ? 'Opening...' : 'Start Conversation' }}
          </button>
        </div>
      </header>

      <section class="nurse-messages-tablebody">
        <div v-if="conversationsLoading" class="nurse-messages-empty nurse-messages-empty-panel">Loading conversations...</div>

        <div v-else class="nurse-messages-tablewrap">
          <table class="nurse-messages-table">
            <thead>
              <tr>
                <th>Photo</th>
                <th>Patient</th>
                <th>Last Message</th>
                <th>Date</th>
                <th></th>
              </tr>
            </thead>

            <tbody>
              <tr v-if="!paginatedConversations.length">
                <td colspan="5" class="nurse-messages-empty">No conversations found.</td>
              </tr>

              <tr v-for="conversation in paginatedConversations" :key="conversation.conversationId" class="nurse-messages-row" @click="openConversation(conversation)">
                <td class="nurse-messages-photo-cell">
                  <div class="nurse-messages-patient-photo">
                    <img v-if="conversation.patientPhoto" :src="conversation.patientPhoto" alt="Patient photo" />
                    <span v-else>No photo</span>
                  </div>
                </td>

                <td>
                  <span class="nurse-messages-patient-name">{{ conversation.patientName }} {{ conversation.patientSurname }}</span>
                </td>

                <td class="nurse-messages-lastmessage">
                  <div class="nurse-messages-lastmessage-wrap">
                    <span :class="['nurse-messages-lastmessage-text', conversation.unreadCount > 0 ? 'nurse-messages-lastmessage-text-unread' : '']">
                      {{ conversation.lastMessagePreview || 'No messages yet.' }}
                    </span>

                    <span v-if="conversation.unreadCount > 0" class="nurse-messages-unreadbadge">{{ conversation.unreadCount }}</span>
                  </div>
                </td>

                <td class="nurse-messages-date">{{ conversation.lastMessageDateFormatted || '-' }}</td>

                <td class="nurse-messages-arrowcell">
                  <span class="nurse-messages-arrow">›</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <footer class="app-pagination">
        <button type="button" class="app-pagination-button" :disabled="currentPage === 1" @click="goToPreviousPage">&lt;</button>
        <span class="app-pagination-text">Page {{ currentPage }} of {{ totalPages }}</span>
        <button type="button" class="app-pagination-button" :disabled="currentPage === totalPages" @click="goToNextPage">&gt;</button>
      </footer>
    </section>

    <section v-else class="nurse-chat-card">
      <header class="nurse-chat-head">
        <button type="button" class="nurse-chat-back" @click="goBackToConversations">&lt;- Back</button>

        <div v-if="conversationDetailLoading" class="nurse-messages-message">Loading conversation...</div>

        <div v-else-if="conversationDetail" class="nurse-chat-user">
          <div class="nurse-chat-user-photo">
            <img v-if="conversationDetail.patientPhoto" :src="conversationDetail.patientPhoto" alt="Patient photo" />
            <span v-else>No photo</span>
          </div>

          <div class="nurse-chat-user-info">
            <h2>{{ conversationDetail.patientName }} {{ conversationDetail.patientSurname }}</h2>
            <p>Patient</p>
          </div>
        </div>
      </header>

      <section ref="messagesContainerRef" class="nurse-chat-body">
        <div v-if="messagesLoading" class="nurse-messages-empty nurse-messages-empty-panel nurse-chat-loading">Loading messages...</div>

        <div v-else-if="!conversationMessages.length" class="nurse-messages-empty nurse-messages-empty-panel">No messages yet.</div>

        <div v-else class="nurse-chat-messages">
          <div v-for="message in conversationMessages" :key="message.messageId" :class="['nurse-chat-message-row', message.isOwnMessage ? 'nurse-chat-message-row-own' : 'nurse-chat-message-row-other']">
            <article :class="['nurse-chat-message-bubble', message.isOwnMessage ? 'nurse-chat-message-bubble-own' : 'nurse-chat-message-bubble-other']">
              <p class="nurse-chat-message-text">{{ message.messageText }}</p>
              <span class="nurse-chat-message-time">{{ message.messageDateFormatted }}</span>
            </article>
          </div>
        </div>
      </section>

      <footer class="nurse-chat-footer">
        <textarea v-model="newMessageText" class="nurse-chat-textarea" placeholder="Write a message..." @keydown.enter.exact.prevent="sendMessage"></textarea>

        <div class="nurse-chat-actions">
          <button type="button" class="nurse-chat-send" :disabled="sendLoading || !newMessageText.trim()" @click="sendMessage">
            {{ sendLoading ? 'Sending...' : 'Send' }}
          </button>
        </div>
      </footer>
    </section>
  </section>
</template>

<style src="../styles/nurse-messages.css"></style>