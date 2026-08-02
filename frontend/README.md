# Partner Data Sharing Explorer

Minimal Angular UI for exploring the ASP.NET Core middleware demo in the parent repo.

The UI sends real requests to the API and shows:

- selected API key and role
- request headers and body
- HTTP status and response body
- response correlation id
- audit events created by protected API calls

## Run

Start the API from the repo root:

```powershell
dotnet run --project PartnerDataSharing.Api
```

Start the Angular dev server from this folder:

```powershell
npm start
```

Open `http://localhost:4200`.

`proxy.conf.json` forwards `/health` and `/api` to `http://localhost:5080`, matching the API launch profile.

## Build

```powershell
npm run build
```
