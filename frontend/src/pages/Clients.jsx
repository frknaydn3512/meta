import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getClients, createClient, deleteClient } from '../api/clients'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

export default function Clients() {
  const [clients, setClients] = useState([])
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ name: '', email: '', industry: '' })
  const [loading, setLoading] = useState(false)
  const t = useT()

  async function load() {
    const { data } = await getClients()
    setClients(data.data ?? [])
  }

  useEffect(() => { load() }, [])

  async function handleCreate(e) {
    e.preventDefault()
    setLoading(true)
    try {
      await createClient(form)
      toast.success(t('client_created'))
      setForm({ name: '', email: '', industry: '' })
      setShowForm(false)
      load()
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('failed_create_client'))
    } finally {
      setLoading(false)
    }
  }

  async function handleDelete(id, name) {
    if (!confirm(t('confirm_delete_client', { name }))) return
    try {
      await deleteClient(id)
      toast.success(t('client_deleted'))
      load()
    } catch {
      toast.error(t('failed_delete_client'))
    }
  }

  const fields = [
    { key: 'name', label: t('name') },
    { key: 'email', label: t('email') },
    { key: 'industry', label: t('industry') },
  ]

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t('clients')}</h1>
        <button onClick={() => setShowForm(!showForm)} className="bg-blue-600 text-white text-sm font-medium px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors">
          {t('add_client')}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="bg-white border border-gray-200 rounded-xl p-5 space-y-4">
          <h2 className="font-semibold text-gray-800">{t('new_client')}</h2>
          <div className="grid grid-cols-3 gap-3">
            {fields.map(({ key, label }) => (
              <div key={key}>
                <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
                <input
                  type={key === 'email' ? 'email' : 'text'}
                  required={key !== 'industry'}
                  value={form[key]}
                  onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                  className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            ))}
          </div>
          <div className="flex gap-2">
            <button type="submit" disabled={loading} className="bg-blue-600 text-white text-sm px-4 py-1.5 rounded-lg hover:bg-blue-700 disabled:opacity-50">
              {loading ? t('saving') : t('save')}
            </button>
            <button type="button" onClick={() => setShowForm(false)} className="text-sm px-4 py-1.5 rounded-lg border border-gray-300 hover:bg-gray-50">
              {t('cancel')}
            </button>
          </div>
        </form>
      )}

      <div className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-100">
        {clients.length === 0 && <p className="text-sm text-gray-500 px-5 py-4">{t('no_clients_yet')}</p>}
        {clients.map((c) => (
          <div key={c.id} className="flex items-center justify-between px-5 py-3">
            <div>
              <Link to={`/clients/${c.id}`} className="text-sm font-medium text-blue-600 hover:underline">{c.name}</Link>
              <p className="text-xs text-gray-500">{c.email} {c.industry ? `· ${c.industry}` : ''}</p>
            </div>
            <button onClick={() => handleDelete(c.id, c.name)} className="text-xs text-red-500 hover:text-red-700">
              {t('delete')}
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}
