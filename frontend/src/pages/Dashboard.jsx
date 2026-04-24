import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getClients } from '../api/clients'
import { getReports } from '../api/reports'
import useT from '../hooks/useT'

const STATUS_COLOR = {
  Completed: 'bg-green-100 text-green-700',
  Processing: 'bg-yellow-100 text-yellow-700',
  Pending: 'bg-gray-100 text-gray-600',
  Failed: 'bg-red-100 text-red-700',
}

export default function Dashboard() {
  const [clients, setClients] = useState([])
  const [reports, setReports] = useState([])
  const t = useT()

  useEffect(() => {
    getClients().then(({ data }) => setClients(data.data ?? []))
    getReports().then(({ data }) => setReports(data.data ?? []))
  }, [])

  const recent = reports.slice(0, 5)
  const months = t('months_short')

  const STATUS_LABEL = {
    Completed: t('status_completed'),
    Processing: t('status_processing'),
    Pending: t('status_pending'),
    Failed: t('status_failed'),
  }

  return (
    <div className="space-y-8">
      <h1 className="text-2xl font-bold text-gray-900">{t('nav_dashboard')}</h1>

      <div className="grid grid-cols-3 gap-4">
        <StatCard label={t('total_clients')} value={clients.length} />
        <StatCard label={t('total_reports')} value={reports.length} />
        <StatCard label={t('completed_reports')} value={reports.filter((r) => r.status === 'Completed').length} />
      </div>

      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-lg font-semibold text-gray-800">{t('recent_reports')}</h2>
          <Link to="/reports" className="text-sm text-blue-600 hover:underline">{t('view_all')}</Link>
        </div>
        {recent.length === 0 ? (
          <p className="text-sm text-gray-500">{t('no_reports_yet')}</p>
        ) : (
          <div className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-100">
            {recent.map((r) => (
              <div key={r.id} className="flex items-center justify-between px-5 py-3">
                <div>
                  <p className="text-sm font-medium text-gray-800">{r.clientName}</p>
                  <p className="text-xs text-gray-500">{r.accountName} · {months[r.month - 1]} {r.year}</p>
                </div>
                <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${STATUS_COLOR[r.status] ?? ''}`}>
                  {STATUS_LABEL[r.status] ?? r.status}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

function StatCard({ label, value }) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 px-6 py-5">
      <p className="text-sm text-gray-500">{label}</p>
      <p className="text-3xl font-bold text-gray-900 mt-1">{value}</p>
    </div>
  )
}
