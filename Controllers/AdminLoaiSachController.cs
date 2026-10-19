using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/admin/loaisach")]
[ApiController]
public class AdminLoaiSachController : ControllerBase
{
    private readonly BookParadiseDbContext _context;
    public AdminLoaiSachController(BookParadiseDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.LoaiSachs.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(LoaiSach loaiSach)
    {
        _context.LoaiSachs.Add(loaiSach);
        await _context.SaveChangesAsync();
        return Ok(loaiSach);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, LoaiSach loaiSach)
    {
        if (id != loaiSach.MaLoai) return BadRequest();
        _context.Entry(loaiSach).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var loai = await _context.LoaiSachs.FindAsync(id);
        if (loai == null) return NotFound();
        _context.LoaiSachs.Remove(loai);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã xóa loại sách" });
    }
}