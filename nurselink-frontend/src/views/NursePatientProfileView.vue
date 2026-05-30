<script setup>
import { onMounted, watch } from 'vue'
import AppFeedbackModal from '../components/AppFeedbackModal.vue'
import ReportModal from '../views/ReportModal.vue'
import { useNursePatientProfile } from '../composables/useNursePatientProfile'

const {
  patient,
  loading,
  errorMessage,
  successMessage,
  reportsLoading,
  reportsErrorMessage,
  reportsCurrentPage,
  reportsTotalPages,
  paginatedReports,
  isReportModalOpen,
  reportDetailLoading,
  reportDetailErrorMessage,
  reportSaveLoading,
  reportSaveErrorMessage,
  selectedReport,
  painLevels,
  loadPatientProfileData,
  openViewReportModal,
  closeViewReportModal,
  saveNurseObservations,
  clearNursePatientProfileFeedback,
  goBack,
  goToPreviousReportsPage,
  goToNextReportsPage
} = useNursePatientProfile()

watch(reportsTotalPages, () => {
  if (reportsCurrentPage.value > reportsTotalPages.value) {
    reportsCurrentPage.value = reportsTotalPages.value
  }
})

onMounted(() => {
  loadPatientProfileData()
})
</script>

<template>
  <section class="nurse-patient-profile">
    <p v-if="loading" class="nurse-patient-profile-message">Loading patient profile...</p>

    <p v-else-if="errorMessage" class="nurse-patient-profile-message nurse-patient-profile-message-error">{{ errorMessage }}</p>

    <section v-else-if="patient" class="nurse-patient-profile-main">
      <button type="button" class="app-button app-button-secondary nurse-patient-profile-backbutton" @click="goBack">
        &lt;- Back to Dashboard
      </button>

      <section class="nurse-patient-profile-card">
        <section class="nurse-patient-profile-top">
          <aside class="nurse-patient-profile-side">
            <div class="nurse-patient-profile-photo">
              <img v-if="patient.photo" :src="patient.photo" alt="Patient photo" />
              <span v-else>No photo</span>
            </div>

            <span :class="['app-badge', patient.statusLabel === 'Stable' ? 'app-badge-stable' : patient.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">
              {{ patient.statusLabel }}
            </span>
          </aside>

          <section class="nurse-patient-profile-content">
            <header class="nurse-patient-profile-header">
              <div class="nurse-patient-profile-header-left">
                <h2 class="nurse-patient-profile-name">{{ patient.name }} {{ patient.surname }}</h2>
                <p class="nurse-patient-profile-id">Patient ID: {{ patient.patientId }}</p>
              </div>

              <div class="nurse-patient-profile-assigned">
                <div class="nurse-patient-profile-assigned-photo">
                  <img v-if="patient.assignedNursePhoto" :src="patient.assignedNursePhoto" alt="Assigned nurse photo" />
                  <span v-else>No photo</span>
                </div>

                <div class="nurse-patient-profile-assigned-info">
                  <span class="nurse-patient-profile-assigned-label">Assigned Nurse</span>
                  <strong class="nurse-patient-profile-assigned-name">{{ patient.assignedNurseName || '-' }}</strong>
                </div>
              </div>
            </header>

            <section class="nurse-patient-profile-panels">
              <article class="nurse-patient-profile-panel">
                <h3 class="nurse-patient-profile-panel-title">Personal Information</h3>

                <div class="nurse-patient-profile-info-grid">
                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Email</span>
                    <strong class="nurse-patient-profile-value">{{ patient.email || '-' }}</strong>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Phone</span>
                    <strong class="nurse-patient-profile-value">{{ patient.phone || '-' }}</strong>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Birthdate</span>
                    <strong class="nurse-patient-profile-value">{{ patient.birthdate || '-' }}</strong>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Age</span>
                    <strong class="nurse-patient-profile-value">{{ patient.age ?? '-' }}</strong>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Active</span>
                    <strong class="nurse-patient-profile-value">{{ patient.active ? 'Yes' : 'No' }}</strong>
                  </div>
                </div>
              </article>

              <article class="nurse-patient-profile-panel">
                <h3 class="nurse-patient-profile-panel-title">Surgery Information</h3>

                <div class="nurse-patient-profile-info-grid">
                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Surgery</span>
                    <strong class="nurse-patient-profile-value">{{ patient.surgeryName || '-' }}</strong>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Date</span>
                    <strong class="nurse-patient-profile-value">{{ patient.surgeryDate || '-' }}</strong>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Alerts</span>
                    <strong class="nurse-patient-profile-value">
                      <span :class="['app-badge', 'app-badge-small', patient.alertCount === 0 ? 'app-badge-stable' : patient.alertCount <= 2 ? 'app-badge-warning' : 'app-badge-alert']">{{ patient.alertCount }}</span>
                    </strong>
                  </div>
                </div>
              </article>
            </section>
          </section>
        </section>
      </section>

      <section class="nurse-patient-profile-card">
        <header class="nurse-patient-profile-section-header">
          <h3 class="nurse-patient-profile-section-title">Patient Observations</h3>
        </header>

        <section class="nurse-patient-profile-observations">
          {{ patient.patientObservations || 'No observations available.' }}
        </section>
      </section>

      <section class="nurse-patient-profile-card">
        <header class="nurse-patient-profile-section-header">
          <h3 class="nurse-patient-profile-section-title">Symptom Reports</h3>
        </header>

        <section class="nurse-patient-profile-tablebody">
          <div v-if="reportsLoading" class="nurse-patient-profile-empty">Loading reports...</div>

          <p v-else-if="reportsErrorMessage" class="nurse-patient-profile-message nurse-patient-profile-message-error">{{ reportsErrorMessage }}</p>

          <div v-else class="nurse-patient-profile-tablewrap">
            <table class="nurse-patient-profile-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Pain Level</th>
                  <th>Fever</th>
                  <th>Bleeding</th>
                  <th>Swelling</th>
                  <th>Alerts</th>
                  <th>View</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!paginatedReports.length">
                  <td colspan="8" class="nurse-patient-profile-empty">No symptom reports found.</td>
                </tr>

                <tr v-for="report in paginatedReports" :key="report.reportId">
                  <td>{{ report.reportDate || '-' }}</td>

                  <td class="nurse-patient-profile-col-center">
                    <span :class="['app-badge', report.statusLabel === 'Stable' ? 'app-badge-stable' : report.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">{{ report.statusLabel }}</span>
                  </td>

                  <td class="nurse-patient-profile-col-center">
                    <span :class="['nurse-patient-profile-pain', report.painLevel >= 7 ? 'nurse-patient-profile-pain-alert' : 'nurse-patient-profile-pain-ok']">{{ report.painLevel ?? '-' }}</span>
                  </td>

                  <td class="nurse-patient-profile-col-center">
                    <span :class="['nurse-patient-profile-kpi-dot', report.hasFever ? 'nurse-patient-profile-kpi-dot-yes' : 'nurse-patient-profile-kpi-dot-no']" :title="report.hasFever ? 'Yes' : 'No'"></span>
                  </td>

                  <td class="nurse-patient-profile-col-center">
                    <span :class="['nurse-patient-profile-kpi-dot', report.hasBleeding ? 'nurse-patient-profile-kpi-dot-yes' : 'nurse-patient-profile-kpi-dot-no']" :title="report.hasBleeding ? 'Yes' : 'No'"></span>
                  </td>

                  <td class="nurse-patient-profile-col-center">
                    <span :class="['nurse-patient-profile-kpi-dot', report.hasSwelling ? 'nurse-patient-profile-kpi-dot-yes' : 'nurse-patient-profile-kpi-dot-no']" :title="report.hasSwelling ? 'Yes' : 'No'"></span>
                  </td>

                  <td class="nurse-patient-profile-col-center">
                    <span :class="['app-badge', 'app-badge-small', report.alertCount === 0 ? 'app-badge-stable' : report.alertCount <= 2 ? 'app-badge-warning' : 'app-badge-alert']">{{ report.alertCount }}</span>
                  </td>

                  <td class="nurse-patient-profile-col-center">
                    <button type="button" class="app-action-button" aria-label="View report" title="View report" @click="openViewReportModal(report)">
                      <img src="/icons/view.png" alt="View report" class="app-action-icon" />
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <footer class="app-pagination nurse-patient-profile-pagination">
          <button type="button" class="app-pagination-button" :disabled="reportsCurrentPage === 1" @click="goToPreviousReportsPage">&lt;</button>
          <span class="app-pagination-text">Page {{ reportsCurrentPage }} of {{ reportsTotalPages }}</span>
          <button type="button" class="app-pagination-button" :disabled="reportsCurrentPage === reportsTotalPages" @click="goToNextReportsPage">&gt;</button>
        </footer>
      </section>
    </section>

    <ReportModal :visible="isReportModalOpen" mode="nurse-review" :report="selectedReport || {}" :pain-levels="painLevels" :loading="reportDetailLoading" :submit-loading="reportSaveLoading" :error-message="reportDetailErrorMessage || reportSaveErrorMessage" @close="closeViewReportModal" @submit="saveNurseObservations" @update-nurse-observations="selectedReport.nurseObservations = $event" />

    <AppFeedbackModal :visible="!!(successMessage || reportSaveErrorMessage)" :message="reportSaveErrorMessage || successMessage" :type="reportSaveErrorMessage ? 'error' : 'success'" @close="clearNursePatientProfileFeedback" />
  </section>
</template>

<style src="../styles/nurse-patient-profile.css"></style>