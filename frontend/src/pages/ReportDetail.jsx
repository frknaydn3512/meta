import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getReport, downloadReport, sendReportEmail } from '../api/reports'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

const STATUS_COLOR = {
  Completed: 'bg-green-100 text-green-700',
  Processing: 'bg-yellow-100 text-yellow-700',
  Pending: 'bg-gray-100 text-gray-600',
  Failed: 'bg-red-100 text-red-700',
}

export default function ReportDetail() {
  const { id } = useParams()
  const [report, setReport] = useState(null)
  const [sending, setSending] = useState(false)
  const t = useT()

  useEffect(() => {
    getReport(id).then(({ data }) => setReport(data.data))
  }, [id])

  async function handleViewPdf() {
    try {
      const { data } = await downloadReport(id)
      const url = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }))
      window.open(url, '_blank')
    } catch {
      toast.error(t('download_failed'))
    }
  }

  async function handleSendEmail() {
    setSending(true)
    try {
      await sendReportEmail(id)
      toast.success(t('email_sent'))
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('failed_send_email'))
    } finally {
      setSending(false)
    }
  }

  if (!report) return <p className="text-sm text-gray-500">{t('loading')}</p>

  const MONTHS = t('months')
  const STATUS_LABEL = {
    Completed: t('status_completed'),
    Processing: t('status_processing'),
    Pending: t('status_pending'),
    Failed: t('status_failed'),
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {report.clientName} — {MONTHS[report.month - 1]} {report.year}
          </h1>
          <p className="text-sm text-gray-500 mt-1">{report.accountName}</p>
        </div>
        <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${STATUS_COLOR[report.status] ?? ''}`}>
          {STATUS_LABEL[report.status] ?? report.status}
        </span>
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-3">
        <Row label={t('report_id')} value={`#${report.id}`} />
        <Row label={t('period')} value={`${MONTHS[report.month - 1]} ${report.year}`} />
        <Row label={t('client')} value={report.clientName} />
        <Row label={t('meta_account')} value={report.accountName} />
        <Row label={t('created')} value={new Date(report.createdAt).toLocaleString()} />
        {report.completedAt && <Row label={t('completed')} value={new Date(report.completedAt).toLocaleString()} />}
        <Row label={t('public_link')} value={
          <a href={`/r/${report.slug}`} target="_blank" rel="noreferrer" className="text-blue-600 hover:underline break-all">
            /r/{report.slug}
          </a>
        } />
      </div>

      {report.status === 'Completed' && (
        <div className="flex gap-3">
          <button onClick={handleViewPdf} className="bg-blue-600 text-white text-sm font-medium px-5 py-2 rounded-lg hover:bg-blue-700 transition-colors">
            {t('download_pdf')}
          </button>
          <button onClick={handleSendEmail} disabled={sending} className="border border-gray-300 text-gray-700 text-sm font-medium px-5 py-2 rounded-lg hover:bg-gray-50 disabled:opacity-50 transition-colors">
            {sending ? t('sending') : t('send_email')}
          </button>
        </div>
      )}
    </div>
  )
}

function Row({ label, value }) {
  return (
    <div className="flex items-center gap-4">
      <span className="text-sm text-gray-500 w-32 shrink-0">{label}</span>
      <span className="text-sm text-gray-900">{value}</span>
    </div>
  )
}
