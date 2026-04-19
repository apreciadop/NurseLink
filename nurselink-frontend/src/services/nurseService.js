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

export async function getAssignedPatientsByNurse(nurseId) {
  if (!nurseId) {
    throw new Error('Nurse identifier was not found.')
  }

  const response = await fetch(`${API_URL}/api/Nurses/${nurseId}/assignedPatients`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading assigned patients.')
}

export async function getReportsByPatient(patientId) {
  if (!patientId) {
    throw new Error('Patient identifier was not found.')
  }

  const response = await fetch(`${API_URL}/api/Reports/patient/${patientId}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading patient reports.')
}