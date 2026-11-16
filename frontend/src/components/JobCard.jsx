import { STATUSES } from '../statuses.js'

// One card on the board. It receives its data and actions from BoardPage through props.
export default function JobCard({ job, onEdit, onDelete, onMove }) {
  // Adding T00:00 makes the browser read the date in local time, not UTC.
  const appliedOn = new Date(job.appliedOn + 'T00:00').toLocaleDateString(undefined, {
    day: 'numeric', month: 'short', year: 'numeric',
  })

  return (
    <article
      className="card"
      draggable
      onDragStart={(e) => e.dataTransfer.setData('text/plain', String(job.id))}
    >
      <h3>{job.role}</h3>
      <p className="company">{job.company}</p>
      <p className="meta">
        {job.location && <span>{job.location} · </span>}
        Applied {appliedOn}
      </p>
      {job.notes && <p className="notes">{job.notes}</p>}

      <div className="card-actions">
        <select
          aria-label="Change status"
          value={job.status}
          onChange={(e) => onMove(e.target.value)}
        >
          {STATUSES.map((status) => <option key={status}>{status}</option>)}
        </select>
        {job.jobUrl && <a href={job.jobUrl} target="_blank" rel="noreferrer">Link</a>}
        <button className="link" onClick={onEdit}>Edit</button>
        <button className="link danger" onClick={onDelete}>Delete</button>
      </div>
    </article>
  )
}
