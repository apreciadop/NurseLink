import { apiRequest } from './authService'

export async function getDashboardKpis() {
  return await apiRequest('/api/Admin/dashboardKpis', { method: 'GET' }, 'Error loading dashboard KPIs.')
}

export async function getPatientsWithAlerts() {
  return await apiRequest('/api/Admin/patientsWithAlerts', { method: 'GET' }, 'Error loading patients with alerts.')
}

export async function getUnassignedPatients() {
  return await apiRequest('/api/Admin/unassignedPatients', { method: 'GET' }, 'Error loading unassigned patients.')
}

export async function getNurses() {
  return await apiRequest('/api/Nurses', { method: 'GET' }, 'Error loading nurses.')
}

export async function getNursesDetailed() {
  return await apiRequest('/api/Nurses/nursesDetailed', { method: 'GET' }, 'Error loading detailed nurses.')
}

export async function getNurseById(id) {
  return await apiRequest(`/api/Nurses/${id}`, { method: 'GET' }, 'Error loading nurse.')
}

export async function getAssignedPatientsByNurse(id) {
  return await apiRequest(`/api/Nurses/${id}/assignedPatients`, { method: 'GET' }, 'Error loading assigned patients.')
}

export async function createNurse(payload) {
  return await apiRequest('/api/Nurses/create', { method: 'POST', body: payload }, 'Error creating nurse.')
}

export async function updateNurse(id, payload) {
  return await apiRequest(`/api/Nurses/update/${id}`, { method: 'PUT', body: payload }, 'Error updating nurse.')
}

export async function createAssignment(payload) {
  return await apiRequest('/api/Assignments/create', { method: 'POST', body: payload }, 'Error creating assignment.')
}

export async function deleteAssignmentByPatient(patientId) {
  return await apiRequest(`/api/Assignments/patient/${patientId}`, { method: 'DELETE' }, 'Error deleting assignment.')
}

export async function getPatientsDetailed() {
  return await apiRequest('/api/Patients/patientsDetailed', { method: 'GET' }, 'Error getting detailed patients list.')
}

export async function createPatient(payload) {
  return await apiRequest('/api/Patients/create', { method: 'POST', body: payload }, 'Error creating patient.')
}

export async function getPatientById(id) {
  return await apiRequest(`/api/Patients/${id}`, { method: 'GET' }, 'Error loading patient.')
}

export async function updatePatient(id, payload) {
  return await apiRequest(`/api/Patients/update/${id}`, { method: 'PUT', body: payload }, 'Error updating patient.')
}

export async function getSurgeryTypes() {
  return await apiRequest('/api/SurgeryType', { method: 'GET' }, 'Error getting surgery types.')
}

export async function getReportsByPatient(patientId, pageNumber = 1, pageSize = 8) {
  return await apiRequest(`/api/Reports/patient/${patientId}?pageNumber=${pageNumber}&pageSize=${pageSize}`, { method: 'GET' }, 'Error loading patient reports.')
}

export async function getReportById(reportId) {
  return await apiRequest(`/api/Reports/${reportId}`, { method: 'GET' }, 'Error loading report details.')
}