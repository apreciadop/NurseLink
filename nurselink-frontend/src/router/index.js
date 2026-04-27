import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import AdminView from '../views/AdminView.vue'
import AdminDashboardView from '../views/AdminDashboardView.vue'
import AdminNursesView from '../views/AdminNursesView.vue'
import AdminNurseProfileView from '../views/AdminNurseProfileView.vue'
import AdminPatientsView from '../views/AdminPatientsView.vue'
import AdminPatientProfileView from '../views/AdminPatientProfileView.vue'
import AdminAssignmentsView from '../views/AdminAssignmentsView.vue'
import NurseView from '../views/NurseView.vue'
import NurseDashboardView from '../views/NurseDashboardView.vue'
import NurseMessagesView from '../views/NurseMessagesView.vue'
import NursePatientProfileView from '../views/NursePatientProfileView.vue'
import PatientView from '../views/PatientView.vue'
import PatientDashboardView from '../views/PatientDashboardView.vue'
import PatientMessagesView from '../views/PatientMessagesView.vue'
import { logoutUser } from '../services/authService'

const routes = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    component: LoginView
  },
  {
    path: '/admin',
    component: AdminView,
    meta: { requiresAuth: true, role: 'Admin' },
    children: [
      {
        path: '',
        redirect: '/admin/dashboard'
      },
      {
        path: 'dashboard',
        component: AdminDashboardView
      },
      {
        path: 'nurses',
        component: AdminNursesView
      },
      {
        path: 'nurses/:id',
        component: AdminNurseProfileView
      },
      {
        path: 'patients',
        component: AdminPatientsView
      },
      {
        path: 'patients/:id',
        component: AdminPatientProfileView
      },
      {
        path: 'assignments',
        component: AdminAssignmentsView
      }
    ]
  },
  {
    path: '/nurse',
    component: NurseView,
    meta: { requiresAuth: true, role: 'Nurse' },
    children: [
      {
        path: '',
        redirect: '/nurse/dashboard'
      },
      {
        path: 'dashboard',
        component: NurseDashboardView
      },
      {
        path: 'patients/:id',
        component: NursePatientProfileView
      },
      {
        path: 'messages',
        component: NurseMessagesView
      },
      {
        path: 'messages/:conversationId',
        component: NurseMessagesView
      }
    ]
  },
  {
    path: '/patient',
    component: PatientView,
    meta: { requiresAuth: true, role: 'Patient' },
    children: [
      {
        path: '',
        redirect: '/patient/dashboard'
      },
      {
        path: 'dashboard',
        component: PatientDashboardView
      },
      {
        path: 'messages',
        component: PatientMessagesView
      },
      {
        path: 'messages/:conversationId',
        component: PatientMessagesView
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

function isTokenExpired(token) {
  if (!token) {
    return true
  }

  try {
    const payloadBase64 = token.split('.')[1]

    if (!payloadBase64) {
      return true
    }

    const normalizedPayload = payloadBase64
      .replace(/-/g, '+')
      .replace(/_/g, '/')

    const decodedPayload = JSON.parse(atob(normalizedPayload))
    const expiration = decodedPayload.exp

    if (!expiration) {
      return true
    }

    const currentTimeInSeconds = Math.floor(Date.now() / 1000)

    return expiration <= currentTimeInSeconds
  } catch {
    return true
  }
}

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  const role = localStorage.getItem('role')

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const requiredRole = to.matched.find(record => record.meta.role)?.meta.role

  if (requiresAuth) {
    if (!token || isTokenExpired(token)) {
      logoutUser()
      return '/login'
    }
  }

  if (requiredRole && role !== requiredRole) {
    logoutUser()
    return '/login'
  }

  if (to.path === '/login' && token && !isTokenExpired(token)) {
    if (role === 'Admin') {
      return '/admin/dashboard'
    }

    if (role === 'Nurse') {
      return '/nurse/dashboard'
    }

    if (role === 'Patient') {
      return '/patient/dashboard'
    }
  }

  return true
})

export default router