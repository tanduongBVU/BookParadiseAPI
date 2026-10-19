using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BookParadise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SachYeuThichController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public SachYeuThichController(BookParadiseDbContext context)
        {
            _context = context;
        }

        // GET: api/SachYeuThich/5 (Lấy wishlist của 1 User)
        [HttpGet("{maNguoiDung}")]
        public async Task<IActionResult> GetWishlistByUser(int maNguoiDung)
        {
            // Lấy danh sách ID sách mà user này đã yêu thích
            var danhSach = await _context.SachYeuThiches
                .Where(w => w.MaNguoiDung == maNguoiDung)
                .Select(w => w.MaSach)
                .ToListAsync();

            return Ok(danhSach);
        }

        // POST: api/SachYeuThich (Thêm hoặc Xóa yêu thích - Toggle)
        // Hàm này rất tiện: Gửi lên lần 1 là Thêm (Tim đỏ), gửi lần 2 là Xóa (Hủy tim)
        [HttpPost]
        public async Task<IActionResult> ToggleWishlist([FromBody] SachYeuThich request)
        {
            var existingItem = await _context.SachYeuThiches
                .FirstOrDefaultAsync(w => w.MaNguoiDung == request.MaNguoiDung && w.MaSach == request.MaSach);

            if (existingItem != null)
            {
                // Nếu đã có trong wishlist -> Xóa (Bỏ yêu thích)
                _context.SachYeuThiches.Remove(existingItem);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã bỏ khỏi danh sách yêu thích", isLiked = false });
            }
            else
            {
                // Nếu chưa có -> Thêm mới
                var newItem = new SachYeuThich
                {
                    MaNguoiDung = request.MaNguoiDung,
                    MaSach = request.MaSach,
                    NgayThem = System.DateTime.UtcNow
                };
                _context.SachYeuThiches.Add(newItem);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã thêm vào danh sách yêu thích", isLiked = true });
            }
        }
    }
}