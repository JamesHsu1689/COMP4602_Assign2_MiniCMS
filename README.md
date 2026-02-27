# MiniCMS – COMP 4602 Assignment 1  
**Mini CMS Backend (ASP.NET Core + EF Core + SQLite)**

## Project Overview

This project is a **backend-only Mini Content Management System (CMS)** developed for **COMP 4602 – Assignment 1**.

It implements a simple 3-tier architecture:

- **Presentation Layer**
  - Razor Pages / MVC Views
  - Admin Area
  - Swagger UI for API testing

- **Business Logic Layer**
  - Controllers (MVC + REST API)
  - Role-based authorization
  - Server-side HTML sanitization

- **Data Access Layer**
  - Entity Framework Core (Code-First)
  - SQLite database
  - Identity for authentication/roles

The system supports:

- Article CRUD (Admin only)
- Public article viewing
- REST API endpoints
- Identity authentication with roles
- Server-side HTML sanitization
- Database seeding

---

## Tech Stack

- .NET (ASP.NET Core)
- ASP.NET Core MVC + Web API
- Entity Framework Core (Code-First)
- SQLite
- ASP.NET Core Identity
- Swagger (Swashbuckle)
- Ganss.XSS (HtmlSanitizer)
- Quill Rich Text Editor (Admin UI)

---

## Repository Structure

```
MiniCMS.slnx
MiniCMS.Web/
    ├── Areas/Admin/...
    ├── Controllers/
    ├── Data/
    ├── Models/
    ├── Views/
    ├── minicms.db
    ├── minicms.db-wal
    ├── minicms.db-shm
```

- `MiniCMS.Web/` contains the ASP.NET Core web application.
- SQLite database file is located at:

```
MiniCMS.Web/minicms.db
```

---

## Setup Instructions (From Scratch)

### 1. Prerequisites

- .NET SDK (recommended: .NET 8 or version matching project target)

Verify installation:

```
dotnet --version
```

---

### 2. Clone the Repository

```
git clone <your-repo-url>
cd MiniCMS
```

---

### 3. Restore Dependencies

```
dotnet restore
```

---

### 4. Build the Project

```
dotnet build
```

Expected: Build succeeds with no errors.

---

### 5. Run the Application

From the solution root:

```
dotnet run --project MiniCMS.Web
```

The application will start and display a local URL such as:

```
https://localhost:xxxx
```

---

## Database & Migrations

- Uses EF Core Code-First
- SQLite database file: `MiniCMS.Web/minicms.db`
- Migrations are already included.
- On startup:
  - Database is created (if missing)
  - Migrations are applied
  - `DbSeeder.SeedAsync()` runs

### Seeded Data

On first run, the app seeds:

- **Admin user**
  - Email: `a@a.a`
  - Password: `P@$$w0rd`
  - Role: `Admin`

- **6 sample articles**

No manual migration commands are required for normal execution.

---

# Testing Instructions

---

## A) Swagger API Testing

### Swagger UI Location

```
https://localhost:<port>/swagger
```

Swagger is enabled via `AddSwaggerGen()` and maps API controllers.

### API Base Route

```
/api/articles
```

### Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/articles` | Returns all articles |
| GET | `/api/articles/{id}` | Returns single article |
| POST | `/api/articles` | Create article |
| PUT | `/api/articles/{id}` | Update article |
| DELETE | `/api/articles/{id}` | Delete article |

All endpoints are testable directly in Swagger.

---

## B) Admin Site

### Login URL

```
/Identity/Account/Login
```

### Seeded Admin Credentials

```
Email:    a@a.a
Password: P@$$w0rd
```

### Admin Article Management

```
/Admin/Articles
```

Features:

- Create article
- Edit article
- Delete article
- View article list

Admin routes are protected by role-based authorization:
- Only users in the `Admin` role can access.

### Rich Text Editor

- Uses Quill for WYSIWYG editing.
- HTML content is submitted to the server and sanitized before saving.

---

## C) Public Viewing

Public routes:

```
/Articles
```

Displays list of articles.

```
/Articles/Details/{id}
```

Displays a single article.

Stored article content is rendered using:

```
@Html.Raw(...)
```

But only after server-side sanitization.

---

# Security Note: Sanitization vs Encoding

This project implements server-side HTML sanitization.

### What happens:

1. User submits HTML (via Quill).
2. The server sanitizes input using Ganss.XSS HtmlSanitizer.
3. Sanitized HTML is stored in the database.
4. The sanitized HTML is rendered using `Html.Raw`.

### Why this is safe:

- Dangerous tags like `<script>` are removed before saving.
- Only safe HTML is stored.
- Raw rendering is safe because content has already been sanitized.

This prevents XSS while still allowing rich text formatting.

---

# Known Limitations

- No SPA frontend (backend-focused assignment)
- API endpoints are not authentication-protected (unless extended)
- No pagination implemented (unless added)
- No advanced validation or rate limiting
- No production deployment configuration

This project is intended for educational demonstration of backend architecture and security fundamentals.

---

# Submission Checklist (TA Quick Marking Guide)

✔ EF Core Code-First with SQLite  
✔ Database migrations included  
✔ SQLite DB file generated  
✔ Identity authentication configured  
✔ Admin role seeded  
✔ Seeded admin user (a@a.a / P@$$w0rd)  
✔ 6 seeded articles  
✔ REST API (/api/articles)  
✔ Swagger UI enabled  
✔ Full CRUD via API  
✔ Admin Area restricted to Admin role  
✔ Quill rich text editor integration  
✔ Server-side HTML sanitization (HtmlSanitizer)  
✔ Public article listing  
✔ Public article details page  
✔ Sanitized HTML rendered using Html.Raw  

---

# Fresh Clone Smoke Test (Minimal Commands)

From a clean machine:

```
git clone <repo-url>
cd MiniCMS
dotnet restore
dotnet build
dotnet run --project MiniCMS.Web
```

### Expected Results

- Build succeeds with no errors
- Application launches locally
- `/swagger` loads successfully
- `/Identity/Account/Login` works
- Admin login succeeds with seeded credentials
- `/Admin/Articles` accessible after login
- `/Articles` displays seeded articles

---

# Connection String Key Check

If `appsettings.json` contains:

```
"DefaultConne ction"
```

It should be corrected to:

```
"DefaultConnection"
```

If it already uses `"DefaultConnection"`, no change is required.

---

**End of Assignment 1 – Mini CMS Backend**
