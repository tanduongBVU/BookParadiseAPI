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
            _context.Sachs.Add(sach);
            await _context.SaveChangesAsync();
            return Ok(sach);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Sach sach)
        {
            if (id != sach.MaSach) return BadRequest();
            _context.Entry(sach).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sach = await _context.Sachs.FindAsync(id);
            if (sach == null) return NotFound();
            _context.Sachs.Remove(sach);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa sách" });
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