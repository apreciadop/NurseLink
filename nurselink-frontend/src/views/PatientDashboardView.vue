<script setup>
import { onMounted, watch } from 'vue'
import AppFeedbackModal from '../components/AppFeedbackModal.vue'
import ReportModal from '../views/ReportModal.vue'
import { usePatientDashboard } from '../composables/usePatientDashboard'

const {
  loading,
  errorMessage,
  successMessage,
  patient,
  reports,
  reportDateFilter,
  reportStatusFilter,
  reportsLoading,
  reportsErrorMessage,
  paginatedReports,
  reportsCurrentPage,
  reportsTotalPages,
  isCreateReportModalOpen,
  createReportLoading,
  createReportErrorMessage,
  isViewReportModalOpen,
  reportDetailLoading,
  reportDetailErrorMessage,
  selectedReport,
  painLevels,
  reportForm,
  createModalReport,
  loadPatientDashboard,
  openCreateReportModal,
  closeCreateReportModal,
  submitCreateReport,
  openViewReportModal,
  closeViewReportModal,
  resetReportsPage,
  goToPreviousReportsPage,
  goToNextReportsPage,
  clearPatientDashboardFeedback
} = usePatientDashboard()

watch(reportDateFilter, () => {
  resetReportsPage()
})

watch(reportStatusFilter, () => {
  resetReportsPage()
})

watch(reportsTotalPages, () => {
  if (reportsCurrentPage.value > reportsTotalPages.value) {
    reportsCurrentPage.value = reportsTotalPages.value
  }
})

onMounted(() => {
  loadPatientDashboard()
})
</script>

