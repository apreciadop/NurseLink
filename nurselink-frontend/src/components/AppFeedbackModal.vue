<script setup>
defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  type: {
    type: String,
    default: 'success'
  },
  message: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['close'])
</script>

<template>
  <section v-if="visible" class="app-feedback-modal" @click.self="emit('close')">
    <article class="app-feedback-card">
      <h3 :class="['app-feedback-title', type === 'error' ? 'app-feedback-title-error' : 'app-feedback-title-success']">
        {{ type === 'error' ? 'Error' : 'Success' }}
      </h3>

      <p class="app-feedback-message">{{ message }}</p>

      <button type="button" class="app-button app-button-primary" @click="emit('close')">Accept</button>
    </article>
  </section>
</template>