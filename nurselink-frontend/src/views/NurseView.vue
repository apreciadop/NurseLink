<script src="../scripts/nurseView.js"></script>

<template>
  <section class="nurse">
    <aside :class="['nurse-sidebar', { 'nurse-sidebar-open': isSidebarOpen }]">
      <header class="nurse-sidebarbrand">
        <img src="/logo.png" alt="NurseLink Logo" class="nurse-sidebarlogo" />
      </header>

      <nav class="nurse-sidebarnav" aria-label="Nurse menu">
        <ul class="nurse-sidebarmenu">
          <li class="nurse-sidebaritem">
            <router-link
              to="/nurse/dashboard"
              class="nurse-sidebarlink"
              :class="{ 'nurse-sidebarlink-active': isDashboardSection }"
              @click="closeSidebar"
            >
              <img
                src="/icons/dashboardBlack.png"
                class="nurse-sidebaricon nurse-sidebaricon-default"
                alt=""
              />
              <img
                src="/icons/dashboardBlue.png"
                class="nurse-sidebaricon nurse-sidebaricon-active"
                alt=""
              />
              <span>Dashboard</span>
            </router-link>
          </li>

          <li class="nurse-sidebaritem">
            <router-link
              to="/nurse/messages"
              class="nurse-sidebarlink"
              :class="{ 'nurse-sidebarlink-active': isMessagesSection }"
              @click="closeSidebar"
            >
              <img
                src="/icons/messagesBlack.png"
                class="nurse-sidebaricon nurse-sidebaricon-default"
                alt=""
              />
              <img
                src="/icons/messagesBlue.png"
                class="nurse-sidebaricon nurse-sidebaricon-active"
                alt=""
              />
              <span>Messages</span>
            </router-link>
          </li>
        </ul>
      </nav>
    </aside>

    <button
      v-if="isSidebarOpen"
      type="button"
      class="nurse-overlay"
      aria-label="Close menu"
      @click="closeSidebar"
    ></button>

    <section class="nurse-main">
      <header class="nurse-topbar">
        <div class="nurse-topbarheading">
          <button
            type="button"
            class="nurse-topbarmenu-button"
            @click="toggleSidebar"
          >
            Menu
          </button>

          <img :src="currentSectionIcon" alt="" class="nurse-topbarheading-icon" />

          <h1 class="nurse-topbartitle">{{ currentSectionTitle }}</h1>

          <span v-if="nurseName" class="nurse-topbarname">
            {{ nurseName }}
          </span>
        </div>

        <button
          type="button"
          class="nurse-topbarlogout"
          @click="openLogoutConfirm"
        >
          Logout
        </button>
      </header>

      <main class="nurse-content">
        <router-view />
      </main>

      <footer class="nurse-footer">
        © 2026 NurseLink · Version 1.0
      </footer>
    </section>

    <section v-if="showLogoutConfirm" class="logout-modal">
      <div class="logout-modalcard">
        <p>Are you sure you want to log out?</p>

        <div class="logout-modalactions">
          <button
            type="button"
            class="logout-modalcancel"
            @click="cancelLogout"
          >
            Cancel
          </button>

          <button
            type="button"
            class="logout-modalconfirm"
            @click="confirmLogout"
          >
            Logout
          </button>
        </div>
      </div>
    </section>
  </section>
</template>

<style src="../styles/nurse.css"></style>