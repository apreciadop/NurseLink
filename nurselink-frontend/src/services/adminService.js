import { API_URL } from '../config/api'
import { getAuthHeaders } from './authService'

async function readResponse(response, fallbackMessage) {
  const responseText = await response.text()

  let data = null
  let message = fallbackMessage

  if (responseText) {
    try {
      data = JSON.parse(responseText)

      if (data?.message) {
        message = data.message
      } else if (data?.title) {
        message = data.title
      }
    } catch {
      message = responseText
    }
  }

  if (!response.ok) {
    if (data?.errors) {
      const firstErrorKey = Object.keys(data.errors)[0]
      const firstErrorMessage = data.errors[firstErrorKey]?.[0]

      throw new Error(firstErrorMessage || message)
    }

    throw new Error(message)
  }

  return data
}

export async function getDashboardKpis() {
  const response = await fetch(`${API_URL}/api/Admin/dashboardKpis`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading dashboard KPIs.')
}

export async function getPatientsWithAlerts() {
  const response = await fetch(`${API_URL}/api/Admin/patientsWithAlerts`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading patients with alerts.')
}

export async function getUnassignedPatients() {
  const response = await fetch(`${API_URL}/api/Admin/unassignedPatients`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading unassigned patients.')
}

export async function getNurses() {
  const response = await fetch(`${API_URL}/api/Nurses`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading nurses.')
}

export async function getNursesDetailed() {
  const response = await fetch(`${API_URL}/api/Nurses/nursesDetailed`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading detailed nurses.')
}

export async function getNurseById(id) {
  const response = await fetch(`${API_URL}/api/Nurses/${id}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading nurse.')
}

export async function getAssignedPatientsByNurse(id) {
  const response = await fetch(`${API_URL}/api/Nurses/${id}/assignedPatients`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading assigned patients.')
}

export async function createNurse(payload) {
  const response = await fetch(`${API_URL}/api/Nurses/create`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(payload)
  })

  return await readResponse(response, 'Error creating nurse.')
}

export async function updateNurse(id, payload) {
  const response = await fetch(`${API_URL}/api/Nurses/update/${id}`, {
    method: 'PUT',
    headers: getAuthHeaders(),
    body: JSON.stringify(payload)
  })

  return await readResponse(response, 'Error updating nurse.')
}

export async function createAssignment(payload) {
  const response = await fetch(`${API_URL}/api/Assignments/create`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(payload)
  })

  return await readResponse(response, 'Error creating assignment.')
}

export async function deleteAssignmentByPatient(patientId) {
  const response = await fetch(`${API_URL}/api/Assignments/patient/${patientId}`, {
    method: 'DELETE',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error deleting assignment.')
}

export async function getPatientsDetailed() {
  const response = await fetch(`${API_URL}/api/Patients/patientsDetailed`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error getting detailed patients list.')
}

export async function createPatient(payload) {
  const response = await fetch(`${API_URL}/api/Patients/create`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(payload)
  })

  return await readResponse(response, 'Error creating patient.')
}

export async function getPatientById(id) {
  const response = await fetch(`${API_URL}/api/Patients/${id}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading patient.')
}

export async function updatePatient(id, payload) {
  const response = await fetch(`${API_URL}/api/Patients/update/${id}`, {
    method: 'PUT',
    headers: getAuthHeaders(),
    body: JSON.stringify(payload)
  })

  return await readResponse(response, 'Error updating patient.')
}

export async function getSurgeryTypes() {
  const response = await fetch(`${API_URL}/api/SurgeryType`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error getting surgery types.')
}