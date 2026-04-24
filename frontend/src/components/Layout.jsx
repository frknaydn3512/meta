import { NavLink, useNavigate } from 'react-router-dom'
import useAuthStore from '../store/authStore'
import useLangStore from '../store/langStore'
import useT from '../hooks/useT'

export default function Layout({ children }) {
  const logout = useAuthStore((s) => s.logout)
  const navigate = useNavigate()
  const { lang, toggle } = useLangStore()
  const t = useT()

  const navItems = [
    { to: '/dashboard', label: t('nav_dashboard') },
    { to: '/clients', label: t('nav_clients') },
    { to: '/reports', label: t('nav_reports') },
    { to: '/settings/template', label: t('nav_whitelabel') },
  ]

  function handleLogout() {
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen flex">
      {/* Sidebar */}
      <aside className="w-56 bg-white border-r border-gray-200 flex flex-col">
        <div className="px-6 py-5 border-b border-gray-200 flex items-center justify-between">
          <span className="text-lg font-bold text-blue-600">AdReport</span>
          <button
            onClick={toggle}
            className="text-xs font-medium text-gray-500 hover:text-gray-800 border border-gray-200 rounded px-1.5 py-0.5 transition-colors"
          >
            {lang === 'en' ? 'TR' : 'EN'}
          </button>
        </div>
        <nav className="flex-1 px-3 py-4 space-y-1">
          {navItems.map(({ to, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `block px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-blue-50 text-blue-700'
                    : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
                }`
              }
            >
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="px-3 py-4 border-t border-gray-200">
          <button
            onClick={handleLogout}
            className="w-full px-3 py-2 text-sm font-medium text-gray-600 hover:text-red-600 hover:bg-red-50 rounded-md transition-colors text-left"
          >
            {t('logout')}
          </button>
        </div>
      </aside>

      {/* Main */}
      <main className="flex-1 overflow-auto bg-gray-50">
        <div className="max-w-5xl mx-auto px-6 py-8">{children}</div>
      </main>
    </div>
  )
}
