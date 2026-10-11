
using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public NguoiDungController(BookParadiseDbContext context)
        {
            _context = context;
        }

        // GET: api/NguoiDung
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var danhSach = await _context.NguoiDungs
                .Select(n => new
                {
                    n.MaNguoiDung,
                    n.HoTen,
                    n.Email,
                    n.TrangThaiAI,
                    n.HienThi
                })
                .ToListAsync();

            return Ok(danhSach);
        }

        // GET: api/NguoiDung/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var nguoiDung = await _context.NguoiDungs
                .Where(n => n.MaNguoiDung == id)
                .Select(n => new
                {
                    n.MaNguoiDung,
                    n.HoTen,
                    n.Email,
                    n.TrangThaiAI,
                    n.HienThi
                })
                .FirstOrDefaultAsync();

            if (nguoiDung == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy người dùng."
                });
            }

            return Ok(nguoiDung);
        }


        // POST: api/NguoiDung
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NguoiDung nguoiDung)
        {
            if (string.IsNullOrWhiteSpace(nguoiDung.HoTen) ||
                string.IsNullOrWhiteSpace(nguoiDung.Email) ||
                string.IsNullOrWhiteSpace(nguoiDung.MatKhau))
            {
                return BadRequest(new
                {
                    message = "Họ tên, email và mật khẩu không được để trống."
                });
            }

            bool emailTonTai = await _context.NguoiDungs
                .AnyAsync(n => n.Email == nguoiDung.Email);

            if (emailTonTai)
            {
                return Conflict(new
                {
                    message = "Email đã tồn tại."
                });
            }

            _context.NguoiDungs.Add(nguoiDung);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = nguoiDung.MaNguoiDung },
                new
                {
                    nguoiDung.MaNguoiDung,
                    nguoiDung.HoTen,
                    nguoiDung.Email,
                    nguoiDung.TrangThaiAI,
                    nguoiDung.HienThi
                });
        }

    }
}
