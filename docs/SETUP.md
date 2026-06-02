# Step-by-Step Setup Guide
## Pay-as-you-go Azure — First Time

Work through each phase in order. Every section ends with a ✅ checkpoint so you know you're ready to move on.

---

## Phase 0 — Install tools on your machine

### 0.1 Azure CLI

Download and install from:
https://aka.ms/installazurecliwindows

After install, open a **new** PowerShell / Command Prompt and verify:

```powershell
az version
# Should show "azure-cli": "2.x.x"
```

### 0.2 .NET 8 SDK

Download from: https://dotnet.microsoft.com/download/dotnet/8.0
(pick the **SDK** installer, not Runtime)

```powershell
dotnet --version
# Should show 8.x.x
```

### 0.3 Node.js 20 LTS

Download from: https://nodejs.org (LTS version)

```powershell
node --version   # v20.x.x
npm --version    # 10.x.x
```

### 0.4 Git

Download from: https://git-scm.com/download/win
Accept all defaults during install.

```powershell
git --version
```

✅ **Checkpoint:** All four commands above return version numbers.

---

## Phase 1 — Azure account & subscription

### 1.1 Confirm your Pay-as-you-go subscription

1. Go to https://portal.azure.com and sign in
2. Click the search bar at the top → type **Subscriptions** → click the result
3. You should see one subscription with type **Pay-As-You-Go**
4. Click on it and copy the **Subscription ID** (a UUID like `a1b2c3d4-...`)

> 💡 **Cost note:** The resources in this project are designed to be cheap for dev:
> - Azure Functions Consumption plan: ~$0 until significant traffic
> - Azure SQL Serverless: pauses after 1 hour idle — you pay only while active (~$0.40/hour when running)
> - Key Vault: ~$0.03/10,000 operations
> - Storage account: < $1/month
> **Estimated dev cost: < $5/month** if you pause the SQL server when not developing.

### 1.2 Log in with the CLI

```powershell
az login
```

A browser window opens. Sign in with the same account you used for the portal.
Back in the terminal you'll see a JSON list of subscriptions.

### 1.3 Set your active subscription

Replace `<YOUR_SUBSCRIPTION_ID>` with the UUID you copied:

```powershell
az account set --subscription "<YOUR_SUBSCRIPTION_ID>"

# Confirm it's active:
az account show --query "{name:name, id:id, state:state}"
```

You should see `"state": "Enabled"`.

### 1.4 Register required resource providers

Pay-as-you-go subscriptions sometimes have providers unregistered. Run this once:

```powershell
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.KeyVault
az provider register --namespace Microsoft.Storage
az provider register --namespace Microsoft.Insights

# Check status (wait until all show "Registered"):
az provider show --namespace Microsoft.Web      --query registrationState
az provider show --namespace Microsoft.Sql      --query registrationState
az provider show --namespace Microsoft.KeyVault --query registrationState
```

> ⏳ Registration can take 1–2 minutes. Re-run the check commands until all say `"Registered"`.

✅ **Checkpoint:** `az account show` shows your subscription as Enabled, all providers Registered.

---

## Phase 2 — Create Azure resource groups

Resource groups are folders that hold your Azure resources. We create one for dev and one for prod.

```powershell
az group create --name rg-mobileapp-dev  --location centralindia
az group create --name rg-mobileapp-prod --location centralindia
```

> 📍 **Location:** `centralindia` = Pune. Other India options: `southindia` (Chennai), `westindia` (Mumbai)
> Full list: `az account list-locations --query "[].name" -o tsv`

Confirm both exist:

```powershell
az group list --query "[?contains(name,'mobileapp')].{Name:name, Location:location, State:properties.provisioningState}" -o table
```

You should see both groups with `State = Succeeded`.

✅ **Checkpoint:** Both resource groups appear in the table.

---

## Phase 3 — GitHub repository

### 3.1 Create the repo

1. Go to https://github.com/new
2. Set:
   - **Repository name:** `mobileapp` (or your preferred name)
   - **Visibility:** Private
   - **Do NOT** tick "Add a README" (the scaffold already has one)
3. Click **Create repository**
4. Copy the HTTPS URL shown (e.g. `https://github.com/yourusername/mobileapp.git`)

### 3.2 Push the scaffold

```powershell
cd E:\repo\app

git init
git add .
git commit -m "Initial project scaffold"
git branch -M main
git remote add origin https://github.com/praga-ai/myarea.git
git push -u origin main
```

> 💡 If Git asks for credentials, sign in with your GitHub username + a **Personal Access Token** (not your password).
> Create a token at: GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic) → Generate new token
> Scopes needed: `repo`

### 3.3 Enable branch protection

