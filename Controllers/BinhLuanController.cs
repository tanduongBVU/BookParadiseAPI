
using BookParadise.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BinhLuanController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public BinhLuanController(BookParadiseDbContext context)
        {
            _context = context;
        }

        // GET: api/BinhLuan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var danhSach = await _context.BinhLuans
                .Select(b => new
                {
                    b.MaBinhLuan,
                    b.MaNguoiDung,
                    b.MaSach,
                    b.NoiDung,
                    b.NgayBinhLuan,
                    b.TrangThaiAI,
                    b.HienThi
                })
                .ToListAsync();

            return Ok(danhSach);
        }

        // GET: api/BinhLuan/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var binhLuan = await _context.BinhLuans
                .Where(b => b.MaBinhLuan == id)
                .Select(b => new
                {
                    b.MaBinhLuan,
                    b.MaNguoiDung,
                    b.MaSach,
                    b.NoiDung,
                    b.NgayBinhLuan,
                    b.TrangThaiAI,
                    b.HienThi
                })
                .FirstOrDefaultAsync();

            if (binhLuan == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy bình luận."
                });
            }

            return Ok(binhLuan);
        }


        // POST: api/BinhLuan
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] BookParadise.Models.BinhLuan binhLuan)
        {
            if (string.IsNullOrWhiteSpace(binhLuan.NoiDung))
            {
                return BadRequest(new
                {
                    message = "Nội dung bình luận không được để trống."
                });
            }

            bool nguoiDungTonTai = await _context.NguoiDungs
                .AnyAsync(n => n.MaNguoiDung == binhLuan.MaNguoiDung);

            bool sachTonTai = await _context.Sachs
                .AnyAsync(s => s.MaSach == binhLuan.MaSach);

            if (!nguoiDungTonTai || !sachTonTai)
            {
                return BadRequest(new
                {
                    message = "Mã người dùng hoặc mã sách không tồn tại."
                });
            }

            binhLuan.MaBinhLuan = 0;
            binhLuan.NgayBinhLuan = DateTime.UtcNow;
            binhLuan.TrangThaiAI = "Cho_Kiem_Duyet";
            binhLuan.HienThi = true;

            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Thêm bình luận thành công.",
                binhLuan.MaBinhLuan,
                binhLuan.MaNguoiDung,
                binhLuan.MaSach,
                binhLuan.NoiDung,
                binhLuan.NgayBinhLuan,
                binhLuan.TrangThaiAI,
                binhLuan.HienThi
            });
        }

    }
}
