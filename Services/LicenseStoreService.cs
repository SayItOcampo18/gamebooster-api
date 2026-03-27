using System.Text.Json;
using GameBooster.Api.Models;

namespace GameBooster.Api.Services
{
    public class LicenseStoreService
    {
        private readonly string _filePath;
        private readonly object _lock = new();

        public LicenseStoreService()
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "licenses.json");
        }

        public List<LicenseRecord> GetAll()
        {
            lock (_lock)
            {
                if (!File.Exists(_filePath))
                    return new List<LicenseRecord>();

                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<LicenseRecord>>(json) ?? new List<LicenseRecord>();
            }
        }

        public void SaveAll(List<LicenseRecord> records)
        {
            lock (_lock)
            {
                string json = JsonSerializer.Serialize(records, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_filePath, json);
            }
        }

        public LicenseRecord? GetByAccessId(string accessId)
        {
            return GetAll().FirstOrDefault(x => x.AccessId.Equals(accessId, StringComparison.OrdinalIgnoreCase));
        }

        public LicenseRecord Create(string clientName, int durationDays)
        {
            List<LicenseRecord> records = GetAll();

            string accessId = GenerateAccessId(records);

            var record = new LicenseRecord
            {
                AccessId = accessId,
                ClientName = clientName,
                IsActive = true,
                ExpirationDate = DateTime.UtcNow.AddDays(durationDays),
                DeviceId = string.Empty,
                LastCheckUtc = DateTime.UtcNow
            };

            records.Add(record);
            SaveAll(records);

            return record;
        }

        public bool SetActive(string accessId, bool isActive)
        {
            List<LicenseRecord> records = GetAll();
            var record = records.FirstOrDefault(x => x.AccessId.Equals(accessId, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                return false;

            record.IsActive = isActive;
            SaveAll(records);
            return true;
        }

        public bool Renew(string accessId, int extraDays)
        {
            List<LicenseRecord> records = GetAll();
            var record = records.FirstOrDefault(x => x.AccessId.Equals(accessId, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                return false;

            if (record.ExpirationDate < DateTime.UtcNow)
                record.ExpirationDate = DateTime.UtcNow.AddDays(extraDays);
            else
                record.ExpirationDate = record.ExpirationDate.AddDays(extraDays);

            SaveAll(records);
            return true;
        }

        public ValidateLicenseResponse Validate(string accessId, string deviceId)
        {
            List<LicenseRecord> records = GetAll();
            var record = records.FirstOrDefault(x => x.AccessId.Equals(accessId, StringComparison.OrdinalIgnoreCase));

            if (record == null)
            {
                return new ValidateLicenseResponse
                {
                    Success = false,
                    Message = "Invalid Access ID"
                };
            }

            if (!record.IsActive)
            {
                return new ValidateLicenseResponse
                {
                    Success = false,
                    IsBlocked = true,
                    Message = "This license is blocked.",
                    ClientName = record.ClientName,
                    ExpirationDate = record.ExpirationDate
                };
            }

            if (DateTime.UtcNow > record.ExpirationDate)
            {
                return new ValidateLicenseResponse
                {
                    Success = false,
                    IsExpired = true,
                    Message = "This license has expired.",
                    ClientName = record.ClientName,
                    ExpirationDate = record.ExpirationDate
                };
            }

            if (string.IsNullOrWhiteSpace(record.DeviceId))
            {
                record.DeviceId = deviceId;
            }
            else if (!record.DeviceId.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
            {
                return new ValidateLicenseResponse
                {
                    Success = false,
                    Message = "This license is already bound to another device."
                };
            }

            record.LastCheckUtc = DateTime.UtcNow;
            SaveAll(records);

            return new ValidateLicenseResponse
            {
                Success = true,
                Message = "License valid.",
                ClientName = record.ClientName,
                ExpirationDate = record.ExpirationDate
            };
        }

        private static string GenerateAccessId(List<LicenseRecord> records)
        {
            string id;

            do
            {
                id = Guid.NewGuid().ToString("N")[..10].ToUpper();
            }
            while (records.Any(x => x.AccessId == id));

            return id;
        }
    }
}