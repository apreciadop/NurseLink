import { API_URL } from '../config/api'
import { getAuthHeaders, readApiResponse } from './authService'

export async function getPatientDashboardById(patientId) {
  const response = await fetch(`${API_URL}/api/Patients/${patientId}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readApiResponse(response, 'Error loading patient dashboard.')
}

export async function getReportsByPatient(patientId) {
  const response = await fetch(`${API_URL}/api/Reports/patient/${patientId}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readApiResponse(response, 'Error loading patient reports.')
}

export async function getReportById(reportId) {
  const response = await fetch(`${API_URL}/api/Reports/${reportId}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readApiResponse(response, 'Error loading report details.')
}

export async function createReport(request) {
  const response = await fetch(`${API_URL}/api/Reports/create`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(request)
  })

  return await readApiResponse(response, 'Error creating patient report.')
}

export async function getSurgeryTypes() {
  const response = await fetch(`${API_URL}/api/SurgeryType`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readApiResponse(response, 'Error getting surgery types.')
}

export async function getOrCreateConversationForPatient(patientId) {
  const response = await fetch(`${API_URL}/api/Conversations/patient/${patientId}/getOrCreate`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readApiResponse(response, 'Error preparing conversation.')
}

export async function getConversationDetail(conversationId) {
  const response = await fetch(`${API_URL}/api/Conversations/${conversationId}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readApiResponse(response, 'Error loading conversation detail.')
}

export async function getConversationMessages(conversationId, query = {}) {
  const params = new URLSearchParams()

  if (query.nurseId) {
    params.append('nurseId', String(query.nurseId))
  }

  if (query.patientId) {
    params.append('patientId', String(query.patientId))
  }

  const queryString = params.toString()
  const url = queryString
    ? `${API_URL}/api/Conversations/${conversationId}/messages?${queryString}`
    : `${API_URL}/api/Conversations/${conversationId}/messages`

  const response = await fetch(url, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readApiResponse(response, 'Error loading conversation messages.')
}

export async function sendConversationMessage(conversationId, request) {
  const response = await fetch(`${API_URL}/api/Conversations/${conversationId}/messages`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(request)
  })

  return await readApiResponse(response, 'Error sending message.')
}

export async function markConversationAsRead(conversationId, request) {
  const response = await fetch(`${API_URL}/api/Conversations/${conversationId}/read`, {
    method: 'PUT',
    headers: getAuthHeaders(),
    body: JSON.stringify(request)
  })

  return await readApiResponse(response, 'Error marking messages as read.')
}