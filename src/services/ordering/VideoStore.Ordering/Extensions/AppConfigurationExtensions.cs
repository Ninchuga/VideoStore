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
            var tenantId = configuration["KeyVaultConfiguration:TenantId"];
            var keyVaultURL = configuration["KeyVaultConfiguration:KeyVaultURL"];
            var keyVaultClientId = configuration["KeyVaultConfiguration:ClientId"];
            var keyVaultClientSecret = configuration["KeyVaultConfiguration:ClientSecret"];

            var secretClient = new SecretClient(
                new Uri(keyVaultURL), 
                new ClientSecretCredential(tenantId , keyVaultClientId, keyVaultClientSecret));

            // One way
            configurationBuilder.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions
            {
                // Manager = new PrefixKeyVaultSecretManager(secretPrefix),
                ReloadInterval = TimeSpan.FromMinutes(5)
            });

            // Second way
            //var miCredentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            //{
            //    ManagedIdentityClientId = keyVaultClientId
            //});

            //configuration.AddAzureKeyVault(new Uri(keyVaultURL), miCredentials, new AzureKeyVaultConfigurationOptions
            //{
            //    // Manager = new PrefixKeyVaultSecretManager(secretPrefix),
            //    ReloadInterval = TimeSpan.FromMinutes(5)
            //});

            // Third way -> with this we must be logged in azure portal already with azure cli or manually on UI
            //configuration.AddAzureKeyVault(new SecretClient(new Uri(settings["KeyVaultConfiguration:KeyVaultURL"]),
            //    new DefaultAzureCredential()), new KeyVaultSecretManager());

            // Forth way
            //configuration.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
        }
    }
}
