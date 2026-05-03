<script setup>
import { onMounted } from 'vue'
import AppFeedbackModal from '../components/AppFeedbackModal.vue'
import { useAdminDashboard } from '../composables/useAdminDashboard'

const {
  totalPatients,
  totalNurses,
  totalAlerts,
  totalUnassigned,
  patientsWithAlerts,
  unassignedPatients,
  paginatedPatientsWithAlerts,
  paginatedUnassignedPatients,
  alertsCurrentPage,
  unassignedCurrentPage,
  patientsWithAlertsTotalPages,
  unassignedPatientsTotalPages,
  nurses,
  loading,
  errorMessage,
  successMessage,
  isAssignModalOpen,
  selectedPatient,
  selectedNurseId,
  assignErrorMessage,
  assignLoading,
  loadDashboardData,
  goToPreviousAlertsPage,
  goToNextAlertsPage,
  goToPreviousUnassignedPage,
  goToNextUnassignedPage,
  openAssignModal,
  closeAssignModal,
  submitAssignment,
  clearDashboardFeedback
} = useAdminDashboard()

onMounted(() => {
  loadDashboardData()
})
</script>

<template>
  <section class="admin-dashboard">
    <p v-if="loading" class="admin-dashboardmessage">Loading dashboard data...</p>

    <p v-else-if="errorMessage" class="admin-dashboardmessage admin-dashboardmessage--error">{{ errorMessage }}</p>

    <template v-else>
      <section class="admin-dashboardstats">
        <article class="stat-card stat-card--patients">
          <p class="stat-cardlabel">Total Patients</p>
          <p class="stat-cardvalue">{{ totalPatients }}</p>
        </article>

        <article class="stat-card stat-card--nurses">
          <p class="stat-cardlabel">Total Nurses</p>
          <p class="stat-cardvalue">{{ totalNurses }}</p>
        </article>

        <article class="stat-card stat-card--alerts">
          <p class="stat-cardlabel">Alerts</p>
          <p class="stat-cardvalue">{{ totalAlerts }}</p>
        </article>

        <article class="stat-card stat-card--unassigned">
          <p class="stat-cardlabel">Unassigned</p>
          <p class="stat-cardvalue">{{ totalUnassigned }}</p>
        </article>
      </section>

      <section class="admin-dashboardtables">
        <article class="dashboard-panel">
          <header class="dashboard-panelheader">
            <h2>Patients with Alerts</h2>
          </header>

          <section class="dashboard-panelbody dashboard-panelbody-large">
            <template v-if="patientsWithAlerts.length">
              <div class="dashboard-table-wrapper">
                <table class="dashboard-table dashboard-table-alerts">
                  <thead>
                    <tr>
                      <th>Patient</th>
                      <th>Nurse</th>
                      <th>Date</th>
                      <th>Status</th>
                      <th>Pain</th>
                      <th>Fever</th>
                      <th>Bleeding</th>
                      <th>Swelling</th>
                    </tr>
                  </thead>

                  <tbody>
                    <tr v-for="patient in paginatedPatientsWithAlerts" :key="patient.patientId">
                      <td class="dashboard-name-cell" :title="`${patient.patientName} ${patient.patientSurname}`">{{ patient.patientName }} {{ patient.patientSurname }}</td>
                      <td class="dashboard-name-cell" :title="`${patient.nurseName} ${patient.nurseSurname}`">{{ patient.nurseName }} {{ patient.nurseSurname }}</td>
                      <td class="dashboard-date-cell">{{ patient.reportDate || '-' }}</td>

                      <td class="dashboard-center-cell">
                        <span v-if="patient.reportStatus" :class="['app-badge', 'dashboard-status-badge', patient.reportStatus === 'Stable' ? 'app-badge-stable' : patient.reportStatus === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">{{ patient.reportStatus }}</span>
                        <span v-else>-</span>
                      </td>

                      <td class="dashboard-center-cell">{{ patient.reportPain ?? '-' }}</td>
                      <td class="dashboard-center-cell">{{ patient.reportFever ? 'Yes' : 'No' }}</td>
                      <td class="dashboard-center-cell">{{ patient.reportBleeding ? 'Yes' : 'No' }}</td>
                      <td class="dashboard-center-cell">{{ patient.reportSwelling ? 'Yes' : 'No' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <footer class="app-pagination dashboard-pagination">
                <button type="button" class="app-pagination-button" :disabled="alertsCurrentPage === 1" @click="goToPreviousAlertsPage">&lt;</button>
                <span class="app-pagination-text">Page {{ alertsCurrentPage }} of {{ patientsWithAlertsTotalPages }}</span>
                <button type="button" class="app-pagination-button" :disabled="alertsCurrentPage === patientsWithAlertsTotalPages" @click="goToNextAlertsPage">&gt;</button>
              </footer>
            </template>

            <p v-else>No patients with alerts.</p>
          </section>
        </article>

        <article class="dashboard-panel">
          <header class="dashboard-panelheader">
            <h2>Unassigned Patients</h2>
          </header>

          <section class="dashboard-panelbody dashboard-panelbody-large">
            <template v-if="unassignedPatients.length">
              <div class="dashboard-table-wrapper">
                <table class="dashboard-table dashboard-table-unassigned">
                  <thead>
                    <tr>
                      <th>Patient</th>
                      <th>Surgery</th>
                      <th>Date</th>
                      <th>Action</th>
                    </tr>
                  </thead>

                  <tbody>
                    <tr v-for="patient in paginatedUnassignedPatients" :key="patient.patientId">
                      <td class="dashboard-name-cell" :title="`${patient.patientName} ${patient.patientSurname}`">{{ patient.patientName }} {{ patient.patientSurname }}</td>
                      <td :title="patient.surgeryTypeName || '-'">{{ patient.surgeryTypeName || '-' }}</td>
                      <td>{{ patient.surgeryDate || '-' }}</td>

                      <td class="dashboard-center-cell">
                        <button type="button" class="app-action-button" :aria-label="`Assign patient ${patient.patientName} ${patient.patientSurname}`" title="Assign patient" @click="openAssignModal(patient)">
                          <img src="/icons/assignmentBlack.png" alt="Assign patient to a Nurse" class="app-action-icon" />
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <footer class="app-pagination dashboard-pagination">
                <button type="button" class="app-pagination-button" :disabled="unassignedCurrentPage === 1" @click="goToPreviousUnassignedPage">&lt;</button>
                <span class="app-pagination-text">Page {{ unassignedCurrentPage }} of {{ unassignedPatientsTotalPages }}</span>
                <button type="button" class="app-pagination-button" :disabled="unassignedCurrentPage === unassignedPatientsTotalPages" @click="goToNextUnassignedPage">&gt;</button>
              </footer>
            </template>

            <p v-else>No unassigned patients.</p>
          </section>
        </article>
      </section>
    </template>

    <div v-if="isAssignModalOpen" class="dashboard-modal-overlay" @click="closeAssignModal">
      <div class="dashboard-modal" @click.stop>
        <header class="dashboard-modalheader">
          <h2>Assign Patient</h2>
          <button type="button" class="dashboard-modalclose" aria-label="Close assignment modal" @click="closeAssignModal">×</button>
        </header>

        <section v-if="selectedPatient" class="dashboard-modalbody">
          <div class="dashboard-modalpatient-info">
            <p><strong>Patient:</strong> {{ selectedPatient.patientName }} {{ selectedPatient.patientSurname }}</p>
            <p><strong>Surgery:</strong> {{ selectedPatient.surgeryTypeName || '-' }}</p>
            <p><strong>Date:</strong> {{ selectedPatient.surgeryDate || '-' }}</p>
          </div>

          <div class="dashboard-modalfield">
            <label for="nurseSelect" class="dashboard-modallabel">Select Nurse</label>

            <select id="nurseSelect" v-model="selectedNurseId" class="dashboard-modalselect">
              <option value="">Select a nurse</option>
              <option v-for="nurse in nurses" :key="nurse.nurseId" :value="nurse.nurseId">{{ nurse.name }} {{ nurse.surname }}</option>
            </select>
          </div>
        </section>

        <footer class="dashboard-modalfooter">
          <button type="button" class="app-button app-button-secondary" @click="closeAssignModal">Cancel</button>
          <button type="button" class="app-button app-button-primary" :disabled="assignLoading" @click="submitAssignment">{{ assignLoading ? 'Assigning...' : 'Assign' }}</button>
        </footer>
      </div>
    </div>

    <AppFeedbackModal :visible="!!(successMessage || assignErrorMessage)" :message="assignErrorMessage || successMessage" :type="assignErrorMessage ? 'error' : 'success'" @close="clearDashboardFeedback" />
  </section>
</template>

<style src="../styles/admin-dashboard.css"></style>