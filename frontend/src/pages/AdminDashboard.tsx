import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { api } from '../api';
import type { Service, Appointment } from '../api';
import { AppointmentCard } from '../components/AppointmentCard';
import { ServiceCard } from '../components/ServiceCard';

export function AdminDashboard() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [services, setServices] = useState<Service[]>([]);
  const [showServiceForm, setShowServiceForm] = useState(false);
  const [serviceName, setServiceName] = useState('');
  const [duration, setDuration] = useState('');
  const [price, setPrice] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [appointmentsData, servicesData] = await Promise.all([
        api.getAllAppointments(),
        api.getServices(),
      ]);
      setAppointments(appointmentsData);
      setServices(servicesData);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (id: number, status: number) => {
    try {
      await api.updateAppointmentStatus(id, status);
      loadData();
    } catch (error) {
      console.error('Failed to update appointment:', error);
      alert('Failed to update appointment');
    }
  };

  const handleDeleteService = async (id: number) => {
    if (!confirm('Are you sure you want to delete this service?')) return;
    try {
      await api.deleteService(id);
      loadData();
    } catch (error) {
      console.error('Failed to delete service:', error);
      alert('Failed to delete service');
    }
  };

  const handleCreateService = async (e: FormEvent) => {
    e.preventDefault();
    try {
      await api.createService({
        name: serviceName,
        durationMinutes: parseInt(duration),
        price: parseFloat(price),
      });
      setServiceName('');
      setDuration('');
      setPrice('');
      setShowServiceForm(false);
      loadData();
    } catch (error) {
      console.error('Failed to create service:', error);
      alert('Failed to create service');
    }
  };

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="page">
      <h1>Admin Dashboard</h1>

      <section className="admin-section">
        <div className="section-header">
          <h2>Services Management</h2>
          <button onClick={() => setShowServiceForm(!showServiceForm)} className="btn-primary">
            {showServiceForm ? 'Cancel' : 'Add Service'}
          </button>
        </div>

        {showServiceForm && (
          <form className="service-form" onSubmit={handleCreateService}>
            <div className="form-row">
              <div className="form-group">
                <label htmlFor="serviceName">Service Name</label>
                <input
                  id="serviceName"
                  type="text"
                  value={serviceName}
                  onChange={(e) => setServiceName(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="duration">Duration (minutes)</label>
                <input
                  id="duration"
                  type="number"
                  value={duration}
                  onChange={(e) => setDuration(e.target.value)}
                  required
                />
              </div>
              <div className="form-group">
                <label htmlFor="price">Price</label>
                <input
                  id="price"
                  type="number"
                  step="0.01"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  required
                />
              </div>
            </div>
            <button type="submit" className="btn-primary">
              Create Service
            </button>
          </form>
        )}

        <div className="services-grid">
          {services.map((service) => (
            <ServiceCard
              key={service.id}
              service={service}
              isAdmin
              onDelete={handleDeleteService}
            />
          ))}
        </div>
      </section>

      <section className="admin-section">
        <h2>All Appointments</h2>
        {appointments.length === 0 ? (
          <div className="empty-state">
            <p>No appointments yet.</p>
          </div>
        ) : (
          <div className="appointments-grid">
            {appointments.map((appointment) => (
              <AppointmentCard
                key={appointment.id}
                appointment={appointment}
                onUpdateStatus={handleUpdateStatus}
                isAdmin
              />
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
