# Azure Container Instances Deployment Guide

This document provides step-by-step instructions to set up and deploy the Simple DotNet Service to Azure Container Instances (ACI) using the CI/CD pipeline.

## Overview

The deployment uses:
- **Azure Container Registry (ACR)**: Stores Docker images
- **Azure Container Instances (ACI)**: Runs the containerized application
- **GitHub Actions**: Automates build, test, and deployment

## Prerequisites

- Azure subscription with appropriate permissions
- Azure CLI installed locally (for setup)
- GitHub repository with proper access

## Cost Considerations

Azure Container Instances is cost-effective because:
- **Pay-per-second billing**: Only charged when container is running
- **No always-on costs**: Can stop containers when not needed
- **No infrastructure management**: Serverless container hosting
- **Estimated cost**: ~$0.0000012/second for 1 vCPU and 1.5 GB memory

To minimize costs:
- Stop containers when not in use: `az container stop --name simple-dotnet-service-aci --resource-group <resource-group>`
- Delete containers after testing: `az container delete --name simple-dotnet-service-aci --resource-group <resource-group>`

## Azure Setup Steps

### 1. Create a Resource Group

```bash
az login

# Create resource group
az group create \
  --name simple-dotnet-service-rg \
  --location eastus
```

### 2. Create Azure Container Registry

```bash
# Create container registry (name must be globally unique, lowercase alphanumeric only)
az acr create \
  --resource-group simple-dotnet-service-rg \
  --name <your-unique-registry-name> \
  --sku Basic \
  --admin-enabled true

# Example:
# az acr create \
#   --resource-group simple-dotnet-service-rg \
#   --name simpledotnetserviceacr \
#   --sku Basic \
#   --admin-enabled true
```

### 3. Get Azure Container Registry Credentials

```bash
# Get registry login server
az acr show \
  --name <your-registry-name> \
  --resource-group simple-dotnet-service-rg \
  --query loginServer \
  --output tsv

# Get registry username
az acr credential show \
  --name <your-registry-name> \
  --resource-group simple-dotnet-service-rg \
  --query username \
  --output tsv

# Get registry password
az acr credential show \
  --name <your-registry-name> \
  --resource-group simple-dotnet-service-rg \
  --query passwords[0].value \
  --output tsv
```

### 4. Create Azure Service Principal

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

### 5. Configure GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions, and add the following secrets:

| Secret Name | Description | How to Get |
|------------|-------------|------------|
| `AZURE_CREDENTIALS` | Azure service principal credentials | Output from step 4 (entire JSON) |
| `AZURE_CONTAINER_REGISTRY` | ACR login server | e.g., `simpledotnetserviceacr.azurecr.io` |
| `REGISTRY_USERNAME` | ACR username | From step 3 |
| `REGISTRY_PASSWORD` | ACR password | From step 3 |
| `AZURE_RESOURCE_GROUP` | Resource group name | `simple-dotnet-service-rg` |

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

**AZURE_CONTAINER_REGISTRY**:
```
simpledotnetserviceacr.azurecr.io
```

**REGISTRY_USERNAME**:
```
simpledotnetserviceacr
```

**REGISTRY_PASSWORD**:
```
<password-from-step-3>
```

**AZURE_RESOURCE_GROUP**:
```
simple-dotnet-service-rg
```

## Required Azure Permissions

The service principal needs the following permissions:

1. **Contributor** role on the resource group
2. Access to:
   - Create/update/delete Azure Container Instances
   - Push images to Azure Container Registry
   - Read ACR credentials

## Deployment Workflow

Once secrets are configured, the workflow will automatically:

1. **On Push to `main`** or **Manual Trigger**:
   - Build and test the .NET application
   - Build Docker image
   - Push image to Azure Container Registry
   - Deploy container to Azure Container Instances

2. **On Pull Request**:
   - Build and test only (no deployment)

## Testing the Deployment

After successful deployment, the workflow will output the container URL. Test the API endpoints:

```bash
# Get the FQDN
FQDN=$(az container show \
  --resource-group simple-dotnet-service-rg \
  --name simple-dotnet-service-aci \
  --query ipAddress.fqdn \
  --output tsv)

# Test outbound IP endpoint
curl http://$FQDN:8080/api/ip/outbound

# Test inbound IP endpoint
curl http://$FQDN:8080/api/ip/inbound

# Test headers endpoint
curl http://$FQDN:8080/api/ip/headers
```

## Managing the Container Instance

### View Container Logs

```bash
az container logs \
  --resource-group simple-dotnet-service-rg \
  --name simple-dotnet-service-aci
```

### Stop Container Instance

```bash
az container stop \
  --resource-group simple-dotnet-service-rg \
  --name simple-dotnet-service-aci
```

### Start Container Instance

```bash
az container start \
  --resource-group simple-dotnet-service-rg \
  --name simple-dotnet-service-aci
```

### Delete Container Instance

```bash
az container delete \
  --resource-group simple-dotnet-service-rg \
  --name simple-dotnet-service-aci \
  --yes
```

### View Container Details

```bash
az container show \
  --resource-group simple-dotnet-service-rg \
  --name simple-dotnet-service-aci
```

## Troubleshooting

### Container won't start

1. Check container logs:
   ```bash
   az container logs \
     --resource-group simple-dotnet-service-rg \
     --name simple-dotnet-service-aci
   ```

2. Verify environment variables are set correctly in the workflow

### Cannot push to ACR

1. Verify ACR credentials are correct in GitHub Secrets
2. Ensure admin user is enabled on ACR:
   ```bash
   az acr update \
     --name <your-registry-name> \
     --admin-enabled true
   ```

### Deployment fails

1. Check GitHub Actions workflow logs
2. Verify Azure credentials have proper permissions
3. Ensure resource group exists
4. Check if container name is already in use

## Updating the Deployment

The deployment automatically updates when you:
1. Push changes to the `main` branch
2. Manually trigger the workflow from GitHub Actions

Each deployment:
- Creates a new Docker image with a unique tag (GitHub SHA)
- Updates the container instance to use the new image
- Maintains the latest tag for easy rollback

## Cleanup

To remove all Azure resources:

```bash
# Delete the entire resource group (includes ACR and ACI)
az group delete \
  --name simple-dotnet-service-rg \
  --yes \
  --no-wait
```

## Security Best Practices

1. **Rotate credentials regularly**: Update ACR passwords and service principal secrets
2. **Use managed identities**: Consider upgrading to managed identities for production
3. **Network security**: Configure network policies and firewall rules as needed
4. **Monitor access**: Enable Azure Monitor and logging for security auditing
5. **Least privilege**: Grant minimum required permissions to service principals

## Cost Monitoring

Monitor costs in Azure Portal:
1. Go to Cost Management + Billing
2. View costs by resource group
3. Set up budget alerts

## Additional Resources

- [Azure Container Instances Documentation](https://docs.microsoft.com/azure/container-instances/)
- [Azure Container Registry Documentation](https://docs.microsoft.com/azure/container-registry/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)
