# GitHub Actions OIDC Setup - Complete

## Service Principal Created

Service Principal for GitHub Actions has been created with federated credentials for OIDC authentication.

### Service Principal Details
- **Name**: simple-dotnet-gh-actions
- **App ID (Client ID)**: `7f6bf610-26c5-4939-a9f2-8ace6c78d8a8`
- **Tenant ID**: `e785dc6c-c060-43ad-bf82-d76d697e0150`
- **Subscription ID**: `2dfe4d22-c863-423e-b8c1-d4665a1593ff`
- **Role**: Contributor on subscription level

### Federated Credentials Configured

✅ Federated credential created for: `repo:neilghosh/simple-dontnet-service:ref:refs/heads/main`

This allows GitHub Actions to authenticate to Azure without storing credentials or secrets in GitHub.

---

## GitHub Secrets to Configure

Navigate to your repository on GitHub:

**Path**: Settings → Secrets and variables → Actions → New repository secret

### Required Secrets (Copy-Paste Values)

#### 1. AZURE_CLIENT_ID
```
7f6bf610-26c5-4939-a9f2-8ace6c78d8a8
```

#### 2. AZURE_TENANT_ID
```
e785dc6c-c060-43ad-bf82-d76d697e0150
```

#### 3. AZURE_SUBSCRIPTION_ID
```
2dfe4d22-c863-423e-b8c1-d4665a1593ff
```

---

## How to Set Secrets in GitHub UI

1. Go to your repository: https://github.com/neilghosh/simple-dontnet-service
2. Click **Settings** tab
3. Click **Secrets and variables** → **Actions** in the left sidebar
4. Click **New repository secret** button
5. For each secret:
   - **Name**: (e.g., `AZURE_CLIENT_ID`)
   - **Secret**: Paste the value from above
   - Click **Add secret**

---

## What's Different: OIDC vs Credentials

### ✅ OIDC (What We're Using Now)
- No storing credentials in GitHub
- Token is generated per workflow run
- Token automatically expires after use
- More secure and follows best practices
- No credential rotation needed

### ❌ Credentials (Old Way)
- Store long-lived secrets in GitHub
- Risk of exposure if compromised
- Needs manual rotation
- Less secure

---

## Testing the Workflows

Once GitHub secrets are configured:

1. **Option A**: Push a commit to `main`
   ```bash
   git push origin main
   ```

2. **Option B**: Manually trigger workflow in GitHub UI
   - Go to **Actions** tab
   - Select workflow
   - Click **Run workflow** → **Run workflow**

3. **Monitor Progress**
   - Go to **Actions** tab
   - Click the running workflow
   - Watch build and deployment steps

---

## Expected Workflow Outputs

### On Success ✅
```
✓ build-and-test (build and run tests)
✓ deploy-to-app-service (deploy to Azure)

Output:
App Service URL: https://simple-dotnet-service-efcnewc3fpeydrad.southindia-01.azurewebsites.net
Test the API at: https://simple-dotnet-service-efcnewc3fpeydrad.southindia-01.azurewebsites.net/api/ip/outbound
```

### On Failure ❌
- Check workflow logs for error details
- Verify GitHub secrets are set correctly
- Ensure Service Principal has proper permissions

---

## Verify Secrets are Set

You can verify secrets are configured:

```bash
# Show secret names (values are hidden)
gh secret list --repo neilghosh/simple-dontnet-service
```

Expected output:
```
AZURE_CLIENT_ID         Updated 2025-10-26
AZURE_SUBSCRIPTION_ID   Updated 2025-10-26
AZURE_TENANT_ID         Updated 2025-10-26
```

---

## Next Steps

1. ✅ Go to GitHub repository settings
2. ✅ Add the 3 secrets listed above
3. ✅ Trigger a workflow by pushing to main
4. ✅ Monitor the Actions tab for deployment progress
5. ✅ Test the deployed API endpoint with HTTPS

---

## Security Best Practices

✅ Using federated credentials (OIDC)
✅ No long-lived secrets in GitHub
✅ Service Principal scoped to specific subscription
✅ GitHub repo scoped to specific branch
✅ Automatic token expiration

---

## Troubleshooting

### "AADSTS70025: The client has no configured federated identity credentials"
- ✅ Already fixed - federated credentials are configured
- May need to wait 1-2 minutes for Azure to propagate changes
- Try running workflow again

### "Invalid login credentials"
- Verify all 3 GitHub secrets are set
- Check for typos in secret values
- Ensure secret names exactly match (case-sensitive)

### Workflow times out
- Azure login usually takes 5-10 seconds
- App Service deployment can take 30-60 seconds
- Wait longer before concluding it failed

---

## References

- [GitHub Actions Azure Login with OIDC](https://github.com/Azure/login)
- [Azure Federated Credentials](https://learn.microsoft.com/en-us/azure/active-directory/workload-identities/workload-identity-federation)
- [App Service Deployment](https://learn.microsoft.com/en-us/azure/app-service/)
