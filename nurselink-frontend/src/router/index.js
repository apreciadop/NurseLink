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
import PatientView from '../views/PatientView.vue'
import PatientDashboardView from '../views/PatientDashboardView.vue'

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
        path: 'messages',
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
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from) => {
  const token = localStorage.getItem('token')
  const role = localStorage.getItem('role')

  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const requiredRole = to.matched.find(record => record.meta.role)?.meta.role

  if (requiresAuth && !token) {
    return '/login'
  }

  if (requiredRole && role !== requiredRole) {
    return '/login'
  }

  return true
})

export default router