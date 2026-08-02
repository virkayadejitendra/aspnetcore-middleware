# Partner Data Sharing Middleware Demo

This is a fictional educational project for learning ASP.NET Core custom middleware.
It does not represent any real company, employer, client, product, architecture, workflow, or dataset.

The sample models a generic B2B partner platform where partners can view products, manage their own orders, analytics partners can read aggregated summaries, and compliance users can review audit events.

The repo also includes a minimal Angular explorer in `frontend/`. It lets learners switch demo roles, send real API requests, and see the status code, correlation id, response body, and audit trail produced by the middleware pipeline.

## Middleware Use Cases

- `ApiKeyAuthenticationMiddleware` validates `X-Api-Key`, creates a request identity, and returns `401` for missing or invalid keys.
- `PartnerAccessMiddleware` enforces partner/tenant boundaries and role access rules.
- `DataSharingAuditMiddleware` records access to partner data, analytics, and compliance endpoints.

## Demo API Keys

| Role | API key |
| --- | --- |
| RetailPartner | `retail-demo-key` |
| DistributorPartner | `distributor-demo-key` |
| AnalyticsPartner | `analytics-demo-key` |
| ComplianceUser | `compliance-demo-key` |
| InternalAdmin | `admin-demo-key` |

## Example Requests

```http
GET /health
```

```http
GET /api/products
X-Api-Key: retail-demo-key
```

```http
GET /api/partners/PARTNER-RETAIL-001/orders
X-Api-Key: retail-demo-key
X-Correlation-Id: demo-correlation-001
```

```http
GET /api/analytics/sales-summary
X-Api-Key: analytics-demo-key
```

```http
GET /api/compliance/audit-events
X-Api-Key: compliance-demo-key
```

## Run

```powershell
dotnet run --project PartnerDataSharing.Api
```

OpenAPI is available in Development at `/openapi/v1.json`.

## Explore with Angular UI

Start the API first:

```powershell
dotnet run --project PartnerDataSharing.Api
```

In a second terminal, start the Angular explorer:

```powershell
cd frontend
npm start
```

Open `http://localhost:4200`. The Angular dev server proxies `/health` and `/api` requests to `http://localhost:5080`, so the API does not need CORS changes for local learning.

Useful learner scenarios:

- Select `RetailPartner` and call products or own orders to see allowed requests.
- Select `RetailPartner` and call blocked partner orders to see tenant isolation return `403`.
- Call missing API key to see authentication return `401`.
- Call analytics, then audit events, to see audited protected API access.

## Test

```powershell
dotnet test
```

To build the Angular explorer:

```powershell
cd frontend
npm run build
```

## Deploy to Render

This repo includes a Dockerfile and `render.yaml` blueprint for a Render Web Service.

1. Push the repo to GitHub.
2. In Render, create a new Blueprint from this GitHub repo. Render will use `render.yaml`.
3. After the service is created, copy its Deploy Hook URL from the Render service settings.
4. In GitHub, add a repository secret named `RENDER_DEPLOY_HOOK_URL` with that deploy hook URL.

Pushes to `main` run the GitHub Actions workflow. If restore, build, and tests pass, the workflow triggers a Render deploy.
