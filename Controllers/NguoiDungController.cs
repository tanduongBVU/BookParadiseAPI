
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

        // Thêm class này vào file (có thể để dưới cùng)
        public class LoginRequest
        {
            public string Email { get; set; }
            public string MatKhau { get; set; }
        }

        // Thêm hàm này vào bên trong class NguoiDungController
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.MatKhau))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ email và mật khẩu." });
            }

            // Tìm user trong database có khớp email và mật khẩu không
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.MatKhau == request.MatKhau);

            if (user == null)
            {
                return Unauthorized(new { message = "Sai email hoặc mật khẩu!" });
            }

            // Nếu tài khoản bị khóa (HienThi = false)
            if (!user.HienThi)
            {
                return BadRequest(new { message = "Tài khoản của bạn đã bị khóa." });
            }

            // Trả về dữ liệu user và một token (tạm thời dùng token giả, sau này nếu cần bảo mật cao hơn ta sẽ cài JWT)
            return Ok(new
            {
                message = "Đăng nhập thành công",
                token = "fake-jwt-token-bookparadise",
                user = new
                {
                    id = user.MaNguoiDung,
                    ten = user.HoTen,
                    email = user.Email,
                    // Thay vì gán cứng email, giờ ta lấy trực tiếp từ database
                    role = user.VaiTro
                }
            });
        }


        // 1. API Lấy toàn bộ danh sách người dùng
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.NguoiDungs.ToListAsync();
            return Ok(users);
        }

        // 2. API Xóa người dùng theo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa người dùng thành công." });
        }

        // Class phụ để nhận dữ liệu khi cập nhật quyền
        public class UpdateRoleRequest { public string VaiTro { get; set; } }

        // 3. API Cập nhật phân quyền
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            user.VaiTro = request.VaiTro;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã cập nhật quyền thành công." });
        }

    }
}
