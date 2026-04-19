<script src="../scripts/adminPatientsView.js"></script>

<template>
  <section class="admin-patients">
    <p v-if="loading" class="patients-message">Loading patients...</p>

    <p
      v-else-if="errorMessage"
      class="patients-message patients-message-error"
    >
      {{ errorMessage }}
    </p>

    <section v-else class="patients-main">
      <section class="patients-kpis">
        <article class="patients-kpi patients-kpi-total">
          <p class="patients-kpi-label">Total Patients</p>
          <h2 class="patients-kpi-value">{{ totalPatients }}</h2>
        </article>

        <article class="patients-kpi patients-kpi-stable">
          <p class="patients-kpi-label">Stable Patients</p>
          <h2 class="patients-kpi-value">{{ stablePatients }}</h2>
        </article>

        <article class="patients-kpi patients-kpi-alert">
          <p class="patients-kpi-label">Patients with Alerts</p>
          <h2 class="patients-kpi-value">{{ patientsWithAlerts }}</h2>
        </article>
      </section>

      <section class="patients-tablecard">
        <header class="patients-tablehead">
          <div class="patients-search">
            <input
              v-model="searchTerm"
              type="text"
              class="patients-search-input"
              placeholder="Search patients..."
              aria-label="Search patients"
            />
          </div>

          <button
            type="button"
            class="patients-addbutton"
            @click="openCreatePatientModal"
          >
            + Add Patient
          </button>
        </header>

        <section class="patients-filtersrow">
          <label class="patients-filteroption">
            <input v-model="activeFilter" type="radio" value="all" />
            <span>All</span>
          </label>

          <label class="patients-filteroption">
            <input v-model="activeFilter" type="radio" value="active" />
            <span>Active</span>
          </label>

          <label class="patients-filteroption">
            <input v-model="activeFilter" type="radio" value="inactive" />
            <span>Inactive</span>
          </label>
        </section>

        <section class="patients-tablebody">
          <div class="patients-tablewrap">
            <table class="patients-table">
              <thead>
                <tr>
                  <th>Photo</th>
                  <th>Name</th>
                  <th>Assigned Nurse</th>
                  <th>Surgery</th>
                  <th>Date</th>
                  <th>Phone Number</th>
                  <th>Age</th>
                  <th>Active</th>
                  <th>Status</th>
                  <th>Alerts</th>
                  <th>Action</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!paginatedPatients.length">
                  <td colspan="11" class="patients-empty">
                    No patients found.
                  </td>
                </tr>

                <tr v-for="patient in paginatedPatients" :key="patient.patientId">
                  <td>
                    <div class="patients-photo">
                      <img
                        v-if="patient.photo"
                        :src="patient.photo"
                        alt="Patient photo"
                      />
                      <span v-else>No photo</span>
                    </div>
                  </td>

                  <td>{{ patient.name }} {{ patient.surname }}</td>
                  <td>{{ patient.assignedNurseName || '-' }}</td>
                  <td>{{ patient.surgery || '-' }}</td>
                  <td>{{ patient.surgeryDate || '-' }}</td>
                  <td>{{ patient.phone || '-' }}</td>
                  <td>{{ patient.age ?? '-' }}</td>

                  <td class="patients-col-center">
                    <span
                      :class="[
                        'patients-activebadge',
                        patient.active
                          ? 'patients-activebadge-active'
                          : 'patients-activebadge-inactive'
                      ]"
                    >
                      {{ patient.active ? 'Active' : 'Inactive' }}
                    </span>
                  </td>

                  <td class="patients-col-center">
                    <span
                      :class="[
                        'patients-statusbadge',
                        patient.statusLabel === 'Stable'
                          ? 'patients-statusbadge-stable'
                          : patient.statusLabel === 'Warning'
                            ? 'patients-statusbadge-warning'
                            : 'patients-statusbadge-alert'
                      ]"
                    >
                      {{ patient.statusLabel }}
                    </span>
                  </td>

                  <td class="patients-col-center">
                    {{ patient.alertCount }}
                  </td>

                  <td class="patients-col-center">
                    <div class="patients-actions">
                      <button
                        type="button"
                        class="patients-actionbutton"
                        :disabled="!!patient.assignedNurseId"
                        :aria-label="`Assign patient ${patient.name} ${patient.surname}`"
                        :title="patient.assignedNurseId ? 'Patient already assigned' : 'Assign patient'"
                        @click="openAssignModal(patient)"
                      >
                        <img
                          src="/icons/assignmentBlack.png"
                          alt=""
                          class="patients-actionicon"
                        />
                      </button>

                      <router-link
                        :to="`/admin/patients/${patient.patientId}`"
                        class="patients-actionbutton"
                        :aria-label="`Edit patient ${patient.name} ${patient.surname}`"
                        title="Edit patient"
                      >
                        <img
                          src="/icons/edit.png"
                          alt=""
                          class="patients-actionicon"
                        />
                      </router-link>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <footer class="patients-pagination">
          <button
            type="button"
            class="patients-pagination-button"
            :disabled="currentPage === 1"
            @click="goToPreviousPage"
          >
            &lt;
          </button>

          <span class="patients-pagination-text">
            Page {{ currentPage }} of {{ totalPages }}
          </span>

          <button
            type="button"
            class="patients-pagination-button"
            :disabled="currentPage === totalPages"
            @click="goToNextPage"
          >
            &gt;
          </button>
        </footer>
      </section>
    </section>

    <div
      v-if="isPatientModalOpen"
      class="patients-modal-overlay"
    >
      <div class="patients-modal" @click.stop>
        <header class="patients-modalheader">
          <h2>Add Patient</h2>

          <button
            type="button"
            class="patients-modalclose"
            aria-label="Close patient modal"
            @click="closePatientModal"
          >
            ×
          </button>
        </header>

        <section class="patients-modalbody">
          <div class="patients-modalgrid">
            <div class="patients-modalfield">
              <label for="patientName" class="patients-modallabel">Name</label>
              <input
                id="patientName"
                v-model="patientForm.name"
                type="text"
                class="patients-modalinput"
              />
            </div>

            <div class="patients-modalfield">
              <label for="patientSurname" class="patients-modallabel">Surname</label>
              <input
                id="patientSurname"
                v-model="patientForm.surname"
                type="text"
                class="patients-modalinput"
              />
            </div>

            <div class="patients-modalfield patients-modalfield-full">
              <label for="patientEmail" class="patients-modallabel">Email</label>
              <input
                id="patientEmail"
                v-model="patientForm.email"
                type="email"
                class="patients-modalinput"
              />
            </div>

            <div class="patients-modalfield patients-modalfield-full">
              <label for="patientPassword" class="patients-modallabel">Password</label>
              <input
                id="patientPassword"
                v-model="patientForm.password"
                type="password"
                class="patients-modalinput"
              />
            </div>

            <div class="patients-modalfield">
              <label for="patientBirthdate" class="patients-modallabel">Birthdate</label>
              <input
                id="patientBirthdate"
                v-model="patientForm.birthdate"
                type="date"
                class="patients-modalinput"
              />
            </div>

            <div class="patients-modalfield">
              <label for="patientPhone" class="patients-modallabel">Phone</label>
              <input
                id="patientPhone"
                v-model="patientForm.phone"
                type="text"
                class="patients-modalinput"
              />
            </div>

            <div class="patients-modalfield">
              <label for="patientSurgeryType" class="patients-modallabel">Surgery</label>
              <select
                id="patientSurgeryType"
                v-model="patientForm.surgeryTypeId"
                class="patients-modalinput"
              >
                <option value="">Select surgery type</option>
                <option
                  v-for="surgeryType in surgeryTypes"
                  :key="surgeryType.surgeryTypeId"
                  :value="String(surgeryType.surgeryTypeId)"
                >
                  {{ surgeryType.name }}
                </option>
              </select>
            </div>

            <div class="patients-modalfield">
              <label for="patientSurgeryDate" class="patients-modallabel">Surgery Date</label>
              <input
                id="patientSurgeryDate"
                v-model="patientForm.surgeryDate"
                type="date"
                class="patients-modalinput"
              />
            </div>

            <div class="patients-modalfield patients-modalfield-full">
              <label for="patientObservations" class="patients-modallabel">Observations</label>
              <textarea
                id="patientObservations"
                v-model="patientForm.patientObservations"
                class="patients-modaltextarea"
                rows="4"
              ></textarea>
            </div>

            <div class="patients-modalfield patients-modalfield-full">
              <label for="patientPhoto" class="patients-modallabel">Photo</label>

              <div class="patients-modalphoto-row">
                <div class="patients-modalphoto-preview">
                  <img
                    v-if="photoPreview"
                    :src="photoPreview"
                    alt="Patient preview"
                    class="patients-modalphoto-image"
                  />
                  <div v-else class="patients-modalphoto-placeholder">
                    No image selected
                  </div>
                </div>

                <label for="patientPhoto" class="patients-modalphoto-upload">
                  <span>Select image</span>
                </label>
              </div>

              <input
                id="patientPhoto"
                type="file"
                class="patients-modalfile-input"
                accept="image/*"
                @change="handlePhotoChange"
              />
            </div>
          </div>

          <p v-if="patientFormErrorMessage" class="patients-modalerror">
            {{ patientFormErrorMessage }}
          </p>
        </section>

        <footer class="patients-modalfooter">
          <button
            type="button"
            class="patients-modalbutton patients-modalbutton-secondary"
            @click="closePatientModal"
          >
            Cancel
          </button>

          <button
            type="button"
            class="patients-modalbutton patients-modalbutton-primary"
            :disabled="patientFormLoading"
            @click="submitCreatePatient"
          >
            {{ patientFormLoading ? 'Creating...' : 'Create' }}
          </button>
        </footer>
      </div>
    </div>

    <div
      v-if="isAssignModalOpen"
      class="dashboard-modal-overlay"
      @click="closeAssignModal"
    >
      <div class="dashboard-modal" @click.stop>
        <header class="dashboard-modalheader">
          <h2>Assign Patient</h2>
          <button
            type="button"
            class="dashboard-modalclose"
            aria-label="Close assignment modal"
            @click="closeAssignModal"
          >
            ×
          </button>
        </header>

        <section class="dashboard-modalbody" v-if="selectedPatient">
          <div class="dashboard-modalpatient-info">
            <p><strong>Patient:</strong> {{ selectedPatient.name }} {{ selectedPatient.surname }}</p>
            <p><strong>Surgery:</strong> {{ selectedPatient.surgery }}</p>
            <p><strong>Date:</strong> {{ selectedPatient.surgeryDate }}</p>
          </div>

          <div class="dashboard-modalfield">
            <label for="assignNurseSelect" class="dashboard-modallabel">Select Nurse</label>
            <select
              id="assignNurseSelect"
              v-model="selectedNurseId"
              class="dashboard-modalselect"
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
          </div>

          <p v-if="assignErrorMessage" class="dashboard-modalerror">
            {{ assignErrorMessage }}
          </p>
        </section>

        <footer class="dashboard-modalfooter">
          <button
            type="button"
            class="dashboard-modalbutton dashboard-modalbutton--secondary"
            @click="closeAssignModal"
          >
            Cancel
          </button>

          <button
            type="button"
            class="dashboard-modalbutton dashboard-modalbutton--primary"
            :disabled="assignLoading"
            @click="submitAssignment"
          >
            {{ assignLoading ? 'Assigning...' : 'Assign' }}
          </button>
        </footer>
      </div>
    </div>
  </section>
</template>

<style src="../styles/admin-patients.css"></style>