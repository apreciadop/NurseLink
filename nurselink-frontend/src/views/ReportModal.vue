<script setup>
import { computed } from 'vue'

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  mode: {
    type: String,
    default: 'view'
  },
  report: {
    type: Object,
    default: () => ({
      painLevel: 0,
      hasFever: false,
      hasBleeding: false,
      hasSwelling: false,
      observations: '',
      nurseObservations: ''
    })
  },
  painLevels: {
    type: Array,
    default: () => [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
  },
  loading: {
    type: Boolean,
    default: false
  },
  submitLoading: {
    type: Boolean,
    default: false
  },
  errorMessage: {
    type: String,
    default: ''
  }
})

const emit = defineEmits([
  'close',
  'submit',
  'set-pain-level',
  'set-has-fever',
  'set-has-bleeding',
  'set-has-swelling',
  'update-observations',
  'update-nurse-observations'
])

const isCreateMode = computed(() => {
  return props.mode === 'create'
})

const isNurseReviewMode = computed(() => {
  return props.mode === 'nurse-review'
})

const showNurseObservations = computed(() => {
  return props.mode === 'view' || props.mode === 'nurse-review'
})

const modalTitle = computed(() => {
  if (isCreateMode.value) {
    return 'New Symptom Report'
  }

  return 'Symptom Report'
})

const modalSubtitle = computed(() => {
  if (isCreateMode.value) {
    return 'Please complete your daily recovery report.'
  }

  if (isNurseReviewMode.value) {
    return 'Review the report and add your observations.'
  }

  return 'Report details.'
})

const submitButtonText = computed(() => {
  if (isCreateMode.value) {
    return props.submitLoading ? 'Submitting...' : 'Submit Report'
  }

  return props.submitLoading ? 'Saving...' : 'Save Observations'
})
</script>

<template>
  <section v-if="visible" class="report-modal">
    <div class="report-modal-card">
      <header class="report-modal-head">
        <div class="report-modal-headtext">
          <h2 class="report-modal-title">{{ modalTitle }}</h2>
          <p class="report-modal-subtitle">{{ modalSubtitle }}</p>
        </div>

        <button type="button" class="report-modal-close" aria-label="Close modal" @click="emit('close')">×</button>
      </header>

      <section class="report-modal-body">
        <p v-if="errorMessage" class="report-modal-message report-modal-message-error">{{ errorMessage }}</p>
        <p v-if="loading" class="report-modal-message">Loading report...</p>

        <template v-else>
          <section class="report-modal-section">
            <h3 class="report-modal-section-title">Pain Level</h3>

            <div class="report-modal-painscale">
              <div class="report-modal-painlevels">
                <button v-for="level in painLevels" :key="level" type="button" :disabled="!isCreateMode" :class="['report-modal-painbutton', Number(report.painLevel) === Number(level) ? 'report-modal-painbutton-active' : '', !isCreateMode ? 'report-modal-painbutton-readonly' : '']" @click="emit('set-pain-level', level)">{{ level }}</button>
              </div>

              <div class="report-modal-painlegend">
                <span>No pain</span>
                <span>Worst pain</span>
              </div>
            </div>
          </section>

          <section class="report-modal-symptoms">
            <article class="report-modal-symptomcard">
              <h3 class="report-modal-symptomtitle">Fever</h3>
              <p class="report-modal-symptomtext">Do you have fever?</p>

              <div class="report-modal-toggle">
                <button type="button" :disabled="!isCreateMode" :class="['report-modal-togglebutton', report.hasFever === true ? 'report-modal-togglebutton-yes' : '', !isCreateMode ? 'report-modal-togglebutton-readonly' : '']" @click="emit('set-has-fever', true)">Yes</button>

                <button type="button" :disabled="!isCreateMode" :class="['report-modal-togglebutton', report.hasFever === false ? 'report-modal-togglebutton-no' : '', !isCreateMode ? 'report-modal-togglebutton-readonly' : '']" @click="emit('set-has-fever', false)">No</button>
              </div>
            </article>

            <article class="report-modal-symptomcard">
              <h3 class="report-modal-symptomtitle">Bleeding</h3>
              <p class="report-modal-symptomtext">Are you experiencing bleeding?</p>

              <div class="report-modal-toggle">
                <button type="button" :disabled="!isCreateMode" :class="['report-modal-togglebutton', report.hasBleeding === true ? 'report-modal-togglebutton-yes' : '', !isCreateMode ? 'report-modal-togglebutton-readonly' : '']" @click="emit('set-has-bleeding', true)">Yes</button>

                <button type="button" :disabled="!isCreateMode" :class="['report-modal-togglebutton', report.hasBleeding === false ? 'report-modal-togglebutton-no' : '', !isCreateMode ? 'report-modal-togglebutton-readonly' : '']" @click="emit('set-has-bleeding', false)">No</button>
              </div>
            </article>

            <article class="report-modal-symptomcard">
              <h3 class="report-modal-symptomtitle">Swelling</h3>
              <p class="report-modal-symptomtext">Are you experiencing swelling?</p>

              <div class="report-modal-toggle">
                <button type="button" :disabled="!isCreateMode" :class="['report-modal-togglebutton', report.hasSwelling === true ? 'report-modal-togglebutton-yes' : '', !isCreateMode ? 'report-modal-togglebutton-readonly' : '']" @click="emit('set-has-swelling', true)">Yes</button>

                <button type="button" :disabled="!isCreateMode" :class="['report-modal-togglebutton', report.hasSwelling === false ? 'report-modal-togglebutton-no' : '', !isCreateMode ? 'report-modal-togglebutton-readonly' : '']" @click="emit('set-has-swelling', false)">No</button>
              </div>
            </article>
          </section>

          <section class="report-modal-section">
            <h3 class="report-modal-section-title">Additional Notes</h3>

            <textarea class="report-modal-textarea" :readonly="!isCreateMode" :value="report.observations" :placeholder="isCreateMode ? 'Write additional details about your condition...' : 'No additional notes.'" @input="emit('update-observations', $event.target.value)"></textarea>
          </section>

          <section v-if="showNurseObservations" class="report-modal-section">
            <h3 class="report-modal-section-title">Nurse Observations</h3>

            <textarea class="report-modal-textarea" :readonly="!isNurseReviewMode" :value="report.nurseObservations" :placeholder="isNurseReviewMode ? 'Write your observations here...' : 'No nurse observations.'" @input="emit('update-nurse-observations', $event.target.value)"></textarea>
          </section>
        </template>
      </section>

      <footer class="report-modal-footer">
        <button type="button" class="app-button app-button-secondary" @click="emit('close')">{{ isCreateMode ? 'Cancel' : 'Close' }}</button>

        <button v-if="isCreateMode || isNurseReviewMode" type="button" class="app-button app-button-primary" :disabled="submitLoading || loading" @click="emit('submit')">{{ submitButtonText }}</button>
      </footer>
    </div>
  </section>
</template>

<style src="../styles/report-modal.css"></style>