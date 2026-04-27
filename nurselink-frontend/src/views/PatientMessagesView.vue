<script src="../scripts/patientMessagesView.js"></script>

<template>
  <section class="patient-messages">
    <p v-if="loading" class="patient-messages-message">Loading conversation...</p>
    <p v-else-if="errorMessage" class="patient-messages-message patient-messages-message-error">{{ errorMessage }}</p>

    <section v-else class="patient-chat-card">
      <header class="patient-chat-head">
        <div v-if="conversationDetail" class="patient-chat-user">
          <div class="patient-chat-user-photo">
            <img v-if="conversationDetail.nursePhoto" :src="conversationDetail.nursePhoto" alt="Nurse photo"/>
            <span v-else>No photo</span>
          </div>

          <div class="patient-chat-user-info">
            <h2>{{ conversationDetail.nurseName }} {{ conversationDetail.nurseSurname }}</h2>
            <p>Assigned nurse</p>
          </div>
        </div>
      </header>

      <section ref="messagesContainerRef" class="patient-chat-body">
        <div v-if="conversationDetailLoading || messagesLoading" class="patient-messages-empty patient-messages-empty-panel patient-chat-loading">Loading conversation...</div>
        <div v-else-if="!conversationMessages.length" class="patient-messages-empty patient-messages-empty-panel patient-chat-loading">No messages yet.</div>
        <div v-else class="patient-chat-messages">
          <article v-for="message in conversationMessages" :key="message.messageId" :class="['patient-chat-message-row', message.isOwnMessage ? 'patient-chat-message-row-own' : 'patient-chat-message-row-other']">
            <div :class="['patient-chat-message-bubble', message.isOwnMessage ? 'patient-chat-message-bubble-own' : 'patient-chat-message-bubble-other']">
              <p class="patient-chat-message-text">{{ message.messageText }}</p>
              <span class="patient-chat-message-time">{{ message.messageDateFormatted }}</span>
            </div>
          </article>
        </div>
      </section>

      <footer class="patient-chat-footer">
        <textarea v-model="newMessageText" class="patient-chat-textarea" placeholder="Write a message..." rows="3" @keydown.enter.prevent="sendMessage"></textarea>

        <div class="patient-chat-actions">
          <button type="button" class="patient-chat-send" :disabled="sendLoading || !newMessageText.trim()" @click="sendMessage" -> {{ sendLoading ? 'Sending...' : 'Send' }} </button>
        </div>
      </footer>
    </section>
  </section>
</template>

<style src="../styles/patient-messages.css"></style>