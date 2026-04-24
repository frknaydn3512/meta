import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getClient } from '../api/clients'
import { getMetaAccounts, connectMetaAccount, disconnectMetaAccount, getMetaOAuthUrl } from '../api/metaAccounts'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

export default function ClientDetail() {
  const { id } = useParams()
  const [client, setClient] = useState(null)
  const [accounts, setAccounts] = useState([])
  const [oauthLoading, setOauthLoading] = useState(false)
  const [showManual, setShowManual] = useState(false)
  const [form, setForm] = useState({ accountId: '', accessToken: '' })
  const [manualLoading, setManualLoading] = useState(false)
  const t = useT()

  async function loadAccounts() {
    const { data } = await getMetaAccounts()
    setAccounts((data.data ?? []).filter((a) => a.clientId === Number(id)))
  }

  useEffect(() => {
    getClient(id).then(({ data }) => setClient(data.data))
    loadAccounts()
  }, [id])

  async function handleConnectWithMeta() {
    setOauthLoading(true)
    try {
      const redirectUri = window.location.origin + '/oauth/callback'
      const { data } = await getMetaOAuthUrl(Number(id), redirectUri)
      window.location.href = data.data.url
    } catch {
      toast.error(t('failed_connect'))
      setOauthLoading(false)
    }
  }

  async function handleManualConnect(e) {
    e.preventDefault()
    setManualLoading(true)
    try {
      await connectMetaAccount({ clientId: Number(id), ...form })
      toast.success(t('meta_connected'))
      setForm({ accountId: '', accessToken: '' })
      setShowManual(false)
      loadAccounts()
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('failed_connect'))
    } finally {
      setManualLoading(false)
    }
  }

  async function handleDisconnect(accountId, name) {
    if (!confirm(t('confirm_disconnect', { name }))) return
    try {
      await disconnectMetaAccount(accountId)
      toast.success(t('disconnected'))
      loadAccounts()
    } catch {
      toast.error(t('failed_disconnect'))
    }
  }

  if (!client) return <p className="text-sm text-gray-500">{t('loading')}</p>

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">{client.name}</h1>
        <p className="text-sm text-gray-500">{client.email}{client.industry ? ` · ${client.industry}` : ''}</p>
      </div>

      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-lg font-semibold text-gray-800">{t('meta_ad_accounts')}</h2>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setShowManual(!showManual)}
              className="text-sm font-medium px-3 py-2 rounded-lg border border-gray-300 hover:bg-gray-50 transition-colors"
            >
              {t('manual_token')}
            </button>
            <button
              onClick={handleConnectWithMeta}
              disabled={oauthLoading}
              className="flex items-center gap-2 bg-[#1877F2] text-white text-sm font-medium px-4 py-2 rounded-lg hover:bg-[#166fe5] disabled:opacity-50 transition-colors"
            >
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
              </svg>
              {oauthLoading ? t('connecting') : t('connect_with_meta')}
            </button>
          </div>
        </div>

        {showManual && (
          <form onSubmit={handleManualConnect} className="bg-white border border-gray-200 rounded-xl p-5 space-y-4 mb-4">
            <h3 className="font-semibold text-gray-800 text-sm">{t('connect_meta_account')}</h3>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">{t('account_id')}</label>
                <input required value={form.accountId} onChange={(e) => setForm({ ...form, accountId: e.target.value })} placeholder={t('account_id_placeholder')} className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 mb-1">{t('access_token')}</label>
                <input required value={form.accessToken} onChange={(e) => setForm({ ...form, accessToken: e.target.value })} placeholder={t('access_token_placeholder')} className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            </div>
            <div className="flex gap-2">
              <button type="submit" disabled={manualLoading} className="bg-blue-600 text-white text-sm px-4 py-1.5 rounded-lg hover:bg-blue-700 disabled:opacity-50">
                {manualLoading ? t('connecting') : t('connect')}
              </button>
              <button type="button" onClick={() => setShowManual(false)} className="text-sm px-4 py-1.5 rounded-lg border border-gray-300 hover:bg-gray-50">
                {t('cancel')}
              </button>
            </div>
          </form>
        )}

        <div className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-100">
          {accounts.length === 0 && <p className="text-sm text-gray-500 px-5 py-4">{t('no_accounts_connected')}</p>}
          {accounts.map((a) => (
            <div key={a.id} className="flex items-center justify-between px-5 py-3">
              <div>
                <p className="text-sm font-medium text-gray-800">{a.accountName}</p>
                <p className="text-xs text-gray-500">ID: {a.accountId} · {a.currency}</p>
              </div>
              <button onClick={() => handleDisconnect(a.id, a.accountName)} className="text-xs text-red-500 hover:text-red-700">
                {t('disconnect')}
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
