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

            // One way
            //var secretClient = new SecretClient(
            //    new Uri(keyVaultConfig.KeyVaultUrl),
            //    new ClientSecretCredential(keyVaultConfig.TenantId, keyVaultConfig.ClientId, keyVaultConfig.ClientSecret));

            // Other way
            // The DefaultAzureCredential attempts to authenticate by using multiple mechanisms
            // environment, managed identity, Visual Studio, Azure CLI, Azure PowerShell, Interactive Browser
            var secretClient = new SecretClient(new Uri(keyVaultConfig.KeyVaultUrl), new DefaultAzureCredential());

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
