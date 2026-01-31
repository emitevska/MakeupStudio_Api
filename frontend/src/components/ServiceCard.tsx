import type { Service } from '../api';

interface ServiceCardProps {
  service: Service;
  selected?: boolean;
  onSelect?: (service: Service) => void;
  onDelete?: (id: number) => void;
  isAdmin?: boolean;
}

export function ServiceCard({ service, selected, onSelect, onDelete, isAdmin }: ServiceCardProps) {
  return (
    <div className={`service-card ${selected ? 'selected' : ''}`}>
      <h3>{service.name}</h3>
      <div className="service-details">
        <p className="duration">{service.durationMinutes} minutes</p>
        <p className="price">${service.price}</p>
      </div>
      <div className="service-actions">
        {onSelect && (
          <button
            onClick={() => onSelect(service)}
            className={selected ? 'btn-secondary' : 'btn-primary'}
          >
            {selected ? 'Selected' : 'Select'}
          </button>
        )}
        {isAdmin && onDelete && (
          <button onClick={() => onDelete(service.id)} className="btn-danger">
            Delete
          </button>
        )}
      </div>
    </div>
  );
}
