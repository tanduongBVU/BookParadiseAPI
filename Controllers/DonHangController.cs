
using BookParadise.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public DonHangController(BookParadiseDbContext context)
        {
            _context = context;
        }

        // GET: api/DonHang
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var danhSach = await _context.DonHangs
                .Select(d => new
                {
                    d.MaDonHang,
                    d.MaNguoiDung,
                    d.NgayDat,
                    d.TongTien,
                    d.TrangThaiAI,
                    d.HienThi
                })
                .ToListAsync();

            return Ok(danhSach);
        }

        // GET: api/DonHang/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var donHang = await _context.DonHangs
                .Where(d => d.MaDonHang == id)
                .Select(d => new
                {
                    d.MaDonHang,
                    d.MaNguoiDung,
                    d.NgayDat,
                    d.TongTien,
                    d.TrangThaiAI,
                    d.HienThi
                })
                .FirstOrDefaultAsync();

            if (donHang == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy đơn hàng."
                });
            }

            return Ok(donHang);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookParadise.Models.DonHang donHang)
        {
            if (donHang.TongTien < 0) return BadRequest(new { message = "Tổng tiền không được âm." });

            bool nguoiDungTonTai = await _context.NguoiDungs.AnyAsync(n => n.MaNguoiDung == donHang.MaNguoiDung);
            if (!nguoiDungTonTai) return BadRequest(new { message = "Mã người dùng không tồn tại." });

            // LOGIC MỚI: Trừ lượt dùng Mã giảm giá nếu có áp dụng
            if (!string.IsNullOrEmpty(donHang.MaCode))
            {
                var voucher = await _context.MaGiamGias.FirstOrDefaultAsync(v => v.Code == donHang.MaCode);
                if (voucher != null && voucher.SoLuong > 0)
                {
                    voucher.SoLuong -= 1;
                }
            }

            donHang.MaDonHang = 0;
            donHang.NgayDat = DateTime.UtcNow;
            donHang.TrangThaiAI = "Cho_Kiem_Duyet";
            donHang.HienThi = true;

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm đơn hàng thành công.", donHang.MaDonHang });
        }

        // ================= KHU VỰC API DÀNH CHO ADMIN =================

        // 1. ADMIN: Lấy tất cả đơn hàng kèm tên Người Dùng
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var danhSach = await (from d in _context.DonHangs
                                  join n in _context.NguoiDungs on d.MaNguoiDung equals n.MaNguoiDung
                                  orderby d.NgayDat descending
                                  select new
                                  {
                                      d.MaDonHang,
                                      d.NgayDat,
                                      d.TongTien,
                                      d.TrangThaiAI,
                                      TenNguoiDung = n.HoTen,
                                      Email = n.Email
                                  }).ToListAsync();
            return Ok(danhSach);
        }

        // 2. ADMIN: Lấy chi tiết của 1 Đơn hàng kèm Tên Sách
        [HttpGet("admin/{id}/chitiet")]
        public async Task<IActionResult> GetChiTietForAdmin(int id)
        {
            var chiTiet = await (from ct in _context.ChiTietDonHangs
                                     // Đã sửa _context.Saches thành _context.Sach
                                 join s in _context.Sachs on ct.MaSach equals s.MaSach
                                 where ct.MaDonHang == id
                                 select new
                                 {
                                     ct.MaChiTiet,
                                     ct.MaSach,
                                     ct.SoLuong,
                                     ct.DonGia,
                                     TenSach = s.TenSach,
                                     HinhAnh = s.HinhAnh
                                 }).ToListAsync();
            return Ok(chiTiet);
        }

        // 3. ADMIN: Cập nhật Trạng thái đơn hàng
        [HttpPatch("admin/{id}/trangthai")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string trangThaiMoi)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound("Không tìm thấy đơn hàng.");

            donHang.TrangThaiAI = trangThaiMoi;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công!" });
        }

    }
}
