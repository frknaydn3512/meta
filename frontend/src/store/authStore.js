import { create } from 'zustand'

const useAuthStore = create((set) => ({
  accessToken: localStorage.getItem('accessToken') || null,
  isAuthenticated: !!localStorage.getItem('accessToken'),

  login(tokens) {
    localStorage.setItem('accessToken', tokens.accessToken)
    localStorage.setItem('refreshToken', tokens.refreshToken)
    set({ accessToken: tokens.accessToken, isAuthenticated: true })
  },

  logout() {
    localStorage.clear()
    set({ accessToken: null, isAuthenticated: false })
  },
}))

export default useAuthStore
