// Every call from React to the .NET API goes through this one file.

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5062'
const STORAGE_KEY = 'jobtracker-user'

// ----- Remembering the logged-in user in the browser (survives a page refresh) -----

export function getSavedUser() {
  const text = localStorage.getItem(STORAGE_KEY)
  return text ? JSON.parse(text) : null
}

export function saveUser(user) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(user))
}

export function clearUser() {
  localStorage.removeItem(STORAGE_KEY)
}

// ----- One helper that sends a request and returns the JSON answer -----

async function request(method, path, body) {
  const user = getSavedUser()
  const headers = {}
  if (body) headers['Content-Type'] = 'application/json'
  // The login token goes in the Authorization header, so the API knows who we are.
  if (user) headers['Authorization'] = `Bearer ${user.token}`

  let response
  try {
    response = await fetch(API_URL + path, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    })
  } catch {
    throw new Error('Cannot reach the server. Is the API running?')
  }

  if (response.status === 204) return null // 204 = success with nothing to send back

  const data = await response.json().catch(() => null)

  if (!response.ok) {
    // 401 while logged in means the token expired: log out and go to the login page.
    if (response.status === 401 && user) {
      clearUser()
      window.location.href = '/login'
    }
    throw new Error(readErrorMessage(data))
  }

  return data
}

// The API reports problems in two shapes: { message } or { errors: { Field: [..] } }.
function readErrorMessage(data) {
  if (data?.message) return data.message
  if (data?.errors) return Object.values(data.errors).flat()[0]
  return 'Something went wrong. Please try again.'
}

// ----- The API calls the screens use -----

export const api = {
  register: (name, email, password) => request('POST', '/api/auth/register', { name, email, password }),
  login: (email, password) => request('POST', '/api/auth/login', { email, password }),

  getJobs: () => request('GET', '/api/jobs'),
  createJob: (job) => request('POST', '/api/jobs', job),
  updateJob: (id, job) => request('PUT', `/api/jobs/${id}`, job),
  updateJobStatus: (id, status) => request('PATCH', `/api/jobs/${id}/status`, { status }),
  deleteJob: (id) => request('DELETE', `/api/jobs/${id}`),
}
