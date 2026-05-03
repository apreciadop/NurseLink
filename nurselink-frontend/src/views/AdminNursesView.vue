<script setup>
import { onMounted } from 'vue'
import AppFeedbackModal from '../components/AppFeedbackModal.vue'
import { useAdminNurses } from '../composables/useAdminNurses'

const {
  loading,
  errorMessage,
  successMessage,
  searchTerm,
  statusFilter,
  currentPage,
  totalPages,
  paginatedNurses,
  isNurseModalOpen,
  nurseModalMode,
  nurseForm,
  photoPreview,
  nurseFormErrorMessage,
  nurseFormLoading,
  openCreateNurseModal,
  closeNurseModal,
  handlePhotoChange,
  loadNursesDetailed,
  submitCreateNurse,
  goToPreviousPage,
  goToNextPage,
  clearAdminNursesFeedback
} = useAdminNurses()

onMounted(() => {
  loadNursesDetailed()
})
</script>

<template>
  <section class="admin-nurses">
    <p v-if="loading" class="admin-nursesmessage">Loading nurses...</p>

    <p v-else-if="errorMessage" class="admin-nursesmessage admin-nursesmessage-error">{{ errorMessage }}</p>

    <section v-else class="admin-nursescontent">
      <article class="nurses-panel">
        <header class="nurses-paneltoolbar">
          <div class="nurses-panelsearch-wrapper">
            <input v-model="searchTerm" type="text" class="app-input" placeholder="Search nurses or phone number..." aria-label="Search nurses" />
          </div>

          <button type="button" class="app-button app-button-primary nurses-paneladd-button" @click="openCreateNurseModal">+ Add Nurse</button>
        </header>

        <section class="nurses-panelfilters">
          <label class="app-filter-option">
            <input v-model="statusFilter" type="radio" value="all" />
            <span>All</span>
          </label>

          <label class="app-filter-option">
            <input v-model="statusFilter" type="radio" value="active" />
            <span>Active</span>
          </label>

          <label class="app-filter-option">
            <input v-model="statusFilter" type="radio" value="inactive" />
            <span>Inactive</span>
          </label>
        </section>

        <section class="nurses-paneltable-section">
          <div class="nurses-table-wrapper">
            <table class="nurses-table">
              <thead>
                <tr>
                  <th>Photo</th>
                  <th>Name</th>
                  <th>Patients</th>
                  <th>Alerts</th>
                  <th>Phone Number</th>
                  <th>Status</th>
                  <th class="nurses-tableactions-column">Action</th>
                </tr>
              </thead>

              <tbody>
                <tr v-if="!paginatedNurses.length">
                  <td colspan="7" class="nurses-tableempty">No nurses available.</td>
                </tr>

                <tr v-for="nurse in paginatedNurses" :key="nurse.nurseId">
                  <td>
                    <div class="nurses-photo">
                      <img v-if="nurse.photo" :src="nurse.photo" alt="Nurse photo" />
                      <span v-else>No photo</span>
                    </div>
                  </td>

                  <td>{{ nurse.name }} {{ nurse.surname }}</td>
                  <td>{{ nurse.patientCount }}</td>
                  <td>{{ nurse.alertCount }}</td>
                  <td>{{ nurse.phoneNumber }}</td>

                  <td>
                    <span :class="['app-badge', nurse.active ? 'app-badge-active' : 'app-badge-inactive']">{{ nurse.active ? 'Active' : 'Inactive' }}</span>
                  </td>

                  <td>
                    <router-link :to="`/admin/nurses/${nurse.nurseId}`" class="app-action-button" :aria-label="`Edit nurse ${nurse.name} ${nurse.surname}`" title="Edit nurse">
                      <img src="/icons/edit.png" alt="Edit nurse" class="app-action-icon" />
                    </router-link>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <footer class="app-pagination">
          <button type="button" class="app-pagination-button" :disabled="currentPage === 1" @click="goToPreviousPage">&lt;</button>
          <span class="app-pagination-text">Page {{ currentPage }} of {{ totalPages }}</span>
          <button type="button" class="app-pagination-button" :disabled="currentPage === totalPages" @click="goToNextPage">&gt;</button>
        </footer>
      </article>
    </section>

    <div v-if="isNurseModalOpen" class="nurses-modal-overlay">
      <div class="nurses-modal" @click.stop>
        <header class="nurses-modalheader">
          <h2>{{ nurseModalMode === 'create' ? 'Add Nurse' : 'Edit Nurse' }}</h2>
          <button type="button" class="nurses-modalclose" aria-label="Close nurse modal" @click="closeNurseModal">×</button>
        </header>

        <section class="nurses-modalbody">
          <div class="nurses-modalgrid">
            <div class="nurses-modalfield">
              <label for="nurseName" class="nurses-modallabel">Name</label>
              <input id="nurseName" v-model="nurseForm.name" type="text" class="nurses-modalinput" />
            </div>

            <div class="nurses-modalfield">
              <label for="nurseSurname" class="nurses-modallabel">Surname</label>
              <input id="nurseSurname" v-model="nurseForm.surname" type="text" class="nurses-modalinput" />
            </div>

            <div class="nurses-modalfield nurses-modalfield-full">
              <label for="nurseEmail" class="nurses-modallabel">Email</label>
              <input id="nurseEmail" v-model="nurseForm.email" type="email" class="nurses-modalinput" />
            </div>

            <div class="nurses-modalfield nurses-modalfield-full">
              <label for="nursePassword" class="nurses-modallabel">Password</label>
              <input id="nursePassword" v-model="nurseForm.password" type="password" class="nurses-modalinput" />
            </div>

            <div class="nurses-modalfield">
              <label for="nurseBirthdate" class="nurses-modallabel">Birthdate</label>
              <input id="nurseBirthdate" v-model="nurseForm.birthdate" type="date" class="nurses-modalinput" />
            </div>

            <div class="nurses-modalfield">
              <label for="nursePhone" class="nurses-modallabel">Phone</label>
              <input id="nursePhone" v-model="nurseForm.phone" type="text" class="nurses-modalinput" />
            </div>

            <div class="nurses-modalfield nurses-modalfield-full">
              <label for="nursePhoto" class="nurses-modallabel">Photo</label>

              <div class="nurses-modalphoto-row">
                <div class="nurses-modalphoto-preview">
                  <img v-if="photoPreview" :src="photoPreview" alt="Nurse preview" class="nurses-modalphoto-image" />
                  <div v-else class="nurses-modalphoto-placeholder">No image selected</div>
                </div>

                <label for="nursePhoto" class="nurses-modalphoto-upload">
                  <span>Select image</span>
                </label>
              </div>

              <input id="nursePhoto" type="file" class="nurses-modalfile-input" accept="image/*" @change="handlePhotoChange" />
            </div>
          </div>
        </section>

        <footer class="nurses-modalfooter">
          <button type="button" class="app-button app-button-secondary" @click="closeNurseModal">Cancel</button>
          <button type="button" class="app-button app-button-primary" :disabled="nurseFormLoading" @click="submitCreateNurse">{{ nurseFormLoading ? 'Creating...' : 'Create' }}</button>
        </footer>
      </div>
    </div>

    <AppFeedbackModal :visible="!!(successMessage || nurseFormErrorMessage)" :message="nurseFormErrorMessage || successMessage" :type="nurseFormErrorMessage ? 'error' : 'success'" @close="clearAdminNursesFeedback" />
  </section>
</template>

<style src="../styles/admin-nurses.css"></style>