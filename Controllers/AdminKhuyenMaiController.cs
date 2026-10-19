using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/admin/khuyenmai")]
[ApiController]
public class AdminKhuyenMaiController : ControllerBase
{
    private readonly BookParadiseDbContext _context;
    public AdminKhuyenMaiController(BookParadiseDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await _context.KhuyenMais.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(KhuyenMai km)
    {
        _context.KhuyenMais.Add(km);
        await _context.SaveChangesAsync();
        return Ok(km);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, KhuyenMai km)
    {
        if (id != km.MaKM) return BadRequest();
        _context.Entry(km).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var km = await _context.KhuyenMais.FindAsync(id);
        if (km == null) return NotFound();
        _context.KhuyenMais.Remove(km);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã xóa khuyến mãi" });
    }
}