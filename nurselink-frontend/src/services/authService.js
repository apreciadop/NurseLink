import { API_URL } from '../config/api'

export async function readApiResponse(response, fallbackMessage) {
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
    if (response.status === 401) {
      logoutUser()
      window.location.href = '/login'
      throw new Error('Session expired.')
    }

    if (data?.errors) {
      const firstErrorKey = Object.keys(data.errors)[0]
      const firstErrorMessage = data.errors[firstErrorKey]?.[0]

      throw new Error(firstErrorMessage || message)
    }

    throw new Error(message)
  }

  return data
}

export async function loginUser(email, password) {
  const response = await fetch(`${API_URL}/api/Auth/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      email,
      password
    })
  })

  return await readApiResponse(response, 'Login failed.')
}

export async function forgotPassword(request) {
  const response = await fetch(`${API_URL}/api/Auth/forgotPassword`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(request)
  })

  return await readApiResponse(response, 'Error updating password.')
}

export function getAuthToken() {
  return localStorage.getItem('token')
}

export function getUserRole() {
  return localStorage.getItem('role')
}

export function getAuthHeaders() {
  const token = getAuthToken()

  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`
  }
}

export function logoutUser() {
  localStorage.removeItem('token')
  localStorage.removeItem('role')
  localStorage.removeItem('userName')
  localStorage.removeItem('name')
  localStorage.removeItem('fullName')
  localStorage.removeItem('user')
  localStorage.removeItem('userId')
  localStorage.removeItem('email')
  localStorage.removeItem('nurseId')
  localStorage.removeItem('patientId')
}