import { useEffect, useState, useMemo, useRef } from 'react'
import { api } from '../api.js'
import { useAuth } from '../AuthContext.jsx'
import JobCard from '../components/JobCard.jsx'
import JobForm from '../components/JobForm.jsx'
import { STATUSES } from '../statuses.js'

export default function BoardPage() {
  const { user, logout } = useAuth()

  const [jobs, setJobs] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('All')
  // null = form closed, {} = adding a new job, a job object = editing that job
  const [formJob, setFormJob] = useState(null)
  const [dragOverStatus, setDragOverStatus] = useState(null)
  const searchRef = useRef(null)

  // Runs once when the page opens: load this user's jobs from the API.
  useEffect(() => {
    api.getJobs()
      .then(setJobs)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])
// Runs once when the page opens: put the cursor in the search box.
  useEffect(() => {
    searchRef.current.focus()
  }, [])

  async function handleSave(fields) {
    if (formJob.id) {
      const updated = await api.updateJob(formJob.id, fields)
      setJobs(jobs.map((job) => (job.id === updated.id ? updated : job)))
    } else {
      const created = await api.createJob(fields)
      setJobs([created, ...jobs])
    }
    setFormJob(null)
  }

  async function handleMove(id, status) {
    const job = jobs.find((j) => j.id === id)
    if (!job || job.status === status) return
    try {
      const updated = await api.updateJobStatus(id, status)
      setJobs(jobs.map((j) => (j.id === id ? updated : j)))
    } catch (err) {
      setError(err.message)
    }
  }

  async function handleDelete(job) {
    if (!window.confirm(`Delete the application for ${job.role} at ${job.company}?`)) return
    try {
      await api.deleteJob(job.id)
      setJobs(jobs.filter((j) => j.id !== job.id))
    } catch (err) {
      setError(err.message)
    }
  }

  function handleDrop(event, status) {
    event.preventDefault()
    setDragOverStatus(null)
    const id = Number(event.dataTransfer.getData('text/plain'))
    handleMove(id, status)
  }

  // Search matches company, role or location.
  const term = search.trim().toLowerCase()


  const visibleJobs = useMemo(() => {
    return jobs.filter((job) => {
      const matchesSearch = [job.company, job.role, job.location ?? '']
        .some((text) => text.toLowerCase().includes(term))
      const matchesStatus = statusFilter === 'All' || job.status === statusFilter
      return matchesSearch && matchesStatus
    })

  }, [jobs, term, statusFilter])

  return (
    <div className="page">
      <header className="topbar">
        <h1>Job Tracker</h1>
        <div className="topbar-right">
          <span className="muted">Hi, {user.name}</span>
          <button onClick={logout}>Log out</button>
        </div>
      </header>

      <div className="toolbar">
        <input
          className="search"
          type="search"
          ref={searchRef}
          placeholder="Search company, role or location"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select className="status-filter" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
          <option value="All">All</option>
          {STATUSES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
        <span className="muted">{jobs.length} {jobs.length === 1 ? 'application' : 'applications'}</span>
        <button className="primary" onClick={() => setFormJob({})}>+ Add application</button>
      </div>

      {error && <p className="error">{error}</p>}

      {loading ? (
        <p className="muted center">Loading…</p>
      ) : (
        <div className="board">
          {STATUSES.map((status) => {
            const columnJobs = visibleJobs.filter((job) => job.status === status)
            return (
              <section
                key={status}
                className={`column ${dragOverStatus === status ? 'drag-over' : ''}`}
                onDragOver={(e) => { e.preventDefault(); setDragOverStatus(status) }}
                onDragLeave={() => setDragOverStatus(null)}
                onDrop={(e) => handleDrop(e, status)}
              >
                <h2>
                  <span className={`dot ${status.toLowerCase()}`} />
                  {status}
                  <span className="count">{columnJobs.length}</span>
                </h2>

                {columnJobs.map((job) => (
                  <JobCard
                    key={job.id}
                    job={job}
                    onEdit={() => setFormJob(job)}
                    onDelete={() => handleDelete(job)}
                    onMove={(newStatus) => handleMove(job.id, newStatus)}
                  />
                ))}

                {columnJobs.length === 0 && (
                  <p className="empty">{term ? 'No matches' : 'Nothing here yet'}</p>
                )}
              </section>
            )
          })}
        </div>
      )}

      {formJob && (
        <JobForm job={formJob} onSave={handleSave} onCancel={() => setFormJob(null)} />
      )}
    </div>
  )
}
