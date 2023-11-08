using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using VideoStore.IdentityService.Constants;
using VideoStore.IdentityService.Models;

namespace VideoStore.IdentityService.Extensions
{
    public static class AppConfigurationExtensions
    {
        public static void ConfigureAzureKeyVault(this IConfigurationBuilder configurationBuilder)
        {
            var configuration = configurationBuilder.Build();
            var keyVaultConfig = configuration.GetSection(IdentityConstants.KeyVaultSectionName).Get<KeyVaultConfig>()
                ?? throw new NullReferenceException($"{nameof(KeyVaultConfig)} must have a value.");

            bool appInDocker = configuration.GetValue<bool>(IdentityConstants.FeatureFlags.RunningInDocker);
            var keyVaultUri = new Uri(keyVaultConfig.KeyVaultUrl);

            // One way
            //var secretClient = new SecretClient(
            //    keyVaultUri,
            //    new ClientSecretCredential(keyVaultConfig.TenantId, keyVaultConfig.ClientId, keyVaultConfig.ClientSecret));

            SecretClient secretClient;
            if(appInDocker)
            {
                // When using Docker and docker compose, you can feed environment variables to the container and use these to get a working credential.
                // This creates a an Azure Credential, but if the DefaultAzureCredential doesn’t success,
                // will fall back to an environmentCredential.
                // This environment credential requires three variables to exist and be populated:
                // AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID
                var credential = new ChainedTokenCredential(new DefaultAzureCredential(), new EnvironmentCredential());
                secretClient = new SecretClient(keyVaultUri, credential);
            }
            else
            {
                // Other way
                // The DefaultAzureCredential attempts to authenticate by using multiple mechanisms
                // environment, managed identity, Visual Studio, Azure CLI, Azure PowerShell, Interactive Browser
                secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());
            }

            // One way
            //configurationBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions
            //{
            //    ReloadInterval = TimeSpan.FromMinutes(5)
            //});

            // Second way
            configurationBuilder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
        }
    }
}
