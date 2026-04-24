import { create } from 'zustand'
import en from '../i18n/en'
import tr from '../i18n/tr'

const saved = localStorage.getItem('lang') || 'en'

const useLangStore = create((set) => ({
  lang: saved,
  dict: saved === 'tr' ? tr : en,
  toggle() {
    set((s) => {
      const next = s.lang === 'en' ? 'tr' : 'en'
      localStorage.setItem('lang', next)
      return { lang: next, dict: next === 'tr' ? tr : en }
    })
  },
}))

export default useLangStore
