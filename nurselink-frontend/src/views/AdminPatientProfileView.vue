<script setup>
import { onMounted } from 'vue'
import AppFeedbackModal from '../components/AppFeedbackModal.vue'
import ReportModal from '../views/ReportModal.vue'
import { useAdminPatientProfile } from '../composables/useAdminPatientProfile'

const {
  loading,
  errorMessage,
  saveLoading,
  saveMessage,
  saveErrorMessage,
  patientForm,
  surgeryTypes,
  reports,
  reportsLoading,
  reportsErrorMessage,
  reportsCurrentPage,
  reportsTotalPages,
  hasReports,
  isReportModalOpen,
  reportDetailLoading,
  reportDetailErrorMessage,
  selectedReport,
  painLevels,
  loadProfileData,
  handlePhotoChange,
  submitUpdatePatient,
  openViewReportModal,
  closeViewReportModal,
  clearSaveFeedback,
  goToPreviousReportsPage,
  goToNextReportsPage
} = useAdminPatientProfile()

onMounted(() => {
  loadProfileData()
})
</script>

<template>
  <section class="admin-patient-profile">
    <div class="patient-profile-top">
      <router-link to="/admin/patients" class="patient-profile-back"><- Back to Patients</router-link>
    </div>

    <p v-if="loading" class="patient-profile-message">Loading patient profile...</p>

    <p v-else-if="errorMessage" class="patient-profile-message patient-profile-message-error">{{ errorMessage }}</p>

    <section v-else class="patient-profile-main">
      <section class="patient-profile-toprow">
        <div class="patient-profile-left">
          <div class="patient-profile-summary patient-profile-summary-top">
            <h2 class="patient-profile-name">{{ patientForm.name }} {{ patientForm.surname }}</h2>
            <p class="patient-profile-id">Patient ID: {{ patientForm.patientId }}</p>
          </div>

          <div class="patient-profile-photo-column">
            <div class="patient-profile-photo">
              <img v-if="patientForm.photo" :src="patientForm.photo" alt="Patient photo" />
              <span v-else>No photo</span>
            </div>

            <label for="patientPhotoInput" class="patient-profile-photo-button">
              <img src="/icons/cameraBlue.png" alt="Change Photo" class="patient-profile-photo-button-icon" />
              <span>Change Photo</span>
            </label>

            <input id="patientPhotoInput" type="file" accept="image/*" class="patient-profile-fileinput" @change="handlePhotoChange" />

            <span :class="['app-badge', patientForm.statusLabel === 'Stable' ? 'app-badge-stable' : patientForm.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">
              {{ patientForm.statusLabel }}
            </span>
          </div>
        </div>

        <div class="patient-profile-right">
          <h3 class="patient-profile-section-title">Patient Details</h3>

          <div class="patient-profile-formgrid patient-profile-formgrid-threecols">
            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientName">Name</label>
              <input id="patientName" v-model="patientForm.name" type="text" class="patient-profile-input" />
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientSurname">Surname</label>
              <input id="patientSurname" v-model="patientForm.surname" type="text" class="patient-profile-input" />
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientBirthdate">Birthdate</label>
              <input id="patientBirthdate" v-model="patientForm.birthdate" type="date" class="patient-profile-input" />
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientEmail">Email</label>
              <input id="patientEmail" v-model="patientForm.email" type="email" class="patient-profile-input" />
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientPassword">Password</label>
              <input id="patientPassword" v-model="patientForm.password" type="password" class="patient-profile-input" placeholder="Leave empty to keep current password" />
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientPhone">Phone</label>
              <input id="patientPhone" v-model="patientForm.phone" type="text" class="patient-profile-input" />
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="assignedNurse">Assigned Nurse</label>
              <input id="assignedNurse" :value="patientForm.assignedNurseName || 'No nurse assigned'" type="text" class="patient-profile-input" readonly />
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientSurgery">Surgery</label>
              <select id="patientSurgery" v-model="patientForm.surgeryTypeId" class="patient-profile-input patient-profile-select">
                <option :value="0" disabled>Select surgery</option>
                <option v-for="surgeryType in surgeryTypes" :key="surgeryType.surgeryTypeId" :value="surgeryType.surgeryTypeId">{{ surgeryType.name }}</option>
              </select>
            </div>

            <div class="patient-profile-field">
              <label class="patient-profile-label" for="patientSurgeryDate">Surgery Date</label>
              <input id="patientSurgeryDate" v-model="patientForm.surgeryDate" type="date" class="patient-profile-input" />
            </div>

            <div class="patient-profile-statusrow">
              <span class="patient-profile-label">Status</span>

              <div class="patient-profile-statusgroup">
                <label class="patient-profile-statusoption">
                  <input v-model="patientForm.active" type="radio" :value="true" />
                  <span>Active</span>
                </label>

                <label class="patient-profile-statusoption">
                  <input v-model="patientForm.active" type="radio" :value="false" />
                  <span>Inactive</span>
                </label>
              </div>

              <button type="button" class="app-button app-button-primary" :disabled="saveLoading" @click="submitUpdatePatient">{{ saveLoading ? 'Saving...' : 'Save' }}</button>
            </div>
          </div>
        </div>
      </section>

      <section class="patient-profile-observations">
        <h3 class="patient-profile-section-title">Observations</h3>
        <textarea v-model="patientForm.patientObservations" class="patient-profile-textarea"></textarea>
      </section>

      <section class="patient-profile-reports">
        <header class="patient-profile-reportshead">
          <h3 class="patient-profile-reportstitle">Symptom Reports</h3>
        </header>

        <div class="patient-profile-reportsbody">
          <div v-if="reportsLoading" class="patient-profile-empty patient-profile-empty-panel">Loading reports...</div>

          <p v-else-if="reportsErrorMessage" class="patient-profile-message patient-profile-message-error">{{ reportsErrorMessage }}</p>

          <div v-else class="patient-profile-tablewrap">
            <table class="patient-profile-table">
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
                <tr v-if="!hasReports">
                  <td colspan="8" class="patient-profile-empty">No symptom reports found.</td>
                </tr>

                <tr v-for="report in reports" :key="report.reportId">
                  <td>{{ report.reportDate || '-' }}</td>

                  <td class="patient-profile-col-center">
                    <span :class="['app-badge', report.statusLabel === 'Stable' ? 'app-badge-stable' : report.statusLabel === 'Warning' ? 'app-badge-warning' : 'app-badge-alert']">{{ report.statusLabel }}</span>
                  </td>

                  <td class="patient-profile-col-center">
                    <span :class="['patient-profile-pain', report.painLevel >= 7 ? 'patient-profile-pain-alert' : 'patient-profile-pain-ok']">{{ report.painLevel ?? '-' }}</span>
                  </td>

                  <td class="patient-profile-col-center">
                    <span :class="['patient-profile-kpi-dot', report.hasFever ? 'patient-profile-kpi-dot-yes' : 'patient-profile-kpi-dot-no']" :title="report.hasFever ? 'Yes' : 'No'"></span>
                  </td>

                  <td class="patient-profile-col-center">
                    <span :class="['patient-profile-kpi-dot', report.hasBleeding ? 'patient-profile-kpi-dot-yes' : 'patient-profile-kpi-dot-no']" :title="report.hasBleeding ? 'Yes' : 'No'"></span>
                  </td>

                  <td class="patient-profile-col-center">
                    <span :class="['patient-profile-kpi-dot', report.hasSwelling ? 'patient-profile-kpi-dot-yes' : 'patient-profile-kpi-dot-no']" :title="report.hasSwelling ? 'Yes' : 'No'"></span>
                  </td>

                  <td class="patient-profile-col-center">
                    <span :class="['app-badge', 'app-badge-small', report.alertCount === 0 ? 'app-badge-stable' : report.alertCount <= 2 ? 'app-badge-warning' : 'app-badge-alert']">{{ report.alertCount }}</span>
                  </td>

                  <td class="patient-profile-col-center">
                    <button type="button" class="app-action-button" @click.stop="openViewReportModal(report)" aria-label="View report" title="View report">
                      <img src="/icons/view.png" alt="View report" class="app-action-icon" />
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <footer v-if="hasReports" class="app-pagination patient-profile-pagination">
            <button type="button" class="app-pagination-button" :disabled="reportsCurrentPage === 1 || reportsLoading" @click="goToPreviousReportsPage">&lt;</button>
            <span class="app-pagination-text">Page {{ reportsCurrentPage }} of {{ reportsTotalPages }}</span>
            <button type="button" class="app-pagination-button" :disabled="reportsCurrentPage === reportsTotalPages || reportsLoading" @click="goToNextReportsPage">&gt;</button>
          </footer>
        </div>
      </section>
    </section>

    <ReportModal :visible="isReportModalOpen" mode="view" :report="selectedReport || {}" :pain-levels="painLevels" :loading="reportDetailLoading" :error-message="reportDetailErrorMessage" @close="closeViewReportModal" />

    <AppFeedbackModal :visible="!!(saveMessage || saveErrorMessage)" :message="saveErrorMessage || saveMessage" :type="saveErrorMessage ? 'error' : 'success'" @close="clearSaveFeedback" />
  </section>
</template>

<style src="../styles/admin-patient-profile.css"></style>