<script src="../scripts/nurseMessagesView.js"></script>

<template>
  <section class="nurse-messages">
    <p v-if="loading" class="nurse-messages-message">Loading...</p>

    <p v-else-if="errorMessage" class="nurse-messages-message nurse-messages-message-error">{{ errorMessage }}</p>

    <template v-else>
      <section v-if="!isConversationView" class="nurse-messages-card">
        <header class="nurse-messages-card-head">
          <div class="nurse-messages-filters">
            <div class="nurse-messages-filterfield">
              <label for="nurseMessagesPatientFilter">Patient</label>
              <select id="nurseMessagesPatientFilter" v-model="selectedPatientId" class="nurse-messages-filterinput">
                <option value="">All</option>
                <option v-for="patient in patientOptions" :key="patient.patientId" :value="String(patient.patientId)">{{ patient.label }}</option>
              </select>
            </div>

            <div class="nurse-messages-filterfield">
              <label for="nurseMessagesDateFilter">Date</label>
              <input id="nurseMessagesDateFilter" v-model="dateFilter" type="date" class="nurse-messages-filterinput"/>
            </div>

            <div class="nurse-messages-filterfield nurse-messages-filterfield-message">
              <label for="nurseMessagesMessageFilter">Message</label>
              <input id="nurseMessagesMessageFilter" v-model="messageFilter" type="text" class="nurse-messages-filterinput" placeholder="Search message ..."/>
            </div>
          </div>

          <div class="nurse-messages-head-actions">
            <button type="button" class="nurse-messages-start-button" :disabled="!selectedPatientId || startConversationLoading" @click="startConversationWithSelectedPatient">{{ startConversationLoading ? 'Opening...' : 'Start Conversation' }}</button>
          </div>
        </header>

        <section class="nurse-messages-tablebody">
          <div v-if="conversationsLoading" class="nurse-messages-empty nurse-messages-empty-panel">Loading conversations...</div>

          <div v-else class="nurse-messages-tablewrap">
            <table class="nurse-messages-table">
              <thead>
                <tr>
                  <th>Patient</th>
                  <th>Last message</th>
                  <th>Date</th>
                  <th></th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!paginatedConversations.length">
                  <td colspan="4" class="nurse-messages-empty">No conversations found.</td>
                </tr>

                <tr v-for="conversation in paginatedConversations" :key="conversation.conversationId" class="nurse-messages-row" @click="openConversation(conversation)">
                  <td class="nurse-messages-patient">
                    <span class="nurse-messages-patient-name">{{ conversation.patientName }} {{ conversation.patientSurname }}</span>
                  </td>

                  <td class="nurse-messages-lastmessage">
                    <div class="nurse-messages-lastmessage-wrap">
                      <span :class="['nurse-messages-lastmessage-text', conversation.unreadCount > 0 ? 'nurse-messages-lastmessage-text-unread' : '']">{{ conversation.lastMessagePreview || 'No messages yet.' }}</span>
                      <span v-if="conversation.unreadCount > 0" class="nurse-messages-unreadbadge">{{ conversation.unreadCount }}</span>
                    </div>
                  </td>

                  <td class="nurse-messages-date"> {{ conversation.lastMessageDateFormatted || '-' }} </td>

                  <td class="nurse-messages-arrowcell">
                    <span class="nurse-messages-arrow">&gt;</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <footer class="nurse-messages-pagination">
          <button type="button" class="nurse-messages-pagination-button" :disabled="currentPage === 1" @click.stop="goToPreviousPage">&lt;</button>
          <span class="nurse-messages-pagination-text">Page {{ currentPage }} of {{ totalPages }}</span>
          <button type="button" class="nurse-messages-pagination-button" :disabled="currentPage === totalPages" @click.stop="goToNextPage">&gt;</button>
        </footer>
      </section>

      <section v-else class="nurse-chat-card">
        <header class="nurse-chat-head">
          <button type="button" class="nurse-chat-back" @click="goBackToConversations"><- Back</button>

          <div v-if="conversationDetail" class="nurse-chat-user">
            <div class="nurse-chat-user-photo">
              <img v-if="conversationDetail.patientPhoto" :src="conversationDetail.patientPhoto" alt="Patient photo"/>
              <span v-else>No photo</span>
            </div>

            <div class="nurse-chat-user-info">
              <h2>{{ conversationDetail.patientName }} {{ conversationDetail.patientSurname }}</h2>
              <p>Patient conversation</p>
            </div>
          </div>
        </header>

        <section ref="messagesContainerRef" class="nurse-chat-body">
          <div v-if="conversationDetailLoading || messagesLoading" class="nurse-messages-empty nurse-messages-empty-panel nurse-chat-loading">Loading conversation...</div>

          <div v-else-if="!conversationMessages.length" class="nurse-messages-empty nurse-messages-empty-panel nurse-chat-loading">No messages yet.</div>

          <div v-else class="nurse-chat-messages">
            <article v-for="message in conversationMessages" :key="message.messageId" :class="[ 'nurse-chat-message-row', message.isOwnMessage
              ? 'nurse-chat-message-row-own'
              : 'nurse-chat-message-row-other']">
              <div :class="['nurse-chat-message-bubble', message.isOwnMessage ? 'nurse-chat-message-bubble-own' : 'nurse-chat-message-bubble-other']">
                <p class="nurse-chat-message-text">{{ message.messageText }}</p>
                <span class="nurse-chat-message-time">{{ message.messageDateFormatted }}</span>
              </div>
            </article>
          </div>
        </section>

        <footer class="nurse-chat-footer">
          <textarea v-model="newMessageText" class="nurse-chat-textarea" placeholder="Write a message..." rows="3" @keydown.enter.prevent="sendMessage"></textarea>

          <div class="nurse-chat-actions">
            <button type="button" class="nurse-chat-send" :disabled="sendLoading || !newMessageText.trim()" @click="sendMessage" -> {{ sendLoading ? 'Sending...' : 'Send' }}</button>
          </div>
        </footer>
      </section>
    </template>
  </section>
</template>

<style src="../styles/nurse-messages.css"></style>