namespace GameLog_Backend.Configurations
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpireMinutes { get; set; } = 15;
        public int ExpireHours { get; set; } = 0;
        public int RefreshTokenExpireDays { get; set; } = 7;
    }
}