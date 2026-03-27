namespace GameBooster.Api.Models
{
    public class CreateLicenseRequest
    {
        public string ClientName { get; set; } = string.Empty;
        public int DurationDays { get; set; }
    }
}