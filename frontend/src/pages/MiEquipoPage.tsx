import React, { useEffect, useState } from 'react';
import api from '../lib/api';
import type { ApiResponse } from '../services/authService';
import type { UserProfile } from '../store/userProfileStore';
import { EmployeeProfileModal } from '../components/ui/EmployeeProfileModal';
import './LeaderPage.css';

const PAGE_SIZE = 50;

export const MiEquipoPage: React.FC = () => {
  const [teamUsers, setTeamUsers] = useState<UserProfile[]>([]);
  const [teamLoading, setTeamLoading] = useState(false);
  const [teamError, setTeamError] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCi, setSelectedCi] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setTeamLoading(true);
      setTeamError('');
      try {
        const res = await api.get<ApiResponse<UserProfile[]>>('/users/mi-equipo');
        if (!cancelled) {
          setTeamUsers(res.data.data ?? []);
        }
      } catch (err: unknown) {
        if (!cancelled) {
          const axiosError = err as { response?: { data?: { message?: string } } };
          setTeamError(axiosError?.response?.data?.message ?? 'Error al cargar los usuarios.');
        }
      } finally {
        if (!cancelled) setTeamLoading(false);
      }
    };
    load();
    return () => { cancelled = true; };
  }, []);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchQuery]);

  const filteredUsers = teamUsers.filter((u) => {
    const q = searchQuery.trim().toLowerCase();
    if (!q) return true;
    const fullName = `${u.nombres ?? ''} ${u.apellidos ?? ''}`.toLowerCase();
    const ci = (u.ci ?? '').toLowerCase();
    return fullName.includes(q) || ci.includes(q);
  });

  const totalPages = Math.max(1, Math.ceil(filteredUsers.length / PAGE_SIZE));
  const pagedUsers = filteredUsers.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);
  const hasNextPage = currentPage < totalPages;

  return (
    <>
      <div className="leader-page">
        <div className="leader-header">
          <div>
            <h1>Mi Equipo</h1>
            <p>Consulta la información de los usuarios de la empresa</p>
          </div>
        </div>

        <div className="tab-content">
          <div>
            <div className="team-search-bar">
              <input
                type="text"
                className="team-search-input"
                placeholder="Buscar por nombre o cédula..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>

            <div className="team-section">
              <table className="team-table">
                <thead>
                  <tr>
                    <th>Nombre y Apellido</th>
                    <th>CI</th>
                    <th>Departamento</th>
                    <th>Cargo</th>
                    <th>Acción</th>
                  </tr>
                </thead>
                <tbody>
                  {teamLoading && (
                    <tr>
                      <td colSpan={5} className="team-empty">Cargando usuarios...</td>
                    </tr>
                  )}
                  {!teamLoading && teamError && (
                    <tr>
                      <td colSpan={5} className="team-error">{teamError}</td>
                    </tr>
                  )}
                  {!teamLoading && !teamError && filteredUsers.length === 0 && (
                    <tr>
                      <td colSpan={5} className="team-empty">
                        {searchQuery.trim() ? 'No hay resultados para la búsqueda.' : 'No se encontraron usuarios.'}
                      </td>
                    </tr>
                  )}
                  {!teamLoading && !teamError && pagedUsers.map((user, i) => (
                    <tr key={user.ci ?? i}>
                      <td>
                        <strong>
                          {[user.nombres, user.apellidos].filter(Boolean).join(' ') || '—'}
                        </strong>
                      </td>
                      <td>{user.ci ?? '—'}</td>
                      <td>{user.desDepart ?? '—'}</td>
                      <td>{user.desCargo ?? '—'}</td>
                      <td>
                        <button
                          className="action-link-btn"
                          onClick={() => setSelectedCi(user.ci!)}
                        >
                          Ver perfil →
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>

              <div className="team-pagination">
                <button
                  disabled={currentPage === 1}
                  onClick={() => setCurrentPage((p) => p - 1)}
                >
                  ← Anterior
                </button>
                <span>Página {currentPage} de {totalPages}</span>
                <button
                  disabled={!hasNextPage}
                  onClick={() => setCurrentPage((p) => p + 1)}
                >
                  Siguiente →
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {selectedCi && (
        <EmployeeProfileModal ci={selectedCi} onClose={() => setSelectedCi(null)} />
      )}
    </>
  );
};
