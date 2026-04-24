import api from './axios'

export const getClients = () => api.get('/agencyclients')
export const getClient = (id) => api.get(`/agencyclients/${id}`)
export const createClient = (data) => api.post('/agencyclients', data)
export const updateClient = (id, data) => api.put(`/agencyclients/${id}`, data)
export const deleteClient = (id) => api.delete(`/agencyclients/${id}`)
