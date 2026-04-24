import api from './axios'

export const getMetaAccounts = () => api.get('/meta-accounts')
export const connectMetaAccount = (data) => api.post('/meta-accounts', data)
export const disconnectMetaAccount = (id) => api.delete(`/meta-accounts/${id}`)

export const getMetaOAuthUrl = (clientId, redirectUri) =>
  api.get(`/meta-accounts/oauth/url?clientId=${clientId}&redirectUri=${encodeURIComponent(redirectUri)}`)

export const exchangeMetaOAuth = (data) => api.post('/meta-accounts/oauth/exchange', data)
export const confirmMetaOAuth = (data) => api.post('/meta-accounts/oauth/confirm', data)
