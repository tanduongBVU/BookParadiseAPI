using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookParadise.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SachController : ControllerBase
    {
        private readonly BookParadiseDbContext _context;

        public SachController(BookParadiseDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var danhSachSach = await _context.Sachs.ToListAsync();

            return Ok(danhSachSach);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sach = await _context.Sachs
                .FirstOrDefaultAsync(x => x.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }

            return Ok(sach);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Sach sach)
        {
            _context.Sachs.Add(sach);

            await _context.SaveChangesAsync();

            return Ok(sach);
        }
    }
}