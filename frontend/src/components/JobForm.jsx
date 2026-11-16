import { useState } from 'react'
import { STATUSES } from '../statuses.js'

// Today's date as "2026-11-16", the format <input type="date"> and the API both use.
function today() {
  return new Date().toLocaleDateString('en-CA')
}

// The pop-up form used for both "Add application" and "Edit".
export default function JobForm({ job, onSave, onCancel }) {
  const isEditing = Boolean(job.id)

  const [fields, setFields] = useState({
    company: job.company ?? '',
    role: job.role ?? '',
    location: job.location ?? '',
    jobUrl: job.jobUrl ?? '',
    status: job.status ?? 'Applied',
    appliedOn: job.appliedOn ?? today(),
    notes: job.notes ?? '',
  })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  // One change handler for every input: the input's "name" says which field to update.
  function handleChange(event) {
    setFields({ ...fields, [event.target.name]: event.target.value })
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setBusy(true)
    try {
      await onSave({
        ...fields,
        // Send empty optional boxes as null instead of "".
        location: fields.location || null,
        jobUrl: fields.jobUrl || null,
        notes: fields.notes || null,
      })
    } catch (err) {
      setError(err.message)
      setBusy(false)
    }
  }

  return (
    <div className="modal-backdrop" onClick={onCancel}>
      {/* stopPropagation: clicking inside the form must not close it */}
      <form className="modal" onSubmit={handleSubmit} onClick={(e) => e.stopPropagation()}>
        <h2>{isEditing ? 'Edit application' : 'Add application'}</h2>

        {error && <p className="error">{error}</p>}

        <label>
          Company *
          <input name="company" value={fields.company} onChange={handleChange} required autoFocus />
        </label>
        <label>
          Role *
          <input name="role" value={fields.role} onChange={handleChange} required />
        </label>
        <div className="row">
          <label>
            Location
            <input name="location" value={fields.location} onChange={handleChange} />
          </label>
          <label>
            Applied on *
            <input type="date" name="appliedOn" value={fields.appliedOn} onChange={handleChange} required />
          </label>
        </div>
        <div className="row">
          <label>
            Status
            <select name="status" value={fields.status} onChange={handleChange}>
              {STATUSES.map((status) => <option key={status}>{status}</option>)}
            </select>
          </label>
          <label>
            Job link
            <input type="url" name="jobUrl" value={fields.jobUrl} onChange={handleChange} placeholder="https://" />
          </label>
        </div>
        <label>
          Notes
          <textarea name="notes" rows={3} value={fields.notes} onChange={handleChange} />
        </label>

        <div className="modal-actions">
          <button type="button" onClick={onCancel}>Cancel</button>
          <button className="primary" disabled={busy}>{busy ? 'Saving…' : 'Save'}</button>
        </div>
      </form>
    </div>
  )
}
