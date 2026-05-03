<script setup>
import { onMounted, watch } from 'vue'
import AppFeedbackModal from '../components/AppFeedbackModal.vue'
import ReportModal from '../views/ReportModal.vue'
import { useNurseDashboard } from '../composables/useNurseDashboard'

const {
  patients,
  reports,
  selectedPatient,
  searchTerm,
  activeFilter,
  statusFilter,
  currentPage,
  reportsCurrentPage,
  loading,
  errorMessage,
  successMessage,
  reportsLoading,
  reportsErrorMessage,
  totalPages,
  paginatedPatients,
  reportsTotalPages,
  paginatedReports,
  isReportModalOpen,
  reportDetailLoading,
  reportDetailErrorMessage,
  reportSaveLoading,
  reportSaveErrorMessage,
  selectedReport,
  painLevels,
  loadAssignedPatients,
  selectPatient,
  openPatientProfile,
  openViewReportModal,
  closeViewReportModal,
  saveNurseObservations,
  resetPatientsPage,
  goToPreviousPage,
  goToNextPage,
  goToPreviousReportsPage,
  goToNextReportsPage,
  clearNurseDashboardFeedback
} = useNurseDashboard()

watch(searchTerm, () => {
  resetPatientsPage()
})

watch(activeFilter, () => {
  resetPatientsPage()
})

watch(statusFilter, () => {
  resetPatientsPage()
})

watch(totalPages, () => {
  if (currentPage.value > totalPages.value) {
    currentPage.value = totalPages.value
  }
})

watch(reportsTotalPages, () => {
  if (reportsCurrentPage.value > reportsTotalPages.value) {
    reportsCurrentPage.value = reportsTotalPages.value
  }
})

onMounted(() => {
  loadAssignedPatients()
})
</script>

