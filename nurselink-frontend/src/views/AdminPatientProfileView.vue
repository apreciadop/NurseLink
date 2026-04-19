<script src="../scripts/adminPatientProfileView.js"></script>

<template>
  <section class="admin-patient-profile">
    <header class="patient-profile-top">
      <router-link to="/admin/patients" class="patient-profile-back">
        ← Back to Patients
      </router-link>
    </header>

    <p v-if="loading" class="patient-profile-message">Loading patient...</p>

    <p
      v-else-if="errorMessage"
      class="patient-profile-message patient-profile-message-error"
    >
      {{ errorMessage }}
    </p>

    <section v-else class="patient-profile-main">
      <section class="patient-profile-toprow">
        <section class="patient-profile-left">
          <div class="patient-profile-photo">
            <img
              v-if="patientForm.photo"
              :src="patientForm.photo"
              alt="Patient photo"
            />
            <span v-else>No photo</span>
          </div>

          <div class="patient-profile-summary">
            <h2 class="patient-profile-name">
              {{ patientForm.name }} {{ patientForm.surname }}
            </h2>

            <p class="patient-profile-id">Patient ID {{ patientForm.patientId }}</p>

            <label
              for="patientProfilePhotoInput"
              class="patient-profile-photo-button"
            >
              <img
                src="/icons/cameraBlue.png"
                alt=""
                class="patient-profile-photo-button-icon"
              />
              <span>Change Photo</span>
            </label>

            <input
              id="patientProfilePhotoInput"
              type="file"
              class="patient-profile-fileinput"
              accept="image/*"
              @change="handlePhotoChange"
            />
          </div>
        </section>

        <section class="patient-profile-right">
          <h3 class="patient-profile-section-title">Patient Details</h3>

          <div class="patient-profile-formgrid patient-profile-formgrid-threecols">
            <div class="patient-profile-field">
              <label for="patientName" class="patient-profile-label">Name</label>
              <input
                id="patientName"
                v-model="patientForm.name"
                type="text"
                class="patient-profile-input"
                placeholder="Name"
              />
            </div>

            <div class="patient-profile-field">
              <label for="patientSurname" class="patient-profile-label">Surname</label>
              <input
                id="patientSurname"
                v-model="patientForm.surname"
                type="text"
                class="patient-profile-input"
                placeholder="Surname"
              />
            </div>

            <div class="patient-profile-field">
              <label for="patientBirthdate" class="patient-profile-label">Birthdate</label>
              <input
                id="patientBirthdate"
                v-model="patientForm.birthdate"
                type="date"
                class="patient-profile-input"
              />
            </div>

            <div class="patient-profile-field">
              <label for="patientEmail" class="patient-profile-label">Email</label>
              <input
                id="patientEmail"
                v-model="patientForm.email"
                type="email"
                class="patient-profile-input"
                placeholder="Email"
              />
            </div>

            <div class="patient-profile-field">
              <label for="patientPassword" class="patient-profile-label">Password</label>
              <input
                id="patientPassword"
                v-model="patientForm.password"
                type="password"
                class="patient-profile-input"
                placeholder="Leave empty to keep current password"
              />
            </div>

            <div class="patient-profile-field">
              <label for="patientPhone" class="patient-profile-label">Phone</label>
              <input
                id="patientPhone"
                v-model="patientForm.phone"
                type="text"
                class="patient-profile-input"
                placeholder="Phone"
              />
            </div>

            <div class="patient-profile-field">
              <label for="patientAssignedNurse" class="patient-profile-label">Assigned Nurse</label>
              <input
                id="patientAssignedNurse"
                :value="patientForm.assignedNurseName || 'No nurse assigned'"
                type="text"
                class="patient-profile-input"
                readonly
              />
            </div>

            <div class="patient-profile-field">
              <label for="patientSurgeryType" class="patient-profile-label">Surgery</label>
              <select
                id="patientSurgeryType"
                v-model="patientForm.surgeryTypeId"
                class="patient-profile-input patient-profile-select"
              >
                <option :value="0" disabled>Select a surgery</option>
                <option
                  v-for="surgeryType in surgeryTypes"
                  :key="surgeryType.surgeryTypeId"
                  :value="surgeryType.surgeryTypeId"
                >
                  {{ surgeryType.name }}
                </option>
              </select>
            </div>

            <div class="patient-profile-field">
              <label for="patientSurgeryDate" class="patient-profile-label">Surgery Date</label>
              <input
                id="patientSurgeryDate"
                v-model="patientForm.surgeryDate"
                type="date"
                class="patient-profile-input"
              />
            </div>

            <div class="patient-profile-statusrow">
              <span class="patient-profile-label">Status</span>

              <div class="patient-profile-statusgroup">
                <label class="patient-profile-statusoption">
                  <input v-model="patientForm.active" :value="true" type="radio" />
                  <span>Active</span>
                </label>

                <label class="patient-profile-statusoption">
                  <input v-model="patientForm.active" :value="false" type="radio" />
                  <span>Inactive</span>
                </label>
              </div>

              <button
                type="button"
                class="patient-profile-save"
                :disabled="saveLoading"
                @click="submitUpdatePatient"
              >
                {{ saveLoading ? 'Saving...' : 'Save' }}
              </button>
            </div>
          </div>

          <p
            v-if="saveErrorMessage"
            class="patient-profile-message patient-profile-message-error"
          >
            {{ saveErrorMessage }}
          </p>

          <p
            v-if="saveMessage"
            class="patient-profile-message patient-profile-message-success"
          >
            {{ saveMessage }}
          </p>
        </section>
      </section>

      <section class="patient-profile-observations">
        <h3 class="patient-profile-section-title">Observations</h3>

        <textarea
          v-model="patientForm.patientObservations"
          class="patient-profile-textarea"
          placeholder="Write observations..."
        ></textarea>
      </section>

      <section class="patient-profile-reports">
        <header class="patient-profile-reportshead">
          <h3 class="patient-profile-reportstitle">Patient Symptom Reports</h3>
        </header>

        <section class="patient-profile-reportsbody">
          <p class="patient-profile-reportsplaceholder">
            Reports table will be added here later.
          </p>
        </section>
      </section>
    </section>
  </section>
</template>

<style src="../styles/admin-patient-profile.css"></style>