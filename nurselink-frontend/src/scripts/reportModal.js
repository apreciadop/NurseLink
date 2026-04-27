export default {
  name: 'ReportModal',

  props: {
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
  },

  computed: {
    isCreateMode() {
      return this.mode === 'create'
    },

    isNurseReviewMode() {
      return this.mode === 'nurse-review'
    },

    showNurseObservations() {
      return this.mode === 'view' || this.mode === 'nurse-review'
    },

    modalTitle() {
      if (this.isCreateMode) {
        return 'New Symptom Report'
      }

      return 'Symptom Report'
    },

    modalSubtitle() {
      if (this.isCreateMode) {
        return 'Please complete your daily recovery report.'
      }

      if (this.isNurseReviewMode) {
        return 'Review the report and add your observations.'
      }

      return 'Report details.'
    },

    submitButtonText() {
      if (this.isCreateMode) {
        return this.submitLoading ? 'Submitting...' : 'Submit Report'
      }

      return this.submitLoading ? 'Saving...' : 'Save Observations'
    }
  }
}