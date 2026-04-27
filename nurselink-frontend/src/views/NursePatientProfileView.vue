<script src="../scripts/nursePatientProfileView.js"></script>

<template>
  <section class="nurse-patient-profile">
    <p v-if="loading" class="nurse-patient-profile-message">Loading patient profile...</p>

    <p v-else-if="errorMessage" class="nurse-patient-profile-message nurse-patient-profile-message-error">{{ errorMessage }}</p>

    <section v-else-if="patient" class="nurse-patient-profile-main">
      <button type="button" class="nurse-patient-profile-backlink" @click="goBack"><- Back to Patients</button>

      <section class="nurse-patient-profile-card">
        <div class="nurse-patient-profile-top">
          <div class="nurse-patient-profile-side">
            <div class="nurse-patient-profile-photo">
              <img v-if="patient.photo" :src="patient.photo" alt="Patient photo"/>
              <span v-else>No photo</span>
            </div>

            <span :class="['nurse-patient-profile-statusbadge', patient.statusLabel === 'Stable' ? 'nurse-patient-profile-statusbadge-stable' : patient.statusLabel === 'Warning'
              ? 'nurse-patient-profile-statusbadge-warning'
              : 'nurse-patient-profile-statusbadge-alert']">{{ patient.statusLabel }}</span>
          </div>

          <div class="nurse-patient-profile-content">
            <div class="nurse-patient-profile-header">
              <div class="nurse-patient-profile-header-left">
                <h2 class="nurse-patient-profile-name">{{ patient.name }} {{ patient.surname }}</h2>
                <p class="nurse-patient-profile-id">Patient ID #{{ patient.patientId }}</p>
              </div>

              <div v-if="patient.assignedNurseName || patient.assignedNursePhoto" class="nurse-patient-profile-assigned">
                <div class="nurse-patient-profile-assigned-photo">
                  <img v-if="patient.assignedNursePhoto" :src="patient.assignedNursePhoto" alt="Assigned nurse photo"/>
                  <span v-else>N</span>
                </div>

                <div class="nurse-patient-profile-assigned-info">
                  <span class="nurse-patient-profile-assigned-label">Assigned Nurse</span>
                  <span class="nurse-patient-profile-assigned-name">{{ patient.assignedNurseName || '-' }}</span>
                </div>
              </div>
            </div>

            <div class="nurse-patient-profile-panels">
              <section class="nurse-patient-profile-panel">
                <h3 class="nurse-patient-profile-panel-title">Personal Information</h3>

                <div class="nurse-patient-profile-info-grid">
                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Name</span>
                    <span class="nurse-patient-profile-value">{{ patient.name || '-' }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Surname</span>
                    <span class="nurse-patient-profile-value">{{ patient.surname || '-' }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Birthdate</span>
                    <span class="nurse-patient-profile-value">{{ patient.birthdate || '-' }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Email</span>
                    <span class="nurse-patient-profile-value">{{ patient.email || '-' }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Phone</span>
                    <span class="nurse-patient-profile-value">{{ patient.phone || '-' }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Active</span>
                    <span class="nurse-patient-profile-value">{{ patient.active ? 'Active' : 'Inactive' }}</span>
                  </div>
                </div>
              </section>

              <section class="nurse-patient-profile-panel">
                <h3 class="nurse-patient-profile-panel-title">Operation Information</h3>

                <div class="nurse-patient-profile-info-grid">
                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Surgery</span>
                    <span class="nurse-patient-profile-value">{{ patient.surgeryName || '-' }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Surgery Date</span>
                    <span class="nurse-patient-profile-value">{{ patient.surgeryDate || '-' }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Status</span>
                    <span class="nurse-patient-profile-value">{{ patient.statusLabel }}</span>
                  </div>

                  <div class="nurse-patient-profile-info-row">
                    <span class="nurse-patient-profile-label">Alerts</span>
                    <span class="nurse-patient-profile-value">{{ patient.alertCount }}</span>
                  </div>
                </div>
              </section>
            </div>
          </div>
        </div>
      </section>

      <section v-if="patient.patientObservations" class="nurse-patient-profile-card">
        <header class="nurse-patient-profile-section-header">
          <h3 class="nurse-patient-profile-section-title">Observations</h3>
        </header>

        <div class="nurse-patient-profile-observations">{{ patient.patientObservations }}</div>
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
                  <th>Patient Status</th>
                  <th>Pain Level</th>
                  <th>Fever</th>
                  <th>Bleeding</th>
                  <th>Swelling</th>
                  <th>Alerts</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!paginatedReports.length">
                  <td colspan="7" class="nurse-patient-profile-empty">No symptom reports found.</td>
                </tr>

                <tr v-for="report in paginatedReports" :key="report.reportId">
                  <td>{{ report.reportDate || '-' }}</td>

                  <td class="nurse-patient-profile-col-center">
                    <span :class="['nurse-patient-profile-table-status', report.statusLabel === 'Stable' ? 'nurse-patient-profile-table-status-stable' : report.statusLabel === 'Warning'
                      ? 'nurse-patient-profile-table-status-warning'
                      : 'nurse-patient-profile-table-status-alert']">{{ report.statusLabel }}</span>
                  </td>

                  <td>{{ report.painLevel ?? '-' }}</td>
                  <td>{{ report.hasFever ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasBleeding ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasSwelling ? 'Yes' : 'No' }}</td>
                  <td class="nurse-patient-profile-col-center">{{ report.alertCount }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <footer class="nurse-patient-profile-pagination">
          <button type="button" class="nurse-patient-profile-pagination-button" :disabled="reportsCurrentPage === 1" @click="goToPreviousReportsPage">&lt;</button>
          <span class="nurse-patient-profile-pagination-text">Page {{ reportsCurrentPage }} of {{ reportsTotalPages }}</span>
          <button type="button" class="nurse-patient-profile-pagination-button" :disabled="reportsCurrentPage === reportsTotalPages" @click="goToNextReportsPage">&gt;</button>
        </footer>
      </section>
    </section>
  </section>
</template>

<style src="../styles/nurse-patient-profile.css"></style>