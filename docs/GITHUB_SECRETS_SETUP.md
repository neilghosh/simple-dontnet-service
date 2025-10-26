# GitHub Secrets Configuration Guide

## Overview

To enable automatic CI/CD deployments, you need to add 5 GitHub repository secrets. These allow the GitHub Actions workflow to authenticate with Azure and deploy your container automatically.

## Step-by-Step Setup

### Step 1: Get Your Current Azure Credentials

Run these commands in your terminal:

```bash
# Get Container Registry Login Server
REGISTRY_URL=$(az acr show \
  --name simpledotnetregistry565 \
  --query loginServer \
  --output tsv)
echo "AZURE_CONTAINER_REGISTRY=$REGISTRY_URL"

# Get Registry Username
REGISTRY_USER=$(az acr credential show \
  --name simpledotnetregistry565 \
  --query username \
  --output tsv)
echo "REGISTRY_USERNAME=$REGISTRY_USER"

# Get Registry Password
REGISTRY_PASS=$(az acr credential show \
  --name simpledotnetregistry565 \
  --query passwords[0].value \
  --output tsv)
echo "REGISTRY_PASSWORD=$REGISTRY_PASS"

# Get Resource Group
echo "AZURE_RESOURCE_GROUP=simple-dotnet-service-rg"
```

### Step 2: Create Service Principal

This is needed only once and provides full deployment permissions:

```bash
# Get Subscription ID
SUBSCRIPTION_ID=$(az account show --query id --output tsv)

# Create Service Principal
az ad sp create-for-rbac \
  --name "simple-dotnet-service-github-actions" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/simple-dotnet-service-rg \
  --sdk-auth
```

**Copy the entire JSON output** - you'll need this for the `AZURE_CREDENTIALS` secret.

### Step 3: Add Secrets to GitHub

1. Go to your GitHub repository
2. Click **Settings** (top right)
3. In the left sidebar, click **Secrets and variables** → **Actions**
4. Click **New repository secret**
5. Add each secret with the values obtained above:

#### Secret 1: AZURE_CREDENTIALS

**Name:** `AZURE_CREDENTIALS`

**Value:** Paste the entire JSON from Step 2 output

Example (yours will have different values):
```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "your-secret-value",
  "subscriptionId": "2dfe4d22-c863-423e-b8c1-d4665a1593ff",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

#### Secret 2: AZURE_CONTAINER_REGISTRY

**Name:** `AZURE_CONTAINER_REGISTRY`

**Value:** 
```
simpledotnetregistry565.azurecr.io
```

#### Secret 3: REGISTRY_USERNAME

**Name:** `REGISTRY_USERNAME`

**Value:**
```
simpledotnetregistry565
```

#### Secret 4: REGISTRY_PASSWORD

**Name:** `REGISTRY_PASSWORD`

**Value:** The password from Step 1 output

#### Secret 5: AZURE_RESOURCE_GROUP

**Name:** `AZURE_RESOURCE_GROUP`

**Value:**
```
simple-dotnet-service-rg
```

### Step 4: Verify Secrets Were Added

In GitHub, you should see:
- ✅ AZURE_CREDENTIALS
- ✅ AZURE_CONTAINER_REGISTRY
- ✅ REGISTRY_USERNAME
- ✅ REGISTRY_PASSWORD
- ✅ AZURE_RESOURCE_GROUP

(Secrets show masked values once saved)

## Enable Automatic Deployments

Once secrets are configured, the CI/CD pipeline will automatically:

1. **On push to `main` branch:**
   - Build the .NET application
   - Run all tests
   - Build Docker image
   - Push to Azure Container Registry
   - Deploy to Azure Container Instances
   - Update the live API endpoint

2. **Monitor deployment:**
   - Go to **Actions** tab in GitHub
   - Click the latest workflow run
   - Watch real-time progress

## Example: Making a Deployment

```bash
# Make a code change
echo "# Updated" >> README.md

# Commit and push
git add README.md
git commit -m "Update documentation"
git push origin main
```

Then watch GitHub Actions automatically:
1. Build your application
2. Build Docker image
3. Push to registry
4. Deploy to Azure
5. Update the live API

## Verify Active Deployment

After secrets are configured and you push to main:

```bash
# Check latest GitHub Actions run
# (Go to Actions tab in GitHub)

# Check container status
az container show \
  --resource-group simple-dotnet-service-rg \
  --name simple-dotnet-service-aci \
  --query "instanceView.state"

# Test the live API
curl http://simple-dotnet-service-demo.eastus.azurecontainer.io:8080/api/ip/outbound
```

## Rotate Credentials (Recommended Every 90 Days)

### Rotate ACR Password

```bash
# Generate new password
az acr credential rotate \
  --name simpledotnetregistry565 \
  --password-name password1

# Update GitHub secret REGISTRY_PASSWORD with new value
az acr credential show \
  --name simpledotnetregistry565 \
  --query passwords[0].value \
  --output tsv
```

### Rotate Service Principal

```bash
# Delete old service principal
az ad sp delete \
  --id $(az ad sp list --display-name "simple-dotnet-service-github-actions" --query '[0].id' --output tsv)

# Create new one (see Step 2)
SUBSCRIPTION_ID=$(az account show --query id --output tsv)
az ad sp create-for-rbac \
  --name "simple-dotnet-service-github-actions" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/simple-dotnet-service-rg \
  --sdk-auth

# Update GitHub secret AZURE_CREDENTIALS with new JSON
```

## Troubleshooting

### "Workflow fails with authentication error"
1. Verify all 5 secrets are set correctly in GitHub
2. Check JSON formatting for AZURE_CREDENTIALS (must be valid JSON)
3. Ensure Service Principal has Contributor role

### "Container doesn't update after push"
1. Check GitHub Actions logs for errors
2. Verify registry credentials are correct
3. Check if container already exists (may need manual delete/recreate)

### "Cannot authenticate with Azure"
1. Regenerate Service Principal:
   ```bash
   az ad sp delete --id <sp-id>
   # Then create new one (see Step 2)
   ```
2. Update AZURE_CREDENTIALS secret with new JSON

## Additional Resources

- [GitHub Actions Secrets Documentation](https://docs.github.com/en/actions/security-guides/using-secrets-in-github-actions)
- [Azure Container Registry Documentation](https://learn.microsoft.com/azure/container-registry/)
- [Azure Container Instances Documentation](https://learn.microsoft.com/azure/container-instances/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)

---

**After completing these steps, your deployment is fully automated!** 🚀

Every push to `main` will automatically:
1. Build your application
2. Create a Docker image
3. Push to your private registry
4. Deploy to Azure Container Instances
5. Update your live API endpoint

No manual intervention needed!
