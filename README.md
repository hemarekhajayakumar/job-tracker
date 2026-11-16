# Job Tracker

A web app to track job applications: where you applied, for which role, and what stage each application is at.

## Features

- Register and log in (passwords are hashed, sessions use JWT tokens)
- Add, edit and delete job applications
- A board with four columns: Applied, Interview, Offer, Rejected
- Move an application between columns by dragging the card or using its dropdown
- Search by company, role or location
- Each user sees only their own applications

## Tech stack

| Part | Technology | Folder |
|---|---|---|
| Backend | ASP.NET Core Web API (.NET 10), Entity Framework Core, SQLite, JWT authentication | `backend/` |
| Frontend | React 19, React Router, Vite | `frontend/` |

## How it works

```
Browser (React, port 5173)  --HTTP request-->  .NET Web API (port 5062)  --EF Core-->  SQLite file
                            <----- JSON -----
```

### API endpoints

| Method | URL | What it does | Login needed |
|---|---|---|---|
| POST | `/api/auth/register` | Create an account | No |
| POST | `/api/auth/login` | Log in and get a token | No |
| GET | `/api/jobs` | List your applications | Yes |
| POST | `/api/jobs` | Add an application | Yes |
| PUT | `/api/jobs/{id}` | Update an application | Yes |
| PATCH | `/api/jobs/{id}/status` | Change only the status | Yes |
| DELETE | `/api/jobs/{id}` | Delete an application | Yes |

## Run it locally

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Node.js](https://nodejs.org).

### First-time setup

The key that signs login tokens is a secret, so it is not stored in this repository. Set your own once:

```bash
cd backend
dotnet user-secrets set "Jwt:Key" "replace-this-with-any-long-random-text-of-40-or-more-characters"
```

### Start the app

On Windows, double-click `start.bat`. It starts the API and the website and opens your browser. Close the two windows it opens to stop the app.

Or start each part yourself:

```bash
cd backend
dotnet run
```

```bash
cd frontend
npm install
npm run dev
```

Then open http://localhost:5173.

The database is a single file, `backend/jobtracker.db`, created automatically on first run.
