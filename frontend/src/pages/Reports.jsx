import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getReports, createReport, downloadReport } from '../api/reports'
import { getClients } from '../api/clients'
import { getMetaAccounts } from '../api/metaAccounts'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

const STATUS_COLOR = {
  Completed: 'bg-green-100 text-green-700',
  Processing: 'bg-yellow-100 text-yellow-700',
  Pending: 'bg-gray-100 text-gray-600',
  Failed: 'bg-red-100 text-red-700',
}

export default function Reports() {
  const [reports, setReports] = useState([])
  const [clients, setClients] = useState([])
  const [accounts, setAccounts] = useState([])
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ clientId: '', metaAccountId: '', month: new Date().getMonth() + 1, year: new Date().getFullYear() })
  const [loading, setLoading] = useState(false)
  const t = useT()

  async function load() {
    const [r, c, a] = await Promise.all([getReports(), getClients(), getMetaAccounts()])
    setReports(r.data.data ?? [])
    setClients(c.data.data ?? [])
    setAccounts(a.data.data ?? [])
  }

  useEffect(() => { load() }, [])

  const filteredAccounts = accounts.filter((a) => !form.clientId || a.clientId === Number(form.clientId))
  const MONTHS = t('months')

  async function handleDownload(id) {
    try {
      const { data } = await downloadReport(id)
      const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
      window.open(url, '_blank')
    } catch {
      toast.error(t('download_failed'))
    }
  }

  const STATUS_LABEL = {
    Completed: t('status_completed'),
    Processing: t('status_processing'),
    Pending: t('status_pending'),
    Failed: t('status_failed'),
  }

  async function handleCreate(e) {
    e.preventDefault()
    setLoading(true)
    try {
      await createReport({ ...form, clientId: Number(form.clientId), metaAccountId: Number(form.metaAccountId), month: Number(form.month), year: Number(form.year) })
      toast.success(t('report_started'))
      setShowForm(false)
      load()
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('failed_create_report'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">{t('reports')}</h1>
        <button onClick={() => setShowForm(!showForm)} className="bg-blue-600 text-white text-sm font-medium px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors">
          {t('new_report')}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleCreate} className="bg-white border border-gray-200 rounded-xl p-5 space-y-4">
          <h2 className="font-semibold text-gray-800">{t('generate_report')}</h2>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">{t('client')}</label>
              <select required value={form.clientId} onChange={(e) => setForm({ ...form, clientId: e.target.value, metaAccountId: '' })} className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                <option value="">{t('select_client')}</option>
                {clients.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">{t('meta_account')}</label>
              <select required value={form.metaAccountId} onChange={(e) => setForm({ ...form, metaAccountId: e.target.value })} className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                <option value="">{t('select_account')}</option>
                {filteredAccounts.map((a) => <option key={a.id} value={a.id}>{a.accountName}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">{t('month')}</label>
              <select value={form.month} onChange={(e) => setForm({ ...form, month: e.target.value })} className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                {MONTHS.map((m, i) => <option key={i} value={i + 1}>{m}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-600 mb-1">{t('year')}</label>
              <input type="number" min="2020" max="2030" value={form.year} onChange={(e) => setForm({ ...form, year: e.target.value })} className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>
          <div className="flex gap-2">
            <button type="submit" disabled={loading} className="bg-blue-600 text-white text-sm px-4 py-1.5 rounded-lg hover:bg-blue-700 disabled:opacity-50">
              {loading ? t('creating') : t('generate')}
            </button>
            <button type="button" onClick={() => setShowForm(false)} className="text-sm px-4 py-1.5 rounded-lg border border-gray-300 hover:bg-gray-50">{t('cancel')}</button>
          </div>
        </form>
      )}

      <div className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-100">
        {reports.length === 0 && <p className="text-sm text-gray-500 px-5 py-4">{t('no_reports')}</p>}
        {reports.map((r) => (
          <div key={r.id} className="flex items-center justify-between px-5 py-3">
            <div>
              <Link to={`/reports/${r.id}`} className="text-sm font-medium text-blue-600 hover:underline">
                {r.clientName} — {MONTHS[r.month - 1]} {r.year}
              </Link>
              <p className="text-xs text-gray-500">{r.accountName}</p>
            </div>
            <div className="flex items-center gap-3">
              <span
                className={`text-xs font-medium px-2 py-0.5 rounded-full ${STATUS_COLOR[r.status] ?? ''}`}
                title={r.status === 'Failed' && r.errorMessage ? r.errorMessage : undefined}
              >
                {STATUS_LABEL[r.status] ?? r.status}
              </span>
              {r.status === 'Failed' && r.errorMessage && (
                <span className="text-xs text-red-500 max-w-xs truncate" title={r.errorMessage}>
                  {r.errorMessage}
                </span>
              )}
              {r.status === 'Completed' && (
                <button
                  onClick={() => handleDownload(r.id)}
                  className="text-xs text-blue-600 hover:text-blue-800 font-medium"
                  title={t('download_pdf')}
                >
                  ↓ PDF
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
