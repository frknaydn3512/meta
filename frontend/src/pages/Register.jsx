import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { register } from '../api/auth'
import useAuthStore from '../store/authStore'
import useLangStore from '../store/langStore'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

export default function Register() {
  const [form, setForm] = useState({ name: '', email: '', password: '' })
  const [loading, setLoading] = useState(false)
  const setAuth = useAuthStore((s) => s.login)
  const navigate = useNavigate()
  const { lang, toggle } = useLangStore()
  const t = useT()

  async function handleSubmit(e) {
    e.preventDefault()
    setLoading(true)
    try {
      const { data } = await register(form)
      setAuth(data.data)
      navigate('/dashboard')
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('registration_failed'))
    } finally {
      setLoading(false)
    }
  }

  const fields = [
    { key: 'name', label: t('name'), type: 'text' },
    { key: 'email', label: t('email'), type: 'email' },
    { key: 'password', label: t('password'), type: 'password' },
  ]

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-200 w-full max-w-sm">
        <div className="flex items-center justify-between mb-1">
          <h1 className="text-2xl font-bold text-gray-900">{t('create_account')}</h1>
          <button onClick={toggle} className="text-xs font-medium text-gray-500 hover:text-gray-800 border border-gray-200 rounded px-1.5 py-0.5">
            {lang === 'en' ? 'TR' : 'EN'}
          </button>
        </div>
        <p className="text-sm text-gray-500 mb-6">{t('trial_subtitle')}</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          {fields.map(({ key, label, type }) => (
            <div key={key}>
              <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
              <input
                type={type}
                required
                value={form[key]}
                onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-600"
              />
            </div>
          ))}
          <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors">
            {loading ? t('creating') : t('create_account')}
          </button>
        </form>
        <p className="mt-4 text-sm text-center text-gray-500">
          {t('already_account')}{' '}
          <Link to="/login" className="text-blue-600 hover:underline">{t('sign_in')}</Link>
        </p>
      </div>
    </div>
  )
}
