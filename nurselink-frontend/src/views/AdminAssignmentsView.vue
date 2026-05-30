<script setup>
import { onMounted, watch } from 'vue'
import { useAdminAssignments } from '../composables/useAdminAssignments'

const {
  loading,
  errorMessage,
  nurses,
  selectedNurseId,
  assignedSearchTerm,
  availableSearchTerm,
  paginatedAssignedPatients,
  paginatedAvailablePatients,
  assignedCurrentPage,
  availableCurrentPage,
  assignedTotalPages,
  availableTotalPages,
  assignLoading,
  unassignLoading,
  assignErrorMessage,
  isUnassignErrorModalOpen,
  unassignErrorModalMessage,
  loadAssignedPatients,
  loadAssignmentsData,
  assignPatient,
  unassignPatient,
  closeUnassignErrorModal,
  resetAssignedPage,
  resetAvailablePage,
  goToPreviousAssignedPage,
  goToNextAssignedPage,
  goToPreviousAvailablePage,
  goToNextAvailablePage
} = useAdminAssignments()

watch(selectedNurseId, async () => {
  resetAssignedPage()
  assignErrorMessage.value = ''
  closeUnassignErrorModal()

  try {
    await loadAssignedPatients()
  } catch (error) {
    errorMessage.value = error.message || 'Error loading assigned patients.'
  }
})

watch(assignedSearchTerm, () => {
  resetAssignedPage()
})

watch(availableSearchTerm, () => {
  resetAvailablePage()
})

watch(assignedTotalPages, (newTotalPages) => {
  if (assignedCurrentPage.value > newTotalPages) {
    assignedCurrentPage.value = newTotalPages
  }
})

watch(availableTotalPages, (newTotalPages) => {
  if (availableCurrentPage.value > newTotalPages) {
    availableCurrentPage.value = newTotalPages
  }
})

onMounted(() => {
  loadAssignmentsData()
})
</script>

<template>
  <section class="admin-assignments">
    <p v-if="loading" class="assignments-message">Loading assignments...</p>

    <p v-else-if="errorMessage" class="assignments-message assignments-message-error">{{ errorMessage }}</p>

    <section v-else class="assignments-main">
      <section class="assignments-toprow">
        <label for="assignmentNurseSelect" class="assignments-label">Nurse</label>

        <select id="assignmentNurseSelect" v-model="selectedNurseId" class="assignments-nurse-select">
          <option value="">Select a nurse</option>

          <option v-for="nurse in nurses" :key="nurse.nurseId" :value="nurse.nurseId">
            {{ nurse.name }} {{ nurse.surname }}
          </option>
        </select>
      </section>

      <section class="assignments-panels">
        <article class="assignments-panel">
          <header class="assignments-panel-header">
            <h2>Assigned Patients</h2>

            <input v-model="assignedSearchTerm" type="text" class="app-input" placeholder="Search assigned patients..." aria-label="Search assigned patients" />
          </header>

          <section class="assignments-tablebody">
            <div class="assignments-tablewrap">
              <table class="assignments-table">
                <thead>
                  <tr>
                    <th>Patient</th>
                    <th>Surgery</th>
                    <th>Date</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  <tr v-if="!paginatedAssignedPatients.length">
                    <td colspan="4" class="assignments-empty">No assigned patients found.</td>
                  </tr>

                  <tr v-for="patient in paginatedAssignedPatients" :key="patient.patientId">
                    <td>{{ patient.name }} {{ patient.surname }}</td>
                    <td>{{ patient.surgery || '-' }}</td>
                    <td>{{ patient.surgeryDate || '-' }}</td>

                    <td class="assignments-col-action">
                      <button type="button" class="app-action-button" :disabled="unassignLoading" :aria-label="`Unassign patient ${patient.name} ${patient.surname}`" title="Unassign patient" @click="unassignPatient(patient)">
                        <img src="/icons/deleteAssignment.png" alt="" class="app-action-icon" />
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <footer class="app-pagination assignments-pagination-footer">
            <button type="button" class="app-pagination-button" :disabled="assignedCurrentPage === 1" @click="goToPreviousAssignedPage">&lt;</button>
            <span class="app-pagination-text">Page {{ assignedCurrentPage }} of {{ assignedTotalPages }}</span>
            <button type="button" class="app-pagination-button" :disabled="assignedCurrentPage === assignedTotalPages" @click="goToNextAssignedPage">&gt;</button>
          </footer>
        </article>

        <article class="assignments-panel">
          <header class="assignments-panel-header">
            <h2>Unassigned Patients</h2>

            <input v-model="availableSearchTerm" type="text" class="app-input" placeholder="Search unassigned patients..." aria-label="Search unassigned patients" />
          </header>

          <section class="assignments-tablebody">
            <div class="assignments-tablewrap">
              <table class="assignments-table">
                <thead>
                  <tr>
                    <th>Patient</th>
                    <th>Surgery</th>
                    <th>Date</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  <tr v-if="!paginatedAvailablePatients.length">
                    <td colspan="4" class="assignments-empty">No unassigned patients found.</td>
                  </tr>

                  <tr v-for="patient in paginatedAvailablePatients" :key="patient.patientId">
                    <td>{{ patient.patientName }} {{ patient.patientSurname }}</td>
                    <td>{{ patient.surgeryTypeName || '-' }}</td>
                    <td>{{ patient.surgeryDate || '-' }}</td>

                    <td class="assignments-col-action">
                      <button type="button" class="app-action-button" :disabled="assignLoading || !selectedNurseId" :aria-label="`Assign patient ${patient.patientName} ${patient.patientSurname}`" title="Assign patient" @click="assignPatient(patient)">
                        <img src="/icons/assignmentBlack.png" alt="" class="app-action-icon" />
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <footer class="app-pagination assignments-pagination-footer">
            <button type="button" class="app-pagination-button" :disabled="availableCurrentPage === 1" @click="goToPreviousAvailablePage">&lt;</button>
            <span class="app-pagination-text">Page {{ availableCurrentPage }} of {{ availableTotalPages }}</span>
            <button type="button" class="app-pagination-button" :disabled="availableCurrentPage === availableTotalPages" @click="goToNextAvailablePage">&gt;</button>
          </footer>
        </article>
      </section>
    </section>

    <section v-if="isUnassignErrorModalOpen" class="assignments-modal">
      <article class="assignments-modal-card">
        <header class="assignments-modal-header">
          <h2 class="assignments-modal-title">Assignment could not be removed</h2>
        </header>

        <section class="assignments-modal-body">
          <p class="assignments-modal-text">{{ unassignErrorModalMessage }}</p>
        </section>

        <footer class="assignments-modal-actions">
          <button type="button" class="app-button app-button-primary" @click="closeUnassignErrorModal">Accept</button>
        </footer>
      </article>
    </section>
  </section>
</template>

<style src="../styles/admin-assignments.css"></style>