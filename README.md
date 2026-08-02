# Partner Data Sharing Middleware Demo

This is a fictional educational project for learning ASP.NET Core custom middleware.
It does not represent any real company, employer, client, product, architecture, workflow, or dataset.

The sample models a generic B2B partner platform where partners can view products, manage their own orders, analytics partners can read aggregated summaries, and compliance users can review audit events.

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

## Test

```powershell
dotnet test
```

## Deploy to Render

This repo includes a Dockerfile and `render.yaml` blueprint for a Render Web Service.

1. Push the repo to GitHub.
2. In Render, create a new Blueprint from this GitHub repo. Render will use `render.yaml`.
3. After the service is created, copy its Deploy Hook URL from the Render service settings.
4. In GitHub, add a repository secret named `RENDER_DEPLOY_HOOK_URL` with that deploy hook URL.

Pushes to `main` run the GitHub Actions workflow. If restore, build, and tests pass, the workflow triggers a Render deploy.
