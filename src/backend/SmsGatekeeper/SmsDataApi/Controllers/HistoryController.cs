using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmsGatekeeper.Domain;
using SmsGatekeeper.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace SmsDataApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly SmsContext _context;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(SmsContext context, ILogger<HistoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(
            [FromQuery] string? phoneNumber,
            [FromQuery] string? accountId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery][Range(1, int.MaxValue)] int page = 1,
            [FromQuery][Range(1, 100)] int pageSize = 10)
        {
            try
            {
                var query = _context.HistoryRecords.AsQueryable();

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    query = query.Where(r => r.PhoneNumber == phoneNumber);
                }

                if (!string.IsNullOrEmpty(accountId))
                {
                    query = query.Where(r => r.AccountId == accountId);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(r => r.RequestedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(r => r.RequestedAt <= endDate.Value);
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var records = await query
                    .OrderByDescending(r => r.RequestedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    Records = records,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving history records");
                return StatusCode(500, "An error occurred while retrieving history records");
            }
        }
    }
} 