using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [ApiController]
    public class BinhLuanController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        public BinhLuanController(BookParadiseDbContext context) => _context = context;

        // 1. DÀNH CHO KHÁCH HÀNG: Lấy bình luận CỦA 1 SẢN PHẨM (Chỉ lấy những cái Đã Duyệt)
        [HttpGet("api/sach/{maSach}/binhluan")]
        public async Task<IActionResult> GetCommentsForBook(int maSach)
        {
            var comments = await _context.BinhLuans
                .Include(b => b.NguoiDung)
                .Where(b => b.MaSach == maSach && b.TrangThaiAI == "Da_Duyet" && b.HienThi == true)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Select(b => new {
                    b.MaBinhLuan,
                    b.NoiDung,
                    b.NgayBinhLuan,
                    TenNguoiDung = b.NguoiDung != null ? b.NguoiDung.HoTen : "Khách hàng"
                })
                .ToListAsync();
            return Ok(comments);
        }

        // 2. DÀNH CHO KHÁCH HÀNG: Gửi bình luận mới
        [HttpPost("api/binhluan")]
        public async Task<IActionResult> CreateComment([FromBody] BinhLuan binhLuan)
        {
            binhLuan.NgayBinhLuan = DateTime.Now;
            binhLuan.TrangThaiAI = "Cho_Kiem_Duyet"; // Mặc định phải chờ Admin duyệt
            binhLuan.HienThi = true;

            _context.BinhLuans.Add(binhLuan);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã gửi bình luận. Vui lòng chờ Admin kiểm duyệt!" });
        }

        // 3. DÀNH CHO ADMIN: Lấy tất cả bình luận để quản lý
        [HttpGet("api/admin/binhluan")]
        public async Task<IActionResult> GetAllCommentsForAdmin()
        {
            var comments = await _context.BinhLuans
                .Include(b => b.NguoiDung)
                .OrderByDescending(b => b.NgayBinhLuan)
                .Select(b => new {
                    b.MaBinhLuan,
                    b.NoiDung,
                    b.NgayBinhLuan,
                    b.TrangThaiAI,
                    TenNguoiDung = b.NguoiDung != null ? b.NguoiDung.HoTen : "Khách hàng",
                    MaSach = b.MaSach
                })
                .ToListAsync();
            return Ok(comments);
        }

        // 4. DÀNH CHO ADMIN: Duyệt hoặc Hủy bình luận
        [HttpPatch("api/admin/binhluan/{id}/trangthai")]
        public async Task<IActionResult> ChangeCommentStatus(int id, [FromBody] string trangThai)
        {
            var binhLuan = await _context.BinhLuans.FindAsync(id);
            if (binhLuan == null) return NotFound();

            // trangThai sẽ truyền lên là "Da_Duyet" hoặc "Tu_Choi"
            binhLuan.TrangThaiAI = trangThai;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật trạng thái thành công!" });
        }

        // 5. DÀNH CHO ADMIN: Xóa vĩnh viễn bình luận
        [HttpDelete("api/admin/binhluan/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                // Dùng FirstOrDefault quét trực tiếp DB, tránh lỗi tracking khi insert SQL thủ công
                var binhLuan = await _context.BinhLuans.FirstOrDefaultAsync(b => b.MaBinhLuan == id);

                if (binhLuan == null)
                    return BadRequest(new { message = $"Không tìm thấy bình luận có mã {id} trong Database." });

                _context.BinhLuans.Remove(binhLuan);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa bình luận thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Server: " + ex.Message });
            }
        }
    }
}