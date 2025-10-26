# Azure App Service Deployment Guide

This document provides step-by-step instructions to set up and deploy the Simple DotNet Service to Azure App Service using the CI/CD pipeline.

## Overview

The deployment uses:
- **Azure App Service**: Managed platform for hosting web applications
- **GitHub Actions**: Automates build, test, and deployment

## Why Azure App Service?

Azure App Service offers several advantages over Container Instances:

### Key Benefits
- **HTTPS Support**: Built-in SSL/TLS certificates with automatic renewal
- **Custom Domains**: Easy integration with custom domain names
- **Automatic Scaling**: Scale up/out based on demand
- **Deployment Slots**: Blue-green deployments for zero-downtime updates
- **Managed Platform**: Automatic OS and runtime patching
- **Application Insights**: Built-in monitoring and diagnostics
- **Default Port 443**: HTTPS traffic on standard port without configuration

### Cost Considerations

Azure App Service pricing depends on the App Service Plan:
- **Free Tier**: 60 CPU minutes/day allocation, great for testing
- **Basic B1**: ~$13/month, includes custom domains and SSL
- **Standard S1**: ~$70/month, includes deployment slots and autoscaling
- **Premium**: Enhanced performance and advanced features

To minimize costs:
- Use Free or Basic tier for development/testing
- Stop App Service when not in use (Basic tier and above)
- Use deployment slots to test before production

## Prerequisites

- Azure subscription with appropriate permissions
- Azure CLI installed locally (for setup)
- GitHub repository with proper access

## Azure Setup Steps

### 1. Create a Resource Group

```bash
az login

# Create resource group
az group create \
  --name simple-dotnet-service-rg \
  --location eastus
```

### 2. Create Azure App Service Plan

```bash
# Create App Service Plan (B1 = Basic tier)
az appservice plan create \
  --name simple-dotnet-service-plan \
  --resource-group simple-dotnet-service-rg \
  --sku B1 \
  --is-linux

# For Free tier (limited compute time):
# az appservice plan create \
#   --name simple-dotnet-service-plan \
#   --resource-group simple-dotnet-service-rg \
#   --sku F1 \
#   --is-linux
```

### 3. Create Azure App Service Web App

```bash
# Create Web App (name must be globally unique)
az webapp create \
  --name <your-unique-app-name> \
  --resource-group simple-dotnet-service-rg \
  --plan simple-dotnet-service-plan \
  --runtime "DOTNETCORE:8.0"

# Example:
# az webapp create \
#   --name simple-dotnet-service-app \
#   --resource-group simple-dotnet-service-rg \
#   --plan simple-dotnet-service-plan \
#   --runtime "DOTNETCORE:8.0"
```

### 4. Enable HTTPS Only (Recommended)

```bash
# Redirect all HTTP traffic to HTTPS
az webapp update \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg \
  --https-only true
```

### 5. Create Azure Service Principal

Create a service principal for GitHub Actions to authenticate with Azure:

```bash
# Get subscription ID
SUBSCRIPTION_ID=$(az account show --query id --output tsv)

# Create service principal with Contributor role
az ad sp create-for-rbac \
  --name "simple-dotnet-service-github-actions" \
  --role Contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID/resourceGroups/simple-dotnet-service-rg \
  --sdk-auth
```

This command outputs JSON credentials. Save this entire JSON output for the next step.

**Important**: Keep these credentials secure and never commit them to version control.

### 6. Configure GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions, and add the following secrets:

| Secret Name | Description | How to Get |
|------------|-------------|------------|
| `AZURE_CREDENTIALS` | Azure service principal credentials | Output from step 5 (entire JSON) |
| `AZURE_WEBAPP_NAME` | App Service web app name | Your unique app name from step 3 |

#### Example Secret Values:

**AZURE_CREDENTIALS** (entire JSON from service principal creation):
```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "your-client-secret",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

**AZURE_WEBAPP_NAME**:
```
simple-dotnet-service-app
```

## Required Azure Permissions

The service principal needs the following permissions:

1. **Contributor** role on the resource group
2. Access to:
   - Deploy to Azure App Service
   - Update App Service configuration
   - Read App Service details

## Deployment Workflow

Once secrets are configured, the workflow will automatically:

1. **On Push to `main`** or **Manual Trigger**:
   - Build and test the .NET application
   - Publish the application
   - Deploy to Azure App Service

2. **On Pull Request**:
   - Build and test only (no deployment)

## Testing the Deployment

After successful deployment, the workflow will output the App Service URL. Test the API endpoints:

```bash
# Test with HTTPS (default)
curl https://<your-app-name>.azurewebsites.net/api/ip/outbound
curl https://<your-app-name>.azurewebsites.net/api/ip/inbound
curl https://<your-app-name>.azurewebsites.net/api/ip/headers

# Example:
# curl https://simple-dotnet-service-app.azurewebsites.net/api/ip/outbound
```

## Managing the App Service

### View Application Logs

```bash
# Enable logging
az webapp log config \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg \
  --application-logging filesystem \
  --level information

