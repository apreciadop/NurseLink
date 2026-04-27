<script src="../scripts/patientView.js"></script>

<template>
  <section class="patient">
    <aside :class="['patient-sidebar', { 'patient-sidebar-open': isSidebarOpen }]">
      <header class="patient-sidebarbrand">
        <img src="/logo.png" alt="NurseLink Logo" class="patient-sidebarlogo" />
      </header>

      <nav class="patient-sidebarnav" aria-label="Patient menu">
        <ul class="patient-sidebarmenu">
          <li class="patient-sidebaritem">
            <router-link to="/patient/dashboard" class="patient-sidebarlink" :class="{ 'patient-sidebarlink-active': isDashboardSection }" @click="closeSidebar">
              <img src="/icons/dashboardBlack.png" class="patient-sidebaricon patient-sidebaricon-default" alt=""/>
              <img src="/icons/dashboardBlue.png" class="patient-sidebaricon patient-sidebaricon-active" alt=""/>
              <span>Dashboard</span>
            </router-link>
          </li>

          <li class="patient-sidebaritem">
            <router-link to="/patient/messages" class="patient-sidebarlink" :class="{ 'patient-sidebarlink-active': isMessagesSection }" @click="closeSidebar">
              <img src="/icons/messagesBlack.png" class="patient-sidebaricon patient-sidebaricon-default" alt=""/>
              <img src="/icons/messagesBlue.png" class="patient-sidebaricon patient-sidebaricon-active" alt=""/>
              <span>Messages</span>
              <span v-if="unreadMessagesCount > 0" class="patient-sidebarbadge">{{ unreadMessagesBadgeText }}</span>
            </router-link>
          </li>
        </ul>
      </nav>
    </aside>

    <button v-if="isSidebarOpen" type="button" class="patient-overlay" aria-label="Close menu" @click="closeSidebar"></button>

    <section class="patient-main">
      <header class="patient-topbar">
        <div class="patient-topbarheading">
          <button type="button" class="patient-topbarmenu-button" @click="toggleSidebar">Menu</button>
          <img :src="currentSectionIcon" alt="" class="patient-topbarheading-icon" />
          <h1 class="patient-topbartitle">{{ currentSectionTitle }}</h1>
          <span v-if="patientName" class="patient-topbarname">{{ patientName }}</span>
        </div>

        <button type="button" class="patient-topbarlogout" @click="openLogoutConfirm">Logout</button>
      </header>

      <main class="patient-content">
        <router-view />
      </main>

      <footer class="patient-footer">
        © 2026 NurseLink · Version 1.0
      </footer>
    </section>

    <section v-if="showLogoutConfirm" class="logout-modal">
      <div class="logout-modalcard">
        <p>Are you sure you want to log out?</p>

        <div class="logout-modalactions">
          <button type="button" class="logout-modalcancel"@click="cancelLogout">Cancel</button>
          <button type="button" class="logout-modalconfirm" @click="confirmLogout">Logout</button>
        </div>
      </div>
    </section>
  </section>
</template>

<style src="../styles/patient.css"></style>