1. On GitHub, go to your repo → **Settings** → **Branches**
2. Click **Add branch protection rule**
3. Branch name pattern: `main`
4. Tick:
   - ✅ Require a pull request before merging
   - ✅ Require approvals: **1**
   - ✅ Do not allow bypassing the above settings
5. Click **Save changes**

> ℹ️ You'll add "Require status checks" here after the first pipeline run (we don't have status check names yet).

✅ **Checkpoint:** Code is on GitHub, branch protection is enabled on `main`.

---

## Phase 4 — Azure DevOps

### 4.1 Create organisation and project

1. Go to https://dev.azure.com
2. Sign in with **the same Microsoft account** used for Azure (important — same tenant)
3. Click **New organisation** → accept the terms → name it (e.g. `yourname-dev`)
4. Choose region closest to you → **Continue**
5. You'll land on "Create a project":
   - Project name: `MobileApp`
   - Visibility: **Private**
   - Click **Create project**

### 4.2 Create two service principals and service connections

Create two service principals — one per resource group. Each one has minimal permissions and cannot touch the other environment.

**Dev service principal:**

```powershell
$SUB_ID = "<YOUR_SUBSCRIPTION_ID>"

az ad sp create-for-rbac `
  --name "sp-mobileapp-dev" `
  --role "Contributor" `
  --scopes "/subscriptions/$SUB_ID/resourceGroups/rg-mobileapp-dev"
```

**Copy the output** (save somewhere safe):
```json
{
  "appId":        "...",     ← clientId
  "password":     "...",     ← clientSecret (shown ONCE)
  "tenant":       "..."      ← tenantId
}
```

Grant Key Vault RBAC permissions:

```powershell
$SP_DEV = "<appId from output above>"

az role assignment create --assignee $SP_DEV `
  --role "User Access Administrator" `
  --scope "/subscriptions/$SUB_ID/resourceGroups/rg-mobileapp-dev"
```

**Prod service principal:**

Repeat the above with:
```powershell
az ad sp create-for-rbac `
  --name "sp-mobileapp-prod" `
  --role "Contributor" `
  --scopes "/subscriptions/$SUB_ID/resourceGroups/rg-mobileapp-prod"
```

And grant Key Vault RBAC:

```powershell
$SP_PROD = "<appId from prod output>"

az role assignment create --assignee $SP_PROD `
  --role "User Access Administrator" `
  --scope "/subscriptions/$SUB_ID/resourceGroups/rg-mobileapp-prod"
```

### 4.3 Create service connections in Azure DevOps

1. In Azure DevOps → **Project Settings** (bottom-left gear) → **Service connections**
2. **New service connection** → **Azure Resource Manager** → **Next**
3. Choose **Service principal (manual)** → **Next**
4. Change the credential type to **Secret**
5. Fill in for `sc-mobileapp-dev`:

| Field | Value |
|---|---|
| Subscription ID | your subscription ID |
| Subscription name | Pay-As-You-Go |
| Service Principal Id | `appId` from dev output |
| Service principal key | `password` from dev output |
| Tenant ID | `tenant` from dev output |
| Service connection name | `sc-mobileapp-dev` |

6. Click **Verify** → ✅ → **Save**

7. Repeat for `sc-mobileapp-prod` using the prod JSON values and name it `sc-mobileapp-prod`

✅ **Checkpoint:** Both service connections verified and saved, each scoped to its own resource group only.

---

## Phase 5 — Create the pipeline

### 5.1 Add the pipeline

