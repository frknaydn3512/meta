import api from './axios'

export const getTemplate = () => api.get('/template')
export const updateTemplate = (data) => api.put('/template', data)
export const uploadLogo = (file) => {
  const form = new FormData()
  form.append('file', file)
  return api.post('/template/logo', form, { headers: { 'Content-Type': 'multipart/form-data' } })
}
