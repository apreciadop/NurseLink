import { API_URL } from '../config/api'

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

  let data = {}

  try {
    data = await response.json()
  } catch {
    data = {}
  }

  if (!response.ok) {
    throw new Error(data.message || 'Login failed.')
  }

  return data
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