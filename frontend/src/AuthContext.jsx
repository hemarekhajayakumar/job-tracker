import { createContext, useContext, useState } from 'react'
import { api, clearUser, getSavedUser, saveUser } from './api.js'

// A "context" lets any component ask "who is logged in?" without passing props everywhere.
const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  // Start with whoever was logged in before the page was refreshed (or null).
  const [user, setUser] = useState(getSavedUser)

  async function login(email, password) {
    const loggedIn = await api.login(email, password)
    saveUser(loggedIn)
    setUser(loggedIn)
  }

  async function register(name, email, password) {
    const registered = await api.register(name, email, password)
    saveUser(registered)
    setUser(registered)
  }

  function logout() {
    clearUser()
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

// Components call useAuth() to get { user, login, register, logout }.
export function useAuth() {
  return useContext(AuthContext)
}
