# Mobile App

React Native mobile app backed by Azure Functions + Azure SQL DB.

## Structure

```
├── mobile/          React Native app (TypeScript)
├── api/             Azure Functions .NET 8 isolated
├── infra/           Bicep IaC (Key Vault, SQL, Functions)
├── .azuredevops/    Azure DevOps pipeline YAML
└── docs/SETUP.md    Step-by-step setup guide
```

## Quick start

See [docs/SETUP.md](docs/SETUP.md) for the full setup walkthrough.

## Security architecture

| Concern | Solution |
|---|---|
| SQL credentials | Stored in **Azure Key Vault** — never in code or pipeline vars |
| API auth to Azure | **OIDC federated credentials** — no stored service principal secret |
| Function App auth to Key Vault/SQL | **Managed Identity** — no passwords anywhere |
| Key Vault access model | **RBAC** (not access policies) |
| Key Vault data recovery | Soft-delete 90 days + purge protection |
| Transport | TLS 1.2 minimum everywhere |
| Production deployments | Manual approval gate in Azure DevOps |
| Branch protection | PR required + status checks on `main` |
