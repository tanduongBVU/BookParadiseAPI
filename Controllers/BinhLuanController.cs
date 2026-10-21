using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace BookParadise.Controllers
{
    [ApiController]
    public class BinhLuanController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        public BinhLuanController(BookParadiseDbContext context) => _context = context;

        //Thêm hàm kiểm tra đầu vào trong phan AI baor mật

        private static bool CoDauHieuDangNgo(string? noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                return false;

            string[] mauDangNgo =
            {
                // SQL Injection: dieu kien OR/AND co dang so sanh
                @"(?i)\bOR\s+['""]?\w+['""]?\s*=\s*['""]?\w+['""]?",
                @"(?i)\bAND\s+['""]?\w+['""]?\s*=\s*['""]?\w+['""]?",

                // SQL Injection: cac cau lenh SQL dang ngo
                @"(?i)\bUNION\s+SELECT\b",
                @"(?i)\bDROP\s+TABLE\b",
                @"(?i)\bINSERT\s+INTO\b",
                @"(?i)\bDELETE\s+FROM\b",
                @"(?i)\bUPDATE\s+\w+\s+SET\b",

                // SQL Injection: comment SQL ket hop voi dau hieu SQL
                @"(?i)(?:--|/\*|\*/|#)\s*(?:$|\b(?:OR|AND|UNION|SELECT|DROP|INSERT|DELETE|UPDATE)\b)",

                // XSS: the script va giao thuc JavaScript
                @"(?i)<\s*script\b",
                @"(?i)javascript\s*:",
                @"(?i)\bonerror\s*=",
                @"(?i)\bonload\s*=",
                @"(?i)<\s*iframe\b",
                @"(?i)<\s*svg\b[^>]*\bonload\s*="
            };

            foreach (string mau in mauDangNgo)
            {
                if (Regex.IsMatch(noiDung, mau))
                    return true;
            }

            return false;
        }


        private string LayDiaChiIP()
        {
            string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (string.IsNullOrWhiteSpace(ip))
                return "127.0.0.1";

            if (ip == "::1")
                return "127.0.0.1";

            return ip;
        }

        // 1. DÀNH CHO KHÁCH HÀNG: Lấy bình luận CỦA 1 SẢN PHẨM (Chỉ lấy những cái Đã Duyệt)
        [HttpGet("api/sach/{maSach}/binhluan")]
        public async Task<IActionResult> GetCommentsForBook(int maSach)
        {
            var comments = await _context.BinhLuans
                .Include(b => b.NguoiDung)
                .Where(b => b.MaSach == maSach && b.TrangThaiAI == "Da_Duyet" && b.HienThi == true)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Select(b => new {
                    b.MaBinhLuan,
                    b.NoiDung,
                    b.NgayBinhLuan,
                    TenNguoiDung = b.NguoiDung != null ? b.NguoiDung.HoTen : "Khách hàng"
                })
                .ToListAsync();
            return Ok(comments);
        }

        // 2. DÀNH CHO KHÁCH HÀNG: Gửi bình luận mới
        [HttpPost("api/binhluan")]
        public async Task<IActionResult> CreateComment(
    [FromBody] BinhLuan binhLuan)
        {
            if (binhLuan == null ||
                string.IsNullOrWhiteSpace(binhLuan.NoiDung))
            {
                return BadRequest(new
                {
                    message = "Nội dung bình luận không được để trống."
                });
            }

            binhLuan.NgayBinhLuan = DateTime.Now;
            binhLuan.TrangThaiAI = "Cho_Kiem_Duyet";
            binhLuan.HienThi = true;

            bool dauVaoDangNgo = CoDauHieuDangNgo(binhLuan.NoiDung);

            // Lưu bình luận theo cơ chế kiểm duyệt hiện tại.
            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();

            // Nếu phát hiện mẫu đáng ngờ, ghi sự kiện cho AI xử lý.
            if (dauVaoDangNgo)
            {
                var suKien = new SecurityEvent
                {
                    EventType = "suspicious_input",
                    SourceIp = LayDiaChiIP(),
                    Description =
                        $"Phat hien mau dau vao dang ngo trong binh luan. " +
                        $"Ma binh luan: {binhLuan.MaBinhLuan}.",
                    RiskScore = 0,
                    Severity = "Info",
                    DetectionStatus = "Pending",
                    ReviewStatus = "New",
                    IsSimulated = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SecurityEvents.Add(suKien);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Đã gửi bình luận. Vui lòng chờ Admin kiểm duyệt!",
                suspiciousInput = dauVaoDangNgo
            });
        }

        // 3. DÀNH CHO ADMIN: Lấy tất cả bình luận để quản lý
        [HttpGet("api/admin/binhluan")]
        public async Task<IActionResult> GetAllCommentsForAdmin()
        {
            var comments = await _context.BinhLuans
                .Include(b => b.NguoiDung)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Select(b => new {
                    b.MaBinhLuan,
                    b.NoiDung,
                    b.NgayBinhLuan,
                    b.TrangThaiAI,
                    TenNguoiDung = b.NguoiDung != null ? b.NguoiDung.HoTen : "Khách hàng",
                    MaSach = b.MaSach
                })
                .ToListAsync();
            return Ok(comments);
        }

        // 4. DÀNH CHO ADMIN: Duyệt hoặc Hủy bình luận
        [HttpPatch("api/admin/binhluan/{id}/trangthai")]
        public async Task<IActionResult> ChangeCommentStatus(int id, [FromBody] string trangThai)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(id);
            if (binhLuan == null) return NotFound();

            // trangThai sẽ truyền lên là "Da_Duyet" hoặc "Tu_Choi"
            binhLuan.TrangThaiAI = trangThai;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công!" });
        }

        // 5. DÀNH CHO ADMIN: Xóa vĩnh viễn bình luận
        [HttpDelete("api/admin/binhluan/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                // Dùng FirstOrDefault quét trực tiếp DB, tránh lỗi tracking khi insert SQL thủ công
                var binhLuan = await _context.BinhLuans.FirstOrDefaultAsync(b => b.MaBinhLuan == id);

                if (binhLuan == null)
                    return BadRequest(new { message = $"Không tìm thấy bình luận có mã {id} trong Database." });

                _context.BinhLuans.Remove(binhLuan);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa bình luận thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}