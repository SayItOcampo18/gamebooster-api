using Microsoft.AspNetCore.Mvc;
using GameBooster.Api.Models;
using GameBooster.Api.Services;

namespace GameBooster.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicensesController : ControllerBase
    {
        private readonly LicenseStoreService _store;

        public LicensesController(LicenseStoreService store)
        {
            _store = store;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_store.GetAll());
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] CreateLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClientName))
                return BadRequest("Client name is required.");

            if (request.DurationDays <= 0)
                return BadRequest("DurationDays must be greater than 0.");

            var record = _store.Create(request.ClientName, request.DurationDays);
            return Ok(record);
        }

        [HttpPost("validate")]
        public IActionResult Validate([FromBody] ValidateLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccessId))
                return BadRequest("AccessId is required.");

            if (string.IsNullOrWhiteSpace(request.DeviceId))
                return BadRequest("DeviceId is required.");

            var result = _store.Validate(request.AccessId, request.DeviceId);
            return Ok(result);
        }

        [HttpPost("{accessId}/block")]
        public IActionResult Block(string accessId)
        {
            bool success = _store.SetActive(accessId, false);
            if (!success) return NotFound();

            return Ok(new { message = "License blocked." });
        }

        [HttpPost("{accessId}/activate")]
        public IActionResult Activate(string accessId)
        {
            bool success = _store.SetActive(accessId, true);
            if (!success) return NotFound();

            return Ok(new { message = "License activated." });
        }

        [HttpPost("{accessId}/renew/{days}")]
        public IActionResult Renew(string accessId, int days)
        {
            if (days <= 0)
                return BadRequest("Days must be greater than 0.");

            bool success = _store.Renew(accessId, days);
            if (!success) return NotFound();

            return Ok(new { message = "License renewed." });
        }
    }
}