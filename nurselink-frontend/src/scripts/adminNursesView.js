import { onMounted } from 'vue'
import { useAdminNurses } from '../composables/useAdminNurses'

export default {
  name: 'AdminNursesView',

  setup() {
    const {
      loading,
      errorMessage,
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
      goToNextPage
    } = useAdminNurses()

    onMounted(() => {
      loadNursesDetailed()
    })

    return {
      loading,
      errorMessage,
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
      submitCreateNurse,
      goToPreviousPage,
      goToNextPage
    }
  }
}