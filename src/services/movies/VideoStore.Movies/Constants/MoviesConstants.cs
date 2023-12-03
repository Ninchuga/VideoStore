namespace VideoStore.Movies.Constants
{
    public class MoviesConstants
    {
        public const string JwtConfigurationName = "JWT";
        public const string JwtSecretKeyName = "JwtSecret";
        public const string MoviesConnectionStringKey = "MoviesConnectionString";
        public const string AzureServiceBusConnectionStringKey = "AzureServiceBusConnectionString";
        public const string KeyVaultSectionName = "KeyVaultConfiguration";
        public const string OrderingApiBaseUrl = "OrderingApiBaseUrl";
        public const string OrderingApiHttpClientName = "OrderingApi";

        public class TokenClaimTypes
        {
            public const string Email = "email";
            public const string Subject = "sub";
            public const string UserId = "userId";
        }

        public class FeatureFlags
        {
            public const string UseInMemoryDatabase = "UseInMemoryDatabase";
            public const string RunningInDocker = "RunningInDocker";
        }
    }
    
}
