import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { useAuthStore } from './store/authStore';
import { useUIStore } from './store/uiStore';
import { AppLayout } from './components/layout/AppLayout';
import { LoginPage } from './pages/LoginPage';
import { ActivateAccountPage } from './pages/ActivateAccountPage';
import { HomePage } from './pages/HomePage';
import { MyDataPage } from './pages/MyDataPage';
import { CulturePage } from './pages/CulturePage';
import { ConsultasPage } from './pages/ConsultasPage';
import { SolicitudesPage } from './pages/SolicitudesPage';
import { TramitesPage } from './pages/TramitesPage';
import { LeaderPage } from './pages/LeaderPage';
import { ChatPage } from './pages/ChatPage';
import { OnboardingPage } from './pages/OnboardingPage';
import { AdminRolesPage } from './pages/admin/AdminRolesPage';
import './styles/globals.css';

export default function App() {
  const { isAuthenticated } = useAuthStore();
  const { activeSection } = useUIStore();

  return (
    <Router>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/activate" element={<ActivateAccountPage />} />

        {isAuthenticated && (
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
        )}

        <Route path="/" element={<Navigate to={isAuthenticated ? '/home' : '/login'} />} />
      </Routes>
    </Router>
  );
}
