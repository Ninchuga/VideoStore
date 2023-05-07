using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using VideoStore.Movies.Constants;
using VideoStore.Movies.Models;

namespace VideoStore.Movies.Extensions
{
    public static class AppConfigurationExtensions
    {
        public static void ConfigureAzureKeyVault(this IConfigurationBuilder configurationBuilder)
        {
            var configuration = configurationBuilder.Build();
            var keyVaultConfig = configuration.GetSection(MoviesConstants.KeyVaultSectionName).Get<KeyVaultConfig>()
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
