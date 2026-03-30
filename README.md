# MiniCMS – COMP 4602 Assignment 2

## Setup Instructions

### Prerequisites

- .NET SDK (matching project target)
- Docker Desktop (must be installed and running)
- Aspire CLI/tools available

---

### First-Time Setup

From the solution root:

```bash
dotnet restore
dotnet build
```

---

## Run Instructions

Start the full system using Aspire:

```bash
aspire run
```

This will automatically:

- Start SQL Server (Docker container)
- Start backend (MiniCMS.Web)
- Start frontend (MiniCMS.Client)
- Apply migrations
- Seed the database

---

## Test Accounts

```
a@a.a / P@$$w0rd
w@w.w / P@$$w0rd
x@x.x / P@$$w0rd
```

---

## Testing Instructions

After running:

- Frontend: https://localhost:7145
- Backend: https://localhost:7192

Verify:

- Application loads without errors
- Articles are displayed on the homepage
- Clicking an article opens the detail page
- HTML content renders correctly
- No manual database setup was required

---

## Notes

- No manual database setup or EF commands are required
- SQL Server is automatically provisioned via Docker by Aspire
- Docker must be running before `aspire run`