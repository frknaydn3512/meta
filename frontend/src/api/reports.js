import api from './axios'

export const getReports = () => api.get('/reports')
export const getReport = (id) => api.get(`/reports/${id}`)
export const createReport = (data) => api.post('/reports', data)
export const sendReportEmail = (id) => api.post(`/reports/${id}/send-email`)
export const downloadReport = (id) => api.get(`/reports/${id}/download`, { responseType: 'blob' })
export const getPublicReport = (slug) => api.get(`/r/${slug}`)
