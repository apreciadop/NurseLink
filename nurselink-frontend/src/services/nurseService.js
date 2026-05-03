import { apiRequest, buildQueryString } from './authService'

export async function getAssignedPatientsByNurse(nurseId) {
  if (!nurseId) {
    throw new Error('Nurse identifier was not found.')
  }

  return await apiRequest(`/api/Nurses/${nurseId}/assignedPatients`, { method: 'GET' }, 'Error loading assigned patients.')
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

export async function nurseObservations(reportId, request) {
  if (!reportId) {
    throw new Error('Report identifier was not found.')
  }

  return await apiRequest(`/api/Reports/${reportId}/nurseObservations`, { method: 'PUT', body: request }, 'Error saving nurse observations.')
}

export async function getNurseConversations(nurseId) {
  if (!nurseId) {
    throw new Error('Nurse identifier was not found.')
  }

  return await apiRequest(`/api/Conversations/nurse/${nurseId}`, { method: 'GET' }, 'Error loading nurse conversations.')
}

export async function getOrCreateConversation(request) {
  return await apiRequest('/api/Conversations/getOrCreate', { method: 'POST', body: request }, 'Error preparing conversation.')
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