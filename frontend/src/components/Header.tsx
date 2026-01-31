import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../AuthContext';

export function Header() {
  const { isAuthenticated, userEmail, isAdmin, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <header className="header">
      <div className="container">
        <Link to="/" className="logo">
          Makeup Studio
        </Link>
        <nav className="nav">
          <Link to="/services">Services</Link>
          {isAuthenticated ? (
            <>
              <Link to="/book">Book Appointment</Link>
              <Link to="/dashboard">My Appointments</Link>
              {isAdmin && <Link to="/admin">Admin Panel</Link>}
              <div className="user-menu">
                <span className="user-email">{userEmail}</span>
                <button onClick={handleLogout} className="btn-secondary">
                  Logout
                </button>
              </div>
            </>
          ) : (
            <>
              <Link to="/login">Login</Link>
              <Link to="/register" className="btn-primary">
                Register
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  );
}
