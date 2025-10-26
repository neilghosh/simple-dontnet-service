# GitHub Workflows Migration to App Service

## Overview
The GitHub Actions workflows have been updated to deploy to **Azure App Service** instead of Azure Container Instances (ACI), with **Managed Identity (MI)** authentication instead of credentials.

## Deployment Details

### New Deployment Environment
- **Service**: Azure App Service
- **Region**: South India (westindia)
- **App Name**: `simple-dotnet-service`
- **Resource Group**: `simple-dotnet-service-rg`
- **App Service Plan**: ASP-simpledotnetservicerg-b0b4 (Free tier)
- **Default URL**: https://simple-dotnet-service-efcnewc3fpeydrad.southindia-01.azurewebsites.net

### Benefits of App Service
✅ Built-in HTTPS support (automatic certificate on port 443)
✅ Managed Identity authentication (no credential secrets needed)
✅ Zero-downtime deployments
✅ Automatic scaling options
✅ Better CI/CD integration

## Workflows Updated

### 1. `azure-app-service-deploy.yml`
**Purpose**: Deploy compiled .NET code directly to App Service

**Key Changes**:
- ✅ Hardcoded resource values (no secrets needed)
- ✅ Migrated from `AZURE_CREDENTIALS` to Managed Identity auth
- ✅ Uses `azure/login@v2` with `client-id`, `tenant-id`, `subscription-id`
- ✅ Updates app URL output to use dynamic DNS lookup

**Trigger**: Push to `main` branch or manual `workflow_dispatch`

**Steps**:
1. Build and test .NET project
2. Publish release build
3. Login to Azure using Managed Identity
4. Deploy to App Service
5. Output the HTTPS URL for API testing

### 2. `azure-container-deploy.yml`
**Purpose**: Build Docker image, push to ACR, deploy to App Service

**Key Changes**:
- ✅ Renamed from "Build and Deploy to Azure Container Instances"
- ✅ Now "Build and Deploy Docker to Azure App Service"
- ✅ ACR login uses Managed Identity (`az acr login`)
- ✅ Migrated from credential-based ACR auth
- ✅ Docker image deployment to App Service (not ACI)

**Trigger**: Push to `main` branch or manual `workflow_dispatch`

**Steps**:
1. Build and test .NET project
2. Login to Azure using Managed Identity
3. Login to ACR using Managed Identity
4. Build and push Docker image to ACR
5. Deploy Docker image to App Service
6. Output the HTTPS URL for API testing

## GitHub Secrets Required

The following secrets must be configured in GitHub repository settings:

```
AZURE_CLIENT_ID           # Service Principal / Managed Identity Client ID
AZURE_TENANT_ID          # Azure AD Tenant ID
AZURE_SUBSCRIPTION_ID    # Azure Subscription ID
```

### How to Set Up GitHub Secrets

1. Go to GitHub repository → **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Add each secret with the values from your Azure environment:

```bash
# Get these values from Azure
az account show --query "{subscriptionId:id, tenantId:tenantId}" -o json
```

### Federated Credentials (Recommended)

For enhanced security, configure federated credentials instead of storing secrets:

1. Create Service Principal or User-Assigned Managed Identity in Azure
2. Configure federated credential for GitHub
3. GitHub Actions automatically exchanges token without storing secrets

## Testing the Workflows

### Trigger a Deployment

```bash
# Option 1: Push to main
git push origin main

# Option 2: Manual trigger via GitHub CLI
gh workflow run azure-app-service-deploy.yml

# Option 3: Manual trigger from GitHub web UI
# Go to Actions → Select workflow → Run workflow
```

### Monitor Deployment

1. Go to GitHub → **Actions** tab
2. Click the workflow run
3. View step-by-step logs
4. Check final HTTPS URL output

### Verify Deployment

```bash
# Test the API endpoint (HTTPS)
curl https://simple-dotnet-service-efcnewc3fpeydrad.southindia-01.azurewebsites.net/api/ip/outbound

# Expected response:
# {"ipAddress": "x.x.x.x"}
```

## Environment Variables in App Service

App Service automatically sets these environment variables:

| Variable | Value |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:80;https://+:443` |
| `WEBSITE_RUN_FROM_PACKAGE` | `1` |

These are configured during deployment and don't need manual setup.

## Migration from ACI to App Service

### What Changed
| Aspect | ACI | App Service |
|--------|-----|------------|
| **Protocol** | HTTP:8080 | HTTPS:443 (automatic) |
| **Certificate** | Manual setup | Azure-managed (free) |
| **Authentication** | Credentials | Managed Identity |
| **Default Port** | 8080 | 80 (redirects to 443) |
| **Deployment** | Docker only | Code or Docker |

### Old ACI Deployment (Deprecated)
```
http://simple-dotnet-service-15.eastus.azurecontainer.io:8080/api/ip/outbound
```

### New App Service Deployment (Current)
```
https://simple-dotnet-service-efcnewc3fpeydrad.southindia-01.azurewebsites.net/api/ip/outbound
```

## Troubleshooting

### Workflow Fails with "Managed Identity Error"
- ✅ Verify GitHub secrets are set correctly
- ✅ Check Service Principal has permissions on App Service
- ✅ Ensure Federated Credentials are configured

### App Service Shows 404
- ✅ Verify app deployed successfully (check Azure Portal)
- ✅ Check Application Insights logs
- ✅ Verify endpoint paths match controller routes

### Docker Image Not Pulling from ACR
- ✅ Verify Docker image exists in ACR: `az acr repository list --name simpledotnetregistry565`
- ✅ Check App Service has pull permissions to ACR
- ✅ Verify image tag is correct

## Next Steps

1. ✅ Configure GitHub Secrets (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID)
2. ✅ Test workflow by pushing to main or using manual trigger
3. ✅ Monitor Application Insights for any errors
4. ✅ Update any documentation pointing to old ACI URL
5. ✅ (Optional) Decommission ACI after confirming stability

## References

- [Azure App Service Documentation](https://learn.microsoft.com/en-us/azure/app-service/)
- [GitHub Actions Azure Login](https://github.com/Azure/login)
- [Azure Managed Identity](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)
