# Quick Setup Guide for Azure Container Instances Deployment

This is a quick reference guide to get your deployment up and running. For detailed information, see [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md).

## Prerequisites Checklist

- [ ] Azure subscription with active account
- [ ] Azure CLI installed (`az --version` to verify)
- [ ] GitHub repository access (to configure secrets)

## Step-by-Step Setup (5 minutes)

### 1. Login to Azure

```bash
az login
```

### 2. Create Azure Resources

```bash
# Set variables (customize these)
RESOURCE_GROUP="simple-dotnet-service-rg"
REGISTRY_NAME="youruniqueregistryname"  # Must be globally unique, lowercase letters and numbers only (no hyphens, underscores, or special characters)
LOCATION="eastus"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create container registry
az acr create \
  --resource-group $RESOURCE_GROUP \
  --name $REGISTRY_NAME \
  --sku Basic \
  --admin-enabled true
```

### 3. Get Azure Credentials

Run these commands and save the outputs:

```bash
# 1. Get ACR Login Server
az acr show --name $REGISTRY_NAME --query loginServer --output tsv
# Save this as: AZURE_CONTAINER_REGISTRY

# 2. Get ACR Username
az acr credential show --name $REGISTRY_NAME --query username --output tsv
# Save this as: REGISTRY_USERNAME

# 3. Get ACR Password
az acr credential show --name $REGISTRY_NAME --query passwords[0].value --output tsv
# Save this as: REGISTRY_PASSWORD

# 4. Create Service Principal (save entire JSON output)
SUBSCRIPTION_ID=$(az account show --query id --output tsv)
az ad sp create-for-rbac \
  --name "simple-dotnet-service-github-actions" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP \
  --sdk-auth
# Save entire JSON as: AZURE_CREDENTIALS
```

### 4. Configure GitHub Secrets

Go to: **Your GitHub Repository** → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Add these 5 secrets:

| Secret Name | Value | Example |
|------------|-------|---------|
| `AZURE_CREDENTIALS` | Entire JSON from service principal | `{"clientId": "xxx", ...}` |
| `AZURE_CONTAINER_REGISTRY` | ACR login server | `youruniqueregistryname.azurecr.io` |
| `REGISTRY_USERNAME` | ACR username | `youruniqueregistryname` |
| `REGISTRY_PASSWORD` | ACR password | `<long-random-string>` |
| `AZURE_RESOURCE_GROUP` | Resource group name | `simple-dotnet-service-rg` |

### 5. Deploy

Choose one option:

**Option A: Automatic Deploy**
- Push changes to `main` branch
- Workflow automatically runs

**Option B: Manual Deploy**
- Go to **Actions** tab in GitHub
- Select "Build and Deploy to Azure Container Instances"
- Click "Run workflow"
- Select `main` branch
- Click "Run workflow"

### 6. Test Your Deployment

After deployment completes (check Actions tab), get your container URL:

```bash
# Get container URL
FQDN=$(az container show \
  --resource-group $RESOURCE_GROUP \
  --name simple-dotnet-service-aci \
  --query ipAddress.fqdn \
  --output tsv)

echo "Your API URL: http://$FQDN:8080"

# Test the API
curl http://$FQDN:8080/api/ip/outbound
curl http://$FQDN:8080/api/ip/inbound
curl http://$FQDN:8080/api/ip/headers
```

## Common Issues

### Issue: "Registry not found" or "Authentication failed"
**Solution**: Verify ACR credentials are correct in GitHub Secrets

### Issue: "Resource group not found"
**Solution**: Ensure resource group name matches in GitHub Secret `AZURE_RESOURCE_GROUP`

### Issue: Container won't start
**Solution**: Check logs:
```bash
az container logs --resource-group $RESOURCE_GROUP --name simple-dotnet-service-aci
```

### Issue: Workflow fails with "AZURE_CREDENTIALS invalid"
**Solution**: Ensure you copied the ENTIRE JSON output from service principal creation, including the curly braces

## Cost Management

### Stop Container (when not in use)
```bash
az container stop --resource-group $RESOURCE_GROUP --name simple-dotnet-service-aci
```

### Start Container (when needed)
```bash
az container start --resource-group $RESOURCE_GROUP --name simple-dotnet-service-aci
```

### Delete Everything (cleanup)
```bash
# Delete entire resource group (includes all resources)
az group delete --name $RESOURCE_GROUP --yes
```

## Getting Help

- **Detailed Guide**: [AZURE_DEPLOYMENT.md](AZURE_DEPLOYMENT.md)
- **Azure Docs**: https://learn.microsoft.com/azure/container-instances/
- **GitHub Actions Logs**: Check the Actions tab in your repository

## What's Next?

Once deployed:
- ✅ Your API is accessible via public URL
- ✅ Automatic deployments on every push to `main`
- ✅ Pay only for running time
- ✅ Easy to start/stop for cost control

**Pro Tip**: Set up Azure budgets and alerts to monitor costs in the Azure Portal.
