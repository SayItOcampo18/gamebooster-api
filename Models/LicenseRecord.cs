namespace GameBooster.Api.Models
{
    public class LicenseRecord
    {
        public string AccessId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime ExpirationDate { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public DateTime LastCheckUtc { get; set; }
    }
}