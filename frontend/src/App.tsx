import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ActivateAccountPage } from './pages/ActivateAccountPage';
import { FirstLoginPage } from './pages/FirstLoginPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
import { AdminRolesPage } from './pages/admin/AdminRolesPage';
import { AppLayout } from './components/layout/AppLayout';
import { SolicitudesPage } from './pages/SolicitudesPage';
import { OnboardingPage } from './pages/OnboardingPage';
import { ConsultasPage } from './pages/ConsultasPage';
import { TramitesPage } from './pages/TramitesPage';
import { CulturePage } from './pages/CulturePage';
import { useAuthStore } from './store/authStore';
import { LeaderPage } from './pages/LeaderPage';
import { MyDataPage } from './pages/MyDataPage';
import { LoginPage } from './pages/LoginPage';
import { useUIStore } from './store/uiStore';
import { HomePage } from './pages/HomePage';
import { ChatPage } from './pages/ChatPage';
import './styles/globals.css';

export default function App() {
  const { isAuthenticated } = useAuthStore();
  const { activeSection } = useUIStore();

  return (
    <Router>
      <Routes>
        {/* Rutas públicas */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/first-login" element={<FirstLoginPage />} />
        <Route path="/activate" element={<ActivateAccountPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />

        {/* Rutas protegidas */}
        {isAuthenticated ? (
          <>
            <Route path="/admin/roles" element={<AdminRolesPage />} />
            <Route
              path="*"
              element={
                <AppLayout>
                  {activeSection === 'home' && <HomePage />}
                  {activeSection === 'mydata' && <MyDataPage />}
                  {activeSection === 'culture' && <CulturePage />}
                  {activeSection === 'consultas' && <ConsultasPage />}
                  {activeSection === 'solicitudes' && <SolicitudesPage />}
                  {activeSection === 'tramites' && <TramitesPage />}
                  {activeSection === 'leader' && <LeaderPage />}
                  {activeSection === 'chatbot' && <ChatPage />}
                  {activeSection === 'onboarding' && <OnboardingPage />}
                </AppLayout>
              }
            />
          </>
        ) : (
          <Route path="*" element={<Navigate to="/login" replace />} />
        )}

        <Route
          path="/"
          element={<Navigate to={isAuthenticated ? '/home' : '/login'} replace />}
        />
      </Routes>
    </Router>
  );
}
