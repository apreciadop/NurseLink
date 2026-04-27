<script src="../scripts/patientDashboardView.js"></script>

<template>
  <section class="patient-dashboard">
    <p v-if="loading" class="patient-dashboard-message">Loading patient dashboard...</p>

    <p v-else-if="errorMessage" class="patient-dashboard-message patient-dashboard-message-error">{{ errorMessage }}</p>

    <section v-else class="patient-dashboard-main">
      <section class="patient-dashboard-profilecard">
        <div class="patient-dashboard-profileleft">
          <div class="patient-dashboard-photo">
            <img v-if="patient.photo" :src="patient.photo" alt="Patient photo"/>
            <span v-else>No photo</span>
          </div>

          <span :class="['patient-dashboard-statusbadge', patient.statusLabel === 'Stable' ? 'patient-dashboard-statusbadge-stable' : patient.statusLabel === 'Warning'
            ? 'patient-dashboard-statusbadge-warning'
            : 'patient-dashboard-statusbadge-alert']">{{ patient.statusLabel }}</span>
        </div>

        <div class="patient-dashboard-profileinfo">
          <div class="patient-dashboard-titleblock">
            <h2>{{ patient.name }} {{ patient.surname }}</h2>
            <p>Patient ID #{{ patient.patientId }}</p>
          </div>

          <div class="patient-dashboard-twocolumns">
            <section class="patient-dashboard-datacolumn">
              <h3>Personal Information</h3>

              <div class="patient-dashboard-infoitem">
                <span>Name</span>
                <strong>{{ patient.name || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Surname</span>
                <strong>{{ patient.surname || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Birthdate</span>
                <strong>{{ patient.birthdate || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Email</span>
                <strong>{{ patient.email || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Phone</span>
                <strong>{{ patient.phone || '-' }}</strong>
              </div>
            </section>

            <section class="patient-dashboard-datacolumn">
              <h3>Operation Information</h3>

              <div class="patient-dashboard-infoitem">
                <span>Surgery</span>
                <strong>{{ patient.surgery || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Surgery Date</span>
                <strong>{{ patient.surgeryDate || '-' }}</strong>
              </div>

              <div class="patient-dashboard-infoitem">
                <span>Status</span>
                <strong>{{ patient.statusLabel || '-' }}</strong>
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
            <img v-if="patient.assignedNursePhoto" :src="patient.assignedNursePhoto" alt="Assigned nurse photo"/>
            <span v-else>No photo</span>
          </div>

          <div class="patient-dashboard-nurseinfo">
            <span class="patient-dashboard-nurselabel">Assigned Nurse</span>

            <strong class="patient-dashboard-nursename">{{ patient.assignedNurseName || 'Not assigned' }}</strong>
          </div>
        </div>
      </section>

      <section class="patient-dashboard-tablecard">
        <header class="patient-dashboard-reporthead">
          <div class="patient-dashboard-reporttitle-row">
            <h2>Symptom Reports</h2>
            <button type="button" class="patient-dashboard-newreport-button" @click="openCreateReportModal">+ Add Report</button>
          </div>

          <section class="patient-dashboard-reportfilters">
            <div class="patient-dashboard-filterfield">
              <label for="reportDateFilter">Date</label>
              <input id="reportDateFilter" v-model="reportDateFilter" type="date" class="patient-dashboard-filterinput"/>
            </div>

            <div class="patient-dashboard-filterfield">
              <label for="reportStatusFilter">Status</label>
              <select id="reportStatusFilter" v-model="reportStatusFilter" class="patient-dashboard-filterinput">
                <option value="all">All</option>
                <option value="stable">Stable</option>
                <option value="warning">Warning</option>
                <option value="alert">Alert</option>
              </select>
            </div>
          </section>
        </header>

        <section class="patient-dashboard-tablebody">
          <div v-if="reportsLoading" class="patient-dashboard-empty patient-dashboard-empty-panel">Loading reports...</div>
          <p v-else-if="reportsErrorMessage" class="patient-dashboard-message patient-dashboard-message-error">{{ reportsErrorMessage }}</p>

          <div v-else class="patient-dashboard-tablewrap">
            <table class="patient-dashboard-table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Patient Status</th>
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
                  <td colspan="9" class="patient-dashboard-empty">No symptom reports found.</td>
                </tr>

                <tr v-for="report in paginatedReports" :key="report.reportId">
                  <td>{{ report.reportDate || '-' }}</td>

                  <td class="patient-dashboard-col-center">
                    <span :class="['patient-dashboard-reportstatusbadge', report.statusLabel === 'Stable' ? 'patient-dashboard-statusbadge-stable' : report.statusLabel === 'Warning'
                      ? 'patient-dashboard-statusbadge-warning'
                      : 'patient-dashboard-statusbadge-alert']">{{ report.statusLabel }}</span>
                  </td>

                  <td>{{ report.painLevel ?? '-' }}</td>
                  <td>{{ report.hasFever ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasBleeding ? 'Yes' : 'No' }}</td>
                  <td>{{ report.hasSwelling ? 'Yes' : 'No' }}</td>

                  <td class="patient-dashboard-col-center">
                    <span :class="['patient-dashboard-alertbadge', report.alertCount === 0 ? 'patient-dashboard-alertbadge-stable' : report.alertCount <= 2
                      ? 'patient-dashboard-alertbadge-warning'
                      : 'patient-dashboard-alertbadge-alert']">{{ report.alertCount }}</span>
                  </td>

                  <td class="patient-dashboard-col-center">
                    <span v-if="report.hasNurseObservations" class="patient-dashboard-notebadge">Added</span>
                  </td>

                  <td class="patient-dashboard-col-center">
                    <button type="button" class="patient-dashboard-viewicon-button" @click.stop="openViewReportModal(report)" aria-label="View report" title="View report">
                      <img src="/icons/view.png" alt="View report" class="patient-dashboard-viewicon"/>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <footer class="patient-dashboard-pagination">
          <button type="button" class="patient-dashboard-pagination-button" :disabled="reportsCurrentPage === 1" @click="goToPreviousReportsPage">&lt;</button>
          <span class="patient-dashboard-pagination-text">Page {{ reportsCurrentPage }} of {{ reportsTotalPages }}</span>
          <button type="button" class="patient-dashboard-pagination-button" :disabled="reportsCurrentPage === reportsTotalPages" @click="goToNextReportsPage">&gt;</button>
        </footer>
      </section>
    </section>

    <ReportModal :visible="isCreateReportModalOpen" mode="create" :report="createModalReport" :pain-levels="painLevels" :submit-loading="createReportLoading" :error-message="createReportErrorMessage"
      @close="closeCreateReportModal"
      @submit="submitCreateReport"
      @set-pain-level="reportForm.painLevel = $event"
      @set-has-fever="reportForm.hasFever = $event"
      @set-has-bleeding="reportForm.hasBleeding = $event"
      @set-has-swelling="reportForm.hasSwelling = $event"
      @update-observations="reportForm.observations = $event"
    />

    <ReportModal :visible="isViewReportModalOpen" mode="view" :report="selectedReport || {}" :pain-levels="painLevels" :loading="reportDetailLoading" :error-message="reportDetailErrorMessage" @close="closeViewReportModal"/>
  </section>
</template>

<style src="../styles/patient-dashboard.css"></style>