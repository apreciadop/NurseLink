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

export async function getPatientDashboardById(patientId) {
  const response = await fetch(`${API_URL}/api/Patients/${patientId}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading patient dashboard.')
}

export async function getReportsByPatient(patientId) {
  const response = await fetch(`${API_URL}/api/Reports/patient/${patientId}`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error loading patient reports.')
}

export async function createReport(request) {
  const response = await fetch(`${API_URL}/api/Reports/create`, {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(request)
  })

  return await readResponse(response, 'Error creating patient report.')
}

export async function getSurgeryTypes() {
  const response = await fetch(`${API_URL}/api/SurgeryType`, {
    method: 'GET',
    headers: getAuthHeaders()
  })

  return await readResponse(response, 'Error getting surgery types.')
}