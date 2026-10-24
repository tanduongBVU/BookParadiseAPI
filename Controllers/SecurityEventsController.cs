
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


        // API "UỐNG THUỐC GIẢI" - TẠO INDEX ĐỂ TỐI ƯU CSDL
        [HttpPost("fix-index")]
        public async Task<IActionResult> FixIndex()
        {
            try
            {
                // 1. Tạo extension pg_trgm (Hỗ trợ tìm kiếm tiếng Việt cực mạnh)
                await _context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

                // 2. Tạo Index GIN trên cột ten_sach để tối ưu lệnh LIKE
                await _context.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS idx_sach_tensach ON quan_ly_ban_sach.sach USING GIN (ten_sach gin_trgm_ops);");

                return Ok(new { message = "Đã tạo Index thành công! Hãy ra trang chủ tìm kiếm lại để thấy sự khác biệt." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo Index: " + ex.Message });
            }
        }

        // (TÙY CHỌN DÀNH CHO ADMIN) API HỦY INDEX ĐỂ DEMO LẠI TỪ ĐẦU
        [HttpDelete("remove-index")]
        public async Task<IActionResult> RemoveIndex()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("DROP INDEX IF EXISTS quan_ly_ban_sach.idx_sach_tensach;");
                return Ok(new { message = "Đã xóa Index. CSDL đã quay về trạng thái quét chậm." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa Index: " + ex.Message });
            }
        }
    }

    public class UpdateReviewStatusRequest
    {
        public string ReviewStatus { get; set; } = string.Empty;
    }


}