<template>
  <section class="nurse-dashboard">
    <p v-if="loading" class="nurse-dashboard-message">Loading assigned patients...</p>

    <p v-else-if="errorMessage" class="nurse-dashboard-message nurse-dashboard-message-error">{{ errorMessage }}</p>

    <section v-else class="nurse-dashboard-main">
      <section class="nurse-dashboard-tablecard">
        <header class="nurse-dashboard-tablehead">
          <div class="nurse-dashboard-search">
            <input v-model="searchTerm" type="text" class="app-input" placeholder="Search patients, surgery, phone or status ..." aria-label="Search patients" />
          </div>
        </header>

        <section class="nurse-dashboard-filtersrow">
          <div class="app-filter-group">
            <span class="app-filter-label">Activity</span>

            <label class="app-filter-option">
              <input v-model="activeFilter" type="radio" value="all" />
              <span>All</span>
            </label>

            <label class="app-filter-option">
              <input v-model="activeFilter" type="radio" value="active" />
              <span>Active</span>
            </label>

            <label class="app-filter-option">
              <input v-model="activeFilter" type="radio" value="inactive" />
              <span>Inactive</span>
            </label>
          </div>

          <div class="nurse-dashboard-statusfilter app-filter-group">
            <span class="app-filter-label">Status</span>

            <label class="app-filter-option">
              <input v-model="statusFilter" type="radio" value="all" />
              <span>All</span>
            </label>

            <label class="app-filter-option">
              <input v-model="statusFilter" type="radio" value="stable" />
              <span>Stable</span>
            </label>

            <label class="app-filter-option">
              <input v-model="statusFilter" type="radio" value="warning" />
              <span>Warning</span>
            </label>

            <label class="app-filter-option">
              <input v-model="statusFilter" type="radio" value="alert" />
              <span>Alert</span>
            </label>
          </div>
        </section>

        <section class="nurse-dashboard-tablebody">
          <div class="nurse-dashboard-tablewrap">
            <table class="nurse-dashboard-table">
              <thead>
                <tr>
                  <th>Photo</th>
                  <th>Patient</th>
                  <th>Surgery</th>
                  <th>Surgery Date</th>
                  <th>Phone Number</th>
                  <th>Age</th>
                  <th>Active</th>
                  <th>Status</th>
                  <th>Alerts</th>
                  <th>Profile</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!paginatedPatients.length">
                  <td colspan="10" class="nurse-dashboard-empty">No assigned patients found.</td>
                </tr>

                <tr v-for="patient in paginatedPatients" :key="patient.patientId" :class="['nurse-dashboard-row', selectedPatient?.patientId === patient.patientId ? 'nurse-dashboard-row-selected' : '']" @click="selectPatient(patient)">
                  <td>
                    <div class="nurse-dashboard-photo">
                      <img v-if="patient.photo" :src="patient.photo" alt="Patient photo" />
                      <span v-else>No photo</span>
                    </div>
                  </td>

                  <td>{{ patient.name }} {{ patient.surname }}</td>
                  <td>{{ patient.surgery || '-' }}</td>
                  <td>{{ patient.surgeryDate || '-' }}</td>
                  <td>{{ patient.phone || '-' }}</td>
                  <td>{{ patient.age ?? '-' }}</td>

                  <td class="nurse-dashboard-col-center">
                    <span :class="['app-badge', patient.active ? 'app-badge-active' : 'app-badge-inactive']">{{ patient.active ? 'Active' : 'Inactive' }}</span>
                  </td>

                  <td class="nurse-dashboard-col-center">
                    <span :class="['app-badge', patient.statusLabel === 'Stable' ? 'app-badge-stable' : patient.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">{{ patient.statusLabel }}</span>
                  </td>

                  <td class="nurse-dashboard-col-center">{{ patient.alertCount }}</td>

                  <td class="nurse-dashboard-col-center">
                    <button type="button" class="app-action-button" aria-label="View patient profile" title="View patient profile" @click.stop="openPatientProfile(patient)">
                      <img src="/icons/view.png" alt="View profile" class="app-action-icon" />
                    </button>
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

      <section class="nurse-dashboard-tablecard">
        <header class="nurse-dashboard-reporthead">
          <h2 class="nurse-dashboard-reporttitle">
            Symptom Reports
            <span v-if="selectedPatient" class="nurse-dashboard-reportpatient">{{ selectedPatient.name }} {{ selectedPatient.surname }}</span>
          </h2>
        </header>

        <section class="nurse-dashboard-tablebody">
          <div v-if="!selectedPatient" class="nurse-dashboard-empty nurse-dashboard-empty-panel">Select a patient to view symptom reports.</div>

          <div v-else-if="reportsLoading" class="nurse-dashboard-empty nurse-dashboard-empty-panel">Loading reports...</div>

          <p v-else-if="reportsErrorMessage" class="nurse-dashboard-message nurse-dashboard-message-error">{{ reportsErrorMessage }}</p>

          <div v-else class="nurse-dashboard-tablewrap">
            <table class="nurse-dashboard-table nurse-dashboard-reports-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Patient Status</th>
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
                  <td colspan="8" class="nurse-dashboard-empty">No symptom reports found.</td>
                </tr>

                <tr v-for="report in paginatedReports" :key="report.reportId">
                  <td>{{ report.reportDate || '-' }}</td>

                  <td class="nurse-dashboard-col-center">
                    <span :class="['app-badge', report.statusLabel === 'Stable' ? 'app-badge-stable' : report.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">{{ report.statusLabel }}</span>
                  </td>

                  <td>{{ report.painLevel ?? '-' }}</td>
                  <td>{{ report.hasFever ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasBleeding ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasSwelling ? 'Yes' : 'No' }}</td>
                  <td class="nurse-dashboard-col-center">{{ report.alertCount }}</td>

                  <td class="nurse-dashboard-col-center">
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

    <ReportModal :visible="isReportModalOpen" mode="nurse-review" :report="selectedReport || {}" :pain-levels="painLevels" :loading="reportDetailLoading" :submit-loading="reportSaveLoading" :error-message="reportDetailErrorMessage || reportSaveErrorMessage" @close="closeViewReportModal" @submit="saveNurseObservations" @update-nurse-observations="selectedReport.nurseObservations = $event" />

    <AppFeedbackModal :visible="!!(successMessage || reportSaveErrorMessage)" :message="reportSaveErrorMessage || successMessage" :type="reportSaveErrorMessage ? 'error' : 'success'" @close="clearNurseDashboardFeedback" />
  </section>
</template>

<style src="../styles/nurse-dashboard.css"></style>