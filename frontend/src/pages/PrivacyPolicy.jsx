import { Link } from 'react-router-dom'

export default function PrivacyPolicy() {
  return (
    <div className="min-h-screen bg-gray-50 py-12 px-4">
      <div className="max-w-2xl mx-auto bg-white rounded-xl border border-gray-200 shadow-sm p-8 space-y-6">
        <div>
          <Link to="/login" className="text-sm text-blue-600 hover:underline">← Back</Link>
          <h1 className="text-2xl font-bold text-gray-900 mt-3">Privacy Policy</h1>
          <p className="text-sm text-gray-500 mt-1">Last updated: April 2026</p>
        </div>

        <section className="space-y-2">
          <h2 className="text-base font-semibold text-gray-800">1. What We Collect</h2>
          <p className="text-sm text-gray-600 leading-relaxed">
            We collect your agency name, email address, and the Meta ad account access tokens you
            connect through our OAuth integration. We do not collect or store your Meta Business
            password.
          </p>
        </section>

        <section className="space-y-2">
          <h2 className="text-base font-semibold text-gray-800">2. How We Use Your Data</h2>
          <p className="text-sm text-gray-600 leading-relaxed">
            Access tokens are used exclusively to fetch read-only advertising performance data from
            Meta's Marketing API on your behalf, for the purpose of generating PDF reports. We do
            not modify, pause, or delete any of your Meta campaigns.
          </p>
        </section>

        <section className="space-y-2">
          <h2 className="text-base font-semibold text-gray-800">3. Data Storage &amp; Security</h2>
          <p className="text-sm text-gray-600 leading-relaxed">
            All Meta access tokens are encrypted at rest using AES-256 before being stored in our
            database. Tokens are never returned to the browser or exposed in API responses.
          </p>
        </section>

        <section className="space-y-2">
          <h2 className="text-base font-semibold text-gray-800">4. Meta API Permissions</h2>
          <p className="text-sm text-gray-600 leading-relaxed">
            Our integration requests read-only Meta permissions (<code className="text-xs bg-gray-100 px-1 py-0.5 rounded">ads_read</code>,{' '}
            <code className="text-xs bg-gray-100 px-1 py-0.5 rounded">ads_management</code>). You can revoke access at any time
            from your Meta Business Settings under <em>Apps &amp; Websites</em>.
          </p>
        </section>

        <section className="space-y-2">
          <h2 className="text-base font-semibold text-gray-800">5. Data Sharing</h2>
          <p className="text-sm text-gray-600 leading-relaxed">
            We do not sell, rent, or share your data with third parties. Report data is only
            accessible to you (the agency) and the client you choose to share the report link with.
          </p>
        </section>

        <section className="space-y-2">
          <h2 className="text-base font-semibold text-gray-800">6. Data Deletion</h2>
          <p className="text-sm text-gray-600 leading-relaxed">
            You can disconnect any Meta account at any time from the client detail page, which
            permanently removes the stored access token. To delete your agency account entirely,
            contact us at{' '}
            <a href="mailto:support@adreport.app" className="text-blue-600 hover:underline">
              support@adreport.app
            </a>
            .
          </p>
        </section>

        <section className="space-y-2">
          <h2 className="text-base font-semibold text-gray-800">7. Contact</h2>
          <p className="text-sm text-gray-600 leading-relaxed">
            For any questions about this policy, reach us at{' '}
            <a href="mailto:support@adreport.app" className="text-blue-600 hover:underline">
              support@adreport.app
            </a>
            .
          </p>
        </section>
      </div>
    </div>
  )
}
