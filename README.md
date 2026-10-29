# Enterprise Order Platform

A modular-monolith order and inventory backend built with C# and ASP.NET Core on .NET 10.

## Features

- **Identity:** register, login, JWT access tokens, roles (Customer, Warehouse, Support, Admin), lockout
- **Catalog:** products with admin-only writes, public paged reads
- **Inventory:** stock with reserve / release / commit rules and optimistic concurrency
- **Cart and Orders:** per-user cart, checkout with an `Idempotency-Key`, server-side pricing, order state machine
- **Payments:** provider abstraction with a fake provider (no real card data)
- **Fulfilment:** shipments with tracking numbers, order processing and delivery (Warehouse/Admin)
- **Notifications:** notification log written on shipping and delivery
- **Ops:** health check (`/health`), Serilog logging, ProblemDetails errors, GitHub Actions CI, Dockerfile

## Architecture

One deployable API (`src/EnterpriseOrderPlatform.Host`) and one project per module under `src/Modules`.
Each module has its own PostgreSQL schema and DbContext, with layers as folders (Domain, Application, Infrastructure, Contracts).
Modules do not reference each other. Ordering talks to Catalog and Inventory through small ports implemented as adapters in Host.
Architecture tests enforce layer rules inside the Identity module. Integration tests run the real app against a throwaway PostgreSQL (Testcontainers), covering idempotent checkout, the last-unit race and order ownership.

## Run locally

1. Install .NET 10 SDK and Docker Desktop.
2. Copy `.env.example` to `.env` and set a password.
3. `docker compose up -d`
4. Store the connection string and a signing key as user-secrets:
   `dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=eop;Username=eop_user;Password=YOUR_PASSWORD" --project src/EnterpriseOrderPlatform.Host`
   `dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 64 | tr -d '\n')" --project src/EnterpriseOrderPlatform.Host`
5. Apply migrations per module with `dotnet ef database update` (one DbContext per module).
6. `dotnet run --project src/EnterpriseOrderPlatform.Host --urls http://localhost:5080`

## Test

`dotnet test`

## Known limits and future work

- Checkout is not one transaction across modules. A crash between reserving stock and saving could leave stock reserved. Reservation expiry and a transactional outbox would fix this.
- Notifications are written directly, not event-driven.
- Refresh tokens, email confirmation and an admin user-management flow are not implemented.
- Planned: Redis catalog cache, RabbitMQ transport, OpenTelemetry traces, a real payment provider.
