# FrameRent — Audiovisual Equipment Rental Platform

FrameRent is a full-stack .NET solution designed for an audiovisual equipment rental company (cameras, lenses, lighting, microphones, tripods, and stabilizers). It replaces manual spreadsheet and messaging workflows with a centralized platform built following **Clean Architecture** principles.

Customers can browse the active catalog, check date-range availability, request rentals, monitor their rental status, and cancel eligible reservations. Company staff can manage inventory, control the entire lifecycle of each rental (approval, rejection, delivery, return with automatic late-fee calculation), and consult analytical reports.

---

## Architecture & Technologies

### Clean Architecture Layers
- **`FrameRent.Domain`**: Enterprise entities (`Equipment`, `Rental`), enums (`RentalStatus`), role constants, custom domain exceptions, and repository interfaces. Zero external dependencies.
- **`FrameRent.Application`**: Business use cases and application services (`EquipmentService`, `RentalService`, `ReportService`), DTOs, and service contracts (`IAuthService`, `IEquipmentService`, `IRentalService`, `IReportService`). Depends only on `FrameRent.Domain`.
- **`FrameRent.Infrastructure`**: Persistence with Entity Framework Core (`ApplicationDbContext`), PostgreSQL (`Npgsql`), Code-First Migrations, Repository implementations (`EquipmentRepository`, `RentalRepository`), ASP.NET Core Identity + JWT service (`AuthService`), and automatic database seeding (`DbInitializer`).
- **`FrameRent.Api`**: RESTful Web API with JWT Bearer authentication, role-based authorization (`Staff` / `Customer`), global exception handling middleware, and Swagger/OpenAPI documentation.
- **`FrameRent.Web`**: ASP.NET Core MVC client web application (Option A) that consumes the `FrameRent.Application` services directly.

### Tech Stack
- **.NET 10** (`net10.0`)
- **ASP.NET Core Web API** & **ASP.NET Core MVC (Razor Views + Bootstrap 5)**
- **Entity Framework Core 10** (Code First + PostgreSQL `Npgsql.EntityFrameworkCore.PostgreSQL`)
- **ASP.NET Core Identity** (User & Role management)
- **JWT Bearer Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Swashbuckle / Swagger UI** (`Swashbuckle.AspNetCore`)

---

## Business Rules Implemented

1. **Users & Access Control**:
   - Public visitors can browse the active equipment catalog, view equipment details, and check availability by date range.
   - Anyone registering via the platform is automatically assigned the `Customer` role.
   - A pre-seeded `Staff` account is created automatically on application startup.
   - Customers can only view and cancel their own rentals. Staff manages all equipment and all rentals.
2. **Equipment Management**:
   - Only `Staff` can create, update, or deactivate equipment.
   - Equipment is never deleted; it is deactivated (`IsActive = false`).
   - Equipment with `Approved` or `Delivered` rentals cannot be deactivated.
   - Inactive equipment is hidden from the public catalog and cannot be rented.
3. **Rentals & Lifecycle**:
   - **Valid Dates**: `StartDate` cannot be in the past, `EndDate` must be strictly after `StartDate`, and maximum rental duration is **15 days**.
   - **Availability**: Requested units cannot exceed available stock in the requested date range (`TotalStock` minus units in overlapping `Approved` or `Delivered` rentals).
   - **Automatic Cost Calculation**: `TotalCost = days × DailyRate × Quantity` (calculated strictly by the domain).
   - **Customer Active Limit**: A customer cannot have more than **3 simultaneous rentals** in `Pending` or `Approved` status.
   - **Strict State Machine**:
     - `Pending` → `Approved` → `Delivered` → `Returned`
     - `Pending` → `Rejected`
     - `Pending` or `Approved` → `Cancelled` (strictly before `StartDate`)
   - **Re-validation on Approval**: Stock availability is verified again when staff approves a rental.
   - **Late Fee Calculation**: If returned after `EndDate`, `LateFee = lateDays × DailyRate × Quantity × 1.5`.

---

## Default Staff Account Credentials

