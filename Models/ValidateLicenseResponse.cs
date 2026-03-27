namespace GameBooster.Api.Models
{
    public class ValidateLicenseResponse
    {
        public bool Success { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsExpired { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime? ExpirationDate { get; set; }
    }
}