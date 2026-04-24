import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { exchangeMetaOAuth, confirmMetaOAuth } from '../api/metaAccounts'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

export default function OAuthCallback() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const t = useT()

  const [step, setStep] = useState('exchanging') // exchanging | select | confirming | error
  const [accounts, setAccounts] = useState([])
  const [encryptedToken, setEncryptedToken] = useState('')
  const [clientId, setClientId] = useState(null)
  const [selectedAccountId, setSelectedAccountId] = useState('')

  useEffect(() => {
    const code = searchParams.get('code')
    const state = searchParams.get('state') // clientId encoded as state
    const error = searchParams.get('error')

    if (error) {
      setStep('error')
      return
    }

    if (!code || !state) {
      setStep('error')
      return
    }

    const parsedClientId = parseInt(state, 10)
    setClientId(parsedClientId)

    const redirectUri = window.location.origin + '/oauth/callback'

    exchangeMetaOAuth({ clientId: parsedClientId, code, redirectUri })
      .then(({ data }) => {
        setEncryptedToken(data.data.encryptedToken)
        setAccounts(data.data.adAccounts)
        setStep('select')
      })
      .catch(() => setStep('error'))
  }, [])

  async function handleConfirm() {
    if (!selectedAccountId) return
    setStep('confirming')
    try {
      await confirmMetaOAuth({ clientId, accountId: selectedAccountId, encryptedToken })
      toast.success(t('meta_connected'))
      navigate(`/clients/${clientId}`)
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('failed_connect'))
      setStep('select')
    }
  }

  if (step === 'exchanging') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
          <p className="text-sm text-gray-600">{t('oauth_exchanging')}</p>
        </div>
      </div>
    )
  }

  if (step === 'error') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="bg-white p-8 rounded-xl border border-gray-200 shadow-sm text-center max-w-sm w-full">
          <p className="text-sm font-medium text-red-600 mb-4">{t('oauth_error')}</p>
          <button
            onClick={() => navigate(-1)}
            className="text-sm text-blue-600 hover:underline"
          >
            {t('go_back')}
          </button>
        </div>
      </div>
    )
  }

  if (step === 'select' || step === 'confirming') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="bg-white p-8 rounded-xl border border-gray-200 shadow-sm w-full max-w-sm space-y-5">
          <h2 className="text-lg font-semibold text-gray-900">{t('oauth_select_account')}</h2>
          <p className="text-sm text-gray-500">{t('oauth_select_account_hint')}</p>

          <div className="space-y-2">
            {accounts.map((acc) => (
              <label
                key={acc.id}
                className={`flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                  selectedAccountId === acc.id
                    ? 'border-blue-500 bg-blue-50'
                    : 'border-gray-200 hover:bg-gray-50'
                }`}
              >
                <input
                  type="radio"
                  name="account"
                  value={acc.id}
                  checked={selectedAccountId === acc.id}
                  onChange={() => setSelectedAccountId(acc.id)}
                  className="accent-blue-600"
                />
                <div>
                  <p className="text-sm font-medium text-gray-800">{acc.name}</p>
                  <p className="text-xs text-gray-500">ID: {acc.id} · {acc.currency}</p>
                </div>
              </label>
            ))}
          </div>

          <button
            onClick={handleConfirm}
            disabled={!selectedAccountId || step === 'confirming'}
            className="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50 transition-colors"
          >
            {step === 'confirming' ? t('connecting') : t('connect')}
          </button>
        </div>
      </div>
    )
  }

  return null
}
