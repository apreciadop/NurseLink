<script src="../scripts/adminDashboardView.js"></script>

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

          <section class="dashboard-panelbody">
            <template v-if="patientsWithAlerts.length">
              <div class="dashboard-table-wrapper">
                <table class="dashboard-table dashboard-table-alerts">
                  <thead>
                    <tr>
                      <th title="Patient">Patient</th>
                      <th title="Nurse">Nurse</th>
                      <th title="Date">Date</th>
                      <th title="Status">Status</th>
                      <th title="Pain">Pain</th>
                      <th title="Fever">Fever</th>
                      <th title="Bleeding">Bleeding</th>
                      <th title="Swelling">Swelling</th>
                    </tr>
                  </thead>

                  <tbody>
                    <tr v-for="patient in patientsWithAlerts" :key="patient.patientId">
                      <td :title="`${patient.patientName} ${patient.patientSurname}`">{{ patient.patientName }} {{ patient.patientSurname }}</td>
                      <td :title="`${patient.nurseName} ${patient.nurseSurname}`">{{ patient.nurseName }} {{ patient.nurseSurname }}</td>
                      <td :title="patient.reportDate || '-'">{{ patient.reportDate || '-' }}</td>
                      <td :title="patient.reportStatus || '-'">{{ patient.reportStatus || '-' }}</td>
                      <td :title="String(patient.reportPain ?? '-')">{{ patient.reportPain ?? '-' }}</td>
                      <td :title="patient.reportFever ? 'Yes' : 'No'">{{ patient.reportFever ? 'Yes' : 'No' }}</td>
                      <td :title="patient.reportBleeding ? 'Yes' : 'No'">{{ patient.reportBleeding ? 'Yes' : 'No' }}</td>
                      <td :title="patient.reportSwelling ? 'Yes' : 'No'">{{ patient.reportSwelling ? 'Yes' : 'No' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </template>

            <p v-else>No patients with alerts.</p>
          </section>
        </article>

        <article class="dashboard-panel">
          <header class="dashboard-panelheader">
            <h2>Unassigned Patients</h2>
          </header>

          <section class="dashboard-panelbody">
            <template v-if="unassignedPatients.length">
              <div class="dashboard-table-wrapper">
                <table class="dashboard-table dashboard-table-unassigned">
                  <thead>
                    <tr>
                      <th title="Patient">Patient</th>
                      <th title="Surgery">Surgery</th>
                      <th title="Date">Date</th>
                      <th title="Action">Action</th>
                    </tr>
                  </thead>

                  <tbody>
                    <tr v-for="patient in unassignedPatients" :key="patient.patientId">
                      <td :title="`${patient.patientName} ${patient.patientSurname}`">{{ patient.patientName }} {{ patient.patientSurname }}</td>
                      <td :title="patient.surgeryTypeName || '-'">{{ patient.surgeryTypeName || '-' }}</td>
                      <td :title="patient.surgeryDate || '-'">{{ patient.surgeryDate || '-' }}</td>
                      <td>
                        <button type="button" class="dashboard-action-button dashboard-action-button--icon" :aria-label="`Assign patient ${patient.patientName} ${patient.patientSurname}`"
                          title="Assign patient" @click="openAssignModal(patient)">
                          <img src="/icons/assignmentBlack.png" alt="Assign patient to a Nurse" class="dashboard-action-icon-image"/>
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
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

          <button type="button" class="dashboard-modalclose" aria-label="Close assignment modal" @click="closeAssignModal">x</button>
        </header>

        <section v-if="selectedPatient" class="dashboard-modalbody">
          <div class="dashboard-modalpatient-info">
            <p><strong>Patient:</strong>{{ selectedPatient.patientName }} {{ selectedPatient.patientSurname }}</p>

            <p><strong>Surgery:</strong>{{ selectedPatient.surgeryTypeName || '-' }}</p>

            <p><strong>Date:</strong>{{ selectedPatient.surgeryDate || '-' }}</p>
          </div>

          <div class="dashboard-modalfield">
            <label for="nurseSelect" class="dashboard-modallabel">Select Nurse</label>

            <select id="nurseSelect" v-model="selectedNurseId" class="dashboard-modalselect">
              <option value="">Select a nurse</option>

              <option v-for="nurse in nurses" :key="nurse.nurseId" :value="nurse.nurseId">{{ nurse.name }} {{ nurse.surname }}</option>
            </select>
          </div>

          <p v-if="assignErrorMessage" class="dashboard-modalerror">{{ assignErrorMessage }}</p>
        </section>

        <footer class="dashboard-modalfooter">
          <button type="button" class="dashboard-modalbutton dashboard-modalbutton--secondary" @click="closeAssignModal">Cancel</button>

          <button type="button" class="dashboard-modalbutton dashboard-modalbutton--primary" :disabled="assignLoading" @click="submitAssignment">{{ assignLoading ? 'Assigning...' : 'Assign' }}</button>
        </footer>
      </div>
    </div>
  </section>
</template>

<style src="../styles/admin-dashboard.css"></style>