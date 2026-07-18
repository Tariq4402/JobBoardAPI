# Job Board API

A production-inspired RESTful API for a job board platform, built with ASP.NET Core Web API. Supports job posting, job applications, resume and profile picture uploads, JWT authentication with refresh token rotation, full email verification and password reset flows, and role-based authorization for Applicants, Employers, and Admins.

Built by **Tariq Waheed** ([@Tariq4402](https://github.com/Tariq4402)) as a portfolio project to demonstrate backend development skills for .NET Core developer roles.

> **A note on how this was built:** I designed the architecture and wrote the business logic across all controllers, services, and repositories myself — the full authentication flow (registration, login, email verification, password reset and change, refresh token rotation), job and application management, and an authorization vulnerability I identified and fixed on the application-details endpoint. For a handful of infrastructure pieces that were new to me — structured logging, background services, resilience policies, rate limiting, and Swagger/JWT wiring — I studied the concepts and got implementation direction from AI, then built, adapted, and debugged them myself. Every bug along the way (EF Core cascade-delete conflicts, JWT claim mismatches, migration failures, namespace refactors) was diagnosed and resolved through my own testing and review. This project reflects my actual working knowledge of the .NET stack.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Authentication Flow](#authentication-flow)
- [Testing](#testing)
- [Future Improvements](#future-improvements)
- [License](#license)

---

## Features

**Authentication & Authorization**
- JWT access tokens (15 min expiry) + refresh token rotation (7-day expiry, DB-backed, revocable)
- Email verification required before login
- Forgot password / reset password flow via email
- Change password with automatic refresh token invalidation
- Resend verification email
- Role-based authorization: `Admin`, `Employer`, `Applicant`
- Password hashing with BCrypt
- Rate limiting on authentication and resource-heavy endpoints

**Jobs**
- Full CRUD for job postings (Employer-only create/update/delete)
- Job status management (`Open`/`Closed`) — Employer-only
- Filtering by title, location, salary range, job type, and status
- Paginated job listings
- In-memory caching on filtered job listings (5-min TTL)
- Duplicate job posting prevention

**Job Applications**
- Applicants can submit applications with an optional resume upload (PDF/DOC/DOCX, up to 10MB)
- Applicants can withdraw their own applications
- Employers can view all applications for a specific job or across all their posted jobs
- Employers can update application status (`Pending`/`Reviewed`/`Accepted`/`Rejected`)
- Employers can view an applicant's profile — restricted to applicants who applied to their own jobs (prevents unauthorized profile scraping)
- Ownership checks prevent users from viewing each other's application details

**User Management**
- Profile view and update (name, about, company name)
- Profile picture upload (JPG/PNG/WEBP, up to 2MB)
- Email update
- Account deletion with cascade to owned jobs and applications
- Admin: block/unblock users
- Search users by name

**Cross-Cutting Concerns**
- Global exception handling — consistent JSON error responses across the entire API
- FluentValidation for complex business rules + Data Annotations for simple field constraints
- AutoMapper for all entity-to-DTO mappings
- Structured logging with Serilog (console + rolling file sinks)
- Polly retry policies on outbound email sending (handles transient SMTP failures)
- Background service for automatic cleanup of expired refresh, verification, and reset tokens
- CORS configuration
- Swagger/OpenAPI documentation with JWT bearer auth support
- Secrets management via .NET User Secrets (no credentials in source control)

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core Web API (.NET 10) |
| Database | SQL Server |
| ORM | Entity Framework Core |
| Auth | JWT Bearer + Refresh Tokens |
| Password Hashing | BCrypt.Net |
| Validation | FluentValidation + Data Annotations |
| Mapping | AutoMapper |
| Logging | Serilog |
| Resilience | Polly |
| Email | MailKit (SMTP via Gmail) |
| API Docs | Swashbuckle (Swagger/OpenAPI) |
| Caching | IMemoryCache |
| Testing | xUnit + Moq + FluentAssertions |

---

## Architecture

The project follows a clean, layered architecture:

```
Controller → Service → Repository (via Unit of Work) → DbContext
```

- **Controllers** — handle HTTP concerns only: route binding, claims extraction from JWT, delegating to services, and returning results. No business logic lives here.
- **Services** — contain all business logic. Throw typed exceptions (`KeyNotFoundException`, `UnauthorizedAccessException`, `InvalidOperationException`) on failure — no magic status enums or boolean returns.
- **Repositories** — data access only, built on a Generic Repository pattern with entity-specific repositories layered on top for custom queries.
- **Unit of Work** — coordinates repositories and commits changes atomically through a single `DbContext`.
- **Global Exception Filter** — catches all exceptions at the top of the pipeline and maps them to consistent JSON error responses (`KeyNotFoundException` → 404, `UnauthorizedAccessException` → 403, `InvalidOperationException` → 400, unhandled → 500), keeping controllers free of try/catch blocks.

This separation keeps business rules independently testable and controllers thin and readable.

---

## Project Structure

```
JobBoardAPI/
├── Common/             # Shared constants (e.g. Roles)
├── Controllers/        # API endpoints
├── Data/               # DbContext and EF configuration
├── DTOs/               # Request/response data transfer objects
├── Entities/           # Database models
├── Enums/              # JobStatus, JobType, ApplicationStatus
├── Exceptions/         # Global exception filter
├── Mappings/           # AutoMapper profiles
├── Migrations/         # EF Core migrations
├── Repositories/       # Generic + specific repositories, Unit of Work
├── Services/           # Business logic layer
├── Validators/         # FluentValidation rules
├── BackgroundServices/ # Token cleanup background service
├── appsettings.json
└── Program.cs

JobBoardAPI.Tests/
└── Services/           # Unit tests for service layer
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (LocalDB or full instance)
- A Gmail account with an [App Password](https://myaccount.google.com/apppasswords) generated (for email sending)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/Tariq4402/JobBoardAPI.git
   cd JobBoardAPI
   ```

2. **Configure the connection string** in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=JobBoardDB;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Set up User Secrets** (never commit credentials to source control):
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-here"
   dotnet user-secrets set "EmailSettings:AppPassword" "your-gmail-app-password"
   ```

4. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the project**
   ```bash
   dotnet run
   ```

6. **Explore the API** at `https://localhost:{port}/swagger`

---

## Configuration

Key settings in `appsettings.json`:

```json
{
  "JwtSettings": {
    "Issuer": "JobBoardAPI",
    "Audience": "JobBoardClient",
    "ExpiryInMinutes": 15
  },
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Job Board API"
  }
}
```

Secrets (`JwtSettings:SecretKey`, `EmailSettings:AppPassword`) are managed via .NET User Secrets in development and should be set as environment variables in production.

---

## API Endpoints

### Auth (`/api/auth`)
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/register` | Register a new user | — |
| POST | `/login` | Login, returns access + refresh token | — |
| GET | `/verify-email` | Verify email via token | — |
| POST | `/resend-verification-email` | Resend verification email | — |
| POST | `/forgot-password` | Request a password reset email | — |
| POST | `/reset-password` | Reset password with token | — |
| POST | `/change-password` | Change password (logged in) | Required |
| POST | `/refreshtoken` | Exchange refresh token for new token pair | — |
| PATCH | `/revoketoken` | Revoke a refresh token (logout) | Required |

### Jobs (`/api/job`)
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/{jobId}` | Get job by ID | — |
| GET | `/` | Get all jobs | Required |
| GET | `/filter` | Filtered, paginated job listings | Required |
| POST | `/` | Create a job | Employer |
| PUT | `/{jobId}` | Update a job | Employer |
| PATCH | `/{jobId}/status` | Update job status | Employer |
| DELETE | `/{jobId}` | Delete a job | Employer |

### Job Applications (`/api/jobapplication`)
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/` | Submit an application (with optional resume) | Applicant |
| GET | `/{jobApplicationId}` | Get application details | Owner or Employer |
| GET | `/job/{jobId}` | Get all applications for a job | Employer |
| GET | `/employer` | Get all applications across employer's jobs | Employer |
| GET | `/applicant` | Get applicant's own applications | Applicant |
| GET | `/GetApplicantProfile/{jobApplicationId}` | View applicant profile (own jobs only) | Employer |
| PATCH | `/{jobApplicationId}/status` | Update application status | Employer |
| DELETE | `/{jobApplicationId}` | Withdraw an application | Applicant |

### Users (`/api/user`)
| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/MyProfile` | Get own profile | Required |
| GET | `/Search` | Search users by name | Required |
| PUT | `/UpdateMyProfile` | Update profile | Required |
| PUT | `/UpdateEmail` | Update email | Required |
| PATCH | `/UpdateProfilePic` | Upload profile picture | Required |
| DELETE | `/DeleteMyAccount` | Delete own account | Required |
| PATCH | `/BlockUser/{userId}` | Block a user | Admin |
| PATCH | `/UnblockUser/{userId}` | Unblock a user | Admin |

Full request/response schemas are available via Swagger at `/swagger` when running the project.

---

## Authentication Flow

1. **Register** → account created, verification email sent
2. **Verify Email** → use the token from the email → account activated
3. **Login** → returns a short-lived access token (15 min) + refresh token (7 days, stored in DB)
4. **Access token expires** → call `/refreshtoken` → receive a new access + refresh token pair (old refresh token is revoked — rotation, not reuse)
5. **Logout** → call `/revoketoken` to invalidate the current refresh token
6. **Forgot password** → request reset email → submit new password with the reset token
7. Expired and used tokens (refresh, verification, reset) are automatically purged by a background cleanup service every 24 hours

---

## Testing

The project includes a unit test suite covering the service layer, built with **xUnit**, **Moq**, and **FluentAssertions**.

Tests follow the Arrange/Act/Assert pattern with all external dependencies (repositories, email, token generation) mocked via Moq — no database or SMTP server required to run them.

**AuthService** — registration (happy path, duplicate email), login (happy path, user not found, wrong password, unverified email, blocked account)

**JobApplicationService** — submit application (happy path with AutoMapper verification, closed job exception, duplicate application exception), application detail authorization (neither applicant nor employer)

To run the tests:

```bash
dotnet test
```

---

## Future Improvements

- Migrate file storage (profile pictures, resumes) from local disk to cloud storage (Azure Blob / AWS S3)
- Docker containerization for consistent deployment
- CI/CD pipeline via GitHub Actions
- Real-time notifications via SignalR when application status changes
- Admin analytics dashboard (posting trends, application volume)
- API versioning for future breaking changes
- Soft delete for jobs and applications to preserve an audit trail

---

## License

This project is licensed under the MIT License.
