
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


        // POST: api/DonHang
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookParadise.Models.DonHang donHang)
        {
            if (donHang.TongTien < 0)
            {
                return BadRequest(new
                {
                    message = "Tổng tiền không được âm."
                });
            }

            bool nguoiDungTonTai = await _context.NguoiDungs
                .AnyAsync(n => n.MaNguoiDung == donHang.MaNguoiDung);

            if (!nguoiDungTonTai)
            {
                return BadRequest(new
                {
                    message = "Mã người dùng không tồn tại."
                });
            }

            donHang.MaDonHang = 0;
            donHang.NgayDat = DateTime.UtcNow;
            donHang.TrangThaiAI = "Cho_Kiem_Duyet";
            donHang.HienThi = true;

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm đơn hàng thành công.",
                donHang.MaDonHang,
                donHang.MaNguoiDung,
                donHang.NgayDat,
                donHang.TongTien,
                donHang.TrangThaiAI,
                donHang.HienThi
            });
        }

    }
}
