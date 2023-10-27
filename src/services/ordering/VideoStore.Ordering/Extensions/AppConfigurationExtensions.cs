using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using VideoStore.Ordering.Constants;
using VideoStore.Ordering.Models;

namespace VideoStore.Ordering.Extensions
{
    public static class AppConfigurationExtensions
    {
        public static void ConfigureAzureKeyVault(this IConfigurationBuilder configurationBuilder)
        {
            var configuration = configurationBuilder.Build();
            var keyVaultConfig = configuration.GetSection(OrderingConstants.KeyVaultSectionName).Get<KeyVaultConfig>()
                ?? throw new NullReferenceException($"{nameof(KeyVaultConfig)} must have a value.");
            
            var secretClient = new SecretClient(
                new Uri(keyVaultConfig.KeyVaultUrl),
                new ClientSecretCredential(keyVaultConfig.TenantId, keyVaultConfig.ClientId, keyVaultConfig.ClientSecret));

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
