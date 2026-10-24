using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Hosting;


namespace BookParadise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SachController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SachController(
            BookParadiseDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var danhSachSach = await _context.Sachs
                .Where(s => s.HienThi)
                .ToListAsync();

            return Ok(danhSachSach);
        }

        // API GOM DỮ LIỆU CHO TRANG CHỦ (Tải 1 lần được cả 4 mục)
        [HttpGet("home-data")]
        public async Task<IActionResult> GetHomeData()
        {
            // 1. Lấy danh mục sách
            var danhMuc = await _context.LoaiSachs.ToListAsync();

            // 2. Sách mới phát hành (Lấy 8 cuốn có ID lớn nhất - mới thêm vào)
            var sachMoi = await _context.Sachs
            .Where(s => s.HienThi)
            .OrderByDescending(s => s.MaSach)
            .Take(8)
            .ToListAsync();

            // 3. Sách khuyến mãi (Những sách được bật cờ HienThiTrangSale)
            var sachKhuyenMai = await _context.Sachs
            .Where(s => s.HienThi && s.HienThiTrangSale)
            .Take(8)
            .ToListAsync();

            // 4. Sách bán chạy (Tính tổng số lượng bán từ Chi tiết đơn hàng)
            var sachBanChay = await (
            from s in _context.Sachs
            where s.HienThi
            join ct in _context.ChiTietDonHangs
                on s.MaSach equals ct.MaSach into g
            select new
            {
                Sach = s,
                SoLuongBan = g.Sum(x => x.SoLuong)
            })
            .OrderByDescending(x => x.SoLuongBan)
            .Select(x => x.Sach)
            .Take(8)
            .ToListAsync();

            // Trả về 1 cục JSON duy nhất chứa cả 4 danh sách
            return Ok(new
            {
                DanhMuc = danhMuc,
                SachMoi = sachMoi,
                SachKhuyenMai = sachKhuyenMai,
                SachBanChay = sachBanChay
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sach = await _context.Sachs
            .Where(s => s.HienThi)
            .Include(s => s.CacBienThe)
            .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }

            return Ok(sach);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Sach sach)
        {
            _context.Sachs.Add(sach);

            await _context.SaveChangesAsync();

            return Ok(sach);
        }



        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new { message = "Vui lòng nhập từ khóa tìm kiếm." });
            }

            var tuKhoa = keyword.Trim();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 1. KIỂM TRA XEM INDEX ĐÃ TỒN TẠI TRONG CSDL CHƯA
                bool hasIndex = false;
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = "SELECT 1 FROM pg_indexes WHERE indexname = 'idx_sach_tensach';";

                    if (command.Connection!.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    var result = await command.ExecuteScalarAsync();
                    hasIndex = result != null; // Nếu khác null nghĩa là đã có Index
                }

                // 2. NẾU CHƯA CÓ INDEX -> ÉP DB NGỦ 3 GIÂY ĐỂ MÔ PHỎNG QUÉT CHẬM (FULL TABLE SCAN)
                if (!hasIndex)
                {
                    using (var command = _context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = "SELECT pg_sleep(3);";
                        await command.ExecuteNonQueryAsync();
                    }
                }

                // 3. THỰC HIỆN TÌM KIẾM THẬT (Chỉ có vài cuốn sách nên query rất nhanh nếu không bị ép ngủ)
                var danhSachSach = await _context.Sachs
                    .AsNoTracking()
                    .Where(s => s.HienThi && s.TenSach.Contains(tuKhoa))
                    .Take(50)
                    .ToListAsync();

                stopwatch.Stop();

                var thoiGianMs = stopwatch.ElapsedMilliseconds;
                var timKiemCham = thoiGianMs > 2000;

                // 4. GHI LOG NẾU TRUY VẤN VƯỢT QUÁ 2 GIÂY
                if (timKiemCham)
                {
                    var ip = HttpContext.Connection.RemoteIpAddress;
                    var diaChiIP = ip == null ? "Unknown" : (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4().ToString() : ip.ToString());
                    if (diaChiIP == "::1") diaChiIP = "127.0.0.1";

                    var suKien = new SecurityEvent
                    {
                        EventType = "slow_search",
                        SourceIp = diaChiIP,
                        Description = $"Tìm kiếm sách mất {thoiGianMs} ms, từ khóa: {tuKhoa}.",
                        RiskScore = 0,
                        Severity = "Info",
                        DetectionStatus = "Pending",
                        ReviewStatus = "New",
                        IsSimulated = true, // Đánh dấu đây là MÔ PHỎNG thông minh
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Set<SecurityEvent>().Add(suKien);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    keyword = tuKhoa,
                    elapsedMilliseconds = thoiGianMs,
                    slowSearch = timKiemCham,
                    data = danhSachSach
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SEARCH ERROR] {ex.Message}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tìm kiếm sách." });
            }
        }


    }
}