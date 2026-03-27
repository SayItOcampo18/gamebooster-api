namespace GameBooster.Api.Models
{
    public class ValidateLicenseRequest
    {
        public string AccessId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
    }
}