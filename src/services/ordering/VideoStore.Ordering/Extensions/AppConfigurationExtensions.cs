using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace VideoStore.Ordering.Extensions
{
    public static class AppConfigurationExtensions
    {
        public static void ConfigureAzureKeyVault(this IConfigurationBuilder configurationBuilder)
        {
            var configuration = configurationBuilder.Build();
            string tenantId = configuration["KeyVaultConfiguration:TenantId"] ?? throw new NullReferenceException("AzureKeyVault TenantId must have a value.");
            string keyVaultURL = configuration["KeyVaultConfiguration:KeyVaultURL"] ?? throw new NullReferenceException("Key Vault Url must have a value.");
            string keyVaultClientId = configuration["KeyVaultConfiguration:ClientId"] ?? throw new NullReferenceException("AzureKeyVault ClientId must have a value.");
            string keyVaultClientSecret = configuration["KeyVaultConfiguration:ClientSecret"] ?? throw new NullReferenceException("AzureKeyVault ClientSecret must have a value.");

            var secretClient = new SecretClient(
                new Uri(keyVaultURL), 
                new ClientSecretCredential(tenantId , keyVaultClientId, keyVaultClientSecret));

            // One way
            configurationBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions
            {
                ReloadInterval = TimeSpan.FromMinutes(5)
            });

            // Second way
            //configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
        }
    }
}
