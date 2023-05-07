namespace VideoStore.IdentityService.Models
{
    public class KeyVaultConfig
    {
        public string TenantId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string KeyVaultUrl { get; set; } = null!;
    }
}
