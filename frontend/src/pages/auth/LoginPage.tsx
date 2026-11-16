import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import api from '../../api/client';
import { Building2, Lock, Mail, AlertCircle, ArrowRight, CheckCircle2 } from 'lucide-react';

export const LoginPage: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const res = await api.post('/auth/login', { email, password });
      const data = res.data;

      login(data.token, {
        id: data.usuarioId,
        nombreCompleto: data.nombreCompleto,
        email: data.email,
        rol: data.rol,
        estudioId: data.estudioId,
        nombreEstudio: data.nombreEstudio,
        clienteId: data.clienteId,
      });

      if (data.rol === 'ClientePortal') {
        navigate('/portal');
      } else {
        navigate('/dashboard');
      }
    } catch (err: any) {
      if (!err.response) {
        setError('No se pudo conectar con el backend (Error de red). Verifique que la WebApi est� corriendo.');
      } else {
        setError(err.response?.data?.message || 'Error al iniciar sesi�n. Verifique sus credenciales.');
      }
    } finally {
      setLoading(false);
    }
  };

  const handleQuickLogin = (demoEmail: string, demoPass: string) => {
    setEmail(demoEmail);
    setPassword(demoPass);
  };

  return (
    <div className="login-wrapper">
      <div className="login-card">
        <div className="login-header">
          <div className="login-logo">
            <Building2 size={32} />
          </div>
          <h1>Estudio Contable SaaS</h1>
          <p>Control de vencimientos impositivos, tareas y portal de clientes para Argentina</p>
        </div>

        {error && (
          <div className="alert alert-error">
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label>Correo Electrónico</label>
            <div className="input-with-icon">
              <Mail size={18} />
              <input
                type="email"
                required
                placeholder="ejemplo@estudioperez.com.ar"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Contraseña</label>
            <div className="input-with-icon">
              <Lock size={18} />
              <input
                type="password"
                required
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </div>
          </div>

          <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
            {loading ? 'Iniciando sesión...' : 'Ingresar al Sistema'}
            <ArrowRight size={16} />
          </button>
        </form>

        <div className="demo-users-box">
          <div className="demo-header">
            <CheckCircle2 size={16} />
            <span>Acceso rápido de demostración:</span>
          </div>
          <div className="demo-buttons">
            <button
              type="button"
              className="btn-demo"
              onClick={() => handleQuickLogin('admin@estudioperez.com.ar', 'Admin123!')}
            >
              <strong>Admin Estudio</strong>
              <small>admin@estudioperez.com.ar</small>
            </button>

            <button
              type="button"
              className="btn-demo"
              onClick={() => handleQuickLogin('lucia@estudioperez.com.ar', 'Colab123!')}
            >
              <strong>Colaborador</strong>
              <small>lucia@estudioperez.com.ar</small>
            </button>

            <button
              type="button"
              className="btn-demo"
              onClick={() => handleQuickLogin('cliente@distribuidoracentro.com.ar', 'Cliente123!')}
            >
              <strong>Cliente (Portal)</strong>
              <small>cliente@distribuidoracentro.com.ar</small>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

