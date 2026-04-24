import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import axios from 'axios'
import useT from '../hooks/useT'

export default function PublicReport() {
  const { slug } = useParams()
  const [report, setReport] = useState(null)
  const [error, setError] = useState(null)
  const t = useT()

  useEffect(() => {
    axios.get(`/r/${slug}`)
      .then(({ data }) => setReport(data.data))
      .catch(() => setError(t('report_not_found')))
  }, [slug])

  if (error) return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <p className="text-gray-500 text-sm">{error}</p>
    </div>
  )

  if (!report) return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <p className="text-gray-400 text-sm">{t('loading')}</p>
    </div>
  )

  const { template: tmpl, insights: ins } = report
  const primary = tmpl?.primaryColor ?? '#1a56db'
  const MONTHS = t('months')
  const period = `${MONTHS[report.month - 1]} ${report.year}`

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="py-8 px-8 text-white" style={{ background: primary }}>
        <div className="max-w-4xl mx-auto flex items-center justify-between">
          <div>
            {tmpl?.logoUrl
              ? <img src={tmpl.logoUrl} alt={tmpl.agencyDisplayName} className="h-10 object-contain mb-2" />
              : <p className="text-xl font-bold">{tmpl?.agencyDisplayName}</p>}
            <p className="text-white/80 text-sm">{t('meta_ads_report')} — {period}</p>
            <p className="text-white/70 text-xs mt-0.5">{report.clientName} · {report.accountName}</p>
          </div>
        </div>
      </div>

      <div className="max-w-4xl mx-auto px-8 py-8 space-y-8">
        {ins && (
          <div>
            <h2 className="text-lg font-semibold text-gray-800 mb-4">{t('performance_overview')}</h2>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
              <MetricCard label={t('total_spend')} value={`${report.currency} ${(ins.spend ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2 })}`} />
              <MetricCard label={t('impressions')} value={(ins.impressions ?? 0).toLocaleString()} />
              <MetricCard label={t('clicks')} value={(ins.clicks ?? 0).toLocaleString()} />
              <MetricCard label={t('roas')} value={`${(ins.roas ?? 0).toFixed(2)}x`} />
              <MetricCard label={t('ctr')} value={`${(ins.ctr ?? 0).toFixed(2)}%`} />
              <MetricCard label={t('cpc')} value={`${report.currency} ${(ins.cpc ?? 0).toFixed(2)}`} />
              <MetricCard label={t('conversions')} value={(ins.conversions ?? 0).toLocaleString()} />
            </div>
          </div>
        )}

        <div className="border-t border-gray-200 pt-6 text-xs text-gray-400 flex gap-4">
          {tmpl?.agencyEmail && <span>{tmpl.agencyEmail}</span>}
          {tmpl?.agencyPhone && <span>{tmpl.agencyPhone}</span>}
          {tmpl?.agencyWebsite && <span>{tmpl.agencyWebsite}</span>}
        </div>
      </div>
    </div>
  )
}

function MetricCard({ label, value }) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 px-5 py-4">
      <p className="text-xs text-gray-500 mb-1">{label}</p>
      <p className="text-xl font-bold text-gray-900">{value}</p>
    </div>
  )
}