<template>
  <section class="patient-dashboard">
    <p v-if="loading" class="patient-dashboard-message">Loading patient dashboard...</p>

    <p v-else-if="errorMessage" class="patient-dashboard-message patient-dashboard-message-error">{{ errorMessage }}</p>

    <section v-else class="patient-dashboard-main">
      <section class="patient-dashboard-profilecard">
        <div class="patient-dashboard-profileleft">
          <div class="patient-dashboard-photo">
            <img v-if="patient.photo" :src="patient.photo" alt="Patient photo" />
            <span v-else>No photo</span>
          </div>

          <span :class="['app-badge', patient.statusLabel === 'Stable' ? 'app-badge-stable' : patient.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">{{ patient.statusLabel }}</span>
        </div>

        <div class="patient-dashboard-profileinfo">
          <div class="patient-dashboard-titleblock">
            <h2>{{ patient.name }} {{ patient.surname }}</h2>
            <p>Patient ID {{ patient.patientId }}</p>
          </div>

          <div class="patient-dashboard-twocolumns">
            <section class="patient-dashboard-datacolumn">
              <h3>Personal Information</h3>

              <div class="patient-dashboard-infoitem">
                <span>Email</span>
                <strong>{{ patient.email || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Phone</span>
                <strong>{{ patient.phone || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Birthdate</span>
                <strong>{{ patient.birthdate || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Age</span>
                <strong>{{ patient.age ?? '-' }}</strong>
              </div>
            </section>

            <section class="patient-dashboard-datacolumn">
              <h3>Surgery Information</h3>

              <div class="patient-dashboard-infoitem">
                <span>Surgery</span>
                <strong>{{ patient.surgery || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Date</span>
                <strong>{{ patient.surgeryDate || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Alerts</span>
                <strong>{{ patient.alertCount }}</strong>
              </div>
            </section>
          </div>
        </div>

        <div class="patient-dashboard-nurse">
          <div class="patient-dashboard-nursephoto">
            <img v-if="patient.assignedNursePhoto" :src="patient.assignedNursePhoto" alt="Assigned nurse photo" />
            <span v-else>No photo</span>
          </div>

          <div class="patient-dashboard-nurseinfo">
            <span class="patient-dashboard-nurselabel">Assigned Nurse</span>
            <strong class="patient-dashboard-nursename">{{ patient.assignedNurseName || 'No nurse assigned' }}</strong>
          </div>
        </div>
      </section>

      <section class="patient-dashboard-tablecard">
        <header class="patient-dashboard-reporthead">
          <div class="patient-dashboard-reporttitle-row">
            <h2>Symptom Reports</h2>
            <button type="button" class="app-button app-button-primary" @click="openCreateReportModal">+ New Report</button>
          </div>

          <div class="patient-dashboard-reportfilters">
            <div class="patient-dashboard-datefilter">
              <label for="reportDateFilter" class="app-filter-label">Date</label>
              <input id="reportDateFilter" v-model="reportDateFilter" type="date" class="app-input patient-dashboard-dateinput" />
            </div>

            <div class="patient-dashboard-statusfilter">
              <span class="app-filter-label">Status</span>

              <label class="app-filter-option">
                <input v-model="reportStatusFilter" type="radio" value="all" />
                <span>All</span>
              </label>

              <label class="app-filter-option">
                <input v-model="reportStatusFilter" type="radio" value="stable" />
                <span>Stable</span>
              </label>

              <label class="app-filter-option">
                <input v-model="reportStatusFilter" type="radio" value="warning" />
                <span>Warning</span>
              </label>

              <label class="app-filter-option">
                <input v-model="reportStatusFilter" type="radio" value="alert" />
                <span>Alert</span>
              </label>
            </div>
          </div>
        </header>

        <section class="patient-dashboard-tablebody">
          <div v-if="reportsLoading" class="patient-dashboard-empty patient-dashboard-empty-panel">Loading reports...</div>

          <p v-else-if="reportsErrorMessage" class="patient-dashboard-message patient-dashboard-message-error">{{ reportsErrorMessage }}</p>

          <div v-else class="patient-dashboard-tablewrap">
            <table class="patient-dashboard-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Pain Level</th>
                  <th>Fever</th>
                  <th>Bleeding</th>
                  <th>Swelling</th>
                  <th>Alerts</th>
                  <th>Nurse Notes</th>
                  <th>View</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!paginatedReports.length">
                  <td colspan="9" class="patient-dashboard-empty">No reports found.</td>
                </tr>

                <tr v-for="report in paginatedReports" :key="report.reportId">
                  <td>{{ report.reportDate || '-' }}</td>

                  <td class="patient-dashboard-col-center">
                    <span :class="['app-badge', report.statusLabel === 'Stable' ? 'app-badge-stable' : report.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">{{ report.statusLabel }}</span>
                  </td>

                  <td class="patient-dashboard-col-center">{{ report.painLevel ?? '-' }}</td>
                  <td>{{ report.hasFever ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasBleeding ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasSwelling ? 'Yes' : 'No' }}</td>

                  <td class="patient-dashboard-col-center">
                    <span :class="['app-badge', 'app-badge-small', report.alertCount === 0 ? 'app-badge-stable' : report.alertCount <= 2 ? 'app-badge-warning' : 'app-badge-alert']">{{ report.alertCount }}</span>
                  </td>

                  <td class="patient-dashboard-col-center">
                    <span class="app-badge app-badge-note">{{ report.hasNurseObservations ? 'Yes' : 'No' }}</span>
                  </td>

                  <td class="patient-dashboard-col-center">
                    <button type="button" class="app-action-button" aria-label="View report" title="View report" @click.stop="openViewReportModal(report)">
                      <img src="/icons/view.png" alt="View report" class="app-action-icon" />
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <footer class="app-pagination">
          <button type="button" class="app-pagination-button" :disabled="reportsCurrentPage === 1" @click="goToPreviousReportsPage">&lt;</button>
          <span class="app-pagination-text">Page {{ reportsCurrentPage }} of {{ reportsTotalPages }}</span>
          <button type="button" class="app-pagination-button" :disabled="reportsCurrentPage === reportsTotalPages" @click="goToNextReportsPage">&gt;</button>
        </footer>
      </section>
    </section>

    <ReportModal :visible="isCreateReportModalOpen" mode="create" :report="createModalReport" :pain-levels="painLevels" :loading="createReportLoading" :submit-loading="createReportLoading" :error-message="createReportErrorMessage" @close="closeCreateReportModal" @submit="submitCreateReport" @set-pain-level="reportForm.painLevel = $event" @set-has-fever="reportForm.hasFever = $event" @set-has-bleeding="reportForm.hasBleeding = $event" @set-has-swelling="reportForm.hasSwelling = $event" @update-observations="reportForm.observations = $event" />

    <ReportModal :visible="isViewReportModalOpen" mode="view" :report="selectedReport || {}" :pain-levels="painLevels" :loading="reportDetailLoading" :error-message="reportDetailErrorMessage" @close="closeViewReportModal" />

    <AppFeedbackModal :visible="!!successMessage" :message="successMessage" type="success" @close="clearPatientDashboardFeedback" />
  </section>
</template>

<style src="../styles/patient-dashboard.css"></style>