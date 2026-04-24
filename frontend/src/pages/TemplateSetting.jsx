import { useEffect, useState, useRef } from 'react'
import { getTemplate, updateTemplate, uploadLogo } from '../api/template'
import useT from '../hooks/useT'
import toast from 'react-hot-toast'

export default function TemplateSetting() {
  const [form, setForm] = useState({
    primaryColor: '#1a56db',
    secondaryColor: '#f3f4f6',
    agencyDisplayName: '',
    agencyEmail: '',
    agencyPhone: '',
    agencyWebsite: '',
  })
  const [logoUrl, setLogoUrl] = useState(null)
  const [saving, setSaving] = useState(false)
  const [uploading, setUploading] = useState(false)
  const fileRef = useRef()
  const t = useT()

  useEffect(() => {
    getTemplate().then(({ data }) => {
      const tmpl = data.data
      setLogoUrl(tmpl.logoUrl)
      setForm({
        primaryColor: tmpl.primaryColor,
        secondaryColor: tmpl.secondaryColor,
        agencyDisplayName: tmpl.agencyDisplayName,
        agencyEmail: tmpl.agencyEmail ?? '',
        agencyPhone: tmpl.agencyPhone ?? '',
        agencyWebsite: tmpl.agencyWebsite ?? '',
      })
    })
  }, [])

  async function handleSave(e) {
    e.preventDefault()
    setSaving(true)
    try {
      await updateTemplate(form)
      toast.success(t('template_saved'))
    } catch {
      toast.error(t('failed_save'))
    } finally {
      setSaving(false)
    }
  }

  async function handleLogoUpload(e) {
    const file = e.target.files?.[0]
    if (!file) return
    setUploading(true)
    try {
      const { data } = await uploadLogo(file)
      setLogoUrl(data.data)
      toast.success(t('logo_uploaded'))
    } catch (err) {
      toast.error(err.response?.data?.errors?.[0] || t('upload_failed'))
    } finally {
      setUploading(false)
    }
  }

  return (
    <div className="space-y-6 max-w-2xl">
      <h1 className="text-2xl font-bold text-gray-900">{t('whitelabel_settings')}</h1>

      <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
        <h2 className="font-semibold text-gray-800">{t('agency_logo')}</h2>
        {logoUrl && <img src={logoUrl} alt="Logo" className="h-14 object-contain rounded border border-gray-200 p-1" />}
        <div>
          <input ref={fileRef} type="file" accept=".jpg,.jpeg,.png,.svg,.webp" onChange={handleLogoUpload} className="hidden" />
          <button onClick={() => fileRef.current.click()} disabled={uploading} className="border border-gray-300 text-sm px-4 py-1.5 rounded-lg hover:bg-gray-50 disabled:opacity-50">
            {uploading ? t('uploading') : logoUrl ? t('replace_logo') : t('upload_logo')}
          </button>
          <p className="text-xs text-gray-500 mt-1">{t('logo_hint')}</p>
        </div>
      </div>

      <form onSubmit={handleSave} className="bg-white rounded-xl border border-gray-200 p-6 space-y-5">
        <h2 className="font-semibold text-gray-800">{t('branding_contact')}</h2>

        <div className="grid grid-cols-2 gap-4">
          <Field label={t('agency_display_name')} value={form.agencyDisplayName} onChange={(v) => setForm({ ...form, agencyDisplayName: v })} />
          <Field label={t('contact_email')} type="email" value={form.agencyEmail} onChange={(v) => setForm({ ...form, agencyEmail: v })} />
          <Field label={t('phone')} value={form.agencyPhone} onChange={(v) => setForm({ ...form, agencyPhone: v })} />
          <Field label={t('website')} value={form.agencyWebsite} onChange={(v) => setForm({ ...form, agencyWebsite: v })} />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <ColorField label={t('primary_color')} value={form.primaryColor} onChange={(v) => setForm({ ...form, primaryColor: v })} />
          <ColorField label={t('secondary_color')} value={form.secondaryColor} onChange={(v) => setForm({ ...form, secondaryColor: v })} />
        </div>

        <div className="rounded-lg p-4 text-white text-sm font-medium" style={{ background: form.primaryColor }}>
          {t('preview')} — {form.agencyDisplayName || t('your_agency')}
        </div>

        <button type="submit" disabled={saving} className="bg-blue-600 text-white text-sm font-medium px-5 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition-colors">
          {saving ? t('saving') : t('save_changes')}
        </button>
      </form>
    </div>
  )
}

function Field({ label, value, onChange, type = 'text' }) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <input type={type} value={value} onChange={(e) => onChange(e.target.value)} className="w-full border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
    </div>
  )
}

function ColorField({ label, value, onChange }) {
  return (
    <div>
      <label className="block text-xs font-medium text-gray-600 mb-1">{label}</label>
      <div className="flex gap-2 items-center">
        <input type="color" value={value} onChange={(e) => onChange(e.target.value)} className="h-8 w-10 rounded border border-gray-300 cursor-pointer" />
        <input type="text" value={value} onChange={(e) => onChange(e.target.value)} className="flex-1 border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </div>
    </div>
  )
}
