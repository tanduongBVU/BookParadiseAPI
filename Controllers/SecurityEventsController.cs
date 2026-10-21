
using BookParadise.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityEventsController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public SecurityEventsController(BookParadiseDbContext context)
        {
            _context = context;
        }

        // GET: api/security-events
        // Lấy 200 sự kiện có ID lớn nhất trước.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.SecurityEvents
                .AsNoTracking()
                .OrderByDescending(e => e.Id)
                .Take(200)
                .Select(e => new
                {
                    id = e.Id,
                    eventType = e.EventType,
                    sourceIp = e.SourceIp,
                    description = e.Description,
                    riskScore = e.RiskScore,
                    severity = e.Severity,
                    detectionStatus = e.DetectionStatus,
                    reviewStatus = e.ReviewStatus,
                    isSimulated = e.IsSimulated,
                    analysisResult = e.AnalysisResult,
                    createdAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // PUT: api/security-events/{id}/review
        // Cập nhật trạng thái xử lý sự kiện.
        [HttpPut("{id:int}/review")]
        public async Task<IActionResult> UpdateReviewStatus(
            int id,
            [FromBody] UpdateReviewStatusRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.ReviewStatus))
            {
                return BadRequest(new
                {
                    message = "Trạng thái xử lý không được để trống."
                });
            }

            var allowedStatuses = new[]
            {
                "New",
                "Reviewing",
                "Resolved"
            };

            if (!allowedStatuses.Contains(request.ReviewStatus))
            {
                return BadRequest(new
                {
                    message = "Trạng thái không hợp lệ. Chỉ chấp nhận New, Reviewing hoặc Resolved."
                });
            }

            var securityEvent = await _context.SecurityEvents
                .FirstOrDefaultAsync(e => e.Id == id);

            if (securityEvent == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy sự kiện có ID {id}."
                });
            }

            securityEvent.ReviewStatus = request.ReviewStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật trạng thái xử lý thành công.",
                id = securityEvent.Id,
                reviewStatus = securityEvent.ReviewStatus
            });
        }
    }

    public class UpdateReviewStatusRequest
    {
        public string ReviewStatus { get; set; } = string.Empty;
    }
}
