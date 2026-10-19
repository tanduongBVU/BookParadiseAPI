using BookParadise.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookParadise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        public DashboardController(BookParadiseDbContext context) => _context = context;

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow;
            var sevenDaysAgo = today.AddDays(-7);
            var fourteenDaysAgo = today.AddDays(-14);
            var thirtyDaysAgo = today.AddDays(-30);

            // 1. Doanh thu (Chỉ tính các đơn không bị hủy)
            var doanhThu7Ngay = await _context.DonHangs.Where(d => d.NgayDat >= sevenDaysAgo && d.TrangThaiAI != "Đã hủy").SumAsync(d => (decimal?)d.TongTien) ?? 0;
            var doanhThu14Ngay = await _context.DonHangs.Where(d => d.NgayDat >= fourteenDaysAgo && d.TrangThaiAI != "Đã hủy").SumAsync(d => (decimal?)d.TongTien) ?? 0;
            var doanhThu30Ngay = await _context.DonHangs.Where(d => d.NgayDat >= thirtyDaysAgo && d.TrangThaiAI != "Đã hủy").SumAsync(d => (decimal?)d.TongTien) ?? 0;

            // 2. Doanh thu theo danh mục
            var doanhThuTheoDanhMuc = await (from ct in _context.ChiTietDonHangs
                                             join s in _context.Sachs on ct.MaSach equals s.MaSach
                                             // Lưu ý: Nếu báo lỗi CS1061 ở đây, đổi _context.LoaiSachs thành _context.LoaiSach
                                             join l in _context.LoaiSachs on s.MaLoai equals l.MaLoai
                                             join d in _context.DonHangs on ct.MaDonHang equals d.MaDonHang
                                             where d.TrangThaiAI != "Đã hủy"
                                             group new { ct, l } by l.TenLoai into g
                                             select new
                                             {
                                                 Name = g.Key,
                                                 Value = g.Sum(x => x.ct.DonGia * x.ct.SoLuong)
                                             }).ToListAsync();

            // 3. Sản phẩm bán chạy
            var thongKeSach = await (from ct in _context.ChiTietDonHangs
                                     join s in _context.Sachs on ct.MaSach equals s.MaSach
                                     join d in _context.DonHangs on ct.MaDonHang equals d.MaDonHang
                                     where d.TrangThaiAI != "Đã hủy"
                                     group ct by new { s.MaSach, s.TenSach } into g
                                     select new
                                     {
                                         TenSach = g.Key.TenSach,
                                         DaBan = g.Sum(x => x.SoLuong)
                                     }).ToListAsync();

            var banChay = thongKeSach.OrderByDescending(x => x.DaBan).Take(5).ToList();

            // Sản phẩm bán chậm (Hoặc chưa bán được)
            var banCham = await _context.Sachs
                .Where(s => !_context.ChiTietDonHangs.Any(ct => ct.MaSach == s.MaSach))
                .Select(s => new { TenSach = s.TenSach, DaBan = 0 })
                .Take(5).ToListAsync();

            if (banCham.Count < 5)
                banCham.AddRange(thongKeSach.OrderBy(x => x.DaBan).Take(5 - banCham.Count));

            // 4. Đơn hàng gần nhất
            var donHangGanNhat = await (from d in _context.DonHangs
                                        join n in _context.NguoiDungs on d.MaNguoiDung equals n.MaNguoiDung
                                        orderby d.NgayDat descending
                                        select new
                                        {
                                            d.MaDonHang,
                                            TenKhachHang = n.HoTen,
                                            d.TongTien,
                                            d.NgayDat,
                                            TrangThai = d.TrangThaiAI
                                        }).Take(5).ToListAsync();

            // 5. Sản phẩm sắp hết hàng (Dưới 10 cuốn)
            var sapHetHang = await _context.Sachs 
                .Where(s => s.SoLuong > 0 && s.SoLuong <= 10)
                .OrderBy(s => s.SoLuong)
                .Select(s => new { s.TenSach, s.SoLuong })
                .Take(5).ToListAsync();

            return Ok(new
            {
                doanhThu = new { ngay7 = doanhThu7Ngay, ngay14 = doanhThu14Ngay, ngay30 = doanhThu30Ngay },
                doanhThuTheoDanhMuc,
                banChay,
                banCham,
                donHangGanNhat,
                sapHetHang
            });
        }
    }
}