1. In Azure DevOps → **Pipelines** → **New pipeline**
2. **Where is your code?** → GitHub
3. Authenticate to GitHub if prompted
4. Select your `mobileapp` repository
5. **Configure your pipeline** → **Existing Azure Pipelines YAML file**
6. Branch: `main` | Path: `.azuredevops/azure-pipelines.yml`
7. Click **Continue** → **Save** (don't run yet — we need variables first)

### 5.2 Add secret pipeline variables

1. On the pipeline page → **Edit** (top right) → **Variables** (top right)
2. Click **New variable** for each:

| Variable name | Value | Secret? |
|---|---|---|
| `SQL_ADMIN_PASSWORD_DEV` | A strong password (min 12 chars, upper+lower+number+symbol) | ✅ Yes — tick "Keep this value secret" |
| `SQL_ADMIN_PASSWORD_PROD` | A different strong password | ✅ Yes |

> 🔑 **Password rules for Azure SQL:**
> - At least 12 characters
> - Must contain: uppercase, lowercase, digit, and special character
> - Must NOT contain the word "password", "admin", or the login name
> - Example: `Tr0pic@lSunset#99`

3. Click **Save**

### 5.3 Create DevOps Environments with approval gate on prod

1. In Azure DevOps → **Pipelines** → **Environments** (left sidebar)
2. Click **New environment**:
   - Name: `dev` | Resource: None → **Create**
3. Create another:
   - Name: `prod` | Resource: None → **Create**
4. Click on the `prod` environment → **...** (three dots, top-right) → **Approvals and checks**
5. Click **+** → **Approvals**
6. Add yourself as approver → **Create**

✅ **Checkpoint:** Pipeline saved with 2 secret variables, `prod` environment has an approval gate.

---

## Phase 6 — First run

### 6.1 Trigger the pipeline

```powershell
# Make a small change to trigger the pipeline
cd E:\repo\app
git checkout -b feature/trigger-first-run
echo "# trigger" >> README.md
git add README.md
git commit -m "Trigger first pipeline run"
git push -u origin feature/trigger-first-run
```

Then open a Pull Request on GitHub: `feature/trigger-first-run` → `main`.
The pipeline will run on the PR automatically (Build stage only — no deploy on PRs).

Merge the PR. The full pipeline runs including deploy to dev.

### 6.2 Watch the pipeline

In Azure DevOps → **Pipelines** → click the running pipeline.

Expected stages:
```
Build & Test          ✅ ~3 min
  ├── API build & test
  ├── Mobile lint & type-check
  └── IaC validate Bicep

Deploy — Dev          ✅ ~5 min
  ├── Deploy Bicep (creates KV, SQL, Functions)
  └── Deploy Azure Functions app

Deploy — Prod         ⏸ waiting for your approval
```

When `Deploy — Prod` shows **Waiting for review** — go approve it in Azure DevOps or leave it for now.

### 6.3 Verify the deployment

```powershell
curl https://func-mobileapp-dev.azurewebsites.net/api/health
```

Expected response:
```json
{"status":"healthy","timestamp":"2026-06-02T..."}
```

✅ **Checkpoint:** Health endpoint responds with `"status":"healthy"`.

---

## Phase 7 — Post-deploy: add status checks to GitHub branch protection

Now that the pipeline has run, you can add the status check requirement:

1. GitHub → your repo → **Settings** → **Branches** → click **Edit** on the `main` rule
2. Tick ✅ **Require status checks to pass before merging**
3. In the search box type the job names from Azure DevOps (they appear after the first run):
   - `BuildApi`
   - `BuildMobile`
   - `ValidateBicep`
4. Click **Save changes**

Now no one can merge to `main` without those checks passing.

---

## Cost management tips

### Pause SQL when not working

Azure SQL Serverless auto-pauses after 1 hour (configured in the Bicep). But if you want to stop billing entirely during a break:

```powershell
# Pause the database manually
az sql db update \
  --resource-group rg-mobileapp-dev \
  --server sql-mobileapp-dev \
  --name db-mobileapp \
  --compute-model Serverless \
  --auto-pause-delay 60
```

Or just leave the serverless config — it pauses itself.

### Set a spending alert

1. Azure Portal → **Subscriptions** → your subscription
2. **Cost alerts** → **Add**
3. Alert type: **Budget** | Amount: $20 | Notify at 80% and 100%

### Check current spend

```powershell
az consumption usage list --top 10 --query "[].{Service:instanceName, Cost:pretaxCost}" -o table
```

---

## Security checklist

- [ ] No secrets committed to git (`.gitignore` covers `local.settings.json`)
- [ ] Pipeline uses scoped service principals — each can only touch its own resource group
- [ ] Function App uses Managed Identity — no SQL password in app settings
- [ ] Key Vault uses RBAC, not access policies
- [ ] Key Vault soft-delete 90 days + purge protection enabled
- [ ] SQL TLS minimum 1.2
- [ ] Function App HTTPS only, FTPS disabled, TLS 1.2 minimum
- [ ] Branch protection on `main` — PR + status checks required
- [ ] Production deploy requires manual approval

---

## Troubleshooting

### "Insufficient privileges" during role assignment
Your Azure account might not be Owner on the subscription.
Check: `az role assignment list --assignee $(az ad signed-in-user show --query id -o tsv) --query "[].roleDefinitionName"`
You need **Owner** (not just Contributor) to assign roles. If you're missing it — go to Azure Portal → Subscriptions → your sub → Access control (IAM) → confirm your role.

### Pipeline fails with "No hosted parallelism"
Free Azure DevOps orgs need a free parallel job request for public pipelines, or $40/month for private.
Request free tier at: https://aka.ms/azpipelines-parallelism-request (takes 2–3 business days).
Alternative: use **self-hosted agent** on your machine — ask for instructions.

### SQL deploy fails: "Server name already taken"
SQL server names are globally unique. Edit `infra/parameters/dev.bicepparam` and change the prefix, or add your initials to the name in `main.bicep`:
```bicep
var prefix = 'mobileapp-cs-${environment}'
```

### Key Vault name already taken
Same issue — Key Vault names are globally unique. Add a suffix to the name in `main.bicep`.
