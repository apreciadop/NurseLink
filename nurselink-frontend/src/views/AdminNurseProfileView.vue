<script src="../scripts/adminNurseProfileView.js"></script>

<template>
  <section class="admin-nurse-profile">
    <header class="nurse-profile-top">
      <router-link to="/admin/nurses" class="nurse-profile-back">
        ← Back to Nurses
      </router-link>
    </header>

    <p v-if="loading" class="nurse-profile-message">Loading nurse...</p>

    <p
      v-else-if="errorMessage"
      class="nurse-profile-message nurse-profile-message-error"
    >
      {{ errorMessage }}
    </p>

    <section v-else class="nurse-profile-main">
      <section class="nurse-profile-toprow">
        <section class="nurse-profile-left">
          <div class="nurse-profile-photo">
            <img
              v-if="nurseForm.photo"
              :src="nurseForm.photo"
              alt="Nurse photo"
            />
            <span v-else>No photo</span>
          </div>

          <div class="nurse-profile-summary">
            <h2 class="nurse-profile-name">
              {{ nurseForm.name }} {{ nurseForm.surname }}
            </h2>

            <p class="nurse-profile-id">Employee ID {{ nurseForm.nurseId }}</p>

            <label for="profilePhotoInput" class="nurse-profile-photo-button">
              <img
                src="/icons/cameraBlue.png"
                alt=""
                class="nurse-profile-photo-button-icon"
              />
              <span>Change Photo</span>
            </label>

            <input
              id="profilePhotoInput"
              type="file"
              class="nurse-profile-fileinput"
              accept="image/*"
              @change="handlePhotoChange"
            />
          </div>
        </section>

        <section class="nurse-profile-right">
          <h3 class="nurse-profile-section-title">Nurse Details</h3>

          <div class="nurse-profile-formgrid nurse-profile-formgrid-threecols">
            <div class="nurse-profile-field">
              <label for="profileName" class="nurse-profile-label">Name</label>
              <input
                id="profileName"
                v-model="nurseForm.name"
                type="text"
                class="nurse-profile-input"
                placeholder="Name"
              />
            </div>

            <div class="nurse-profile-field">
              <label for="profileSurname" class="nurse-profile-label">Surname</label>
              <input
                id="profileSurname"
                v-model="nurseForm.surname"
                type="text"
                class="nurse-profile-input"
                placeholder="Surname"
              />
            </div>

            <div class="nurse-profile-field">
              <label for="profileBirthdate" class="nurse-profile-label">Birthdate</label>
              <input
                id="profileBirthdate"
                v-model="nurseForm.birthdate"
                type="date"
                class="nurse-profile-input"
              />
            </div>

            <div class="nurse-profile-field">
              <label for="profileEmail" class="nurse-profile-label">Email</label>
              <input
                id="profileEmail"
                v-model="nurseForm.email"
                type="email"
                class="nurse-profile-input"
                placeholder="Email"
              />
            </div>

            <div class="nurse-profile-field">
              <label for="profilePassword" class="nurse-profile-label">Password</label>
              <input
                id="profilePassword"
                v-model="nurseForm.password"
                type="password"
                class="nurse-profile-input"
                placeholder="Leave empty to keep current password"
              />
            </div>

            <div class="nurse-profile-field">
              <label for="profilePhone" class="nurse-profile-label">Phone</label>
              <input
                id="profilePhone"
                v-model="nurseForm.phone"
                type="text"
                class="nurse-profile-input"
                placeholder="Phone"
              />
            </div>

            <div class="nurse-profile-statusrow">
              <span class="nurse-profile-label">Status</span>

              <div class="nurse-profile-statusgroup">
                <label class="nurse-profile-statusoption">
                  <input v-model="nurseForm.active" :value="true" type="radio" />
                  <span>Active</span>
                </label>

                <label class="nurse-profile-statusoption">
                  <input v-model="nurseForm.active" :value="false" type="radio" />
                  <span>Inactive</span>
                </label>
              </div>

              <button
                type="button"
                class="nurse-profile-save"
                :disabled="saveLoading"
                @click="submitUpdateNurse"
              >
                {{ saveLoading ? 'Saving...' : 'Save' }}
              </button>
            </div>
          </div>

          <p
            v-if="saveErrorMessage"
            class="nurse-profile-message nurse-profile-message-error"
          >
            {{ saveErrorMessage }}
          </p>

          <p
            v-if="saveMessage"
            class="nurse-profile-message nurse-profile-message-success"
          >
            {{ saveMessage }}
          </p>
        </section>
      </section>

      <section class="nurse-profile-patients">
        <header class="nurse-profile-patientshead">
          <h3 class="nurse-profile-patientstitle">Assigned Patients</h3>

          <div class="nurse-profile-patientsstats">
            <span class="nurse-profile-patientscount">
              {{ assignedPatientsCount }} patients
            </span>

            <span class="nurse-profile-alertscount">
              {{ assignedAlertsCount }} alerts
            </span>
          </div>

          <div class="nurse-profile-patientssearch">
            <input
              v-model="patientSearchTerm"
              type="text"
              class="nurse-profile-patientssearch-input"
              placeholder="Search Patients..."
              aria-label="Search patients"
            />
          </div>
        </header>

        <section class="nurse-profile-patientsbody nurse-profile-patientsbody-tall">
          <div class="nurse-profile-patients-tablewrap">
            <table class="nurse-profile-patients-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Surgery</th>
                  <th>Date</th>
                  <th>Status</th>
                  <th>Alerts</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!filteredAssignedPatients.length">
                  <td colspan="5" class="nurse-profile-patients-empty">
                    No assigned patients.
                  </td>
                </tr>

                <tr
                  v-for="patient in filteredAssignedPatients"
                  :key="patient.patientId"
                >
                  <td>{{ patient.name }} {{ patient.surname }}</td>
                  <td>{{ patient.surgery }}</td>
                  <td>{{ patient.surgeryDate || '-' }}</td>
                  <td>
                    <span
                      :class="[
                        'nurse-profile-statusbadge',
                        patient.status === 'Stable'
                          ? 'nurse-profile-statusbadge-stable'
                          : patient.status === 'Warning'
                            ? 'nurse-profile-statusbadge-warning'
                            : 'nurse-profile-statusbadge-alert'
                      ]"
                    >
                      {{ patient.status }}
                    </span>
                  </td>
                  <td>{{ patient.alertCount }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </section>
    </section>
  </section>
</template>

<style src="../styles/admin-nurse-profile.css"></style>