using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // Bắt buộc cho IFormFile
using System.IO; // Bắt buộc cho Path và Directory
using System;

namespace BookParadise.Controllers
{
    [Route("api/admin/sach")]
    [ApiController]
    public class AdminSachController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        public AdminSachController(BookParadiseDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var sachs = await _context.Sachs
                .Include(s => s.LoaiSach)
                .Include(s => s.KhuyenMai)
                .Include(s => s.CacBienThe)
                .ToListAsync();
            return Ok(sachs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Sach sach)
        {
            sach.MaSach = 0;

            // Sách mới chờ AI kiểm tra.
            sach.TrangThaiAI = "Cho_Kiem_Duyet";
            sach.HienThi = false;

            _context.Sachs.Add(sach);
            await _context.SaveChangesAsync(); // Lưu xong EF Core sẽ tự cấp MaSach

            // GỌI LỆNH ĐÁNH THỨC PYTHON AI TRỰC TIẾP TỪ C#
            await _context.Database.ExecuteSqlRawAsync($"NOTIFY kenh_sach, '{sach.MaSach}';");

            return Ok(sach);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Sach sach)
        {
            if (id != sach.MaSach) return BadRequest();

            // Tự động chuyển về trạng thái chờ AI duyệt lại khi có chỉnh sửa
            sach.TrangThaiAI = "Cho_Kiem_Duyet";
            sach.HienThi = false; // Tạm ẩn chờ AI xác nhận

            _context.Entry(sach).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // GỌI LỆNH ĐÁNH THỨC PYTHON AI TRỰC TIẾP TỪ C#
            await _context.Database.ExecuteSqlRawAsync($"NOTIFY kenh_sach, '{sach.MaSach}';");

            return NoContent();
        }

        // THÊM MỚI API NÀY ĐỂ XỬ LÝ NÚT MỞ ẨN / TẮT ẨN THỦ CÔNG TRÊN UI
        [HttpPatch("{id}/toggle-visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null) return NotFound();

            // Nếu người dùng đang muốn MỞ hiển thị (hiện tại HienThi đang là false)
            if (!sach.HienThi)
            {
                // Chốt chặn 1: Không cho phép giá âm
                if (sach.Gia < 0)
                {
                    return BadRequest(new { message = "Không thể hiển thị: Sản phẩm đang có giá âm!" });
                }

                // Chốt chặn 2: Không cho phép ký tự rác
                string[] kyTuRac = { "@", "#", "$", "%", "^", "&", "*" };
                foreach (var kyTu in kyTuRac)
                {
                    if (sach.TenSach.Contains(kyTu))
                    {
                        return BadRequest(new { message = $"Không thể hiển thị: Tên sách chứa ký tự rác '{kyTu}'!" });
                    }
                }

                // Vượt qua kiểm tra -> Cho phép hiển thị
                sach.HienThi = true;
                sach.TrangThaiAI = "Hop_Le";
            }
            else
            {
                // Nếu sách đang hiển thị, Admin muốn TẮT đi thì cho phép tắt thoải mái
                sach.HienThi = false;
                sach.TrangThaiAI = "Da_An_Thu_Cong";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật hiển thị thành công", hienThi = sach.HienThi });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sach = await _context.Sachs
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound(new
                {
                    message = $"Không tìm thấy sách có mã {id}."
                });
            }

            // Kiểm tra sách đã được mua chưa.
            bool daDuocMua = await _context.ChiTietDonHangs
                .AnyAsync(ct => ct.MaSach == id);

            // Kiểm tra sách đã có bình luận chưa.
            bool daCoBinhLuan = await _context.BinhLuans
                .AnyAsync(bl => bl.MaSach == id);

            // Nếu có dữ liệu liên quan thì ẩn, không xóa cứng.
            if (daDuocMua || daCoBinhLuan)
            {
                sach.HienThi = false;

                await _context.SaveChangesAsync();

                string lyDo = "";

                if (daDuocMua && daCoBinhLuan)
                    lyDo = "Sách đã được mua và có bình luận.";
                else if (daDuocMua)
                    lyDo = "Sách đã được mua.";
                else
                    lyDo = "Sách đã có bình luận.";

                return Ok(new
                {
                    message = lyDo +
                        " Hệ thống đã ẩn sách thay vì xóa để bảo vệ dữ liệu liên quan.",
                    hienThi = false
                });
            }

            try
            {
                _context.Sachs.Remove(sach);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Đã xóa sách thành công."
                });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "Không thể xóa sách vì đang được dữ liệu khác tham chiếu."
                });
            }
        }


        [HttpPatch("{id}/toggle-sale")]
        public async Task<IActionResult> ToggleSale(int id)
        {
            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null) return NotFound();

            sach.HienThiTrangSale = !sach.HienThiTrangSale;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật hiển thị Sale thành công", hienThiTrangSale = sach.HienThiTrangSale });
        }

        // --- API UPLOAD FILE ĐÃ ĐƯỢC CẤU HÌNH CHUẨN CHO SWAGGER ---
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")] // Giúp Swagger hiểu đây là API nhận File
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Không có file được chọn");

                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "images");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { url = $"/images/{fileName}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server khi upload: {ex.Message}");
            }
        }
    }
}