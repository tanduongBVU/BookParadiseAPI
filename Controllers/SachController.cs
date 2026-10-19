using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SachController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public SachController(BookParadiseDbContext context)
        {
            _context = context;
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
    }
}