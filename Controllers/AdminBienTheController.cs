using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [Route("api/admin/bienthe")]
    [ApiController]
    public class AdminBienTheController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;
        public AdminBienTheController(BookParadiseDbContext context) => _context = context;

        [HttpGet("sach/{maSach}")]
        public async Task<IActionResult> GetBySach(int maSach)
        {
            var bienThes = await _context.BienThes
                .Where(b => b.MaSach == maSach)
                .ToListAsync();
            return Ok(bienThes);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BienThe bienThe)
        {
            _context.BienThes.Add(bienThe);
            await _context.SaveChangesAsync();
            return Ok(bienThe);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BienThe bienThe)
        {
            if (id != bienThe.MaBienThe) return BadRequest();
            _context.Entry(bienThe).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var bienThe = await _context.BienThes.FindAsync(id);
            if (bienThe == null) return NotFound();
            _context.BienThes.Remove(bienThe);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa biến thể" });
        }
    }
}