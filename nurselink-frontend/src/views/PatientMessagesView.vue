<script setup>
import { onBeforeUnmount, onMounted, watch } from 'vue'
import { usePatientMessages } from '../composables/usePatientMessages'

const {
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
} = usePatientMessages()

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
  <section class="patient-messages">
    <p v-if="loading" class="patient-messages-message">Loading messages...</p>

    <p v-else-if="errorMessage" class="patient-messages-message patient-messages-message-error">{{ errorMessage }}</p>

    <section v-else class="patient-chat-card">
      <header class="patient-chat-head">
        <div v-if="conversationDetailLoading" class="patient-messages-message">Loading conversation...</div>

        <div v-else-if="conversationDetail" class="patient-chat-user">
          <div class="patient-chat-user-photo">
            <img v-if="conversationDetail.nursePhoto" :src="conversationDetail.nursePhoto" alt="Nurse photo" />
            <span v-else>No photo</span>
          </div>

          <div class="patient-chat-user-info">
            <h2>{{ conversationDetail.nurseName }} {{ conversationDetail.nurseSurname }}</h2>
            <p>Assigned Nurse</p>
          </div>
        </div>
      </header>

      <section ref="messagesContainerRef" class="patient-chat-body">
        <div v-if="messagesLoading" class="patient-messages-empty patient-messages-empty-panel patient-chat-loading">Loading messages...</div>

        <div v-else-if="!conversationMessages.length" class="patient-messages-empty patient-messages-empty-panel">No messages yet.</div>

        <div v-else class="patient-chat-messages">
          <div v-for="message in conversationMessages" :key="message.messageId" :class="['patient-chat-message-row', message.isOwnMessage ? 'patient-chat-message-row-own' : 'patient-chat-message-row-other']">
            <article :class="['patient-chat-message-bubble', message.isOwnMessage ? 'patient-chat-message-bubble-own' : 'patient-chat-message-bubble-other']">
              <p class="patient-chat-message-text">{{ message.messageText }}</p>
              <span class="patient-chat-message-time">{{ message.messageDateFormatted }}</span>
            </article>
          </div>
        </div>
      </section>

      <footer class="patient-chat-footer">
        <textarea v-model="newMessageText" class="patient-chat-textarea" placeholder="Write a message..." @keydown.enter.exact.prevent="sendMessage"></textarea>

        <div class="patient-chat-actions">
          <button type="button" class="patient-chat-send" :disabled="sendLoading || !newMessageText.trim()" @click="sendMessage">
            {{ sendLoading ? 'Sending...' : 'Send' }}
          </button>
        </div>
      </footer>
    </section>
  </section>
</template>

<style src="../styles/patient-messages.css"></style>