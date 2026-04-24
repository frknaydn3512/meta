import useLangStore from '../store/langStore'

export default function useT() {
  const dict = useLangStore((s) => s.dict)
  return (key, vars) => {
    let str = dict[key] ?? key
    if (vars) Object.entries(vars).forEach(([k, v]) => { str = str.replace(`{${k}}`, v) })
    return str
  }
}
