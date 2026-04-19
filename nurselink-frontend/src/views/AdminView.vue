<script src="../scripts/adminView.js"></script>

<template>
  <section class="admin">
    <aside :class="['admin-sidebar', { 'admin-sidebar-open': isSidebarOpen }]">
      <header class="admin-sidebarbrand">
        <img src="/logo.png" alt="NurseLink Logo" class="admin-sidebarlogo" />
      </header>

      <nav class="admin-sidebarnav" aria-label="Administrator menu">
        <ul class="admin-sidebarmenu">
          <li class="admin-sidebaritem">
            <router-link
              to="/admin/dashboard"
              :class="['admin-sidebarlink', { 'admin-sidebarlink-active': isDashboardSection }]"
            >
              <img
                src="/icons/dashboardBlack.png"
                class="admin-sidebaricon admin-sidebaricon-default"
                alt=""
              />
              <img
                src="/icons/dashboardBlue.png"
                class="admin-sidebaricon admin-sidebaricon-active"
                alt=""
              />
              <span>Dashboard</span>
            </router-link>
          </li>

          <li class="admin-sidebaritem">
            <router-link
              to="/admin/nurses"
              :class="['admin-sidebarlink', { 'admin-sidebarlink-active': isNursesSection }]"
            >
              <img
                src="/icons/nurseBlack.png"
                class="admin-sidebaricon admin-sidebaricon-default"
                alt=""
              />
              <img
                src="/icons/nurseBlue.png"
                class="admin-sidebaricon admin-sidebaricon-active"
                alt=""
              />
              <span>Nurses</span>
            </router-link>
          </li>

          <li class="admin-sidebaritem">
            <router-link
              to="/admin/patients"
              :class="['admin-sidebarlink', { 'admin-sidebarlink-active': isPatientsSection }]"
            >
              <img
                src="/icons/patientBlack.png"
                class="admin-sidebaricon admin-sidebaricon-default"
                alt=""
              />
              <img
                src="/icons/patientBlue.png"
                class="admin-sidebaricon admin-sidebaricon-active"
                alt=""
              />
              <span>Patients</span>
            </router-link>
          </li>

          <li class="admin-sidebaritem">
            <router-link to="/admin/assignments" :class="['admin-sidebarlink', { 'admin-sidebarlink-active': isAssignmentsSection }]">
              <img src="/icons/assignmentBlack.png" class="admin-sidebaricon admin-sidebaricon-default" alt=""/>
              <img src="/icons/assignmentBlue.png" class="admin-sidebaricon admin-sidebaricon-active" alt=""/>
              <span>Assignments</span>
            </router-link>
          </li>
        </ul>
      </nav>
    </aside>

    <button
      v-if="isSidebarOpen"
      class="admin-overlay"
      type="button"
      aria-label="Close sidebar"
      @click="closeSidebar"
    ></button>

    <main class="admin-main">
      <header class="admin-topbar">
        <button
          class="admin-topbarmenu-button"
          type="button"
          @click="toggleSidebar"
        >
          ☰
        </button>

        <section class="admin-topbarheading">
          <img :src="currentSectionIcon" class="admin-topbarheading-icon" alt="" />
          <h1 class="admin-topbartitle">{{ currentSectionTitle }}</h1>
        </section>

        <button
          class="admin-topbarlogout"
          type="button"
          @click="openLogoutConfirm"
        >
          Logout
        </button>
      </header>

      <section class="admin-content">
        <router-view />
      </section>

      <footer class="admin-footer">
        © 2026 NurseLink - Version 1.0
      </footer>
    </main>

    <section v-if="showLogoutConfirm" class="logout-modal">
      <section class="logout-modalcard">
        <p>Are you sure you want to log out?</p>

        <section class="logout-modalactions">
          <button
            class="logout-modalcancel"
            type="button"
            @click="cancelLogout"
          >
            Cancel
          </button>

          <button
            class="logout-modalconfirm"
            type="button"
            @click="confirmLogout"
          >
            Logout
          </button>
        </section>
      </section>
    </section>
  </section>
</template>

<style src="../styles/admin.css"></style>