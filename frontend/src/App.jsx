import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { Toaster } from 'react-hot-toast'
import ProtectedRoute from './components/ProtectedRoute'
import Layout from './components/Layout'
import Login from './pages/Login'
import Register from './pages/Register'
import Dashboard from './pages/Dashboard'
import Clients from './pages/Clients'
import ClientDetail from './pages/ClientDetail'
import Reports from './pages/Reports'
import ReportDetail from './pages/ReportDetail'
import TemplateSetting from './pages/TemplateSetting'
import PublicReport from './pages/PublicReport'
import OAuthCallback from './pages/OAuthCallback'
import PrivacyPolicy from './pages/PrivacyPolicy'

function AppShell({ children }) {
  return (
    <ProtectedRoute>
      <Layout>{children}</Layout>
    </ProtectedRoute>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <Toaster position="top-right" toastOptions={{ duration: 3500 }} />
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/r/:slug" element={<PublicReport />} />
        <Route path="/oauth/callback" element={<OAuthCallback />} />
        <Route path="/privacy" element={<PrivacyPolicy />} />

        <Route path="/dashboard" element={<AppShell><Dashboard /></AppShell>} />
        <Route path="/clients" element={<AppShell><Clients /></AppShell>} />
        <Route path="/clients/:id" element={<AppShell><ClientDetail /></AppShell>} />
        <Route path="/reports" element={<AppShell><Reports /></AppShell>} />
        <Route path="/reports/:id" element={<AppShell><ReportDetail /></AppShell>} />
        <Route path="/settings/template" element={<AppShell><TemplateSetting /></AppShell>} />

        <Route path="/" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  )
}
