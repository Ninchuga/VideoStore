namespace VideoStore.IdentityService.Constants
{
    public class IdentityConstants
    {
        public const string JwtConfigurationName = "JWT";
        public const string JwtSecretKeyName = "JwtSecret";
        public const string IdentityConnectionStringKey = "IdentityConnectionString";
        public const string KeyVaultSectionName = "KeyVaultConfiguration";
        public const string IdentityAppInsightsConnectionStringKey = "IdentityServiceAppInsightsConectionString";

        public class FeatureFlags
        {
            public const string UseInMemoryDatabase = "UseInMemoryDatabase";
            public const string RunningInDocker = "RunningInDocker";
        }
    }
}
