﻿namespace VideoStore.Ordering.Models.Configs
{
    public class JwtConfig
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Secret { get; set; } = null!;
    }
}
