import { apiRequest, buildQueryString } from './authService'

export async function getPatientDashboardById(patientId) {
  if (!patientId) {
    throw new Error('Patient identifier was not found.')
  }

  return await apiRequest(`/api/Patients/${patientId}`, { method: 'GET' }, 'Error loading patient dashboard.')
}

export async function getReportsByPatient(patientId) {
  if (!patientId) {
    throw new Error('Patient identifier was not found.')
  }

  return await apiRequest(`/api/Reports/patient/${patientId}`, { method: 'GET' }, 'Error loading patient reports.')
}

export async function getReportById(reportId) {
  if (!reportId) {
    throw new Error('Report identifier was not found.')
  }

  return await apiRequest(`/api/Reports/${reportId}`, { method: 'GET' }, 'Error loading report details.')
}

export async function createReport(request) {
  return await apiRequest('/api/Reports/create', { method: 'POST', body: request }, 'Error creating patient report.')
}

export async function getSurgeryTypes() {
  return await apiRequest('/api/SurgeryType', { method: 'GET' }, 'Error getting surgery types.')
}

export async function getOrCreateConversationForPatient(patientId) {
  if (!patientId) {
    throw new Error('Patient identifier was not found.')
  }

  return await apiRequest(`/api/Conversations/patient/${patientId}/getOrCreate`, { method: 'GET' }, 'Error preparing conversation.')
}

export async function getConversationDetail(conversationId) {
  if (!conversationId) {
    throw new Error('Conversation identifier was not found.')
  }

  return await apiRequest(`/api/Conversations/${conversationId}`, { method: 'GET' }, 'Error loading conversation detail.')
}

export async function getConversationMessages(conversationId, query = {}) {
  if (!conversationId) {
    throw new Error('Conversation identifier was not found.')
  }

  return await apiRequest(`/api/Conversations/${conversationId}/messages${buildQueryString(query)}`, { method: 'GET' }, 'Error loading conversation messages.')
}

export async function sendConversationMessage(conversationId, request) {
  if (!conversationId) {
    throw new Error('Conversation identifier was not found.')
  }

  return await apiRequest(`/api/Conversations/${conversationId}/messages`, { method: 'POST', body: request }, 'Error sending message.')
}

export async function markConversationAsRead(conversationId, request) {
  if (!conversationId) {
    throw new Error('Conversation identifier was not found.')
  }

  return await apiRequest(`/api/Conversations/${conversationId}/read`, { method: 'PUT', body: request }, 'Error marking messages as read.')
}