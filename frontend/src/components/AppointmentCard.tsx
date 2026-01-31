import { format } from 'date-fns';
import type { Appointment } from '../api';

interface AppointmentCardProps {
  appointment: Appointment;
  onCancel?: (id: number) => void;
  onUpdateStatus?: (id: number, status: number) => void;
  isAdmin?: boolean;
}

const statusNames = ['Pending', 'Confirmed', 'Cancelled', 'Completed'];
const statusColors = ['#ff9800', '#4caf50', '#f44336', '#2196f3'];

export function AppointmentCard({ appointment, onCancel, onUpdateStatus, isAdmin }: AppointmentCardProps) {
  const date = new Date(appointment.appointmentDate);
  const total = appointment.services.reduce((sum, s) => sum + s.price, 0);

  return (
    <div className="appointment-card">
      <div className="appointment-header">
        <h3>{format(date, 'PPP')}</h3>
        <span
          className="status-badge"
          style={{ backgroundColor: statusColors[appointment.status] }}
        >
          {statusNames[appointment.status]}
        </span>
      </div>
      <p className="appointment-time">{format(date, 'p')}</p>
      <div className="appointment-details">
        <p><strong>Client:</strong> {appointment.clientName}</p>
        <p><strong>Phone:</strong> {appointment.phoneNumber}</p>
        <p><strong>Email:</strong> {appointment.email}</p>
      </div>
      <div className="appointment-services">
        <h4>Services:</h4>
        {appointment.services.map((service) => (
          <div key={service.id} className="service-item">
            <span>{service.name}</span>
            <span>${service.price}</span>
          </div>
        ))}
        <div className="total">
          <strong>Total:</strong>
          <strong>${total}</strong>
        </div>
      </div>
      <div className="appointment-actions">
        {isAdmin && onUpdateStatus && appointment.status === 0 && (
          <>
            <button onClick={() => onUpdateStatus(appointment.id, 1)} className="btn-primary">
              Confirm
            </button>
            <button onClick={() => onUpdateStatus(appointment.id, 2)} className="btn-danger">
              Cancel
            </button>
          </>
        )}
        {isAdmin && onUpdateStatus && appointment.status === 1 && (
          <button onClick={() => onUpdateStatus(appointment.id, 3)} className="btn-primary">
            Mark Completed
          </button>
        )}
        {!isAdmin && onCancel && appointment.status !== 2 && appointment.status !== 3 && (
          <button onClick={() => onCancel(appointment.id)} className="btn-danger">
            Cancel Appointment
          </button>
        )}
      </div>
    </div>
  );
}