When the API or Web application starts for the first time, it automatically applies pending EF Core migrations and seeds the default `Staff` account along with initial audiovisual equipment:

- **Role**: `Staff`
- **Email**: `staff@framerent.com`
- **Password**: `StaffPassword123!`

---

## Setup & Execution Instructions

### 1. Prerequisites
- **.NET 10 SDK** installed.
- **PostgreSQL** running locally (or via Docker).

### 2. Configure Database Connection
Update the `DefaultConnection` string in both:
- `src/FrameRent.Api/appsettings.json`
- `src/FrameRent.Web/appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=framerent_db;Username=postgres;Password=postgres"
}
```

*(Optional)* Quick PostgreSQL container with Docker:
```bash
docker run --name framerent-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=framerent_db -p 5432:5432 -d postgres:16
```

### 3. Database Migrations
Migrations are located in `src/FrameRent.Infrastructure/Persistence/Migrations` and are **automatically applied on startup** via `DbInitializer.SeedAsync()`.

If you want to run migrations manually via CLI:
```bash
dotnet ef database update --project src/FrameRent.Infrastructure --startup-project src/FrameRent.Api
```

### 4. Run the REST API (`FrameRent.Api`)
```bash
dotnet run --project src/FrameRent.Api
```
- **Swagger UI**: `http://localhost:5286/swagger` (check console output for exact port).
- Use Swagger to test **Staff** operations (`POST /api/v1/auth/login` with `staff@framerent.com`, then authorize with the JWT token).

### 5. Run the Customer Web Application (`FrameRent.Web`)
```bash
dotnet run --project src/FrameRent.Web
```
- **Web Portal**: `http://localhost:5200`
- Allows customers to register, sign in, browse equipment, check availability by dates, request rentals, view their rental history, and cancel eligible rentals.

---

## API Endpoints Summary

### Authentication
| Method | Route | Access | Description |
|---|---|---|---|
| `POST` | `/api/v1/auth/register` | Public | Registers a new customer |
| `POST` | `/api/v1/auth/login` | Public | Authenticates user and returns JWT |

### Equipment
| Method | Route | Access | Description |
|---|---|---|---|
| `GET` | `/api/v1/equipment` | Public | Lists active equipment |
| `GET` | `/api/v1/equipment/{id}` | Public | Gets equipment details |
| `GET` | `/api/v1/equipment/{id}/availability?from=&to=` | Public | Checks available units in date range |
| `POST` | `/api/v1/equipment` | Staff | Creates new equipment |
| `PUT` | `/api/v1/equipment/{id}` | Staff | Updates existing equipment |
| `PATCH` | `/api/v1/equipment/{id}/deactivate` | Staff | Deactivates equipment |

### Rentals
| Method | Route | Access | Description |
|---|---|---|---|
| `POST` | `/api/v1/rentals` | Customer | Requests a new rental |
| `GET` | `/api/v1/rentals/mine` | Customer | Lists authenticated customer's rentals |
| `GET` | `/api/v1/rentals/{id}` | Owner or Staff | Gets rental details |
| `PATCH` | `/api/v1/rentals/{id}/cancel` | Owner (Customer) | Cancels a pending/approved rental before start date |
| `GET` | `/api/v1/rentals?status=` | Staff | Lists all rentals with optional status filter |
| `PATCH` | `/api/v1/rentals/{id}/approve` | Staff | Approves a pending rental (re-validates stock) |
| `PATCH` | `/api/v1/rentals/{id}/reject` | Staff | Rejects a pending rental |
| `PATCH` | `/api/v1/rentals/{id}/deliver` | Staff | Marks an approved rental as delivered |
| `PATCH` | `/api/v1/rentals/{id}/return` | Staff | Registers return and calculates late fee if applicable |

### Reports
| Method | Route | Access | Description |
|---|---|---|---|
| `GET` | `/api/v1/reports/topequipment` | Staff | Lists most rented equipment ordered descending |

---

## Author

- **Full Name**: Fernando Santos
- **Clan**: Esthercita
- **Email**: fsantosp29@gmail.com
- **ID Document**: 1001821035
