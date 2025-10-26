# Quick Setup Guide for Azure App Service Deployment

This is a quick reference guide to get your deployment up and running on Azure App Service. For detailed information, see [AZURE_APP_SERVICE_DEPLOYMENT.md](AZURE_APP_SERVICE_DEPLOYMENT.md).

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
APP_SERVICE_PLAN="simple-dotnet-service-plan"
WEBAPP_NAME="youruniqueappname"  # Must be globally unique (letters, numbers, hyphens only)
LOCATION="eastus"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service Plan (B1 = Basic tier with SSL support)
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNETCORE:8.0"

# Enable HTTPS only
az webapp update \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --https-only true
```

### 3. Get Azure Credentials

Run these commands and save the outputs:

```bash
# Create Service Principal (save entire JSON output)
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

Add these 2 secrets:

| Secret Name | Value | Example |
|------------|-------|---------|
| `AZURE_CREDENTIALS` | Entire JSON from service principal | `{"clientId": "xxx", ...}` |
| `AZURE_WEBAPP_NAME` | Web app name | `youruniqueappname` |

### 5. Deploy

Choose one option:

**Option A: Automatic Deploy**
- Push changes to `main` branch
- Workflow automatically runs

**Option B: Manual Deploy**
- Go to **Actions** tab in GitHub
- Select "Build and Deploy to Azure App Service"
- Click "Run workflow"
- Select `main` branch
- Click "Run workflow"

### 6. Test Your Deployment

After deployment completes (check Actions tab), test your API:

```bash
# Your app URL (automatically uses HTTPS)
echo "Your API URL: https://$WEBAPP_NAME.azurewebsites.net"

# Test the API endpoints
curl https://$WEBAPP_NAME.azurewebsites.net/api/ip/outbound
curl https://$WEBAPP_NAME.azurewebsites.net/api/ip/inbound
curl https://$WEBAPP_NAME.azurewebsites.net/api/ip/headers
```

## Key Features

✅ **HTTPS by Default**: Automatic SSL/TLS on port 443
✅ **Managed Platform**: No infrastructure management
✅ **Easy Scaling**: Scale up/out as needed
✅ **Built-in Monitoring**: Application Insights integration
✅ **Custom Domains**: Add your own domain names
✅ **Zero Downtime**: With deployment slots (Standard tier+)

## Common Issues

### Issue: "The name is not available"
**Solution**: Choose a different WEBAPP_NAME - it must be globally unique across all Azure App Services

### Issue: "AZURE_CREDENTIALS invalid"
**Solution**: Ensure you copied the ENTIRE JSON output from service principal creation, including curly braces

### Issue: App shows "Application Error"
**Solution**: Check logs:
```bash
az webapp log tail --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP
```

### Issue: HTTP not redirecting to HTTPS
**Solution**: Verify HTTPS-only is enabled:
```bash
az webapp update --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP --https-only true
```

## Cost Management

### App Service Plan Pricing
- **Free (F1)**: Free, 60 CPU min/day allocation (no SSL/custom domains)
- **Basic (B1)**: ~$13/month, includes SSL and custom domains
- **Standard (S1)**: ~$70/month, includes deployment slots and autoscaling

### Stop App Service (when not in use)
```bash
az webapp stop --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP
```

### Start App Service (when needed)
```bash
az webapp start --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP
```

### Delete Everything (cleanup)
```bash
# Delete entire resource group (includes all resources)
az group delete --name $RESOURCE_GROUP --yes
```

## Viewing Logs

### Enable and stream logs
```bash
# Enable logging
az webapp log config \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --application-logging filesystem

# Stream logs in real-time
az webapp log tail \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP
```

## Getting Help

- **Detailed Guide**: [AZURE_APP_SERVICE_DEPLOYMENT.md](AZURE_APP_SERVICE_DEPLOYMENT.md)
- **Azure Docs**: https://learn.microsoft.com/azure/app-service/
- **GitHub Actions Logs**: Check the Actions tab in your repository

## What's Next?

Once deployed:
- ✅ Your API is accessible via HTTPS at `https://<your-app>.azurewebsites.net`
- ✅ Automatic deployments on every push to `main`
- ✅ Built-in SSL/TLS certificates (managed automatically)
- ✅ Easy to scale and monitor

## Comparison: App Service vs Container Instances

**Choose App Service when you want:**
- Built-in HTTPS support on port 443
- Custom domain names with managed SSL
- Deployment slots for staging/production
- Integrated monitoring and scaling
- Fully managed platform

**Choose Container Instances when you want:**
- Pay-per-second billing
- Docker-first deployment
- More control over container configuration
- Lower cost for intermittent workloads

**Pro Tip**: Use Basic (B1) tier for production apps that need SSL and custom domains. Use Free (F1) tier only for testing (no SSL support).
