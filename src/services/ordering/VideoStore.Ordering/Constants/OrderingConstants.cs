namespace VideoStore.Ordering.Constants
{
    public class OrderingConstants
    {
        public const string JwtConfigurationName = "JWT";
        public const string JwtSecretKeyName = "JwtSecret";
        public const string OrderingConnectionStringKey = "OrderingConnectionString";
        public const string AzureServiceBusConnectionStringKey = "AzureServiceBusConnectionString";
        public const string RedisConnectionStringKey = "RedisConnectionString";
        public const string RedisMessagingStoreInstanceName = "messagingstore";
        public const string KeyVaultSectionName = "KeyVaultConfiguration";

        public class FeatureFlags
        {
            public const string UseInMemoryDatabase = "UseInMemoryDatabase";
            public const string RunningInDocker = "RunningInDocker";
        }
    }
}
