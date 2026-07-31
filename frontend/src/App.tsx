import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ActivateAccountPage } from './pages/ActivateAccountPage';
import { FirstLoginPage } from './pages/FirstLoginPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
import { AdministracionPage } from './pages/AdministracionPage';
import { AdminRolesPage } from './pages/admin/AdminRolesPage';
import { AppLayout } from './components/layout/AppLayout';
import { SolicitudesPage } from './pages/SolicitudesPage';
import { ConsultasPage } from './pages/ConsultasPage';
import { TramitesPage } from './pages/TramitesPage';
import { CulturePage } from './pages/CulturePage';
import { useAuthStore } from './store/authStore';
import { LeaderPage } from './pages/LeaderPage';
import { MiEquipoPage } from './pages/MiEquipoPage';
import { MyDataPage } from './pages/MyDataPage';
import { LoginPage } from './pages/LoginPage';
import { useUIStore } from './store/uiStore';
import { ChatPage } from './pages/ChatPage';
import './styles/globals.css';

export default function App() {
  const { isAuthenticated, user } = useAuthStore();
  const { activeSection } = useUIStore();

  const isAdmin      = user?.rolId === '1';
  const isSupervisor = user?.rolId === '2';
  const isEmployee   = user?.rolId === '3';

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
                  {activeSection === 'mydata'      && <MyDataPage />}
                  {activeSection === 'culture'     && <CulturePage />}
                  {activeSection === 'consultas'   && <ConsultasPage />}
                  {activeSection === 'tramites'    && <TramitesPage />}
                  {activeSection === 'solicitudes' && !isEmployee && <SolicitudesPage />}
                  {activeSection === 'leader'          && (isAdmin ? <LeaderPage /> : <Navigate to="/" replace />)}
                  {activeSection === 'miequipo'        && (isSupervisor || isAdmin ? <MiEquipoPage /> : <Navigate to="/" replace />)}
                  {activeSection === 'administracion' && (isAdmin ? <AdministracionPage /> : <Navigate to="/" replace />)}
                  {activeSection === 'soporte'     && !isAdmin && <ChatPage />}
                </AppLayout>
              }
            />
          </>
        ) : (
          <Route path="*" element={<Navigate to="/login" replace />} />
        )}

        <Route
          path="/"
          element={<Navigate to={isAuthenticated ? '/mydata' : '/login'} replace />}
        />
      </Routes>
    </Router>
  );
}
