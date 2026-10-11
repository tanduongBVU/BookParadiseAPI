
using BookParadise.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChiTietDonHangController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public ChiTietDonHangController(BookParadiseDbContext context)
        {
            _context = context;
        }

        // GET: api/ChiTietDonHang
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var danhSach = await _context.ChiTietDonHangs
                .Select(ct => new
                {
                    ct.MaChiTiet,
                    ct.MaDonHang,
                    ct.MaSach,
                    ct.SoLuong,
                    ct.DonGia,
                    ct.TrangThaiAI,
                    ct.HienThi
                })
                .ToListAsync();

            return Ok(danhSach);
        }

        // GET: api/ChiTietDonHang/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var chiTiet = await _context.ChiTietDonHangs
                .Where(ct => ct.MaChiTiet == id)
                .Select(ct => new
                {
                    ct.MaChiTiet,
                    ct.MaDonHang,
                    ct.MaSach,
                    ct.SoLuong,
                    ct.DonGia,
                    ct.TrangThaiAI,
                    ct.HienThi
                })
                .FirstOrDefaultAsync();

            if (chiTiet == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy chi tiết đơn hàng."
                });
            }

            return Ok(chiTiet);
        }


        // POST: api/ChiTietDonHang
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] BookParadise.Models.ChiTietDonHang chiTiet)
        {
            if (chiTiet.SoLuong <= 0 || chiTiet.DonGia < 0)
            {
                return BadRequest(new
                {
                    message = "Số lượng phải lớn hơn 0 và đơn giá không được âm."
                });
            }

            bool donHangTonTai = await _context.DonHangs
                .AnyAsync(d => d.MaDonHang == chiTiet.MaDonHang);

            bool sachTonTai = await _context.Sachs
                .AnyAsync(s => s.MaSach == chiTiet.MaSach);

            if (!donHangTonTai || !sachTonTai)
            {
                return BadRequest(new
                {
                    message = "Mã đơn hàng hoặc mã sách không tồn tại."
                });
            }

            chiTiet.MaChiTiet = 0;
            chiTiet.TrangThaiAI = "Cho_Kiem_Duyet";
            chiTiet.HienThi = true;

            _context.ChiTietDonHangs.Add(chiTiet);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm chi tiết đơn hàng thành công.",
                chiTiet.MaChiTiet,
                chiTiet.MaDonHang,
                chiTiet.MaSach,
                chiTiet.SoLuong,
                chiTiet.DonGia,
                chiTiet.TrangThaiAI,
                chiTiet.HienThi
            });
        }

    }
}
