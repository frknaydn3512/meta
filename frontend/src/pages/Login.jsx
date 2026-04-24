import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { login } from '../api/auth'
import useAuthStore from '../store/authStore'
import useLangStore from '../store/langStore'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

export default function Login() {
  const [form, setForm] = useState({ email: '', password: '' })
  const [loading, setLoading] = useState(false)
  const setAuth = useAuthStore((s) => s.login)
  const navigate = useNavigate()
  const { lang, toggle } = useLangStore()
  const t = useT()

  async function handleSubmit(e) {
    e.preventDefault()
    setLoading(true)
    try {
      const { data } = await login(form)
      setAuth(data.data)
      navigate('/dashboard')
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('login_failed'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-200 w-full max-w-sm">
        <div className="flex items-center justify-between mb-1">
          <h1 className="text-2xl font-bold text-gray-900">{t('sign_in')}</h1>
          <button onClick={toggle} className="text-xs font-medium text-gray-500 hover:text-gray-800 border border-gray-200 rounded px-1.5 py-0.5">
            {lang === 'en' ? 'TR' : 'EN'}
          </button>
        </div>
        <p className="text-sm text-gray-500 mb-6">{t('agency_dashboard')}</p>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">{t('email')}</label>
            <input type="email" required value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">{t('password')}</label>
            <input type="password" required value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors">
            {loading ? t('signing_in') : t('sign_in')}
          </button>
        </form>
        <p className="mt-4 text-sm text-center text-gray-500">
          {t('no_account')}{' '}
          <Link to="/register" className="text-blue-600 hover:underline">{t('register')}</Link>
        </p>
        <p className="mt-3 text-xs text-center text-gray-400">
          <Link to="/privacy" className="hover:underline">{t('privacy_policy_link')}</Link>
        </p>
      </div>
    </div>
  )
}
