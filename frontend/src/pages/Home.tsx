import { Link } from 'react-router-dom';
import { useAuth } from '../AuthContext';

export function Home() {
  const { isAuthenticated } = useAuth();

  return (
    <div className="home">
      <section className="hero">
        <h1>Welcome to Makeup Studio</h1>
        <p className="hero-subtitle">Professional makeup services for every occasion</p>
        <div className="hero-actions">
          <Link to="/services" className="btn-primary btn-lg">
            View Services
          </Link>
          {isAuthenticated ? (
            <Link to="/book" className="btn-secondary btn-lg">
              Book Appointment
            </Link>
          ) : (
            <Link to="/register" className="btn-secondary btn-lg">
              Get Started
            </Link>
          )}
        </div>
      </section>

      <section className="features">
        <div className="feature">
          <div className="feature-icon">💄</div>
          <h3>Professional Services</h3>
          <p>Expert makeup artists with years of experience</p>
        </div>
        <div className="feature">
          <div className="feature-icon">📅</div>
          <h3>Easy Booking</h3>
          <p>Book your appointment online in minutes</p>
        </div>
        <div className="feature">
          <div className="feature-icon">✨</div>
          <h3>Premium Quality</h3>
          <p>High-end products and techniques</p>
        </div>
      </section>
    </div>
  );
}