# Stream logs
az webapp log tail \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg
```

### Stop App Service

```bash
az webapp stop \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg
```

### Start App Service

```bash
az webapp start \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg
```

### Restart App Service

```bash
az webapp restart \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg
```

### Delete App Service

```bash
# Delete web app only
az webapp delete \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg

# Delete App Service Plan (if no other apps are using it)
az appservice plan delete \
  --name simple-dotnet-service-plan \
  --resource-group simple-dotnet-service-rg
```

### View App Service Details

```bash
az webapp show \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg
```

## Advanced Configuration

### Configure Custom Domain

```bash
# Add custom domain
az webapp config hostname add \
  --webapp-name <your-app-name> \
  --resource-group simple-dotnet-service-rg \
  --hostname <your-custom-domain>

# Bind SSL certificate
az webapp config ssl bind \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg
```

### Enable Application Insights

```bash
# Create Application Insights instance
az monitor app-insights component create \
  --app simple-dotnet-service-insights \
  --location eastus \
  --resource-group simple-dotnet-service-rg

# Link to App Service
az webapp config appsettings set \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="<connection-string>"
```

### Configure Environment Variables

```bash
# Set environment variables (app settings)
az webapp config appsettings set \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg \
  --settings ASPNETCORE_ENVIRONMENT=Production
```

### Configure Deployment Slots (Standard tier and above)

```bash
# Create staging slot
az webapp deployment slot create \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg \
  --slot staging

# Swap staging to production
az webapp deployment slot swap \
  --name <your-app-name> \
  --resource-group simple-dotnet-service-rg \
  --slot staging \
  --target-slot production
```

## Troubleshooting

### App won't start

1. Check application logs:
   ```bash
   az webapp log tail \
     --name <your-app-name> \
     --resource-group simple-dotnet-service-rg
   ```

2. Verify runtime is set correctly:
   ```bash
   az webapp config show \
     --name <your-app-name> \
     --resource-group simple-dotnet-service-rg
   ```

3. Ensure ASPNETCORE_ENVIRONMENT is set properly

### Deployment fails

1. Check GitHub Actions workflow logs
2. Verify Azure credentials have proper permissions
3. Ensure resource group and App Service exist
4. Check App Service Plan has available capacity

### HTTPS issues

1. Verify HTTPS-only is enabled:
   ```bash
   az webapp show \
     --name <your-app-name> \
     --resource-group simple-dotnet-service-rg \
     --query httpsOnly
   ```

2. Check SSL bindings:
   ```bash
   az webapp config ssl list \
     --resource-group simple-dotnet-service-rg
   ```

## Updating the Deployment

The deployment automatically updates when you:
1. Push changes to the `main` branch
2. Manually trigger the workflow from GitHub Actions

Each deployment:
- Builds and tests the application
- Publishes the compiled application
- Deploys to Azure App Service
- Provides zero-downtime updates (with deployment slots)

## Cleanup

To remove all Azure resources:

```bash
# Delete the entire resource group (includes App Service and Plan)
az group delete \
  --name simple-dotnet-service-rg \
  --yes \
  --no-wait
```

## Security Best Practices

1. **Rotate credentials regularly**: Update service principal secrets
2. **Use managed identities**: Consider upgrading to managed identities for production
3. **Enable HTTPS only**: Always redirect HTTP to HTTPS
4. **Configure firewall rules**: Restrict access by IP if needed
5. **Enable Application Insights**: Monitor security events and anomalies
6. **Least privilege**: Grant minimum required permissions to service principals
7. **Use deployment slots**: Test changes in staging before production

## Cost Monitoring

Monitor costs in Azure Portal:
1. Go to Cost Management + Billing
2. View costs by resource group
3. Set up budget alerts

### Cost Optimization Tips

- Use Free tier for development and testing
- Stop App Service during non-business hours (requires Basic tier or higher)
- Use Basic tier for simple applications without advanced features
- Monitor usage and adjust App Service Plan as needed

## Comparing with Container Instances

| Feature | App Service | Container Instances |
|---------|-------------|---------------------|
| HTTPS Support | Built-in, automatic | Supported, requires setup |
| Custom Domains | Native support | Requires additional configuration |
| SSL Certificates | Automatic, managed | Manual configuration |
| Deployment Slots | Yes (Standard+) | No |
| Auto-scaling | Yes (Standard+) | Manual scaling |
| Monitoring | Application Insights | Container logs |
| Port 443 | Default for HTTPS | Configurable |
| Cost | Fixed monthly | Pay-per-second |
| Management | Fully managed | Container-managed |

## Additional Resources

- [Azure App Service Documentation](https://learn.microsoft.com/azure/app-service/)
- [Deploy .NET Apps to App Service](https://learn.microsoft.com/azure/app-service/quickstart-dotnetcore)
- [GitHub Actions for Azure](https://github.com/Azure/actions)
- [App Service Pricing](https://azure.microsoft.com/pricing/details/app-service/)
