<script src="../scripts/adminAssignmentsView.js"></script>

<template>
  <section class="admin-assignments">
    <p v-if="loading" class="assignments-message">Loading assignments...</p>

    <p
      v-else-if="errorMessage"
      class="assignments-message assignments-message-error"
    >
      {{ errorMessage }}
    </p>

    <section v-else class="assignments-main">
      <section class="assignments-toprow">
        <label for="assignmentsNurseSelect" class="assignments-label">
          Nurse
        </label>

        <select
          id="assignmentsNurseSelect"
          v-model="selectedNurseId"
          class="assignments-nurse-select"
        >
          <option value="">Select a nurse</option>
          <option
            v-for="nurse in nurses"
            :key="nurse.nurseId"
            :value="nurse.nurseId"
          >
            {{ nurse.name }} {{ nurse.surname }}
          </option>
        </select>
      </section>

      <section class="assignments-panels">
        <article class="assignments-panel">
          <header class="assignments-panel-header">
            <h2>Patients Assigned - {{ filteredAssignedPatients.length }}</h2>

            <input
              v-model="assignedSearchTerm"
              type="text"
              class="assignments-search"
              placeholder="Search Patient ..."
              aria-label="Search assigned patients"
            />
          </header>

          <section class="assignments-tablebody">
            <div class="assignments-tablewrap">
              <table class="assignments-table">
                <thead>
                  <tr>
                    <th>Patient</th>
                    <th>Surgery</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  <tr v-if="!paginatedAssignedPatients.length">
                    <td colspan="3" class="assignments-empty">
                      No assigned patients.
                    </td>
                  </tr>

                  <tr
                    v-for="patient in paginatedAssignedPatients"
                    :key="patient.patientId"
                  >
                    <td>{{ patient.name }} {{ patient.surname }}</td>
                    <td>{{ patient.surgery || '-' }}</td>

                    <td class="assignments-col-action">
                      <button
                        type="button"
                        class="assignments-delete-button"
                        :disabled="unassignLoading"
                        :aria-label="`Unassign patient ${patient.name} ${patient.surname}`"
                        title="Unassign patient"
                        @click="unassignPatient(patient)"
                      >
                        <img
                          src="/icons/deleteAssignment.png"
                          alt=""
                          class="assignments-delete-icon"
                        />
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <footer class="assignments-pagination">
            <button
              type="button"
              class="assignments-pagination-button"
              :disabled="assignedCurrentPage === 1"
              @click="goToPreviousAssignedPage"
            >
              &lt;
            </button>

            <span class="assignments-pagination-text">
              Page {{ assignedCurrentPage }} of {{ assignedTotalPages }}
            </span>

            <button
              type="button"
              class="assignments-pagination-button"
              :disabled="assignedCurrentPage === assignedTotalPages"
              @click="goToNextAssignedPage"
            >
              &gt;
            </button>
          </footer>
        </article>

        <article class="assignments-panel">
          <header class="assignments-panel-header">
            <h2>Available Patients - {{ filteredAvailablePatients.length }}</h2>

            <input
              v-model="availableSearchTerm"
              type="text"
              class="assignments-search"
              placeholder="Search Patient ..."
              aria-label="Search available patients"
            />
          </header>

          <section class="assignments-tablebody">
            <div class="assignments-tablewrap">
              <table class="assignments-table">
                <thead>
                  <tr>
                    <th>Patient</th>
                    <th>Surgery</th>
                    <th>Action</th>
                  </tr>
                </thead>

                <tbody>
                  <tr v-if="!paginatedAvailablePatients.length">
                    <td colspan="3" class="assignments-empty">
                      No available patients.
                    </td>
                  </tr>

                  <tr
                    v-for="patient in paginatedAvailablePatients"
                    :key="patient.patientId"
                  >
                    <td>{{ patient.patientName }} {{ patient.patientSurname }}</td>
                    <td>{{ patient.surgeryTypeName || '-' }}</td>

                    <td class="assignments-col-action">
                      <button
                        type="button"
                        class="assignments-assign-button"
                        :disabled="assignLoading || !selectedNurseId"
                        :aria-label="`Assign patient ${patient.patientName} ${patient.patientSurname}`"
                        title="Assign patient"
                        @click="assignPatient(patient)"
                      >
                        <img
                          src="/icons/assignmentBlack.png"
                          alt=""
                          class="assignments-action-icon"
                        />
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <footer class="assignments-pagination">
            <button
              type="button"
              class="assignments-pagination-button"
              :disabled="availableCurrentPage === 1"
              @click="goToPreviousAvailablePage"
            >
              &lt;
            </button>

            <span class="assignments-pagination-text">
              Page {{ availableCurrentPage }} of {{ availableTotalPages }}
            </span>

            <button
              type="button"
              class="assignments-pagination-button"
              :disabled="availableCurrentPage === availableTotalPages"
              @click="goToNextAvailablePage"
            >
              &gt;
            </button>
          </footer>
        </article>
      </section>

      <p
        v-if="assignErrorMessage"
        class="assignments-message assignments-message-error"
      >
        {{ assignErrorMessage }}
      </p>
    </section>
  </section>
</template>

<style src="../styles/admin-assignments.css"></style>