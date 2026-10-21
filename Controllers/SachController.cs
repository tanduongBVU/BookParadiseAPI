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
            var danhSachSach = await _context.Sachs.ToListAsync();

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
                .OrderByDescending(s => s.MaSach)
                .Take(8)
                .ToListAsync();

            // 3. Sách khuyến mãi (Những sách được bật cờ HienThiTrangSale)
            var sachKhuyenMai = await _context.Sachs
                .Where(s => s.HienThiTrangSale)
                .Take(8)
                .ToListAsync();

            // 4. Sách bán chạy (Tính tổng số lượng bán từ Chi tiết đơn hàng)
            var sachBanChay = await (from s in _context.Sachs
                                     join ct in _context.ChiTietDonHangs on s.MaSach equals ct.MaSach into g
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
                .Include(x => x.CacBienThe)
                .FirstOrDefaultAsync(x => x.MaSach == id);

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
        public async Task<IActionResult> Search(
            [FromQuery] string keyword,
            [FromQuery] bool simulateSlow = false)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest(new
                {
                    message = "Vui lòng nhập từ khóa tìm kiếm."
                });
            }

            var tuKhoa = keyword.Trim();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Chỉ mô phỏng độ trễ khi chủ động truyền
                // simulateSlow=true trong môi trường Development.
                if (simulateSlow && _environment.IsDevelopment())
                {
                    await using var command =
                        _context.Database.GetDbConnection().CreateCommand();

                    command.CommandText = "SELECT pg_sleep(2.5)";

                    if (command.Connection!.State !=
                        System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }

                    await command.ExecuteNonQueryAsync();
                }

                // Đo thời gian xử lý truy vấn tìm kiếm thực tế.
                var danhSachSach = await _context.Sachs
                    .AsNoTracking()
                    .Where(s => s.TenSach.Contains(tuKhoa))
                    .Take(50)
                    .ToListAsync();

                stopwatch.Stop();

                var thoiGianMs = stopwatch.ElapsedMilliseconds;
                var timKiemCham = thoiGianMs > 2000;

                // Chỉ ghi sự kiện khi thời gian xử lý vượt 2 giây.
                if (timKiemCham)
                {
                    var ip = HttpContext.Connection.RemoteIpAddress;

                    var diaChiIP = ip == null
                        ? "Unknown"
                        : ip.IsIPv4MappedToIPv6
                            ? ip.MapToIPv4().ToString()
                            : ip.ToString();

                    if (diaChiIP == "::1")
                    {
                        diaChiIP = "127.0.0.1";
                    }

                    var suKien = new SecurityEvent
                    {
                        EventType = "slow_search",
                        SourceIp = diaChiIP,
                        Description =
                            $"Tìm kiếm sách mất {thoiGianMs} ms, " +
                            $"từ khóa: {tuKhoa}.",
                        RiskScore = 0,
                        Severity = "Info",
                        DetectionStatus = "Pending",
                        ReviewStatus = "New",
                        IsSimulated =
                            simulateSlow && _environment.IsDevelopment(),
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
                // Ghi lỗi để dễ tìm nguyên nhân trong terminal Backend.
                Console.WriteLine(
                    $"[SEARCH ERROR] {ex.Message}");

                return StatusCode(500, new
                {
                    message = "Đã xảy ra lỗi khi tìm kiếm sách."
                });
            }
        }


    }
}