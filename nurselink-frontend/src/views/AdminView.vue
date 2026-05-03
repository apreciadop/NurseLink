<script setup>
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { logoutUser } from '../services/authService'

const route = useRoute()
const router = useRouter()

const isSidebarOpen = ref(false)
const showLogoutConfirm = ref(false)

const isDashboardSection = computed(() => {
  return route.path === '/admin/dashboard'
})

const isNursesSection = computed(() => {
  return route.path.startsWith('/admin/nurses')
})

const isPatientsSection = computed(() => {
  return route.path.startsWith('/admin/patients')
})

const isAssignmentsSection = computed(() => {
  return route.path.startsWith('/admin/assignments')
})

const currentSectionTitle = computed(() => {
  if (isNursesSection.value) {
    return 'Nurses'
  }

  if (isPatientsSection.value) {
    return 'Patients'
  }

  if (isAssignmentsSection.value) {
    return 'Assignments'
  }

  return 'Dashboard'
})

const currentSectionIcon = computed(() => {
  if (isNursesSection.value) {
    return '/icons/nurseBlue.png'
  }

  if (isPatientsSection.value) {
    return '/icons/patientBlue.png'
  }

  if (isAssignmentsSection.value) {
    return '/icons/assignmentBlue.png'
  }

  return '/icons/dashboardBlue.png'
})

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value
}

const closeSidebar = () => {
  isSidebarOpen.value = false
}

const openLogoutConfirm = () => {
  showLogoutConfirm.value = true
}

const cancelLogout = () => {
  showLogoutConfirm.value = false
}

const confirmLogout = () => {
  logoutUser()
  showLogoutConfirm.value = false
  router.push('/login')
}
</script>

<template>
  <section class="admin">
    <aside :class="['admin-sidebar', isSidebarOpen ? 'admin-sidebar-open' : '']">
      <div class="admin-sidebarbrand">
        <img src="/logo.png" alt="NurseLink logo" class="admin-sidebarlogo" />
      </div>

      <nav class="admin-sidebarnav">
        <ul class="admin-sidebarmenu">
          <li class="admin-sidebaritem">
            <router-link to="/admin/dashboard" :class="['admin-sidebarlink', isDashboardSection ? 'admin-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/dashboardBlack.png" alt="" class="admin-sidebaricon admin-sidebaricon-default" />
              <img src="/icons/dashboardBlue.png" alt="" class="admin-sidebaricon admin-sidebaricon-active" />
              <span>Dashboard</span>
            </router-link>
          </li>

          <li class="admin-sidebaritem">
            <router-link to="/admin/nurses" :class="['admin-sidebarlink', isNursesSection ? 'admin-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/nurseBlack.png" alt="" class="admin-sidebaricon admin-sidebaricon-default" />
              <img src="/icons/nurseBlue.png" alt="" class="admin-sidebaricon admin-sidebaricon-active" />
              <span>Nurses</span>
            </router-link>
          </li>

          <li class="admin-sidebaritem">
            <router-link to="/admin/patients" :class="['admin-sidebarlink', isPatientsSection ? 'admin-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/patientBlack.png" alt="" class="admin-sidebaricon admin-sidebaricon-default" />
              <img src="/icons/patientBlue.png" alt="" class="admin-sidebaricon admin-sidebaricon-active" />
              <span>Patients</span>
            </router-link>
          </li>

          <li class="admin-sidebaritem">
            <router-link to="/admin/assignments" :class="['admin-sidebarlink', isAssignmentsSection ? 'admin-sidebarlink-active' : '']" @click="closeSidebar">
              <img src="/icons/assignmentBlack.png" alt="" class="admin-sidebaricon admin-sidebaricon-default" />
              <img src="/icons/assignmentBlue.png" alt="" class="admin-sidebaricon admin-sidebaricon-active" />
              <span>Assignments</span>
            </router-link>
          </li>
        </ul>
      </nav>
    </aside>

    <button v-if="isSidebarOpen" type="button" class="admin-overlay" aria-label="Close sidebar" @click="closeSidebar"></button>

    <section class="admin-main">
      <header class="admin-topbar">
        <button type="button" class="admin-topbarmenu-button" @click="toggleSidebar">Menu</button>

        <div class="admin-topbarheading">
          <img :src="currentSectionIcon" alt="" class="admin-topbarheading-icon" />
          <h1 class="admin-topbartitle">{{ currentSectionTitle }}</h1>
        </div>

        <button type="button" class="admin-topbarlogout" @click="openLogoutConfirm">Logout</button>
      </header>

      <main class="admin-content">
        <router-view />
      </main>

      <footer class="admin-footer">
        <span>© 2026 NurseLink - Version 1.0</span>
      </footer>
    </section>

    <section v-if="showLogoutConfirm" class="logout-modal">
      <article class="logout-modalcard">
        <p>Are you sure you want to log out?</p>

        <div class="logout-modalactions">
          <button type="button" class="logout-modalcancel" @click="cancelLogout">Cancel</button>
          <button type="button" class="logout-modalconfirm" @click="confirmLogout">Logout</button>
        </div>
      </article>
    </section>
  </section>
</template>

<style src="../styles/admin.css"></style>