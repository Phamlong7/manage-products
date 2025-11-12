# Product Inventory Management Platform

This solution delivers a full-stack inventory management experience with a .NET 8 Web API backend and a Next.js 14 frontend. It supports end-to-end product CRUD, rich filtering, and cloud-ready deployment targets (Neon for PostgreSQL, Render for the API, Vercel for the UI).

## Architecture Overview

- **Backend (`manage products`)**
  - ASP.NET Core 8 Web API following Clean Architecture boundaries.
  - Entity Framework Core with PostgreSQL provider (`Npgsql`).
  - Domain-driven repository pattern (`IProductRepository`) to avoid direct `DbContext` usage outside the infrastructure layer.
  - `ProductService` encapsulates business logic and uses a lightweight `Result` object for explicit error handling.
- **Frontend (`product-inventory-ui`)**
  - Next.js App Router (React Server + Client components) with Tailwind-powered styling.
  - Dedicated API client (`lib/api.ts`) reading from `NEXT_PUBLIC_API_BASE_URL` for environment-specific routing.
  - Reusable `ProductForm` component used for both create and update flows.

## Local Development

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- PostgreSQL instance (local or hosted via Neon)

### 1. Configure the Backend

1. Copy `appsettings.json` to `appsettings.Development.json` if it is not already present.
2. Update `ConnectionStrings:ProductDatabase` to point to your PostgreSQL database. Example Neon connection string:
   ```
   Host=your-neon-host;Port=5432;Database=inventory;Username=neondb_user;Password=strong-password;Ssl Mode=Require;Trust Server Certificate=true
   ```
3. (Optional) Adjust `ClientApp:AllowedOrigins` to match your frontend origin during development.
4. Restore & run migrations:
   ```bash
   cd "manage products/manage products"
   dotnet restore
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   dotnet run
   ```
   > The application automatically calls `Database.Migrate()` at startup, so running `dotnet ef database update` is optional but recommended for validating the connection.

### 2. Configure the Frontend

1. Install dependencies and run the dev server:
   ```bash
   cd product-inventory-ui
   npm install
   npm run dev
   ```
2. Create a `.env.local` file with the backend base URL (Render local tunnel or `http://localhost:5000` by default):
   ```env
   NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
   ```
3. Visit `http://localhost:3000` to interact with the UI.

## API Summary

- `GET /api/products` — list with search (`search`), numeric filters (`minPrice`, `maxPrice`), and sort (`price_asc`, `price_desc`, `name_asc`, `name_desc`).
- `GET /api/products/{id}` — fetch single product.
- `POST /api/products` — create product (validates positive price & non-negative stock, enforces unique name).
- `PUT /api/products/{id}` — update product attributes.
- `DELETE /api/products/{id}` — remove product after confirmation on the UI.

All endpoints return structured errors through the shared `Result` mechanics to keep the frontend aware of validation/business failures.

## Deployment Guide

### Database: Neon PostgreSQL

1. Sign in at [Neon](https://console.neon.tech/) and create a new project.
2. Create a database (e.g., `inventory`) and copy the connection string.
3. If required, enable connection pooling and set the SSL mode to `Require`.
4. Update Render and local environment variables with the Neon connection string `ConnectionStrings__ProductDatabase`.

### Backend: Render

1. Push this repository to GitHub.
2. Create a new **Web Service** on Render.
3. Set **Build Command** to `dotnet restore && dotnet publish -c Release -o out`.
4. Set **Start Command** to `dotnet "out/manage products.dll"`.
5. Configure environment variables:
   - `ASPNETCORE_URLS=http://0.0.0.0:10000`
   - `ConnectionStrings__ProductDatabase=<Neon connection string>`
   - `ClientApp__AllowedOrigins=https://your-vercel-domain.vercel.app`
6. After deployment, note the public URL (e.g., `https://inventory-api.onrender.com`) — used by the frontend.

### Frontend: Vercel

1. Import the GitHub repository into Vercel.
2. During setup, add environment variables:
   - `NEXT_PUBLIC_API_BASE_URL=https://inventory-api.onrender.com`
3. Deploy; Vercel will build using `npm install` followed by `npm run build` automatically.
4. Ensure CORS on the backend includes the generated Vercel domain.

## Brief Report

- **Why use a `Result` type?** Centralising success/error responses ensures API consumers receive structured feedback, simplifies controller logic, and prevents leaking exceptions across layers.
- **Repository & Service layers** enforce the Clean Architecture rule: controllers never touch EF Core directly, allowing future swap-outs (e.g., Dapper, testing fakes) without UI changes.
- **Client experience** includes optimistic updates after delete, inline filters and sorters, and a shared form component to keep UX consistent.

## Next Steps

- Add authentication if multi-user access is required.
- Extend filtering (e.g., categories, stock thresholds) via new query parameters in `GetProductsQuery`.
- Add automated tests: xUnit for service logic & Playwright for end-to-end UI flows.

---

Deploy the backend first to expose the API URL, configure the frontend’s `NEXT_PUBLIC_API_BASE_URL`, then redeploy the UI. With Neon handling storage, Render hosting the API, and Vercel serving the React app, the platform is cloud-ready.

